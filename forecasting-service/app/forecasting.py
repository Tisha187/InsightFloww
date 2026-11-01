
from __future__ import annotations

from typing import Any

import numpy as np
import pandas as pd
from statsforecast import StatsForecast
from statsforecast.models import AutoARIMA, AutoETS


SUPPORTED_FREQUENCIES = {
    "Daily": "D",
    "Weekly": "W-MON",
    "Monthly": "MS",
    "Quarterly": "QS",
    "Yearly": "YS",
}


SEASONAL_PERIODS = {
    "Daily": 7,
    "Weekly": 52,
    "Monthly": 12,
    "Quarterly": 4,
    "Yearly": 1,
}


def _validate_series(
    dates: list[str],
    values: list[float],
    horizon: int,
    frequency: str,
) -> pd.DataFrame:

    if len(dates) != len(values):
        raise ValueError("Dates and values must have the same length.")

    if len(values) < 10:
        raise ValueError("At least 10 observations are required.")

    if horizon < 1 or horizon > 365:
        raise ValueError("Horizon must be between 1 and 365.")

    if frequency not in SUPPORTED_FREQUENCIES:
        raise ValueError("Unsupported frequency.")

    frame = pd.DataFrame({
        "ds": pd.to_datetime(dates, errors="coerce"),
        "y": pd.to_numeric(pd.Series(values), errors="coerce"),
    })

    if frame["ds"].isna().any():
        raise ValueError("One or more dates are invalid.")

    if frame["y"].isna().any():
        raise ValueError("Values must be valid numeric numbers.")

    if not np.isfinite(frame["y"].to_numpy(dtype=float)).all():
        raise ValueError("Values must be finite numbers.")

    if (frame["y"] < 0).any():
        raise ValueError("Negative values are not supported yet.")

    frame = frame.sort_values("ds").reset_index(drop=True)

    if frame["ds"].duplicated().any():
        raise ValueError("Duplicate dates found.")

    pandas_frequency = SUPPORTED_FREQUENCIES[frequency]

    period_frequency = {
        "D": "D",
        "W-MON": "W-SUN",
        "MS": "M",
        "QS": "Q",
        "YS": "Y",
    }[pandas_frequency]

    frame["ds"] = (
        frame["ds"]
        .dt.to_period(period_frequency)
        .dt.start_time
    )

    frame = (
        frame.groupby("ds", as_index=False)["y"]
        .sum()
        .sort_values("ds")
        .reset_index(drop=True)
    )

    if len(frame) < 10:
        raise ValueError(
            "At least 10 distinct time periods are required."
        )

    expected_dates = pd.date_range(
        start=frame["ds"].iloc[0],
        periods=len(frame),
        freq=pandas_frequency,
    )

    if not frame["ds"].reset_index(drop=True).equals(
        pd.Series(expected_dates, name="ds")
    ):
        raise ValueError(
            "The time series has missing periods."
        )

    frame.insert(0, "unique_id", "sales")

    return frame[["unique_id", "ds", "y"]]


def _build_models(
    frequency: str,
    history_length: int,
) -> list[Any]:

    season_length = SEASONAL_PERIODS[frequency]

    use_seasonality = (
        season_length > 1
        and history_length >= 2 * season_length
    )

    effective_season_length = (
        season_length if use_seasonality else 1
    )

    return [
        AutoETS(
            season_length=effective_season_length,
            model="ZZZ",
        ),
        AutoARIMA(
            season_length=effective_season_length,
        ),
    ]


def _candidate_forecasts(
    train: pd.DataFrame,
    horizon: int,
    frequency: str,
) -> dict[str, np.ndarray]:
    """Generate predictions from all eligible candidate models."""

    values = train["y"].to_numpy(dtype=float)

    predictions: dict[str, np.ndarray] = {}

    # 1. Naive baseline: repeat the last observed value.
    predictions["NaiveBaseline"] = np.repeat(
        values[-1],
        horizon,
    )

    # 2. Historic average: repeat the historical mean.
    predictions["HistoricAverage"] = np.repeat(
        np.mean(values),
        horizon,
    )

    # 3. Random walk with drift: extend the average change.
    average_change = float(np.mean(np.diff(values)))

    steps = np.arange(1, horizon + 1, dtype=float)

    predictions["RandomWalkWithDrift"] = (
        values[-1] + average_change * steps
    )

    # 4. Seasonal naive: repeat the last complete seasonal cycle.
    season_length = SEASONAL_PERIODS[frequency]

    if (
        season_length > 1
        and len(values) >= 2 * season_length
    ):
        last_season = values[-season_length:]

        predictions["SeasonalNaive"] = np.resize(
            last_season,
            horizon,
        )

    # 5. Statistical models: AutoETS and AutoARIMA.
    models = _build_models(frequency, len(train))

    engine = StatsForecast(
        models=models,
        freq=SUPPORTED_FREQUENCIES[frequency],
        n_jobs=1,
    )

    statistical_forecasts = engine.forecast(
        df=train,
        h=horizon,
    )

    for model in models:
        name = model.__class__.__name__

        if name in statistical_forecasts.columns:
            predictions[name] = (
                statistical_forecasts[name]
                .to_numpy(dtype=float)
            )

    return predictions


def _calculate_metrics(
    errors: list[float],
) -> dict[str, float]:

    error_array = np.asarray(errors, dtype=float)

    return {
        "mae": float(np.mean(np.abs(error_array))),
        "rmse": float(np.sqrt(np.mean(error_array ** 2))),
    }


def _evaluate_models(
    data: pd.DataFrame,
    frequency: str,
    horizon: int,
) -> tuple[dict[str, dict[str, float]], int]:

    total_observations = len(data)

    evaluation_horizon = min(
        max(1, min(horizon, 3)),
        max(1, total_observations // 5),
    )

    season_length = SEASONAL_PERIODS[frequency]

    # Use the same evaluation windows for every candidate.
    # Include seasonal-naive only when enough history exists.
    minimum_training_size = 8

    if (
        season_length > 1
        and total_observations >= 2 * season_length
    ):
        minimum_training_size = max(
            minimum_training_size,
            2 * season_length,
        )

    if total_observations - evaluation_horizon < minimum_training_size:
        raise ValueError(
            "Not enough historical observations for backtesting."
        )

    errors_by_model: dict[str, list[float]] = {}

    evaluation_windows = 0

    for origin in range(
        minimum_training_size,
        total_observations - evaluation_horizon + 1,
    ):

        train = data.iloc[:origin].copy()

        test = data.iloc[
            origin:origin + evaluation_horizon
        ].copy()

        actual = test["y"].to_numpy(dtype=float)

        candidate_predictions = _candidate_forecasts(
            train=train,
            horizon=evaluation_horizon,
            frequency=frequency,
        )

        valid_predictions = {}

        for name, predicted in candidate_predictions.items():

            if (
                len(predicted) != len(actual)
                or not np.isfinite(predicted).all()
            ):
                continue

            # Revenue forecasting currently supports non-negative targets.
            predicted = np.maximum(predicted, 0.0)

            valid_predictions[name] = predicted

        if not valid_predictions:
            continue

        for name, predicted in valid_predictions.items():

            errors = actual - predicted

            errors_by_model.setdefault(name, []).extend(
                errors.tolist()
            )

        evaluation_windows += 1

    if not errors_by_model:
        raise RuntimeError(
            "No forecasting model produced valid backtesting results."
        )

    metrics = {
        name: _calculate_metrics(errors)
        for name, errors in errors_by_model.items()
    }

    return metrics, evaluation_windows


def forecast_series(
    dates: list[str],
    values: list[float],
    horizon: int,
    frequency: str,
) -> dict[str, Any]:

    data = _validate_series(
        dates=dates,
        values=values,
        horizon=horizon,
        frequency=frequency,
    )

    stats_frequency = SUPPORTED_FREQUENCIES[frequency]

    # Evaluate every eligible model on identical backtesting windows.
    metrics, evaluation_windows = _evaluate_models(
        data=data,
        frequency=frequency,
        horizon=horizon,
    )

    # Select the candidate with the lowest overall MAE.
    # Select the best model using MAE with a small tolerance.
    best_mae = min(
        model_metrics["mae"]
        for model_metrics in metrics.values()
    )

    mae_tolerance = 0.01  # 1%

    eligible_models = [
        name
        for name, model_metrics in metrics.items()
        if model_metrics["mae"] <= best_mae * (1 + mae_tolerance)
    ]

    # Prefer simpler models when their MAE is practically equivalent.
    model_priority = [
        "NaiveBaseline",
        "HistoricAverage",
        "RandomWalkWithDrift",
        "SeasonalNaive",
        "AutoETS",
        "AutoARIMA",
    ]

    best_model_name = min(
        eligible_models,
        key=lambda name: model_priority.index(name),
    )

    # Refit the selected candidate on complete historical data.
    final_predictions = _candidate_forecasts(
        train=data,
        horizon=horizon,
        frequency=frequency,
    )

    predictions = final_predictions[best_model_name]

    if (
        len(predictions) != horizon
        or not np.isfinite(predictions).all()
    ):
        raise RuntimeError(
            "The selected model produced invalid predictions."
        )

    predictions = np.maximum(predictions, 0.0)

    future_dates = pd.date_range(
        start=data["ds"].iloc[-1],
        periods=horizon + 1,
        freq=stats_frequency,
    )[1:]

    return {
        "modelUsed": best_model_name,
        "frequency": frequency,
        "horizon": horizon,
        "historical": [
            {
                "date": row.ds.date().isoformat(),
                "actual": float(row.y),
            }
            for row in data.itertuples(index=False)
        ],
        "forecasts": [
            {
                "date": date.date().isoformat(),
                "predicted": float(prediction),
            }
            for date, prediction in zip(
                future_dates,
                predictions,
            )
        ],
        "evaluation": {
            "holdoutObservations": evaluation_windows,
            "evaluationWindows": evaluation_windows,
            "models": metrics,
        },
    }

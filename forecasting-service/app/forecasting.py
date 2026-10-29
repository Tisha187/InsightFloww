
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


def _validate_series(
    dates: list[str],
    values: list[float],
    horizon: int,
    frequency: str,
) -> pd.DataFrame:
    """Validate and prepare a single time series for forecasting."""

    if len(dates) != len(values):
        raise ValueError("Dates and values must have the same length.")

    if len(values) < 10:
        raise ValueError(
            "At least 10 observations are required for forecasting."
        )

    if horizon < 1 or horizon > 365:
        raise ValueError("Horizon must be between 1 and 365.")

    if frequency not in SUPPORTED_FREQUENCIES:
        raise ValueError(
            f"Unsupported frequency. Choose one of: "
            f"{', '.join(SUPPORTED_FREQUENCIES)}."
        )

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
        raise ValueError(
            "Negative values are not supported by this initial version."
        )

    frame = frame.sort_values("ds").reset_index(drop=True)

    if frame["ds"].duplicated().any():
        raise ValueError(
            "Duplicate dates found. Aggregate values by date before forecasting."
        )

    # Normalize dates to the selected frequency and aggregate observations
    # that fall within the same period.
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
            "At least 10 distinct time periods are required after aggregation."
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
            "The time series has missing periods. Fill or handle missing "
            "periods before forecasting."
        )

    frame.insert(0, "unique_id", "sales")

    return frame[["unique_id", "ds", "y"]]


def _build_models(
    frequency: str,
    history_length: int,
) -> list[Any]:
    """Choose model configurations based on frequency and history length."""

    seasonal_periods = {
        "Daily": 7,
        "Weekly": 52,
        "Monthly": 12,
        "Quarterly": 4,
        "Yearly": 1,
    }

    season_length = seasonal_periods[frequency]

    # Enable seasonal models only when at least two seasonal cycles
    # are available in the supplied history.
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


def forecast_series(
    dates: list[str],
    values: list[float],
    horizon: int,
    frequency: str,
) -> dict[str, Any]:
    """Generate forecasts and evaluate candidates using a time holdout."""

    data = _validate_series(
        dates=dates,
        values=values,
        horizon=horizon,
        frequency=frequency,
    )

    stats_frequency = SUPPORTED_FREQUENCIES[frequency]

    # Reserve the latest observations for chronological holdout evaluation.
    test_size = min(
        max(2, horizon),
        max(2, len(data) // 5),
    )

    if len(data) - test_size < 8:
        raise ValueError(
            "Not enough historical observations for a training and test split."
        )

    train = data.iloc[:-test_size].copy()
    test = data.iloc[-test_size:].copy()

    evaluation_models = _build_models(
        frequency,
        len(train),
    )

    evaluation_engine = StatsForecast(
        models=evaluation_models,
        freq=stats_frequency,
        n_jobs=1,
    )

    evaluation = evaluation_engine.forecast(
        df=train,
        h=test_size,
        fitted=False,
    )

    actual = test["y"].to_numpy(dtype=float)
    metrics: dict[str, dict[str, float]] = {}

    for model in evaluation_models:
        name = model.__class__.__name__

        if name not in evaluation.columns:
            continue

        predicted = evaluation[name].to_numpy(dtype=float)
        errors = actual - predicted

        metrics[name] = {
            "mae": float(np.mean(np.abs(errors))),
            "rmse": float(np.sqrt(np.mean(errors ** 2))),
        }

    

    if not metrics:
        raise RuntimeError(
            "No forecasting model produced evaluation results."
        )

    # Naive baseline: every test prediction equals the last training value.
    naive_predictions = np.repeat(
        float(train["y"].iloc[-1]),
        test_size,
    )

    naive_errors = actual - naive_predictions

    metrics["NaiveBaseline"] = {
        "mae": float(np.mean(np.abs(naive_errors))),
        "rmse": float(np.sqrt(np.mean(naive_errors ** 2))),
    }

    # Select the candidate with the lowest holdout MAE.
    best_model_name = min(
        metrics,
        key=lambda name: metrics[name]["mae"],
    )

    # Refit candidate models on the complete history for future forecasting.
    full_engine = StatsForecast(
        models=_build_models(
            frequency,
            len(data),
        ),
        freq=stats_frequency,
        n_jobs=1,
    )

    full_forecasts = full_engine.forecast(
        df=data,
        h=horizon,
    )

    if best_model_name == "NaiveBaseline":
        predictions = np.repeat(
            float(data["y"].iloc[-1]),
            horizon,
        )
        model_used = "NaiveBaseline"
    else:
        predictions = full_forecasts[
            best_model_name
        ].to_numpy(dtype=float)
        model_used = best_model_name

    future_dates = pd.date_range(
        start=data["ds"].iloc[-1],
        periods=horizon + 1,
        freq=stats_frequency,
    )[1:]

    return {
        "modelUsed": model_used,
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
                "predicted": max(0.0, float(prediction)),
            }
            for date, prediction in zip(future_dates, predictions)
        ],
        "evaluation": {
            "holdoutObservations": test_size,
            "models": metrics,
        },
    }

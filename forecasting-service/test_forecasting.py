
from pathlib import Path

import numpy as np
import pandas as pd

from app.forecasting import (
    forecast_series,
    _build_models,
    SUPPORTED_FREQUENCIES,
)
from statsforecast import StatsForecast


# Load the complex test dataset.
DATASET_PATH = (
    Path(__file__).parent.parent
    / "insightflow_complex_weekly_test_dataset.csv"
)

if not DATASET_PATH.exists():
    raise FileNotFoundError(
        f"Test dataset not found: {DATASET_PATH}"
    )

df = pd.read_csv(DATASET_PATH)

print("\n=== ORIGINAL DATASET ===")
print("Rows:", len(df))
print("Columns:", list(df.columns))


# Prepare the weekly revenue series.
df["Date"] = pd.to_datetime(df["Date"], errors="coerce")
df["Revenue"] = pd.to_numeric(df["Revenue"], errors="coerce")

if df["Date"].isna().any() or df["Revenue"].isna().any():
    raise ValueError("Dataset contains invalid dates or revenue values.")

historical = (
    df.groupby("Date", as_index=False)["Revenue"]
    .sum()
    .sort_values("Date")
    .reset_index(drop=True)
)

print("\n=== AGGREGATED WEEKLY REVENUE ===")

for row in historical.itertuples(index=False):
    print(f"{row.Date.date()}: {row.Revenue:.2f}")

dates = historical["Date"].dt.strftime("%Y-%m-%d").tolist()
values = historical["Revenue"].astype(float).tolist()


# Run the forecasting engine.
result = forecast_series(
    dates=dates,
    values=values,
    horizon=6,
    frequency="Weekly",
)

print("\n=== INDIVIDUAL MODEL FORECASTS ===")

models = _build_models("Weekly", len(dates))

engine = StatsForecast(
    models=models,
    freq=SUPPORTED_FREQUENCIES["Weekly"],
    n_jobs=1,
)

individual_forecasts = engine.forecast(
    df=pd.DataFrame({
        "unique_id": ["sales"] * len(dates),
        "ds": pd.to_datetime(dates),
        "y": values,
    }),
    h=6,
)

for model in models:
    name = model.__class__.__name__

    print(f"\n{name}:")

    for value in individual_forecasts[name].tolist():
        print(f"{value:.2f}")


print("\n=== SELECTED MODEL ===")
print(result["modelUsed"])

print("\n=== BACKTESTING METRICS ===")
print("Evaluation windows:", result["evaluation"]["evaluationWindows"])

for model, metrics in result["evaluation"]["models"].items():
    print(
        f"{model}: "
        f"MAE={metrics['mae']:.2f}, "
        f"RMSE={metrics['rmse']:.2f}"
    )


print("\n=== FUTURE FORECAST ===")

for item in result["forecasts"]:
    print(
        f"{item['date']}: "
        f"{item['predicted']:.2f}"
    )


print("\n=== VALIDATION ===")

assert len(result["historical"]) == 16
assert len(result["forecasts"]) == 6

assert all(
    np.isfinite(item["predicted"])
    for item in result["forecasts"]
)

assert all(
    item["predicted"] >= 0
    for item in result["forecasts"]
)

assert result["modelUsed"] in {
    "AutoETS",
    "AutoARIMA",
    "NaiveBaseline",
    "HistoricAverage",
    "RandomWalkWithDrift",
    "SeasonalNaive",
}

print("All complex dataset assertions passed.")

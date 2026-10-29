
from datetime import date
import numpy as np
import pandas as pd

from app.forecasting import forecast_series


# Generate 36 months of synthetic revenue data.
rng = np.random.default_rng(42)
dates = pd.date_range(start="2023-01-01", periods=36, freq="MS")

# Simulated revenue with an upward trend and recurring monthly variation.
values = [
    round(
        100_000
        + i * 2_000
        + 8_000 * np.sin(2 * np.pi * i / 12)
        + rng.normal(0, 1_500),
        2,
    )
    for i in range(len(dates))
]

result = forecast_series(
    dates=[d.date().isoformat() for d in dates],
    values=values,
    horizon=6,
    frequency="Monthly",
)

print("\n=== FORECAST MODEL ===")
print(result["modelUsed"])

print("\n=== EVALUATION METRICS ===")
for model, metrics in result["evaluation"]["models"].items():
    print(
        f"{model}: "
        f"MAE={metrics['mae']:.2f}, "
        f"RMSE={metrics['rmse']:.2f}"
    )

print("\n=== FUTURE FORECAST ===")
for item in result["forecasts"]:
    print(f"{item['date']}: {item['predicted']:.2f}")

print("\n=== VALIDATION ===")
assert len(result["historical"]) == 36
assert len(result["forecasts"]) == 6
assert all(np.isfinite(item["predicted"]) for item in result["forecasts"])
assert all(item["predicted"] >= 0 for item in result["forecasts"])
assert result["modelUsed"] in {
    "AutoETS",
    "AutoARIMA",
    "NaiveBaseline",
}

print("All basic forecast assertions passed.")

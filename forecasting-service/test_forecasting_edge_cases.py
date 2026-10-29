
import pandas as pd

from app.forecasting import forecast_series


def run_case(name, dates, values, horizon=3):
    print(f"\n--- {name} ---")

    try:
        result = forecast_series(
            dates=dates,
            values=values,
            horizon=horizon,
            frequency="Monthly",
        )

        print("Model:", result["modelUsed"])
        print("Forecasts:", result["forecasts"])
    except ValueError as exc:
        print("Validation handled:", exc)


# Case 1: Constant monthly revenue.
dates = pd.date_range("2023-01-01", periods=36, freq="MS")
run_case(
    "Constant sales",
    [d.date().isoformat() for d in dates],
    [10000.0] * 36,
)

# Case 2: One missing month.
dates_with_gap = dates.delete(15)
run_case(
    "Missing month",
    [d.date().isoformat() for d in dates_with_gap],
    [10000.0] * len(dates_with_gap),
)

# Case 3: Only 8 observations.
short_dates = pd.date_range("2025-01-01", periods=8, freq="MS")
run_case(
    "Short history",
    [d.date().isoformat() for d in short_dates],
    [10000.0] * 8,
)

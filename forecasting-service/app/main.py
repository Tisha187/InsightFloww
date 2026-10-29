
from typing import Literal

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field

from app.forecasting import forecast_series


app = FastAPI(
    title="InsightFlow Forecasting Service",
    description="Python forecasting engine for InsightFlow AI.",
    version="0.1.0",
)

@app.get("/")
def root():
    return {
        "message": "InsightFlow AI Forecasting Service",
        "status": "healthy",
        "docs": "/docs",
        "health": "/health"
    }


class ForecastRequest(BaseModel):
    dates: list[str] = Field(min_length=10, max_length=10000)
    values: list[float] = Field(min_length=10, max_length=10000)
    horizon: int = Field(ge=1, le=365)
    frequency: Literal[
        "Daily",
        "Weekly",
        "Monthly",
        "Quarterly",
        "Yearly",
    ]


@app.get("/health")
def health_check():
    return {
        "status": "healthy",
        "service": "insightflow-forecasting",
    }


@app.post("/forecast")
def create_forecast(request: ForecastRequest):
    try:
        return forecast_series(
            dates=request.dates,
            values=request.values,
            horizon=request.horizon,
            frequency=request.frequency,
        )
    except ValueError as exc:
        raise HTTPException(
            status_code=422,
            detail=str(exc),
        ) from exc
    except Exception as exc:
        # Log the exception server-side in a later hardening step.
        raise HTTPException(
            status_code=500,
            detail="Forecast generation failed.",
        ) from exc

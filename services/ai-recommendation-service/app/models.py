from pydantic import BaseModel, Field


class RecommendationOption(BaseModel):
    carrier: str
    amount: float
    eta_days: int = Field(alias="etaDays")


class RecommendationExplanationRequest(BaseModel):
    preference: str
    recommended_carrier: str = Field(alias="recommendedCarrier")
    cheapest_carrier: str = Field(alias="cheapestCarrier")
    fastest_carrier: str = Field(alias="fastestCarrier")
    options: list[RecommendationOption]


class RecommendationExplanationResponse(BaseModel):
    explanation: str

from pydantic import BaseModel


class RecommendationRequest(BaseModel):
    quote_id: str

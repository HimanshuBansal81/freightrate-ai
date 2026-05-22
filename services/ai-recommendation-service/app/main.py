from fastapi import FastAPI

from app.models import RecommendationExplanationRequest, RecommendationExplanationResponse
from app.recommendation_service import RecommendationService

app = FastAPI(title="FreightRate AI Recommendation Service")
recommendation_service = RecommendationService()


@app.get("/health")
def health_check() -> dict[str, str]:
    return {"status": "healthy", "service": "ai-recommendation-service"}


@app.post("/api/recommendations/explain", response_model=RecommendationExplanationResponse)
async def explain_recommendation(
    request: RecommendationExplanationRequest,
) -> RecommendationExplanationResponse:
    explanation = await recommendation_service.explain(request)
    return RecommendationExplanationResponse(explanation=explanation)

from app.models import RecommendationExplanationRequest


def build_fallback_explanation(request: RecommendationExplanationRequest) -> str:
    return (
        f"{request.recommended_carrier} is recommended because it best matches your "
        f"{request.preference} preference based on the calculated price and delivery time."
    )

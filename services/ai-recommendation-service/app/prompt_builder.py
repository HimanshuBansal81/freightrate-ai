from app.models import RecommendationExplanationRequest


def build_prompt(request: RecommendationExplanationRequest) -> str:
    options_text = ", ".join(
        f"{option.carrier}: {option.amount:.2f}, {option.eta_days} days"
        for option in request.options
    )
    return (
        "Explain the backend-selected freight recommendation in under 80 words. "
        "Do not calculate prices, change prices, invent carriers, or override the recommendation. "
        f"Preference: {request.preference}. "
        f"Recommended carrier: {request.recommended_carrier}. "
        f"Cheapest carrier: {request.cheapest_carrier}. "
        f"Fastest carrier: {request.fastest_carrier}. "
        f"Options: {options_text}."
    )

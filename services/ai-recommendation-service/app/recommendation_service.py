import os

from app.fallback_service import build_fallback_explanation
from app.models import RecommendationExplanationRequest
from app.prompt_builder import build_prompt


class RecommendationService:
    def __init__(self) -> None:
        self.provider = os.getenv("AI_PROVIDER", "none").strip().lower()
        self.model = os.getenv("AI_MODEL", "").strip()
        self.timeout_seconds = int(os.getenv("AI_TIMEOUT_SECONDS", "5"))

    async def explain(self, request: RecommendationExplanationRequest) -> str:
        if self.provider == "none":
            return build_fallback_explanation(request)

        try:
            if self.provider == "openai" and os.getenv("OPENAI_API_KEY"):
                return await self._explain_with_openai(request)

            return build_fallback_explanation(request)
        except Exception:
            return build_fallback_explanation(request)

    async def _explain_with_openai(self, request: RecommendationExplanationRequest) -> str:
        # Provider integration is intentionally deferred. The prompt is built here
        # so adding a real client later has a narrow, testable insertion point.
        _ = build_prompt(request)
        return build_fallback_explanation(request)

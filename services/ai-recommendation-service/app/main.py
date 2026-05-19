from fastapi import FastAPI

app = FastAPI(title="FreightRate AI Recommendation Service")


@app.get("/health")
def health_check() -> dict[str, str]:
    return {"status": "healthy", "service": "ai-recommendation-service"}

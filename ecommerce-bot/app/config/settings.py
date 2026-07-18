from pydantic_settings import BaseSettings, SettingsConfigDict

class Settings(BaseSettings):
    POSTGRES_URI: str
    RABBITMQ_URL: str
    OPENAI_API_KEY: str | None = None
    GEMINI_API_KEY: str | None = None
    DISCORD_WEBHOOK_URL: str
    AES_MASTER_KEY: str
    JWT__Key: str
    REDIS_URL: str = "redis://localhost:6379"
    DEEPSEEK_API_KEY: str | None = None
    GROQ_API_KEY: str | None = None

    model_config = SettingsConfigDict(env_file='.env', env_file_encoding='utf-8', extra='ignore', case_sensitive=False)

settings = Settings()

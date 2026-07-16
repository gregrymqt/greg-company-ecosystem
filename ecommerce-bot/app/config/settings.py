from pydantic_settings import BaseSettings, SettingsConfigDict

class Settings(BaseSettings):
    MONGO_URI: str = ""
    POSTGRES_URI: str
    RABBITMQ_URL: str
    OPENAI_API_KEY: str
    GEMINI_API_KEY: str
    DISCORD_WEBHOOK_URL: str
    AES_MASTER_KEY: str
    JWT__Key: str
    REDIS_URL: str = "redis://localhost:6379"
    

    model_config = SettingsConfigDict(env_file='.env', env_file_encoding='utf-8', extra='ignore', case_sensitive=False)

settings = Settings()

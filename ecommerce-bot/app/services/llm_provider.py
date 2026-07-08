import abc
import os
import asyncio
import logging
import openai
from google import genai
from app.models.llm_responses import EnrichedProductResponse
from app.utils.logger import get_logger

logger = get_logger("LLMProvider")

class LLMProvider(abc.ABC):
    @abc.abstractmethod
    async def enrich(self, prompt: str) -> EnrichedProductResponse:
        pass
    
    @property
    @abc.abstractmethod
    def name(self) -> str:
        pass


class OpenAIProvider(LLMProvider):
    def __init__(self):
        self.client = openai.AsyncOpenAI(api_key=os.getenv("OPENAI_API_KEY"))
        
    @property
    def name(self) -> str:
        return "OpenAI"

    async def enrich(self, prompt: str) -> EnrichedProductResponse:
        response = await self.client.beta.chat.completions.parse(
            model="gpt-4o-mini",
            messages=[{"role": "user", "content": prompt}],
            response_format=EnrichedProductResponse,
        )
        return response.choices[0].message.parsed


class GeminiProvider(LLMProvider):
    def __init__(self):
        api_key = os.getenv("GEMINI_API_KEY")
        if not api_key:
            raise ValueError("GEMINI_API_KEY is not configured.")
        self.client = genai.Client(api_key=api_key)
        
    @property
    def name(self) -> str:
        return "Gemini"

    async def enrich(self, prompt: str) -> EnrichedProductResponse:
        # Use google-genai structured output with Pydantic schema
        response = await self.client.aio.models.generate_content(
            model='gemini-2.5-flash',
            contents=prompt,
            config=genai.types.GenerateContentConfig(
                response_mime_type="application/json",
                response_schema=EnrichedProductResponse,
            ),
        )
        # Parse the JSON response manually since google-genai with response_schema returns string JSON
        # For simplicity and correctness with the Pydantic model:
        try:
            return EnrichedProductResponse.model_validate_json(response.text)
        except Exception as e:
            logger.error(f"Erro ao validar schema do Gemini: {e} | Resposta: {response.text}", exc_info=True)
            raise

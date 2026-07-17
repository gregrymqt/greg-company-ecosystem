/**
 * Estados possíveis do pipeline de processamento de cada URL.
 * Mapeia diretamente a máquina de estados controlada pelo Worker e disparada via SSE.
 */
export type ProcessStatus = 'idle' | 'pending' | 'scraping' | 'generating' | 'completed' | 'failed';

/**
 * Dados brutos extraídos do fornecedor original pelas Estratégias 1 e 2 do nosso Scraper.
 * Alimenta o lado esquerdo ("Antes") do nosso ComparisonPanel.
 */
export interface OriginalProductData {
  title: string;
  description: string;
  price?: string;
  imageUrl?: string;
}

/**
 * Output ultra-persuasivo gerado pelos providers de LLM (OpenAI/Gemini).
 * Alimenta o lado direito ("Depois") com copywriting focado em conversão.
 */
export interface EnhancedProductData {
  seoTitle: string;
  copywriting: string;
  tags: string[];
}

/**
 * Entidade que gerencia o estado de processamento individual de cada uma das 3 URLs.
 */
export interface DemoProductItem {
  url: string;
  status: ProcessStatus;
  progress: number; // Progresso percentual (0 a 100) para alimentar a barra de loading
  original?: OriginalProductData;
  enhanced?: EnhancedProductData;
  error?: string;
}

/**
 * Input do formulário de captura da Landing Page freemium.
 * Restringe rigidamente o array para o limite do Rate Limiter.
 */
export interface DemoFormInput {
  urls: string[]; // Validar no client-side para aceitar no máximo 3 strings válidas
}

/**
 * Contrato do payload que o endpoint `/api/v1/demo` via Server-Sent Events (SSE)
 * vai cuspir linha a linha para o React consumir.
 */
export interface SseStreamPayload {
  url: string;
  status: ProcessStatus;
  progress: number;
  original?: OriginalProductData;
  enhanced?: EnhancedProductData;
  error?: string;
}

export interface DemoTriggerResponse {
  status: string;
  ticket_id: string;
  message?: string;
}
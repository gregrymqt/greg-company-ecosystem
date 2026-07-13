import { useState } from 'react';
import { AiScraperService } from '../services/ai-scraper.service';
import { AlertService } from '@/shared/services/alert.service';

export const useAiScraper = () => {
  const [isSavingKey, setIsSavingKey] = useState(false);
  const [isStartingScraper, setIsStartingScraper] = useState(false);

  const saveCredentials = async (data: Record<string, unknown>) => {
    setIsSavingKey(true);
    try {
      await AiScraperService.saveCredentials({
        provider: String(data.provider),
        access_token: String(data.access_token)
      });
      AlertService.notify('Sucesso', 'Credencial salva com sucesso!', 'success');
    } catch (error) {
      AlertService.notify('Erro', 'Falha ao salvar a credencial.', 'error');
    } finally {
      setIsSavingKey(false);
    }
  };

  const startScraping = async (data: Record<string, unknown>) => {
    setIsStartingScraper(true);
    try {
      await AiScraperService.startExtraction(String(data.url));
      AlertService.notify('Sucesso', 'A extração foi iniciada em background', 'success');
    } catch (error) {
      AlertService.notify('Erro', 'Falha ao iniciar a extração.', 'error');
    } finally {
      setIsStartingScraper(false);
    }
  };

  return {
    isSavingKey,
    isStartingScraper,
    saveCredentials,
    startScraping
  };
};

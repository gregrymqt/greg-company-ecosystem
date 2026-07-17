import { useState } from 'react';
import { AiScraperService } from '../services/ai-scraper.service';
import { AlertService } from '@/shared/services/alert.service';
import { useAuth } from '@/features/auth/hooks/useAuth';
import { StorageService, STORAGE_KEYS } from '@/shared/services/storage.service';

export const useAiScraper = () => {
  const [isSavingKey, setIsSavingKey] = useState(false);
  const [isStartingScraper, setIsStartingScraper] = useState(false);
  const [lastTaskId, setLastTaskId] = useState<string | null>(null);
  const { user } = useAuth();

  const tenantId = StorageService.getItem<string>(STORAGE_KEYS.TENANT_ID) || 
                   user?.tenant_id || 
                   (user as any)?.tenantId || 
                   user?.tenants?.[0] || 
                   '';

  const saveCredentials = async (data: Record<string, unknown>) => {
    setIsSavingKey(true);
    try {
      await AiScraperService.saveCredentials({
        tenant_id: tenantId,
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
    setLastTaskId(null);
    try {
      const response = await AiScraperService.startExtraction({
        url: String(data.url),
        tenant_id: tenantId
      });
      setLastTaskId(response.task_id);
      console.log('Scraping task started with ID:', response.task_id);
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
    startScraping,
    lastTaskId
  };
};

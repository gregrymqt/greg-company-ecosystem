import { useState, useCallback } from 'react';
import { PlanService } from '../services/plans.service';
import { AlertService } from '../../../shared/services/alert.service'; // Ajuste o import do seu Alert
import type { 
  PlanAdminSummary, 
  PlanAdminDetail, 
  PagedResult, 
  CreatePlanRequest, 
  UpdatePlanRequest 
} from '../types/plans.type';
import { ApiError } from 'src/shared/services/api.service';

export const useAdminPlans = () => {
    // Estados
    const [loading, setLoading] = useState(false);
    const [plans, setPlans] = useState<PlanAdminSummary[]>([]);
    const [currentPlan, setCurrentPlan] = useState<PlanAdminDetail | null>(null);
    const [pagination, setPagination] = useState<PagedResult<PlanAdminSummary> | null>(null);

    // --- ACTIONS ---

    // Busca lista administrativa (Tabela)
    const fetchAdminPlans = useCallback(async (page: number = 1, pageSize: number = 10) => {
        setLoading(true);
        try {
            const data = await PlanService.getAdminPlans(page, pageSize);
            setPlans(data.items);
            setPagination(data);
        } catch (error) {
            AlertService.error('Erro ao carregar', error instanceof ApiError ? error.message : 'Não foi possível buscar os planos.');
        } finally {
            setLoading(false);
        }
    }, []);

    // Busca detalhes para edição
    const getPlanById = useCallback(async (id: string) => {
        setLoading(true);
        try {
            const data = await PlanService.getById(id);
            setCurrentPlan(data);
            return data;
        } catch (error) {
            AlertService.error('Erro', error instanceof ApiError ? error.message : 'Não foi possível carregar os detalhes.');
            return null;
        } finally {
            setLoading(false);
        }
    }, []);

    // Criação
    const createPlan = async (planData: CreatePlanRequest): Promise<boolean> => {
        setLoading(true);
        try {
            await PlanService.create(planData);
            await AlertService.success('Sucesso!', 'Plano criado com sucesso.');
            return true;
        } catch (error) {
            AlertService.error('Falha ao criar', error instanceof ApiError ? error.message : 'Erro ao comunicar com Mercado Pago.');
            return false;
        } finally {
            setLoading(false);
        }
    };

    // Atualização
    const updatePlan = async (id: string, planData: UpdatePlanRequest): Promise<boolean> => {
        setLoading(true);
        try {
            await PlanService.update(id, planData);
            await AlertService.success('Atualizado!', 'Plano editado com sucesso.');
            return true;
        } catch (error) {
            AlertService.error('Falha na atualização', error instanceof ApiError ? error.message : 'Erro ao salvar alterações.');
            return false;
        } finally {
            setLoading(false);
        }
    };

    // Exclusão
    const deletePlan = async (id: string): Promise<boolean> => {
        const { isConfirmed } = await AlertService.confirm(
            'Tem certeza?',
            'Isso removerá o plano permanentemente e pode afetar novas assinaturas.'
        );

        if (!isConfirmed) return false;

        setLoading(true);
        try {
            await PlanService.delete(id);
            await AlertService.success('Removido', 'Plano excluído.');
            
            // Atualiza lista localmente para não precisar refazer o fetch
            setPlans((prev) => prev.filter((p) => p.id !== id));
            return true;
        } catch (error) {
            AlertService.error('Erro ao excluir', error instanceof ApiError ? error.message : 'Verifique vínculos deste plano.');
            return false;
        } finally {
            setLoading(false);
        }
    };

    return {
        plans,
        currentPlan,
        pagination,
        loading,
        fetchAdminPlans,
        getPlanById,
        createPlan,
        updatePlan,
        deletePlan
    };
};
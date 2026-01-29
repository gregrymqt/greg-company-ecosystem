import { useState, useCallback } from 'react';
import { PlanService } from '../services/plans.service';
import type { PlanPublic } from '../types/plans.type';
import { ApiError } from 'src/shared/services/api.service';

export const usePublicPlans = () => {
    const [loading, setLoading] = useState(false);
    const [publicPlans, setPublicPlans] = useState<PlanPublic[]>([]);

    // Busca apenas os planos "Vitrine" (Allow)
    const fetchPublicPlans = useCallback(async () => {
        setLoading(true);
        try {
            // Normalmente vitrine não precisa de paginação complexa, pegamos a primeira página cheia
            const data = await PlanService.getPublicPlans(1, 50); 
            setPublicPlans(data.items);
        } catch (error) {
            console.error(error instanceof ApiError ? error.message : 'Erro ao carregar planos públicos.');
            // Em área pública, as vezes é melhor não mostrar Alert pop-up, 
            // mas sim renderizar um componente de "Erro ao carregar preços"
        } finally {
            setLoading(false);
        }
    }, []);

    return {
        publicPlans,
        loading,
        fetchPublicPlans
    };
};
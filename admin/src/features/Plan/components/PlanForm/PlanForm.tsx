import React, { useMemo } from 'react';
import { type SubmitHandler } from 'react-hook-form';

import styles from './PlanForm.module.scss';
import { Form } from '@/components/Form';
import { useAdminPlans } from '../../hooks/useAdminPlans';
import type { PlanAdminDetail, UpdatePlanRequest, CreatePlanRequest } from '../../types';

// Interface "Plana" para controlar o formulário visualmente
interface PlanFormSchema {
    reason: string;
    description: string;
    transactionAmount: number;
    frequency: number;
    frequencyType: string;
}

interface PlanFormProps {
    initialData?: PlanAdminDetail | null; // Se vier preenchido, é Edição
    onSuccess: () => void; // Callback para fechar modal ou atualizar lista
    onCancel?: () => void;
}

export const PlanForm: React.FC<PlanFormProps> = ({ initialData, onSuccess, onCancel }) => {
    const { createPlan, updatePlan, loading } = useAdminPlans();

    const isEditing = !!initialData;

    // 2. Valores Iniciais (Mapeia do DTO do Backend para o Form Schema)
    const defaultValues: Partial<PlanFormSchema> = useMemo(() => {
        if (!initialData) return {
            frequencyType: 'months',
            frequency: 1
        };

        return {
            reason: initialData.name,
            description: initialData.description || '',
            transactionAmount: initialData.transactionAmount,
            frequency: initialData.frequency,
            frequencyType: initialData.frequencyType
        };
    }, [initialData]);

    // 3. Função de Submit
    const onSubmit: SubmitHandler<PlanFormSchema> = async (data) => {

        // Monta o objeto aninhado que o Backend/Mercado Pago espera
        const payloadCommon = {
            reason: data.reason,
            description: data.description,
            auto_recurring: {
                frequency: Number(data.frequency),
                frequency_type: data.frequencyType,
                transaction_amount: Number(data.transactionAmount),
                currency_id: 'BRL' // Fixo BRL conforme contexto
            },
            included_course_ids: []
        };

        let success = false;

        if (isEditing && initialData) {
            // Lógica de Update
            const updatePayload: UpdatePlanRequest = { ...payloadCommon };
            success = await updatePlan(initialData.publicId, updatePayload);
        } else {
            // Lógica de Create
            const createPayload: CreatePlanRequest = { ...payloadCommon };
            success = await createPlan(createPayload);
        }

        if (success) {
            onSuccess();
        }
    };

    return (
        <div className={styles['plan-form-container']}>
            <div className={styles['plan-form-header']}>
                <h3>{isEditing ? `Editar Plano` : 'Novo Plano de Assinatura'}</h3>
                <p>Preencha as informações para configurar a cobrança no Mercado Pago.</p>
            </div>

            <div className={styles['generic-form']}>
                <Form<PlanFormSchema>
                    onSubmit={onSubmit}
                    defaultValues={defaultValues}
                >
                    <Form.Input 
                      name="reason" 
                      label="Nome do Plano" 
                      placeholder="Ex: Plano Gold Mensal" 
                      validation={{ required: 'O nome do plano é obrigatório.' }} 
                      colSpan={12} 
                    />
                    
                    <Form.Input 
                      name="transactionAmount" 
                      label="Valor (R$)" 
                      type="number" 
                      placeholder="0.00" 
                      validation={{
                          required: 'Informe o valor.',
                          min: { value: 1, message: 'O valor mínimo é 1.' }
                      }} 
                      colSpan={6} 
                    />
                    
                    <Form.Input 
                      name="frequency" 
                      label="Frequência" 
                      type="number" 
                      placeholder="Ex: 1" 
                      validation={{ required: 'Obrigatório', min: 1 }} 
                      colSpan={3} 
                    />

                    <Form.Select 
                      name="frequencyType" 
                      label="Tipo" 
                      options={[
                          { label: 'Meses', value: 'months' },
                          { label: 'Dias', value: 'days' }
                      ]} 
                      validation={{ required: 'Selecione' }} 
                      colSpan={3} 
                    />

                    <Form.Textarea 
                      name="description" 
                      label="Descrição / Benefícios" 
                      placeholder="Descreva os detalhes que aparecerão para o cliente..." 
                      validation={{ required: 'A descrição é obrigatória.' }} 
                      colSpan={12} 
                    />

                    <Form.Actions>
                        {/* Botão de Cancelar Injetado como Children (opcional) */}
                        {onCancel && (
                            <button
                                type="button"
                                className={styles['cancel-btn']}
                                onClick={onCancel}
                                disabled={loading}
                            >
                                Cancelar
                            </button>
                        )}
                        <Form.Submit isLoading={loading}>
                            {isEditing ? 'Salvar Alterações' : 'Criar Plano'}
                        </Form.Submit>
                    </Form.Actions>
                </Form>
            </div>
        </div>
    );
};

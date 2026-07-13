import { Form } from '@/components/Form';
import styles from '../styles/ChargebackFilter.module.scss';
import type { FilterFormData } from '../types/chargeback.dtos';

interface Props {
  onFilter: (data: FilterFormData) => void;
  isLoading: boolean;
}

export const ChargebackFilter = ({ onFilter, isLoading }: Props) => {
  
  return (
    <div className={styles['chargeback-filter']}>
      <Form<FilterFormData>
        onSubmit={onFilter}
      >
        <Form.Input 
          name="searchTerm" 
          label="Buscar" 
          placeholder="ID, Cliente ou Valor..." 
          colSpan={6} 
        />
        <Form.Select 
          name="status" 
          label="Status" 
          options={[
            { label: 'Todos', value: '' },
            { label: 'Novo', value: 'Novo' },
            { label: 'Aguardando Evidências', value: 'AguardandoEvidencias' },
            { label: 'Em Análise', value: 'EmAnalise' },
            { label: 'Finalizado', value: 'Finalizado' }
          ]} 
          colSpan={4} 
        />
        <Form.Actions>
          <Form.Submit isLoading={isLoading}>
            Filtrar
          </Form.Submit>
        </Form.Actions>
      </Form>
    </div>
  );
};

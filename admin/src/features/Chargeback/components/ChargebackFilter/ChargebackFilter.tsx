import { Form } from '@/components/Form';
import styles from './ChargebackFilter.module.scss';
import type { FilterFormData } from '../../types/chargeback.dtos';

interface Props {
  onFilter: (data: FilterFormData) => void;
  isLoading: boolean;
}

export const ChargebackFilter = ({ onFilter, isLoading }: Props) => {
  
  return (
    <div className={styles.chargebackFilter}>
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
            { label: 'Evidências Enviadas', value: 'EvidenciasEnviadas' },
            { label: 'Ganhamos', value: 'Ganhamos' },
            { label: 'Perdemos', value: 'Perdemos' }
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

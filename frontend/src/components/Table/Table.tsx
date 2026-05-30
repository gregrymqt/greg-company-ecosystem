import { type ReactNode } from 'react';
import styles from './Table.module.scss';

// Definição de uma Coluna
export interface TableColumn<T> {
  header: string; // Título da coluna (ex: "Nome", "Preço")
  accessor?: keyof T; // A chave direta no objeto (ex: "name", "price")
  
  // Render customizado (opcional). Útil para formatar datas, moedas ou botões.
  render?: (item: T, index: number) => ReactNode; 
  
  // Largura opcional (ex: "100px" ou "20%")
  width?: string;
  
  // Alinhamento do conteúdo
  align?: 'left' | 'center' | 'right';
  
  // Classe CSS customizada para a coluna
  className?: string;
}

interface TableProps<T> {
  data: T[];
  columns: TableColumn<T>[];
  keyExtractor: (item: T) => string | number; // Identificador único da linha
  
  isLoading?: boolean; // Para mostrar loading state
  emptyMessage?: string; // Mensagem se não tiver dados
  
  // Callbacks opcionais
  onRowClick?: (item: T, index: number) => void; // Clique na linha
  rowClassName?: (item: T, index: number) => string; // Classe CSS customizada por linha
  
  // Estilo customizado
  className?: string;
}

export const Table = <T,>({ 
  data, 
  columns, 
  keyExtractor,
  isLoading = false,
  emptyMessage = "Nenhum dado encontrado.",
  onRowClick,
  rowClassName,
  className = ''
}: TableProps<T>) => {

  if (isLoading) {
    return <div className={styles.loading}>Carregando dados...</div>;
  }

  if (!data || data.length === 0) {
    return <div className={styles.empty}>{emptyMessage}</div>;
  }

  return (
    <div className={`${styles.tableContainer} ${className}`}>
      <table className={styles.table}>
        <thead>
          <tr>
            {columns.map((col, index) => (
              <th 
                key={index} 
                style={{ 
                  width: col.width,
                  textAlign: col.align || 'left'
                }}
                className={col.className}
              >
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((item, itemIndex) => (
            <tr 
              key={keyExtractor(item)}
              onClick={onRowClick ? () => onRowClick(item, itemIndex) : undefined}
              className={rowClassName ? rowClassName(item, itemIndex) : ''}
              style={onRowClick ? { cursor: 'pointer' } : undefined}
            >
              {columns.map((col, colIndex) => (
                <td 
                  key={colIndex} 
                  data-label={col.header}
                  style={{ textAlign: col.align || 'left' }}
                  className={col.className}
                >
                  {/* Lógica: Se tiver render customizado, usa. Se não, usa o accessor direto. */}
                  {col.render 
                    ? col.render(item, itemIndex) 
                    : (col.accessor ? String(item[col.accessor]) : '-')}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
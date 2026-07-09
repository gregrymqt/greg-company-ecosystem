import { Form } from '../../../../components/Form/Form';
import styles from './UrlInputForm.module.scss';

interface UrlInputFormProps {
  /** Callback disparado com o array de URLs válidas filtradas */
  onSubmit: (urls: string[]) => void;
  /** Estado de processamento vindo do useFreeSample para desabilitar os inputs */
  isLoading: boolean;
}

/**
 * DTO interno para gerenciar os inputs mapeados individualmente 
 * exigidos pelo padrão do React Hook Form do seu projeto.
 */
interface FormInputs {
  url1: string;
  url2: string;
  url3: string;
}

export function UrlInputForm({ onSubmit, isLoading }: UrlInputFormProps) {
  
  // Regra Regex padronizada para validação client-side de URLs de e-commerce
  const urlValidationRule = {
    pattern: {
      value: /^(https?:\/\/)?([\da-z.-]+)\.([a-z.]{2,6})([\/\w .-]*)*\/?$/,
      message: "Insira um endereço de link válido (ex: https://fornecedor.com/produto)",
    },
  };

  /**
   * Captura o envio estruturado, remove inputs vazios ou espelhados 
   * e despacha o lote sanitizado para o pipeline do worker.
   */
  const handleFormSubmit = (data: FormInputs) => {
    const rawUrls = [data.url1, data.url2, data.url3];
    const cleanUrls = rawUrls.filter((url) => url && url.trim() !== "");
    
    onSubmit(cleanUrls);
  };

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <h2>Copywriting Agressivo & SEO Gerados por IA</h2>
        <p>
          Cole até 3 links de produtos de seus fornecedores abaixo. Nosso bot fará a varredura da página, 
          limpará o HTML e usará IA para criar ofertas de alta conversão instantaneamente.
        </p>
      </header>

      <Form<FormInputs>
        onSubmit={handleFormSubmit}
        defaultValues={{ url1: "", url2: "", url3: "" }}
        className={styles.glassForm}
      >
        <Form.Input<FormInputs>
          name="url1"
          label="Link do Produto Principal"
          placeholder="https://fornecedor-exemplo.com.br/produto-vencedor"
          disabled={isLoading}
          colSpan={12}
          validation={{
            required: "A primeira URL é obrigatória para iniciar a simulação gratuita.",
            ...urlValidationRule,
          }}
        />

        <Form.Input<FormInputs>
          name="url2"
          label="Link do Produto 2 (Opcional)"
          placeholder="https://fornecedor-exemplo.com.br/segundo-produto"
          disabled={isLoading}
          colSpan={12}
          validation={urlValidationRule}
        />

        <Form.Input<FormInputs>
          name="url3"
          label="Link do Produto 3 (Opcional)"
          placeholder="https://fornecedor-exemplo.com.br/terceiro-produto"
          disabled={isLoading}
          colSpan={12}
          validation={urlValidationRule}
        />

        <Form.Actions>
          <Form.Submit isLoading={isLoading} loadingText="Otimizando Estruturas...">
            <i className="fas fa-bolt" style={{ marginRight: "8px" }}></i>
            Turbinar Anúncios e Páginas de Vendas
          </Form.Submit>
        </Form.Actions>
      </Form>
    </div>
  );
}
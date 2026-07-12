import { Card } from '../../../../components/Card/Card';
import styles from './ConversionCTA.module.scss';
import { useNavigate } from 'react-router-dom';

interface ConversionCTAProps {
  /** Indica se o componente aparece por sucesso na demo ou por bloqueio de limite */
  reason: 'success' | 'limit_reached';
  /** Callback para levar ao checkout ou contato comercial */
  onUpgrade: () => void;
  onCancel?: () => void;
}

export function ConversionCTA({ reason, onUpgrade, onCancel }: ConversionCTAProps) {
  const isLimit = reason === 'limit_reached';
  const navigate = useNavigate();

  return (
    <section className={styles.ctaWrapper}>
      <Card className={`${styles.ctaCard} ${isLimit ? styles.limitAlert : styles.successPulse}`}>
        <Card.Body 
          title={isLimit ? "Limite Gratuito Atingido!" : "Gostou do Resultado?"}
        >
          <div className={styles.content}>
            <p className={styles.description}>
              {isLimit 
                ? "Você atingiu o limite de 3 URLs por hora. Para processar seu estoque inteiro com SEO agressivo e copywriting de alta conversão, mude para o plano Premium."
                : "Este é apenas o começo. Imagine ter cada produto da sua loja otimizado com essa mesma qualidade, gerando tráfego orgânico e reduzindo seu CAC drasticamente."}
            </p>

            <ul className={styles.benefitsList}>
              <li><i className="fas fa-check" /> Processamento em Lote (Sem limites)</li>
              <li><i className="fas fa-check" /> Exportação Direta para Shopify / Nuvemshop</li>
              <li><i className="fas fa-check" /> Suporte a múltiplos Tenants e BYOK (Sua chave OpenAI)</li>
            </ul>
          </div>
        </Card.Body>

        <Card.Actions>
          <button className={styles.secondaryBtn} onClick={onCancel ? onCancel : () => navigate('/')}>
            Voltar ao Início
          </button>
          <button className={styles.primaryBtn} onClick={onUpgrade}>
            {isLimit ? 'Liberar Acesso Ilimitado' : 'Turbinar Minha Loja Agora'}
            <i className="fas fa-arrow-right" style={{ marginLeft: '10px' }} />
          </button>
        </Card.Actions>
      </Card>
    </section>
  );
}
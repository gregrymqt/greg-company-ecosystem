// src/pages/Payment/PaymentLayout.tsx
import React, { useState, useMemo } from "react";
import { useParams } from "react-router-dom";
import styles from "./PaymentLayout.module.scss";

import type { SidebarItem } from "@/components/SideBar/types/sidebar.types";
import { CreditCardPayment } from '../components/CreditCard/CreditCardPayment';
import { PixPayment } from '../components/Pix/PixPayment';
import type { PaymentMethodId } from '../types';
import { Sidebar } from "@/components/SideBar/Sidebar";
import { usePaymentCheckout } from '../hooks/usePaymentCheckout';
import { useAuth } from "@/features/auth/hooks/useAuth";

export const PaymentLayout: React.FC = () => {
  const { planId } = useParams<{ planId: string }>();
  const { user } = useAuth();

  const [activeTab, setActiveTab] = useState<PaymentMethodId>("credit-card");
  const [paymentSuccess, setPaymentSuccess] = useState(false);

  // Hook que centraliza a busca de dados do plano e preferenceId do Mercado Pago
  const {
    planData,
    preferenceId,
    loading,
    error,
  } = usePaymentCheckout(planId || "");

  // --- LÓGICA DE SIDEBAR ---
  const sidebarItems: SidebarItem[] = useMemo(() => {
    if (!planData) return [];

    const items: SidebarItem[] = [];
    const isMensal = planData.frequency === 1;

    if (isMensal) {
      items.push({ id: "pix", label: "PIX", icon: "fas fa-qrcode" });
    }
    items.push({
      id: "credit-card",
      label: "Cartão de Crédito",
      icon: "fas fa-credit-card",
    });

    return items;
  }, [planData?.frequency]);

  const currentTabExists = sidebarItems.find((i) => i.id === activeTab);
  if (!currentTabExists && sidebarItems.length > 0) {
    setActiveTab(sidebarItems[0].id as PaymentMethodId);
  }

  const handleSuccess = () => {
    setPaymentSuccess(true);
  };

  // --- LOADING / SKELETON UI ---
  // UI/UX: Tratamento de Loading da Preferência / Plano (Evita cintilações no F5)
  if (loading) {
    return (
      <div className={styles.layoutContainer}>
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', width: '100%', minHeight: '100vh' }}>
          <div className="spinner-border text-primary" role="status" style={{ width: '3rem', height: '3rem' }}></div>
          <p style={{ marginTop: '1rem', color: '#555', fontSize: '1.2rem' }}>Preparando seu ambiente seguro de pagamento...</p>
        </div>
      </div>
    );
  }

  // --- ERRO ---
  if (error || !planData) {
    return (
      <div className={styles.layoutContainer}>
        <div className="alert alert-warning" style={{ margin: '3rem auto', maxWidth: '500px', textAlign: 'center' }}>
          <i className="fas fa-exclamation-triangle" style={{ fontSize: '2rem', marginBottom: '1rem' }}></i>
          <p>{error || "Não foi possível carregar os dados do plano selecionado."}</p>
          <button
            className="btn btn-outline-primary"
            onClick={() => window.location.reload()}
          >
            Tentar novamente
          </button>
        </div>
      </div>
    );
  }

  const userParams = user ? { name: user.name, email: user.email } : undefined;

  // --- RENDERIZAÇÃO ---
  const renderContent = () => {
    if (paymentSuccess) {
      return (
        <div className={styles.successWrapper}>
          <i className="fas fa-check-circle"></i>
          <h2>Tudo certo!</h2>
          <p>Seu plano {planData.name} foi ativado com sucesso.</p>
        </div>
      );
    }

    switch (activeTab) {
      case "pix":
        return (
          <PixPayment
            amount={planData.amount}
            planId={planData.publicId}
            planName={planData.name || ''}
            userParams={userParams}
            onPaymentSuccess={handleSuccess}
          />
        );

      case "credit-card":
        // UI/UX: Tratamento de Erro ao buscar ID do MP
        if (!preferenceId) {
          return (
            <div className="alert alert-warning">
              <i className="fas fa-exclamation-triangle"></i>
              <p>Ambiente seguro do Mercado Pago indisponível no momento.</p>
              <button
                className="btn btn-outline-primary btn-sm"
                onClick={() => window.location.reload()}
              >
                Tentar novamente
              </button>
            </div>
          );
        }

        return (
          <CreditCardPayment
            planId={planData.publicId}
            planName={planData.name || ''}
            frequency={planData.frequency}
            amount={planData.amount}
            mode={planData.frequency === 1 ? "payment" : "subscription"}
            preferenceId={preferenceId}
            onPaymentSuccess={handleSuccess}
          />
        );

      default:
        return <div>Selecione um método de pagamento.</div>;
    }
  };

  return (
    <div className={styles.layoutContainer}>
      <Sidebar
        items={sidebarItems}
        activeItemId={activeTab}
        onItemClick={(item) => setActiveTab(item.id as PaymentMethodId)}
        logo={<h3>Greg Pay</h3>}
      >
        <div style={{ padding: "20px", color: "#fff", cursor: "pointer" }} onClick={() => window.history.back()}>
          <i className="fas fa-arrow-left"></i> Voltar
        </div>
      </Sidebar>

      <main className={styles.mainContent}>
        <div className={styles.orderSummary}>
          <div className={styles.planInfo}>
            <span className={styles.label}>Plano selecionado:</span>
            <span className={styles.value}>{planData.name}</span>
          </div>
          <div className={styles.priceInfo}>
            <span className={styles.label}>Total a pagar:</span>
            <span className={styles.value}>
              {new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(planData.amount)}
            </span>
          </div>
        </div>

        <div className={styles.contentCard}>
          {!paymentSuccess && (
            <h2>
              {activeTab === "pix" ? "Pagamento à Vista (PIX)" : "Dados do Cartão de Crédito"}
            </h2>
          )}
          {renderContent()}
        </div>
      </main>
    </div>
  );
};

import { useEffect, useState, useCallback } from "react";
import { socketService } from "@/shared/services/socket.service";
import { AppHubsBIFastAPI } from "@/shared/enums/hub.enums";
import { financialService } from "../services/financial.service";
import { AlertService } from "@/shared/services/alert.service";
import type { PaymentSummary, RevenueMetrics } from "../types/financial.types";
import { ApiError } from "@/shared/services/api.service";

export const useFinancialAnalytics = () => {
  const [paymentSummary, setPaymentSummary] = useState<PaymentSummary | null>(
    null,
  );
  const [revenue, setRevenue] = useState<RevenueMetrics | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const loadInitialData = useCallback(async () => {
    try {
      setIsLoading(true);
      const [pData, rData] = await Promise.all([
        financialService.getPaymentSummary(),
        financialService.getRevenueMetrics(),
      ]);
      setPaymentSummary(pData);
      setRevenue(rData);
    } catch (error) {
      if (error instanceof ApiError) {
        console.error("❌ Erro ao carregar dados financeiros:", error);
        AlertService.error("Erro Financeiro", error.message);
      }
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const hub = AppHubsBIFastAPI.BI_FINANCIAL;

    socketService.connect(hub).then(() => {
      // O seu backend exige Invoke para ativar o fluxo de dados
      socketService.invoke(hub, "SubscribeToRevenueUpdates", {});
      socketService.invoke(hub, "SubscribeToNewPayments", {});
    });

    // Listener para o broadcast global de receita
    socketService.on<{ revenue: RevenueMetrics }>(
      hub,
      "RevenueUpdate",
      (payload) => {
        setRevenue(payload.revenue);
        AlertService.notify(
          "Receita Atualizada",
          "Novos dados de faturamento recebidos.",
          "success",
        );
      },
    );

    // Listener para dados iniciais via socket (opcional ao REST)
    socketService.on<{ paymentSummary: PaymentSummary }>(
      hub,
      "InitialData",
      (payload) => {
        setPaymentSummary(payload.paymentSummary);
      },
    );

    loadInitialData();

    return () => {
      socketService.off(hub, "RevenueUpdate");
      socketService.off(hub, "InitialData");
    };
  }, [loadInitialData]);

  const handleManualSync = async () => {
    try {
      const res = await financialService.syncToRows();
      AlertService.success("Sincronização", res.message);
    } catch (error) {
      if (error instanceof ApiError) {
        console.error("❌ Erro ao solicitar atualização manual:", error);
        AlertService.error("Erro", error.message);
      }
    }
  };

  return {
    paymentSummary,
    revenue,
    isLoading,
    handleManualSync,
    refresh: loadInitialData,
  };
};

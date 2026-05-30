// src/features/Payment/Public/hooks/usePixPayment.ts
import { useState, useEffect } from "react";
import { PixService } from "../services/pix.service";
import { AlertService } from "@/shared/services/alert.service";
import { socketService } from "@/shared/services/socket.service";
import { AppHubs } from "@/shared/enums/hub.enums";
import { useSocketListener } from "@/shared/hooks/useSocket";
import { ApiError } from "@/shared/services/api.service";

import type {
  CreatePixDTO,
  PixResponse,
  PixStep,
  PixDocType,
  PaymentSocketMessage
} from "../../shared";

interface UsePixPaymentProps {
  planId: string;
  amount: number;
  onSuccess: () => void;
}

export const usePixPayment = ({
  planId,
  amount,
  onSuccess,
}: UsePixPaymentProps) => {
  const [step, setStep] = useState<PixStep>("FORM");
  const [loading, setLoading] = useState(false);
  const [pixData, setPixData] = useState<PixResponse | null>(null);
  const [docTypes, setDocTypes] = useState<PixDocType[]>([]);
  const [error, setError] = useState<string | null>(null);

  // 1. Conexão WebSocket (SignalR)
  useEffect(() => {
    socketService.connect(AppHubs.Payment);
    return () => {
      socketService.disconnect(AppHubs.Payment);
    };
  }, []);

  // 2. Carrega os tipos de documento (CPF, CNPJ, etc.)
  useEffect(() => {
    const loadDocTypes = async () => {
      try {
        const types = await PixService.getDocTypes();
        setDocTypes(types);
      } catch (err) {
        console.error("Erro ao carregar tipos de doc:", err);
      }
    };
    loadDocTypes();
  }, []);

  // 3. Ouvinte de Mensagens do Backend (Processing, Approved, Failed)
  useSocketListener<PaymentSocketMessage>(
    AppHubs.Payment,
    "UpdatePaymentStatus",
    (data) => {
      console.log("🔔 PIX Socket Update:", data);

      if (data.status === "processing") {
        AlertService.notify("Processando", data.message, "info");
      }

      if (data.status === "approved") {
        AlertService.notify("Pagamento Aprovado!", data.message, "success");
        setStep("SUCCESS");
        onSuccess();
      }

      if (data.status === "failed" || data.status === "error") {
        AlertService.error("Erro no Pagamento", data.message);
        setStep("ERROR");
        setError(data.message);
      }
    }
  );

  // 4. Função para criar PIX
  const handleCreatePix = async (formData: {
    firstName: string;
    lastName: string;
    email: string;
    identificationType: string;
    identificationNumber: string;
  }) => {
    setLoading(true);
    setError(null);

    try {
      const payload: CreatePixDTO = {
        transactionAmount: amount,
        description: `Plano - ${planId}`,
        paymentMethodId: "pix",
        planExternalId: planId,
        payer: {
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          identification: {
            type: formData.identificationType,
            number: formData.identificationNumber,
          },
        },
      };

      const response = await PixService.createPix(payload);

      setPixData(response);
      setStep("QR_CODE");

      AlertService.notify("PIX Gerado", "Aguardando pagamento...", "info");
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
        AlertService.error("Erro ao criar PIX", err.message);
        return;
      }
    } finally {
      setLoading(false);
    }
  };

  const copyPixCode = async () => {
    if (pixData?.qrCode) {
      await navigator.clipboard.writeText(pixData.qrCode);
      AlertService.notify(
        "Copiado!",
        "Código PIX copiado para a área de transferência.",
        "success"
      );
      return true;
    }
    return false;
  };

  return {
    step,
    loading,
    pixData,
    docTypes,
    error,
    handleCreatePix,
    copyPixCode,
    setStep,
  };
};

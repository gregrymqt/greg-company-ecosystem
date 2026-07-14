import React, { useRef, useEffect } from "react";
import styles from '../styles/ClaimChat.module.scss';
import { Form } from "@/components/Form";
import { useClaimChatLogic } from '../../hooks/useClaimChatLogic';

interface Props {
  claimId: string;
}

import type { ReplyFormData } from "../../types/claim.dtos";

export const ClaimChat: React.FC<Props> = ({ claimId }) => {
  const { messages, handleSendResponse, isSending } = useClaimChatLogic({
    claimId,
  });
  const bottomRef = useRef<HTMLDivElement>(null);

  // Auto-scroll para baixo quando chega mensagem nova
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);



  return (
    <div className={styles.container}>
      {/* 1. ÁREA DE MENSAGENS (Timeline) */}
      <div className={styles.timeline}>
        {messages.length === 0 && (
          <p className={styles.empty}>Nenhuma mensagem trocada ainda.</p>
        )}

        {messages.map((msg) => (
          <div
            key={msg.messageId}
            className={`${styles.bubbleWrapper} ${msg.isMe ? styles.me : styles.them
              }`}
          >
            <div className={styles.bubble}>
              <div className={styles.header}>
                <span className={styles.role}>
                  {msg.senderRole === "mediator"
                    ? "Mediação MP"
                    : msg.isMe
                      ? "Você"
                      : "Outro lado"}
                </span>
                <span className={styles.date}>
                  {new Date(msg.dateCreated).toLocaleDateString()}
                </span>
              </div>

              <p className={styles.content}>{msg.content}</p>

              {/* Renderização de Anexos recebidos */}
              {msg.attachments && msg.attachments.length > 0 && (
                <div className={styles.attachmentsList}>
                  {msg.attachments.map((att, idx) => (
                    <a
                      key={idx}
                      href={att}
                      target="_blank"
                      rel="noopener noreferrer"
                    >
                      📎 Anexo {idx + 1}
                    </a>
                  ))}
                </div>
              )}
            </div>
          </div>
        ))}
        <div ref={bottomRef} />
      </div>

      {/* 2. ÁREA DE INPUT (Usando seu GenericForm) */}
      <div className={styles.footer}>
        <Form<ReplyFormData>
          onSubmit={handleSendResponse}
        >
          <Form.Textarea name="message" label="Sua Resposta" placeholder="Descreva sua resposta ou solução..." validation={{ required: "A mensagem é obrigatória" }} colSpan={12} />
          <Form.Input name="attachments" label="Anexar Comprovantes" type="file" accept="image/*, .pdf" multiple colSpan={12} />

          <Form.Actions>
            <Form.Submit isLoading={isSending}>
              {isSending ? "Enviando..." : "Enviar Resposta"}
            </Form.Submit>
          </Form.Actions>
        </Form>
      </div>
    </div>
  );
};

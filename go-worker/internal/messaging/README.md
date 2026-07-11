# 📁 internal/messaging/

**Objetivo:** Gerenciar as conexões, roteamento e consumo das mensagens do RabbitMQ (AMQP).

**O que colocar aqui:**
- `email_consumer.go`: A classe ou rotina que consome a fila de envio de e-mails (`email.send.queue`).
- `video_consumer.go`: Consumidor para tarefas de transcodificação de vídeo.
- Lógicas de *Ack*, *Nack* (com re-enfileiramento) e reconexão em caso de queda do broker.

**Regras:**
- Esta camada não deve possuir regras estritas de negócios. Ao receber a mensagem e deserializá-la, ela deve chamar as funções da pasta `processor/`.

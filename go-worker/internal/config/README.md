# 📁 internal/config/

**Objetivo:** Centralizar o parsing e validação de variáveis de ambiente.

**O que colocar aqui:**
- Estruturas (structs) que representam a configuração do App.
- Funções para ler do `.env` ou do ambiente do Sistema Operacional (ex: pacote `viper` ou nativo).

**Regras:**
- O sistema inteiro deve depender destas *structs* parseadas, nunca chamar `os.Getenv("VAR")` aleatoriamente no meio de lógicas de negócio.

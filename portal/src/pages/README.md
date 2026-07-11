# 📁 pages/

**Objetivo:** Arquivos que representam telas inteiras (Rotas) da aplicação.

Neste diretório, cada arquivo ou subdiretório corresponde a uma URL ou visão completa que o usuário acessa no navegador.

**Exemplos do que colocar aqui:**
- `Home.tsx` (para a rota `/`)
- `Login.tsx` (para a rota `/login`)
- `CourseDetails.tsx` (para a rota `/course/:id`)

**Regras:**
- Os arquivos daqui devem ser o "ponto de amarração" (glue code). Eles importam os componentes globais (da pasta `components/`) e os blocos de negócios específicos (da pasta `features/`) para compor a tela.
- **Não** coloque regras de negócio pesadas ou lógicas complexas diretamente nas páginas. Delegue isso aos *hooks* e componentes das *features*.
- Toda página deve ser exportada e consumida exclusivamente pelo arquivo de configuração de rotas (`routes/`).

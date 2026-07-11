# 📁 pages/

**Objetivo:** Arquivos que representam telas inteiras (Dashboards e Telas de Gestão).

**Exemplos do que colocar aqui:**
- `Dashboard.tsx` (para a visão geral de KPIs)
- `UserManagement.tsx` (para listar usuários)
- `Settings.tsx` (configurações do painel)

**Regras:**
- Os arquivos daqui atuam como *Controllers* da UI. Eles agregam componentes globais (ex: Topbar) com as funcionalidades injetadas da pasta `features/`.
- Evite colocar chamadas HTTP (`axios`) aqui. Delegue-as aos hooks expostos pelas suas *features*.

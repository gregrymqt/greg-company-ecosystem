# Analytics Feature - Documentação

## 📁 Estrutura Criada

```
src/features/analytics/
├── components/
│   └── AnalyticsCarousel/
│       ├── AnalyticsCarousel.tsx    # Componente principal do carrossel
│       ├── ProductCard.tsx          # Card individual de produto
│       └── index.ts                 # Barrel export
├── hooks/
│   ├── useAnalytics.ts              # Hook principal (estado + fetch)
│   └── useAnalyticsCarousel.ts      # Hook de lógica do carrossel
├── services/
│   └── analytics.service.ts         # Comunicação com FastAPI (porta 8888)
├── styles/
│   ├── AnalyticsCarousel.module.scss
│   ├── AnalyticsPage.module.scss
│   └── ProductCard.module.scss
└── types/
    └── analytics.types.ts           # Interfaces TypeScript

src/pages/Analytics/
└── AnalyticsPage.tsx                # Página principal
```

## ✅ Conformidade com o Padrão do Projeto

### 1. **Estrutura de Diretórios**
- ✅ Segue rigorosamente o padrão de `src/features/`
- ✅ Organização: `components/`, `hooks/`, `services/`, `styles/`, `types/`
- ✅ Página separada em `src/pages/Analytics/`

### 2. **Integração com Base Services**
- ✅ Utiliza `ApiService` pattern para requisições
- ✅ Implementa `reportToMcp` em todas as chamadas de API
- ✅ Tratamento de erros com `AlertService`

### 3. **UI/UX e Estilização**
- ✅ **Mobile-First**: Todos os estilos começam mobile e expandem para desktop
- ✅ **Variáveis SCSS**: Importa `@use '@/styles/variables' as *`
- ✅ **Componentes Genéricos**: Reutiliza `Card` e `Carousel` de `src/components/`
- ✅ **Swiper**: Configurado com breakpoints (1 → 2 → 3 cards)

### 4. **TypeScript Estrito**
- ✅ Todas as interfaces definidas em `analytics.types.ts`
- ✅ Tipagem completa em hooks, services e componentes
- ✅ Uso de generics nos componentes reutilizáveis

### 5. **Registro de Rota**
- ✅ Rota `/admin/analytics` registrada em `AppRoutes.tsx`
- ✅ Protegida com `ProtectedRoute` (apenas Admin)

## 🔌 Integração com FastAPI

### Endpoints Esperados (porta 8888)

```typescript
GET  /api/analytics/dashboard    # Retorna dashboard + produtos
GET  /api/analytics/products     # Produtos filtrados (query params)
POST /api/analytics/sync         # Sincroniza dados com fonte externa
GET  /api/analytics/export/excel # Download de arquivo Excel
```

### Monitoramento MCP
Todas as requisições reportam para `http://localhost:8888/log`:

```json
{
  "source": "Analytics",
  "url": "http://localhost:8888/api/analytics/dashboard",
  "method": "GET",
  "status": 200
}
```

## 🎨 Recursos Visuais

### Dashboard Cards
- **Total de Produtos**
- **Produtos Críticos** (estoque baixo/esgotado)
- **Estoque Médio**
- **Receita Total**
- **Produtos Esgotados**
- **Última Sincronização**

### Actions
- 🔄 **Sincronizar Dados** (com FastAPI)
- 📥 **Exportar Excel** (download direto)
- 🔍 **Filtros** (status, categoria, estoque min/max)

### Carrossel
- **Mobile**: 1 card por vez
- **Tablet**: 2 cards
- **Desktop**: 3 cards
- **Autoplay**: 4 segundos
- **Navegação**: Setinhas + bolinhas

## 🚀 Como Usar

### 1. Acessar o Dashboard
```
http://localhost:5173/admin/analytics
```
(Requer autenticação como Admin)

### 2. Iniciar o FastAPI
O serviço Analytics espera um servidor FastAPI rodando em:
```bash
# Exemplo de comando (ajuste conforme seu setup)
cd bi-dashboard
python src/main.py
# ou uvicorn se tiver API REST
uvicorn main:app --port 8888
```

### 3. Desenvolvimento
```bash
cd system-app/frontend
npm run dev
```

## 📦 Dependências Utilizadas

- `swiper` / `swiper/react` - Carrossel
- `lucide-react` - Ícones (RefreshCw, Download, Filter)
- React Hook Form pattern (via hooks customizados)
- SweetAlert2 (via AlertService)

## 🎯 Próximos Passos (Opcional)

1. **Modal de Detalhes**: Ao clicar em um produto, abrir modal com mais informações
2. **Gráficos**: Integrar Chart.js ou Recharts para visualizações
3. **WebSocket**: Atualização em tempo real via SignalR (já disponível no projeto)
4. **Notificações**: Alertas automáticos quando estoque crítico

## 🧪 Testing

Teste a feature verificando:
- [ ] Dashboard carrega corretamente
- [ ] Filtros aplicam e limpam valores
- [ ] Sincronização chama o endpoint correto
- [ ] Exportação baixa arquivo Excel
- [ ] Carrossel funciona em mobile/tablet/desktop
- [ ] Responsividade mobile-first
- [ ] MCP recebe logs das requisições

---

**Feature Analytics implementada com sucesso!** 🎉

from fastapi import APIRouter, HTTPException, BackgroundTasks, Depends, status
from src.features.financial.service import create_financial_service, FinancialService

router = APIRouter()

# ==============================================================================
# NOVO ENDPOINT: SYNC ROWS
# ==============================================================================

@router.post("/sync-rows", status_code=status.HTTP_202_ACCEPTED)
async def sync_financial_rows(
    background_tasks: BackgroundTasks,
    service: FinancialService = Depends(create_financial_service)
):
    """
    Atualiza TODOS os dashboards financeiros (Pagamentos, Receita, Chargeback)
    no Rows.com em background.
    """
    try:
        background_tasks.add_task(service.sync_financial_to_rows)
        return {
            "message": "Sincronização Financeira (Full) iniciada.",
            "targets": ["Payments", "Revenue", "Chargebacks"]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

# ==============================================================================
# ENDPOINTS EXISTENTES (Refatorados para usar Depends)
# ==============================================================================

@router.get("/summary")
async def get_payment_summary(
    service: FinancialService = Depends(create_financial_service)
):
    """Retorna resumo de pagamentos"""
    try:
        summary = service.get_payment_summary()
        # Retornamos o DTO direto ou convertemos para dict se preferir
        return summary
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/revenue")
async def get_revenue_metrics(
    service: FinancialService = Depends(create_financial_service)
):
    """Retorna métricas de receita"""
    try:
        return service.get_revenue_metrics()
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
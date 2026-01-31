"""
Financial REST Routes
"""

from fastapi import APIRouter, HTTPException
from ...features.financial import create_financial_service

router = APIRouter()


@router.get("/summary")
async def get_payment_summary():
    """Retorna resumo de pagamentos"""
    try:
        service = create_financial_service()
        summary = service.get_payment_summary()
        
        return {
            "totalPayments": summary.TotalPayments,
            "totalApproved": float(summary.TotalApproved),
            "totalPending": float(summary.TotalPending),
            "totalCancelled": float(summary.TotalCancelled),
            "uniqueCustomers": summary.UniqueCustomers,
            "avgTicket": float(summary.AvgTicket),
            "approvalRate": summary.ApprovalRate
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.get("/revenue")
async def get_revenue_metrics():
    """Retorna métricas de receita"""
    try:
        service = create_financial_service()
        metrics = service.get_revenue_metrics()
        
        return {
            "totalRevenue": float(metrics.TotalRevenue),
            "monthlyRevenue": float(metrics.MonthlyRevenue),
            "yearlyRevenue": float(metrics.YearlyRevenue),
            "totalTransactions": metrics.TotalTransactions,
            "avgTransactionValue": float(metrics.AverageTransactionValue),
            "topPaymentMethod": metrics.TopPaymentMethod,
            "paymentMethodDistribution": metrics.PaymentMethodDistribution
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

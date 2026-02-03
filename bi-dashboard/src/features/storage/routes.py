"""
Storage Routes
Endpoints REST para analytics de armazenamento
🔒 Protegido com autenticação JWT
"""

import logging
from typing import Optional, List, Dict, Any

from fastapi import APIRouter, Query, Depends, BackgroundTasks, HTTPException, status

# Ajuste os imports conforme a estrutura exata da sua pasta src/
from src.features.storage.service import create_storage_service, StorageService
from src.features.storage.schemas import StorageStatsDTO, FileDetailDTO
from src.core.security import get_current_user

logger = logging.getLogger(__name__)

# Router com autenticação obrigatória para todos os endpoints
router = APIRouter()

# ==============================================================================
# NOVO ENDPOINT: SINCRONIZAÇÃO COM ROWS
# ==============================================================================

@router.post("/sync-rows", status_code=status.HTTP_202_ACCEPTED)
async def sync_storage_rows(
    background_tasks: BackgroundTasks,
    service: StorageService = Depends(create_storage_service)
):
    """
    Dispara a sincronização dos dados de Storage (KPIs e Maiores Arquivos)
    para o Rows.com em background.
    """
    try:
        # Adiciona a tarefa na fila para não travar a resposta da API
        background_tasks.add_task(service.sync_storage_to_rows)
        
        return {
            "message": "Sincronização de Storage iniciada em background.",
            "targets": ["Overview", "Largest Files"]
        }
    except Exception as e:
        logger.error(f"Erro ao iniciar sync de storage: {str(e)}")
        raise HTTPException(status_code=500, detail="Falha ao iniciar sincronização com Rows")

# ==============================================================================
# ENDPOINTS DE LEITURA (GET)
# ==============================================================================

@router.get("/overview", response_model=None)
async def get_storage_overview(
    service: StorageService = Depends(create_storage_service)
):
    """Retorna visão geral de armazenamento (Total GB, Qtd Arquivos, etc)"""
    try:
        stats = service.get_storage_overview()
        return stats
    except Exception as e:
        logger.error(f"Erro ao buscar storage overview: {str(e)}")
        raise HTTPException(status_code=500, detail="Erro interno ao buscar overview")


@router.get("/largest-files", response_model=None)
async def get_largest_files(
    limit: int = Query(10, ge=1, le=100, description="Número de arquivos a retornar"),
    service: StorageService = Depends(create_storage_service)
):
    """
    Retorna os maiores arquivos do sistema.
    """
    try:
        files = service.get_largest_files(limit)
        
        # Mantendo o formato de resposta que você já usava
        return {
            'TotalResults': len(files),
            'Files': [
                {
                    'Id': file.Id,
                    'FileName': file.FileName,
                    'FeatureCategoria': file.FeatureCategoria,
                    'TamanhoBytes': file.TamanhoBytes,
                    'SizeMB': float(file.SizeMB),
                    'CriadoEm': file.CriadoEm,
                    'ModificadoEm': file.ModificadoEm
                }
                for file in files
            ]
        }
    except Exception as e:
        logger.error(f"Erro ao buscar largest files: {str(e)}")
        raise HTTPException(status_code=500, detail="Erro interno ao buscar maiores arquivos")


@router.get("/by-category/{categoria}", response_model=None)
async def get_files_by_category(
    categoria: str,
    limit: Optional[int] = Query(50, ge=1, le=500, description="Número de arquivos a retornar"),
    service: StorageService = Depends(create_storage_service)
):
    """
    Retorna arquivos de uma categoria específica (Ex: Videos, Imagens)
    """
    try:
        files = service.get_files_by_category(categoria, limit)
        
        return {
            'Category': categoria,
            'TotalResults': len(files),
            'Files': [
                {
                    'Id': file.Id,
                    'FileName': file.FileName,
                    'FeatureCategoria': file.FeatureCategoria,
                    'TamanhoBytes': file.TamanhoBytes,
                    'SizeMB': float(file.SizeMB),
                    'CriadoEm': file.CriadoEm,
                    'ModificadoEm': file.ModificadoEm
                }
                for file in files
            ]
        }
    except Exception as e:
        logger.error(f"Erro ao buscar files by category '{categoria}': {str(e)}")
        raise HTTPException(status_code=500, detail=f"Erro ao buscar categoria {categoria}")


@router.get("/growth-trend", response_model=None)
async def get_storage_growth_trend(
    days: int = Query(30, ge=1, le=365, description="Número de dias para análise"),
    service: StorageService = Depends(create_storage_service)
):
    """
    Retorna tendência de crescimento de armazenamento nos últimos X dias
    """
    try:
        trend = service.get_storage_growth_trend(days)
        return trend
    except Exception as e:
        logger.error(f"Erro ao buscar storage growth trend: {str(e)}")
        raise HTTPException(status_code=500, detail="Erro ao calcular tendência de crescimento")
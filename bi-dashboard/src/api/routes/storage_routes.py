"""
Storage Routes
Endpoints REST para analytics de armazenamento
🔒 Protegido com autenticação JWT
"""

from fastapi import APIRouter, Query, Depends
from typing import Optional
from ...features.storage import create_storage_service
from ...features.storage.schemas import StorageStatsDTO, FileDetailDTO
from ...core.security import get_current_user
import logging

logger = logging.getLogger(__name__)

# Router com autenticação obrigatória
router = APIRouter(dependencies=[Depends(get_current_user)])


@router.get("/overview", response_model=None)
async def get_storage_overview():
    """
    Retorna visão geral de armazenamento com breakdown por categoria
    
    Returns:
        StorageStatsDTO com estatísticas totais e breakdown por categoria
    """
    try:
        service = create_storage_service()
        stats = service.get_storage_overview()
        
        # Converte para dict para resposta JSON
        return {
            'TotalFiles': stats.TotalFiles,
            'TotalBytes': stats.TotalBytes,
            'TotalGB': float(stats.TotalGB),
            'TotalMB': float(stats.TotalMB),
            'LargestCategory': stats.LargestCategory,
            'SmallestCategory': stats.SmallestCategory,
            'CategoryBreakdown': [
                {
                    'FeatureCategoria': cat.FeatureCategoria,
                    'TotalFiles': cat.TotalFiles,
                    'TotalBytes': cat.TotalBytes,
                    'TotalGB': float(cat.TotalGB),
                    'PercentageOfTotal': float(cat.PercentageOfTotal),
                    'AvgFileSizeMB': float(cat.AvgFileSizeMB)
                }
                for cat in stats.CategoryBreakdown
            ]
        }
    except Exception as e:
        logger.error(f"Erro ao buscar storage overview: {str(e)}")
        raise


@router.get("/largest-files", response_model=None)
async def get_largest_files(
    limit: int = Query(10, ge=1, le=100, description="Número de arquivos a retornar")
):
    """
    Retorna os maiores arquivos do sistema
    
    Args:
        limit: Quantidade de arquivos (padrão: 10, máx: 100)
    
    Returns:
        Lista de FileDetailDTO ordenada por tamanho
    """
    try:
        service = create_storage_service()
        files = service.get_largest_files(limit)
        
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
        raise


@router.get("/by-category/{categoria}", response_model=None)
async def get_files_by_category(
    categoria: str,
    limit: Optional[int] = Query(50, ge=1, le=500, description="Número de arquivos a retornar")
):
    """
    Retorna arquivos de uma categoria específica
    
    Args:
        categoria: Nome da categoria (ex: 'Videos', 'Imagens')
        limit: Quantidade de arquivos (padrão: 50, máx: 500)
    
    Returns:
        Lista de FileDetailDTO filtrada por categoria
    """
    try:
        service = create_storage_service()
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
        raise


@router.get("/growth-trend", response_model=None)
async def get_storage_growth_trend(
    days: int = Query(30, ge=1, le=365, description="Número de dias para análise")
):
    """
    Retorna tendência de crescimento de armazenamento
    
    Args:
        days: Quantidade de dias para análise (padrão: 30, máx: 365)
    
    Returns:
        Dados de crescimento diário + resumo
    """
    try:
        service = create_storage_service()
        trend = service.get_storage_growth_trend(days)
        
        return trend
    except Exception as e:
        logger.error(f"Erro ao buscar storage growth trend: {str(e)}")
        raise

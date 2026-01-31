#!/usr/bin/env python3
"""
Script para executar a API do BI Dashboard
FastAPI + WebSocket
"""

import uvicorn
import sys
from pathlib import Path

# Adiciona o diretório raiz ao path
root_dir = Path(__file__).parent
sys.path.insert(0, str(root_dir))


if __name__ == "__main__":
    print("🚀 Iniciando BI Dashboard API...")
    print("📡 WebSocket Hubs disponíveis:")
    print("   - ws://localhost:8000/hubs/claims")
    print("   - ws://localhost:8000/hubs/financial")
    print("   - ws://localhost:8000/hubs/subscriptions")
    print("   - ws://localhost:8000/hubs/support")
    print()
    print("📚 Documentação: http://localhost:8000/docs")
    print()
    
    uvicorn.run(
        "src.api.main:app",
        host="0.0.0.0",
        port=8000,
        reload=True,
        log_level="info"
    )

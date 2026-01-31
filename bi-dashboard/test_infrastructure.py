"""
Test Script - Database & Repositories
Script para testar as conexões e repositórios criados
"""

import sys
from pathlib import Path

# Adiciona o diretório src ao path
sys.path.insert(0, str(Path(__file__).parent / "src"))

from data.infrastructure.database import db_connection
from data.infrastructure.mongo_client import mongo_connection
from data.repositories.financial_repository import financial_repository
from data.repositories.support_repository import support_repository


def test_sql_connection():
    """Testa conexão com SQL Server"""
    print("\n" + "="*60)
    print("🧪 TESTE 1: Conexão SQL Server")
    print("="*60)
    
    success = db_connection.test_connection()
    if success:
        print("✅ SQL Server: CONECTADO")
    else:
        print("❌ SQL Server: FALHOU")
    
    return success


def test_mongo_connection():
    """Testa conexão com MongoDB"""
    print("\n" + "="*60)
    print("🧪 TESTE 2: Conexão MongoDB")
    print("="*60)
    
    success = mongo_connection.test_connection()
    if success:
        print("✅ MongoDB: CONECTADO")
        mongo_connection.list_collections()
    else:
        print("❌ MongoDB: FALHOU")
    
    return success


def test_financial_repository():
    """Testa Financial Repository"""
    print("\n" + "="*60)
    print("🧪 TESTE 3: Financial Repository")
    print("="*60)
    
    try:
        # Teste 1: Resumo de Pagamentos
        print("\n📊 Resumo de Pagamentos:")
        summary = financial_repository.get_payments_summary()
        for key, value in summary.items():
            print(f"  - {key}: {value}")
        
        # Teste 2: Resumo de Assinaturas
        print("\n📊 Resumo de Assinaturas:")
        sub_summary = financial_repository.get_subscriptions_summary()
        for key, value in sub_summary.items():
            print(f"  - {key}: {value}")
        
        # Teste 3: Planos
        print("\n📋 Planos Disponíveis:")
        plans = financial_repository.get_all_plans()
        print(f"  - Total de Planos: {len(plans)}")
        for plan in plans:
            print(f"  - {plan.get('Name')}: R$ {plan.get('TransactionAmount')}")
        
        print("\n✅ Financial Repository: OK")
        return True
        
    except Exception as e:
        print(f"\n❌ Financial Repository: ERRO - {e}")
        return False


def test_support_repository():
    """Testa Support Repository"""
    print("\n" + "="*60)
    print("🧪 TESTE 4: Support Repository")
    print("="*60)
    
    try:
        # Verifica se a collection existe
        exists = support_repository.check_collection_exists()
        print(f"\n📁 Collection 'support_tickets' existe: {exists}")
        
        if exists:
            # Teste 1: Resumo de Tickets
            print("\n📊 Resumo de Tickets:")
            summary = support_repository.get_tickets_summary()
            for key, value in summary.items():
                print(f"  - {key}: {value}")
            
            # Teste 2: Tickets por Contexto
            print("\n📊 Tickets por Contexto:")
            by_context = support_repository.get_tickets_by_context_count()
            for item in by_context:
                print(f"  - {item.get('Context')}: {item.get('Count')}")
        
        print("\n✅ Support Repository: OK")
        return True
        
    except Exception as e:
        print(f"\n❌ Support Repository: ERRO - {e}")
        return False


def main():
    """Executa todos os testes"""
    print("\n" + "="*60)
    print("🚀 TESTES DE INFRAESTRUTURA - BI DASHBOARD")
    print("="*60)
    
    results = {
        "SQL Server": test_sql_connection(),
        "MongoDB": test_mongo_connection(),
        "Financial Repository": test_financial_repository(),
        "Support Repository": test_support_repository(),
    }
    
    # Resumo Final
    print("\n" + "="*60)
    print("📋 RESUMO DOS TESTES")
    print("="*60)
    
    for test_name, passed in results.items():
        status = "✅ PASSOU" if passed else "❌ FALHOU"
        print(f"{test_name}: {status}")
    
    total = len(results)
    passed = sum(results.values())
    
    print(f"\nTotal: {passed}/{total} testes passaram")
    
    if passed == total:
        print("\n🎉 TODOS OS TESTES PASSARAM!")
    else:
        print(f"\n⚠️  {total - passed} teste(s) falharam")


if __name__ == "__main__":
    main()

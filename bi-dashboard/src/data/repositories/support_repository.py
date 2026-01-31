"""
Support Repository
Repositório para acesso aos dados de suporte (MongoDB - SupportTicketDocument)
Baseado em Documents/Models/SupportTicketDocument.cs
"""

from typing import List, Dict, Optional, Any
from datetime import datetime, timedelta
from pymongo.collection import Collection
from ..infrastructure.mongo_client import get_mongo_collection


class SupportRepository:
    """
    Repository para operações de suporte (MongoDB)
    Collection: support_tickets
    """
    
    COLLECTION_NAME = "support_tickets"
    
    def __init__(self):
        self.collection: Collection = get_mongo_collection(self.COLLECTION_NAME)
    
    # ==================== QUERIES BÁSICAS ====================
    
    def get_all_tickets(self, limit: Optional[int] = None) -> List[Dict[str, Any]]:
        """
        Busca todos os tickets de suporte
        Campos baseados em SupportTicketDocument.cs:
        - Id (ObjectId)
        - UserId (string)
        - Context (string - categoria)
        - Explanation (string - mensagem)
        - Status (string - Open, InProgress, Closed)
        - CreatedAt (datetime)
        """
        cursor = self.collection.find().sort("CreatedAt", -1)
        
        if limit:
            cursor = cursor.limit(limit)
        
        tickets = []
        for ticket in cursor:
            # Converte ObjectId para string para serialização
            ticket['_id'] = str(ticket['_id'])
            tickets.append(ticket)
        
        return tickets
    
    def get_tickets_by_status(self, status: str) -> List[Dict[str, Any]]:
        """
        Busca tickets por status (Open, InProgress, Closed)
        """
        cursor = self.collection.find({"Status": status}).sort("CreatedAt", -1)
        
        tickets = []
        for ticket in cursor:
            ticket['_id'] = str(ticket['_id'])
            tickets.append(ticket)
        
        return tickets
    
    def get_tickets_by_user(self, user_id: str) -> List[Dict[str, Any]]:
        """
        Busca todos os tickets de um usuário específico
        """
        cursor = self.collection.find({"UserId": user_id}).sort("CreatedAt", -1)
        
        tickets = []
        for ticket in cursor:
            ticket['_id'] = str(ticket['_id'])
            tickets.append(ticket)
        
        return tickets
    
    def get_tickets_by_context(self, context: str) -> List[Dict[str, Any]]:
        """
        Busca tickets por categoria/contexto (Financeiro, Bug, Dúvida, etc)
        """
        cursor = self.collection.find({"Context": context}).sort("CreatedAt", -1)
        
        tickets = []
        for ticket in cursor:
            ticket['_id'] = str(ticket['_id'])
            tickets.append(ticket)
        
        return tickets
    
    # ==================== MÉTRICAS E ANALYTICS ====================
    
    def get_tickets_summary(self) -> Dict[str, Any]:
        """
        Retorna resumo estatístico dos tickets
        """
        pipeline = [
            {
                "$group": {
                    "_id": None,
                    "TotalTickets": {"$sum": 1},
                    "OpenTickets": {
                        "$sum": {"$cond": [{"$eq": ["$Status", "Open"]}, 1, 0]}
                    },
                    "InProgressTickets": {
                        "$sum": {"$cond": [{"$eq": ["$Status", "InProgress"]}, 1, 0]}
                    },
                    "ClosedTickets": {
                        "$sum": {"$cond": [{"$eq": ["$Status", "Closed"]}, 1, 0]}
                    },
                }
            }
        ]
        
        result = list(self.collection.aggregate(pipeline))
        
        if result:
            summary = result[0]
            summary.pop('_id', None)  # Remove o _id do grupo
            return summary
        
        return {
            "TotalTickets": 0,
            "OpenTickets": 0,
            "InProgressTickets": 0,
            "ClosedTickets": 0
        }
    
    def get_tickets_by_context_count(self) -> List[Dict[str, Any]]:
        """
        Retorna contagem de tickets por contexto/categoria
        """
        pipeline = [
            {
                "$group": {
                    "_id": "$Context",
                    "Count": {"$sum": 1}
                }
            },
            {
                "$project": {
                    "_id": 0,
                    "Context": "$_id",
                    "Count": 1
                }
            },
            {
                "$sort": {"Count": -1}
            }
        ]
        
        return list(self.collection.aggregate(pipeline))
    
    def get_tickets_created_in_period(
        self, 
        start_date: datetime, 
        end_date: datetime
    ) -> List[Dict[str, Any]]:
        """
        Busca tickets criados em um período específico
        """
        cursor = self.collection.find({
            "CreatedAt": {
                "$gte": start_date,
                "$lte": end_date
            }
        }).sort("CreatedAt", -1)
        
        tickets = []
        for ticket in cursor:
            ticket['_id'] = str(ticket['_id'])
            tickets.append(ticket)
        
        return tickets
    
    def get_tickets_created_last_days(self, days: int = 7) -> List[Dict[str, Any]]:
        """
        Busca tickets criados nos últimos X dias
        """
        start_date = datetime.utcnow() - timedelta(days=days)
        end_date = datetime.utcnow()
        
        return self.get_tickets_created_in_period(start_date, end_date)
    
    def get_average_resolution_time(self) -> Dict[str, Any]:
        """
        Calcula tempo médio de resolução de tickets (Open -> Closed)
        Nota: Requer campo UpdatedAt ou ClosedAt no documento
        """
        pipeline = [
            {
                "$match": {"Status": "Closed"}
            },
            {
                "$group": {
                    "_id": None,
                    "TotalClosed": {"$sum": 1},
                    # Aqui você pode adicionar cálculo de tempo se tiver campo de fechamento
                }
            }
        ]
        
        result = list(self.collection.aggregate(pipeline))
        
        if result:
            return result[0]
        
        return {"TotalClosed": 0}
    
    def get_most_active_users(self, limit: int = 10) -> List[Dict[str, Any]]:
        """
        Retorna usuários com mais tickets criados
        """
        pipeline = [
            {
                "$group": {
                    "_id": "$UserId",
                    "TicketCount": {"$sum": 1}
                }
            },
            {
                "$project": {
                    "_id": 0,
                    "UserId": "$_id",
                    "TicketCount": 1
                }
            },
            {
                "$sort": {"TicketCount": -1}
            },
            {
                "$limit": limit
            }
        ]
        
        return list(self.collection.aggregate(pipeline))
    
    # ==================== UTILITIES ====================
    
    def count_tickets(self) -> int:
        """Retorna contagem total de tickets"""
        return self.collection.count_documents({})
    
    def count_open_tickets(self) -> int:
        """Retorna contagem de tickets abertos"""
        return self.collection.count_documents({"Status": "Open"})
    
    def check_collection_exists(self) -> bool:
        """Verifica se a collection existe"""
        from ..infrastructure.mongo_client import mongo_connection
        collections = mongo_connection.database.list_collection_names()
        return self.COLLECTION_NAME in collections


# Singleton instance
support_repository = SupportRepository()

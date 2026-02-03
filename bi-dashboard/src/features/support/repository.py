from typing import List, Dict, Any, Optional
from ...core.infrastructure.database_mongo import get_mongo_db # Seu novo helper do motor

class SupportRepository:
    """
    Repository Assíncrono para Tickets (MongoDB/Motor)
    """
    
    COLLECTION_NAME = "support_tickets"
    
    def __init__(self):
        # Em Motor, pegamos o DB, mas as operações são awaitables
        self.db = get_mongo_db()
        self.collection = self.db[self.COLLECTION_NAME]
    
    async def get_all_tickets(self, limit: int = 50) -> List[Dict[str, Any]]:
        """Busca tickets ordenados por data (Non-blocking)"""
        # Motor: .find() não aceita await, mas o cursor resultante sim (para converter em lista)
        cursor = self.collection.find().sort("CreatedAt", -1)
        
        # O Motor exige um 'length' no to_list. Se quiser todos, use um número alto ou None (cuidado com memória)
        tickets = await cursor.to_list(length=limit)
        
        # Conversão de ObjectId para string
        for t in tickets:
            t['_id'] = str(t['_id'])
        
        return tickets
    
    async def get_tickets_summary(self) -> Dict[str, Any]:
        """Agregação de status via Pipeline do Mongo"""
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
        
        # Aggregate no Motor também devolve um cursor
        cursor = self.collection.aggregate(pipeline)
        result = await cursor.to_list(length=1)
        
        if result:
            summary = result[0]
            summary.pop('_id', None)
            return summary
        
        return {
            "TotalTickets": 0,
            "OpenTickets": 0,
            "InProgressTickets": 0,
            "ClosedTickets": 0
        }
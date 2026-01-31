"""
Support Repository
MongoDB repository for support tickets
"""

from typing import List, Dict, Optional, Any
from datetime import datetime, timedelta
from pymongo.collection import Collection
from ...core.infrastructure import get_mongo_db


class SupportRepository:
    """Repository para tickets de suporte (MongoDB)"""
    
    COLLECTION_NAME = "support_tickets"
    
    def __init__(self):
        db = get_mongo_db()
        self.collection: Collection = db[self.COLLECTION_NAME]
    
    def get_all_tickets(self, limit: Optional[int] = None) -> List[Dict[str, Any]]:
        """Busca todos os tickets"""
        cursor = self.collection.find().sort("CreatedAt", -1)
        
        if limit:
            cursor = cursor.limit(limit)
        
        tickets = []
        for ticket in cursor:
            ticket['_id'] = str(ticket['_id'])
            tickets.append(ticket)
        
        return tickets
    
    def get_tickets_summary(self) -> Dict[str, Any]:
        """Resumo de tickets por status"""
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
            summary.pop('_id', None)
            return summary
        
        return {
            "TotalTickets": 0,
            "OpenTickets": 0,
            "InProgressTickets": 0,
            "ClosedTickets": 0
        }

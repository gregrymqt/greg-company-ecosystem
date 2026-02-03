from pydantic import BaseModel
from typing import Optional

class CourseDTO(BaseModel):
    """DTO para Course (baseado no logError.md)"""
    Id: str
    Name: str
    IsActive: bool
    TotalVideos: int

class ContentSummaryDTO(BaseModel):
    """KPIs calculados de conteúdo"""
    TotalCourses: int
    ActiveCourses: int
    TotalVideosLib: int # Soma de todos os vídeos
    AvgVideosPerCourse: float
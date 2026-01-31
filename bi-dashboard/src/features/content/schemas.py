"""
Content Feature - Schemas
DTOs para análise de conteúdo (cursos e vídeos)
"""

from dataclasses import dataclass


@dataclass
class CourseDTO:
    """DTO para Course"""
    Id: str
    Name: str
    IsActive: bool
    TotalVideos: int


@dataclass
class VideoDTO:
    """DTO para Video"""
    Id: int
    Title: str
    CourseId: str
    IsActive: bool

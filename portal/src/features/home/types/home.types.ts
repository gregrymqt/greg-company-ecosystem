/**
 * Tipagem compartilhada de Home - Alinhada com HomeDtos.cs do Backend
 * 
 * Esta é a fonte única de verdade (Single Source of Truth) para tipos de Home.
 * Tanto Admin quanto Public devem importar daqui.
 */

// =================================================================
// DTOs DE LEITURA (O que vem do Backend)
// =================================================================

/**
 * Representa um slide do Hero Carousel
 * Baseado em HeroSlideDto.cs
 */
export interface HeroSlideDto {
  id: string;
  imageUrl: string;
  title: string;
  subtitle: string;
  actionText: string;
  actionUrl: string;
  audience?: 'student' | 'merchant' | 'universal';
}

/**
 * Representa um serviço/card na seção de serviços
 * Baseado em ServiceDto.cs
 */
export interface ServiceDto {
  id: string;
  iconClass: string;
  title: string;
  description: string;
  actionText: string;
  actionUrl: string;
  audience?: 'student' | 'merchant' | 'universal';
}

/**
 * Resposta completa do endpoint GET /api/Home
 * Baseado em HomeContentDto.cs
 */
export interface HomeContentDto {
  hero: HeroSlideDto[];
  services: ServiceDto[];
}


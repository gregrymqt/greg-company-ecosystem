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
  id: number;
  imageUrl: string;
  title: string;
  subtitle: string;
  actionText: string;
  actionUrl: string;
}

/**
 * Representa um serviço/card na seção de serviços
 * Baseado em ServiceDto.cs
 */
export interface ServiceDto {
  id: number;
  iconClass: string;
  title: string;
  description: string;
  actionText: string;
  actionUrl: string;
}

/**
 * Resposta completa do endpoint GET /api/Home
 * Baseado em HomeContentDto.cs
 */
export interface HomeContentDto {
  hero: HeroSlideDto[];
  services: ServiceDto[];
}

// =================================================================
// DTOs DE ESCRITA (O que o Frontend envia para Create/Update)
// =================================================================

/**
 * Dados para criar/atualizar um Hero Slide
 * Baseado em CreateUpdateHeroDto.cs
 */
export interface CreateUpdateHeroData {
  title: string;
  subtitle: string;
  actionText: string;
  actionUrl: string;
}

/**
 * Dados para criar/atualizar um Service
 * Baseado em CreateUpdateServiceDto.cs
 */
export interface CreateUpdateServiceData {
  iconClass: string;
  title: string;
  description: string;
  actionText: string;
  actionUrl: string;
}

// =================================================================
// TIPOS ESPECÍFICOS DE FORMULÁRIO (UI)
// =================================================================

/**
 * Valores do formulário de Hero (com suporte a upload de imagem)
 */
export interface HeroFormValues extends CreateUpdateHeroData {
  newImage?: FileList;  // Input type="file" retorna FileList
}

/**
 * Valores do formulário de Service (alias para clareza)
 */
export type ServiceFormValues = CreateUpdateServiceData;

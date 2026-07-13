/**
 * Tipagem compartilhada de Profile - Alinhada com DTOs do Backend
 * 
 * Esta é a fonte única de verdade (Single Source of Truth) para tipos de Profile.
 */

// =================================================================
// USER ACCOUNT - Baseado em UserProfileDto.cs
// =================================================================

/**
 * Perfil do usuário autenticado
 * Baseado em UserProfileDto.cs
 */
export interface UserProfileDto {
  name: string;
  email: string;
  avatarUrl: string;
}

export interface AvatarUpdateResponse {
  avatarUrl: string;
  message: string;
}

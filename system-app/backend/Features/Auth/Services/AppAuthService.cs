﻿using System.Security.Claims;
using MeuCrudCsharp.Features.Auth.Dtos;
using MeuCrudCsharp.Features.Auth.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Payments.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Subscriptions.Interfaces;
using MeuCrudCsharp.Models;
using Microsoft.AspNetCore.Identity;

namespace MeuCrudCsharp.Features.Auth.Services
{
    public class AuthService : IAppAuthService
    {
        private readonly UserManager<Users> _userManager;
        private readonly IUserRepository _userRepository; // <-- Nova dependência
        private readonly IUserRoleRepository _userRoleRepository; // <-- Nova dependência
        private readonly IJwtService _jwtService; // <-- Nova dependência
        private readonly ILogger<AuthService> _logger;
        private readonly IPaymentRepository _paymentRepository; // <-- Nova dependência
        private readonly ISubscriptionRepository _subscriptionRepository; // <-- Nova dependência

        // O DbContext foi REMOVIDO daqui!
        public AuthService(
            UserManager<Users> userManager,
            IUserRepository userRepository,
            IJwtService jwtService,
            ILogger<AuthService> logger,
            IPaymentRepository paymentRepository,
            ISubscriptionRepository subscriptionRepository,
            IUserRoleRepository userRoleRepository
        )
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _jwtService = jwtService;
            _userRoleRepository = userRoleRepository;
            _paymentRepository = paymentRepository;
            _subscriptionRepository = subscriptionRepository;
            _logger = logger;
        }

        public async Task<Users> SignInWithGoogleAsync(
            ClaimsPrincipal googleUserPrincipal,
            HttpContext httpContext
        )
        {
            string? googleId = googleUserPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            string? email = googleUserPrincipal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException(
                    "Não foi possível obter os dados do provedor externo."
                );
            }
            var user = await _userRepository.FindByGoogleIdAsync(googleId);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogInformation(
                        "Nenhum usuário encontrado para {Email}. Criando uma nova conta.",
                        email
                    );
                    string? name = googleUserPrincipal.FindFirstValue(ClaimTypes.Name);
                    string? avatar = googleUserPrincipal.FindFirstValue("urn:google:picture");
                    user = toUserEntity(user);
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Não foi possível criar o usuário: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                        );
                    }
                    await addRolesToUser(user);
                }
                await _userManager.AddLoginAsync(
                    user,
                    new UserLoginInfo("Google", googleId, "Google")
                );
            }
            var jwtString = await _jwtService.GenerateJwtTokenAsync(user);
            httpContext.Response.Cookies.Append(
                "jwt",
                jwtString,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddHours(7),
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                }
            );
            _logger.LogInformation("Usuário {UserId} logado com sucesso via Google.", user.Id);
            return user;
        }

        public async Task<UserSessionDto> GetAuthenticatedUserDataAsync(string userId)
        {
            // 1. Busca dados básicos do usuário
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null) //
                throw new ResourceNotFoundException("Usuário não encontrado.");

            // 2. Consultas Paralelas
            var paymentTask = _paymentRepository.HasAnyPaymentByUserIdAsync(userId);
            var subTask = _subscriptionRepository.HasActiveSubscriptionByUserIdAsync(userId);
            var rolesTask = _userRoleRepository.GetRolesByUserIdAsync(userId);

            // Aguardamos todas terminarem
            await Task.WhenAll(paymentTask, subTask, rolesTask);

            // 3. Monta o DTO com os resultados
            return new UserSessionDto
            {
                PublicId = user.PublicId, //
                Name = user.Name ?? "Usuário", //

                // CORREÇÃO CS8601: Tratando possível nulo do IdentityUser.Email
                Email = user.Email ?? string.Empty, //

                // Agora isso funciona pois adicionamos a propriedade na Model
                AvatarUrl = user.AvatarUrl, //

                HasPaymentHistory = paymentTask.Result, //

                // CORREÇÃO: Faltava uma vírgula aqui
                HasActiveSubscription = subTask.Result, //

                // 4. Preenche as Roles
                Roles = rolesTask.Result ?? new List<string>(),
            };
        }

        private Users toUserEntity(Users user)
        {
            return new Users
            {
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl,
                GoogleId = user.GoogleId,
                EmailConfirmed = user.EmailConfirmed,
            };
        }

        private async Task addRolesToUser(Users user)
        {
            if (
                user.Email != null
                && user.Email.Equals(
                    "emailGenéricoAdmin@gmail.com",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
        }

        /// <summary>
        /// Login com email e senha
        /// </summary>
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            // 1. Validar email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning(
                    "Tentativa de login com email não encontrado: {Email}",
                    request.Email
                );
                throw new UnauthorizedAccessException("Email ou senha inválidos.");
            }

            // 2. Validar senha
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning(
                    "Tentativa de login com senha incorreta para o usuário: {UserId}",
                    user.Id
                );
                throw new UnauthorizedAccessException("Email ou senha inválidos.");
            }

            // 3. Gerar token JWT com expiração
            var (token, expiration) = await _jwtService.GenerateJwtTokenWithExpirationAsync(user);

            // 4. Gerar refresh token (por enquanto, um GUID simples)
            // TODO: Implementar refresh token persistente no banco de dados
            var refreshToken = Guid.NewGuid().ToString();

            // 5. Buscar dados completos do usuário
            var userSession = await GetAuthenticatedUserDataAsync(user.Id);

            _logger.LogInformation("Login bem-sucedido para o usuário: {UserId}", user.Id);

            // 6. Retornar resposta completa
            return new LoginResponseDto
            {
                User = userSession,
                Token = token,
                RefreshToken = refreshToken,
                Expiration = expiration,
            };
        }

        /// <summary>
        /// Registro de novo usuário
        /// </summary>
        public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // 1. Validar se senhas coincidem
            if (request.Password != request.ConfirmPassword)
            {
                throw new InvalidOperationException("As senhas não coincidem.");
            }

            // 2. Verificar se email já existe
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email já cadastrado.");
            }

            // 3. Criar novo usuário
            var user = new Users
            {
                UserName = request.Email,
                Email = request.Email,
                Name = request.Name,
                EmailConfirmed = false,
                AvatarUrl = string.Empty,
                PublicId = Guid.NewGuid(),
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Erro ao criar usuário: {Errors}", errors);
                throw new InvalidOperationException($"Não foi possível criar o usuário: {errors}");
            }

            // 4. Adicionar role padrão
            await addRolesToUser(user);

            // 5. Gerar token JWT com expiração
            var (token, expiration) = await _jwtService.GenerateJwtTokenWithExpirationAsync(user);

            // 6. Gerar refresh token
            var refreshToken = Guid.NewGuid().ToString();

            // 7. Buscar dados completos do usuário
            var userSession = await GetAuthenticatedUserDataAsync(user.Id);

            _logger.LogInformation("Novo usuário registrado com sucesso: {UserId}", user.Id);

            // 8. Retornar resposta completa
            return new LoginResponseDto
            {
                User = userSession,
                Token = token,
                RefreshToken = refreshToken,
                Expiration = expiration,
            };
        }
    }
}

using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Interfaces;
using MeuCrudCsharp.Features.Caching.Application.Interfaces;
using MeuCrudCsharp.Features.Exceptions;
using MeuCrudCsharp.Features.MercadoPago.Plans.Application.DTOs;
using MeuCrudCsharp.Features.MercadoPago.Plans.Application.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Plans.Application.Mappers;
using MeuCrudCsharp.Features.MercadoPago.Plans.Application.Utils;
using MeuCrudCsharp.Features.Shared.Domain.Interfaces;
using MeuCrudCsharp.Features.MercadoPago.Plans.Domain.Entities;
using Microsoft.Extensions.Options;

namespace MeuCrudCsharp.Features.MercadoPago.Plans.Application.Services
{
    public class PlanService : IPlanService
    {
        private static class CacheKeys
        {
            public const string ActiveDbPlans = "ActiveDbPlans";
            public const string ActiveApiPlans = "ActiveApiPlans";
        }

        private readonly ICacheService _cacheService;
        private readonly IPlanRepository _planRepository;
        private readonly IMercadoPagoPlanService _mercadoPagoPlanService;
        private readonly GeneralSettings _generalSettings;
        private readonly ILogger<PlanService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public PlanService(
            ICacheService cacheService,
            ILogger<PlanService> logger,
            IPlanRepository planRepository,
            IMercadoPagoPlanService mercadoPagoPlanService,
            IOptions<GeneralSettings> generalSettings,
            IUnitOfWork unitOfWork
        )
        {
            _cacheService = cacheService;
            _planRepository = planRepository;
            _mercadoPagoPlanService = mercadoPagoPlanService;
            _generalSettings = generalSettings.Value;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<PlanEditDto> GetPlanEditDtoByIdAsync(Guid publicId)
        {
            var plan = await _planRepository.GetByPublicIdAsync(publicId, asNoTracking: true);

            return (
                    plan == null
                        ? null
                        : PlanMapper.MapPlanToEditDto(plan)
                ) ?? throw new InvalidOperationException();
        }

        public async Task<PlanDto> GetPlanDtoByIdAsync(Guid publicId)
        {
            var plan = await _planRepository.GetByPublicIdAsync(publicId, asNoTracking: true);
            if (plan == null || !plan.IsActive)
            {
                throw new ResourceNotFoundException($"Plano com ID {publicId} não encontrado ou inativo.");
            }
            return PlanMapper.MapDbPlanToDto(plan);
        }

        public async Task<PlanDto> CreatePlanAsync(CreatePlanDto createDto)
        {
            if (
                !Enum.TryParse<PlanFrequencyType>(
                    createDto.AutoRecurring.FrequencyType,
                    true,
                    out var frequencyTypeEnum
                )
            )
            {
                throw new ArgumentException(
                    $"Valor de frequncia invlido: '{createDto.AutoRecurring.FrequencyType}'."
                );
            }

            var newPlan = new Plan
            {
                Name = createDto.Reason,
                Description = createDto.Description,
                TransactionAmount = createDto.AutoRecurring.TransactionAmount,
                CurrencyId = createDto.AutoRecurring.CurrencyId,
                FrequencyInterval = createDto.AutoRecurring.Frequency,
                FrequencyType = frequencyTypeEnum,
                IsActive = true,
                IncludedCourseIds = createDto.IncludedCourseIds ?? new List<string>()
            };

            var mercadoPagoPayload = new
            {
                reason = createDto.Reason,
                auto_recurring = createDto.AutoRecurring,
                back_url = _generalSettings.BaseUrl,
                external_reference = newPlan.PublicId.ToString(),
            };

            PlanResponseDto mpPlanResponse;
            try
            {
                mpPlanResponse = await _mercadoPagoPlanService.CreatePlanAsync(mercadoPagoPayload);
            }
            catch (ExternalApiException ex)
            {
                _logger.LogError(
                    ex,
                    "Erro na API externa ao criar plano '{PlanName}'.",
                    createDto.Reason
                );
                throw;
            }

            newPlan.ExternalPlanId = mpPlanResponse.Id;
            await _planRepository.AddAsync(newPlan);
            await _unitOfWork.CommitAsync();

            await _cacheService.RemoveAsync(CacheKeys.ActiveDbPlans);
            await _cacheService.RemoveAsync(CacheKeys.ActiveApiPlans);

            _logger.LogInformation(
                "Plano '{PlanName}' criado com sucesso. ID: {PlanId}, ExternalId: {ExternalId}",
                newPlan.Name,
                newPlan.Id,
                newPlan.ExternalPlanId
            );

            return PlanMapper.MapDbPlanToDto(newPlan);
        }

        public async Task<PlanDto> UpdatePlanAsync(Guid publicId, UpdatePlanDto updateDto)
        {
            var localPlan =
                await _planRepository.GetByPublicIdAsync(publicId, asNoTracking: false)
                ?? throw new ResourceNotFoundException($"Plano com ID {publicId} no encontrado.");

            var originalName = localPlan.Name;
            var originalTransactionAmount = localPlan.TransactionAmount;
            var originalFrequencyInterval = localPlan.FrequencyInterval;
            var originalFrequencyType = localPlan.FrequencyType;
            var originalDescription = localPlan.Description;

            PlanUtils.ApplyUpdateDtoToPlan(localPlan, updateDto);

            try
            {
                var payloadForMercadoPago = new
                {
                    reason = updateDto.Reason,
                    auto_recurring = updateDto.AutoRecurring,
                };

                await _mercadoPagoPlanService.UpdatePlanAsync(
                    localPlan.ExternalPlanId,
                    payloadForMercadoPago
                );

                await _unitOfWork.CommitAsync();

                await _cacheService.RemoveAsync(CacheKeys.ActiveDbPlans);
                await _cacheService.RemoveAsync(CacheKeys.ActiveApiPlans);

                _logger.LogInformation(
                    "Plano {PlanId} atualizado com sucesso no DB e na API externa.",
                    localPlan.ExternalPlanId
                );

                return PlanMapper.MapDbPlanToDto(localPlan);
            }
            catch (ExternalApiException ex)
            {
                _logger.LogError(
                    ex,
                    "Erro na API externa ao atualizar plano '{PlanName}'. Rollback automtico.",
                    localPlan.Name
                );

                localPlan.Name = originalName;
                localPlan.TransactionAmount = originalTransactionAmount;
                localPlan.FrequencyInterval = originalFrequencyInterval;
                localPlan.FrequencyType = originalFrequencyType;
                localPlan.Description = originalDescription;
                _logger.LogInformation(
                    "Rollback concludo. Alteraes locais no plano '{PlanName}' foram desfeitas.",
                    localPlan.Name
                );

                throw;
            }
        }

        public async Task DeletePlanAsync(Guid publicId)
        {
            var localPlan =
                await _planRepository.GetByPublicIdAsync(publicId, asNoTracking: false)
                ?? throw new ResourceNotFoundException($"Plano com ID {publicId} no encontrado.");

            if (!localPlan.IsActive)
            {
                _logger.LogWarning(
                    "Tentativa de desativar o plano {PlanId} que j est inativo.",
                    localPlan.ExternalPlanId
                );
                return;
            }

            localPlan.IsActive = false;

            try
            {
                await _mercadoPagoPlanService.CancelPlanAsync(localPlan.ExternalPlanId);

                await _unitOfWork.CommitAsync();

                await _cacheService.RemoveAsync(CacheKeys.ActiveDbPlans);
                await _cacheService.RemoveAsync(CacheKeys.ActiveApiPlans);

                _logger.LogInformation(
                    "Plano {PlanId} desativado com sucesso no DB e na API externa.",
                    localPlan.ExternalPlanId
                );
            }
            catch (ExternalApiException ex)
            {
                _logger.LogError(
                    ex,
                    "Erro na API externa ao desativar plano '{PlanName}'. Rollback automtico.",
                    localPlan.Name
                );

                localPlan.IsActive = true;

                _logger.LogInformation(
                    "Rollback concludo. Plano '{PlanName}' permanece ativo localmente.",
                    localPlan.Name
                );

                throw;
            }
        }

        public async Task<PagedResultDto<PlanDto>> GetActiveApiPlansAsync(int page, int pageSize)
        {
            var limit = pageSize;
            var offset = (page - 1) * pageSize;
            var status = "active";
            var sortBy = "transaction_amount";
            var criteria = "asc";

            _logger.LogInformation(
                "Buscando pgina {Page} de planos da API do Mercado Pago.",
                page
            );
            var activePlansFromApi = await _mercadoPagoPlanService.SearchActivePlansAsync(
                limit,
                offset,
                status,
                sortBy,
                criteria
            );

            var totalCount = await GetTotalActiveApiPlansCountAsync();

            var planResponseDtos = activePlansFromApi.ToList();
            if (planResponseDtos.Count == 0)
            {
                return new PagedResultDto<PlanDto>([], page, pageSize, totalCount);
            }

            var externalIds = planResponseDtos.Select(p => p.Id).ToList();
            var localPlans = await _planRepository.GetByExternalIdsAsync(externalIds);
            var localPlansDict = localPlans.ToDictionary(p => p.ExternalPlanId, p => p);

            var mappedPlans = new List<PlanDto>();
            foreach (var apiPlan in planResponseDtos)
            {
                if (localPlansDict.TryGetValue(apiPlan.Id, out var localPlan))
                {
                    mappedPlans.Add(PlanMapper.MapApiPlanToDto(apiPlan, localPlan));
                }
                else
                {
                    _logger.LogWarning(
                        "Plano '{ExternalId}' existe no MP mas no localmente.",
                        apiPlan.Id
                    );
                }
            }

            return new PagedResultDto<PlanDto>(mappedPlans, page, pageSize, totalCount);
        }

        private async Task<int> GetTotalActiveApiPlansCountAsync()
        {
            return await _cacheService.GetOrCreateAsync(
                    "TotalActiveApiPlansCount",
                async () =>
                {
                    var allPlans = await _mercadoPagoPlanService.SearchActivePlansAsync(
                        1000,
                        0,
                        "active",
                        "date_created",
                        "desc"
                    );
                    return allPlans.Count();
                },
                TimeSpan.FromMinutes(15)
            );
        }

        public async Task<PagedResultDto<PlanDto>> GetActiveDbPlansAsync(int page, int pageSize)
        {
            _logger.LogInformation("Buscando planos ativos paginados do banco de dados.");

            try
            {
                var cacheKey = $"{CacheKeys.ActiveDbPlans}_{page}_{pageSize}";
                return await _cacheService.GetOrCreateAsync(
                    cacheKey,
                    async () =>
                    {
                        var pagedResult = await _planRepository.GetActivePlansAsync(page, pageSize);
                        var planDtos = pagedResult.Items.Select(PlanMapper.MapDbPlanToDto).ToList();

                        return new PagedResultDto<PlanDto>(
                            planDtos,
                            pagedResult.CurrentPage,
                            pagedResult.PageSize,
                            pagedResult.TotalCount
                        );
                    },
                    TimeSpan.FromMinutes(15)
                ) ?? new PagedResultDto<PlanDto>([], page, pageSize, 0);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Falha ao buscar planos paginados do repositrio ou cache.");
                throw new AppServiceException("No foi possvel carregar os planos.", dbEx);
            }
        }
    }
}

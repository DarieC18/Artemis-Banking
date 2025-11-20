using ArtemisBanking.Application.DTOs.Commerce;
using ArtemisBanking.Application.DTOs.Common;
using ArtemisBanking.Application.Interfaces.Persistence;
using ArtemisBanking.Application.Interfaces.Services;
using ArtemisBanking.Domain.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBanking.Application.Services
{
    public class CommerceServiceApi : ICommerceServiceApi
    {
        private readonly IGenericRepository<Commerce> _commerceRepository;
        private readonly IMapper _mapper;

        public CommerceServiceApi(
            IGenericRepository<Commerce> commerceRepository,
            IMapper mapper)
        {
            _commerceRepository = commerceRepository;
            _mapper = mapper;
        }

        // GET paginado
        public async Task<Result<PagedResult<CommerceListDto>>> GetAllAsync(int? page, int? pageSize)
        {
            var query = _commerceRepository
                .GetAllQuery()
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt);

            var totalCount = await query.CountAsync();

            if (page is null || pageSize is null || page <= 0 || pageSize <= 0)
            {
                var list = await query.ToListAsync();
                var mapped = _mapper.Map<List<CommerceListDto>>(list);

                var resultAll = new PagedResult<CommerceListDto>
                {
                    Data = mapped,
                    CurrentPage = 1,
                    TotalPages = 1,
                    TotalCount = totalCount
                };

                return Result<PagedResult<CommerceListDto>>.Ok(resultAll);
            }

            var currentPage = page.Value;
            var size = pageSize.Value;

            var items = await query
                .Skip((currentPage - 1) * size)
                .Take(size)
                .ToListAsync();

            var data = _mapper.Map<List<CommerceListDto>>(items);

            var paged = new PagedResult<CommerceListDto>
            {
                Data = data,
                CurrentPage = currentPage,
                TotalPages = (int)Math.Ceiling(totalCount / (double)size),
                TotalCount = totalCount
            };

            return Result<PagedResult<CommerceListDto>>.Ok(paged);
        }

        // GET por ID
        public async Task<Result<CommerceDetailDto>> GetByIdAsync(int id)
        {
            var commerce = await _commerceRepository.GetById(id);
            if (commerce is null)
                return Result<CommerceDetailDto>.Fail("Comercio no encontrado.");

            var dto = _mapper.Map<CommerceDetailDto>(commerce);
            return Result<CommerceDetailDto>.Ok(dto);
        }

        // POST crear
        public async Task<Result<int>> CreateAsync(CommerceCreateUpdateDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Name))
                errors.Add("El nombre del comercio es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                errors.Add("La descripción del comercio es obligatoria.");

            if (errors.Any())
                return Result<int>.Fail(errors);

            var entity = _mapper.Map<Commerce>(dto);
            entity.IsActive = true;
            entity.CreatedAt = DateTime.UtcNow;

            var created = await _commerceRepository.AddAsync(entity);

            if (created is null)
                return Result<int>.Fail("No se pudo crear el comercio.");

            return Result<int>.Ok(created.Id);
        }

        // PUT actualizar
        public async Task<Result> UpdateAsync(int id, CommerceCreateUpdateDto dto)
        {
            var commerce = await _commerceRepository.GetById(id);
            if (commerce is null)
                return Result.Fail("Comercio no encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Description))
            {
                return Result.Fail("Nombre y descripción son obligatorios.");
            }

            commerce.Name = dto.Name;
            commerce.Description = dto.Description;
            commerce.LogoUrl = dto.Logo;

            var updated = await _commerceRepository.UpdateAsync(id, commerce);
            if (updated is null)
                return Result.Fail("No se pudo actualizar el comercio.");

            return Result.Ok();
        }

        // PATCH cambiar estado
        public async Task<Result> ChangeStatusAsync(int id, bool status)
        {
            var commerce = await _commerceRepository.GetById(id);
            if (commerce is null)
                return Result.Fail("Comercio no encontrado.");

            commerce.IsActive = status;

            var updated = await _commerceRepository.UpdateAsync(id, commerce);
            if (updated is null)
                return Result.Fail("No se pudo cambiar el estado del comercio.");

            return Result.Ok();
        }
    }
}

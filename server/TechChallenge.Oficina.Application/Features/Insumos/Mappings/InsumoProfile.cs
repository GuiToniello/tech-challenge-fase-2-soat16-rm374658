using AutoMapper;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Domain.Features.Insumos;

namespace TechChallenge.Oficina.Application.Features.Insumos.Mappings;

public sealed class InsumoProfile : Profile
{
    public InsumoProfile()
    {
        CreateMap<Insumo, InsumoViewModel>();
    }
}

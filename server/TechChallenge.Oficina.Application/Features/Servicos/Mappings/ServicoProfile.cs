using AutoMapper;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Domain.Features.Insumos;
using TechChallenge.Oficina.Domain.Features.Servicos;

namespace TechChallenge.Oficina.Application.Features.Servicos.Mappings;

public sealed class ServicoProfile : Profile
{
    public ServicoProfile()
    {
        CreateMap<Insumo, InsumoResumoViewModel>();
        CreateMap<Servico, ServicoViewModel>();
        CreateMap<ItemServico, ItemServicoViewModel>()
            .ForMember(dest => dest.InsumoNome, opt => opt.MapFrom(src => src.Insumo.Nome));
    }
}

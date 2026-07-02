using AutoMapper;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Domain.Features.Veiculos;

namespace TechChallenge.Oficina.Application.Features.Veiculos.Mappings;

public sealed class VeiculoProfile : Profile
{
    public VeiculoProfile()
    {
        CreateMap<Veiculo, VeiculoViewModel>()
            .ForMember(destino => destino.Placa, origem => origem.MapFrom(veiculo => veiculo.Placa.Valor));
    }
}

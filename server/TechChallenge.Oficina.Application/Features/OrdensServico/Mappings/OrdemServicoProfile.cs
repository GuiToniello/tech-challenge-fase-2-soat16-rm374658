using AutoMapper;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Domain.Features.Orcamentos;
using TechChallenge.Oficina.Domain.Features.OrdensServico;
using TechChallenge.Oficina.Domain.Features.OrdensServico.Enums;
using TechChallenge.Oficina.Domain.Features.OrdensServico.VOs;
using TechChallenge.Oficina.Domain.Features.Servicos;

namespace TechChallenge.Oficina.Application.Features.OrdensServico.Mappings;

public sealed class OrdemServicoProfile : Profile
{
    public OrdemServicoProfile()
    {
        CreateMap<Servico, ServicoResumoOrdemServicoViewModel>();
        CreateMap<HistoricoStatusOrdemServico, HistoricoStatusOrdemServicoViewModel>();
        CreateMap<OrcamentoServico, OrcamentoServicoViewModel>();
        CreateMap<Orcamento, OrcamentoViewModel>();

        CreateMap<OrdemServico, AcompanhamentoOrdemServicoViewModel>()
            .ForMember(destino => destino.Status, configuracao => configuracao.MapFrom(origem => (int)origem.Status))
            .ForMember(destino => destino.StatusDescricao, configuracao => configuracao.MapFrom(origem => origem.Status.ObterDescricao()))
            .ForMember(destino => destino.HistoricoStatus, configuracao => configuracao.MapFrom(origem => origem.HistoricoStatus));

        CreateMap<OrdemServico, OrdemServicoViewModel>()
            .ForMember(destino => destino.Status, configuracao => configuracao.MapFrom(origem => (int)origem.Status))
            .ForMember(destino => destino.StatusDescricao, configuracao => configuracao.MapFrom(origem => origem.Status.ObterDescricao()));
    }
}

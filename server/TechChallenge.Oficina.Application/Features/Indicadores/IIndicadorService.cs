using TechChallenge.Oficina.Application.Features.Indicadores.Queries;
using TechChallenge.Oficina.Application.Features.Indicadores.ViewModels;

namespace TechChallenge.Oficina.Application.Features.Indicadores;

public interface IIndicadorService : TechChallenge.Oficina.Domain.Features.Indicadores.IIndicadorService<ObterIndicadoresQuery, IndicadorViewModel>
{
}

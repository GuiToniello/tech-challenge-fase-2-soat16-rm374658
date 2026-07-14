using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;

namespace TechChallenge.Oficina.API.Features.OrdensServico
{
    public static class OrdensServicoExtensions
    {
        public static RouteGroupBuilder MapOrdensServicoEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/ordens-servico")
                .WithTags("OrdensServico");

            group.MapPost(
                string.Empty,
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    CriarOrdemServicoCommand command,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.Post(command, cancellationToken))
                .Produces<OrdemServicoViewModel>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                "/{id:guid}",
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.GetById(id, cancellationToken))
                .WithName("GetOrdemServicoById")
                .Produces<OrdemServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                string.Empty,
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.Get(cancellationToken))
                .Produces<IReadOnlyCollection<OrdemServicoViewModel>>(StatusCodes.Status200OK);

            group.MapGet(
                "/{id:guid}/acompanhamento",
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.GetAcompanhamento(id, cancellationToken))
                .Produces<AcompanhamentoOrdemServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                "/cliente/{clienteId:guid}",
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    Guid clienteId,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.GetByCliente(clienteId, cancellationToken))
                .Produces<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPut(
                string.Empty,
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    AtualizarOrdemServicoCommand command,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.Put(command, cancellationToken))
                .Produces<OrdemServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete(
                "/{id:guid}",
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.Delete(id, cancellationToken))
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost(
                "/{id:guid}/em-diagnostico",
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.AlterarParaEmDiagnostico(id, cancellationToken))
                .Produces<OrdemServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost(
                "/{id:guid}/em-execucao",
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.AlterarParaEmExecucao(id, cancellationToken))
                .Produces<OrdemServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost(
                "/{id:guid}/finalizar",
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.AlterarParaFinalizada(id, cancellationToken))
                .Produces<OrdemServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost(
                "/{id:guid}/entregar",
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.AlterarParaEntregue(id, cancellationToken))
                .Produces<OrdemServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost(
                "/{id:guid}/gerar-orcamento",
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.GerarOrcamento(id, cancellationToken))
                .Produces<OrdemServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost(
                "/{id:guid}/enviar-orcamento",
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.EnviarOrcamento(id, cancellationToken))
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost(
                "/{id:guid}/aprovar-orcamento",
                (
                    OrdensServicoEndpoints ordensServicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => ordensServicoEndpoints.AprovarOrcamento(id, cancellationToken))
                .Produces<OrdemServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        public static void RegisterOrdensServicoEndpoints(this IServiceCollection services)
        {
            services.AddScoped<OrdensServicoEndpoints>();
        }
    }
}

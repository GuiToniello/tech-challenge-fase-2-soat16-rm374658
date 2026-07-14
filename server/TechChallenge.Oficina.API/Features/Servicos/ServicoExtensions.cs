using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;

namespace TechChallenge.Oficina.API.Features.Servicos
{
    public static class ServicoExtensions
    {
        public static RouteGroupBuilder MapServicoEndpoints(
            this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/servicos")
                .WithTags("Servicos");

            group.MapPost(
                string.Empty,
                (
                    ServicoEndpoints servicoEndpoints,
                    CriarServicoCommand command,
                    CancellationToken cancellationToken
                ) => servicoEndpoints.Post(command, cancellationToken))
                .Produces<ServicoViewModel>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                "/{id:guid}",
                (
                    ServicoEndpoints servicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => servicoEndpoints.GetById(id, cancellationToken))
                .WithName("GetServicoById")
                .Produces<ServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                string.Empty,
                (
                    ServicoEndpoints servicoEndpoints,
                    CancellationToken cancellationToken
                ) => servicoEndpoints.Get(cancellationToken))
                .Produces<IReadOnlyCollection<ServicoViewModel>>(StatusCodes.Status200OK);

            group.MapPut(
                string.Empty,
                (
                    ServicoEndpoints servicoEndpoints,
                    AtualizarServicoCommand command,
                    CancellationToken cancellationToken
                ) => servicoEndpoints.Put(command, cancellationToken))
                .Produces<ServicoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete(
                "/{id:guid}",
                (
                    ServicoEndpoints servicoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => servicoEndpoints.Delete(id, cancellationToken))
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        public static void RegisterServicoEndpoints(this IServiceCollection services)
        {
            services.AddScoped<ServicoEndpoints>();
        }
    }
}

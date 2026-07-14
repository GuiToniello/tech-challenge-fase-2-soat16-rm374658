using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;

namespace TechChallenge.Oficina.API.Features.Insumos
{
    public static class InsumoExtensions
    {
        public static RouteGroupBuilder MapInsumoEndpoints(
            this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/insumos")
                .WithTags("Insumos");

            group.MapPost(
                string.Empty,
                (
                    InsumoEndpoints insumoEndpoints,
                    CriarInsumoCommand command,
                    CancellationToken cancellationToken
                ) => insumoEndpoints.Post(command, cancellationToken))
                .Produces<InsumoViewModel>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet(
                "/{id:guid}",
                (
                    InsumoEndpoints insumoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => insumoEndpoints.GetById(id, cancellationToken))
                .WithName("GetInsumoById")
                .Produces<InsumoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                string.Empty,
                (
                    InsumoEndpoints insumoEndpoints,
                    CancellationToken cancellationToken
                ) => insumoEndpoints.Get(cancellationToken))
                .Produces<IReadOnlyCollection<InsumoViewModel>>(StatusCodes.Status200OK);

            group.MapPut(
                string.Empty,
                (
                    InsumoEndpoints insumoEndpoints,
                    AtualizarInsumoCommand command,
                    CancellationToken cancellationToken
                ) => insumoEndpoints.Put(command, cancellationToken))
                .Produces<InsumoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete(
                "/{id:guid}",
                (
                    InsumoEndpoints insumoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => insumoEndpoints.Delete(id, cancellationToken))
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        public static void RegisterInsumoEndpoints(this IServiceCollection services)
        {
            services.AddScoped<InsumoEndpoints>();
        }
    }
}

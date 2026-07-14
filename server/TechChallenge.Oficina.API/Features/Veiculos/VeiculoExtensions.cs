using TechChallenge.Oficina.Application.Features.Veiculos.Commands;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;

namespace TechChallenge.Oficina.API.Features.Veiculos
{
    public static class VeiculoExtensions
    {
        public static RouteGroupBuilder MapVeiculoEndpoints(
            this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/veiculos")
                .WithTags("Veiculos");

            group.MapPost(
                string.Empty,
                (
                    VeiculoEndpoints veiculoEndpoints,
                    CriarVeiculoCommand command,
                    CancellationToken cancellationToken
                ) => veiculoEndpoints.Post(command, cancellationToken))
                .Produces<VeiculoViewModel>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                "/{id:guid}",
                (
                    VeiculoEndpoints veiculoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => veiculoEndpoints.GetById(id, cancellationToken))
                .WithName("GetVeiculoById")
                .Produces<VeiculoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                string.Empty,
                (
                    VeiculoEndpoints veiculoEndpoints,
                    Guid? clienteId,
                    CancellationToken cancellationToken
                ) => veiculoEndpoints.Get(clienteId, cancellationToken))
                .Produces<IReadOnlyCollection<VeiculoViewModel>>(StatusCodes.Status200OK);

            group.MapPut(
                string.Empty,
                (
                    VeiculoEndpoints veiculoEndpoints,
                    AtualizarVeiculoCommand command,
                    CancellationToken cancellationToken
                ) => veiculoEndpoints.Put(command, cancellationToken))
                .Produces<VeiculoViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete(
                "/{id:guid}",
                (
                    VeiculoEndpoints veiculoEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => veiculoEndpoints.Delete(id, cancellationToken))
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        public static void RegisterVeiculoEndpoints(this IServiceCollection services)
        {
            services.AddScoped<VeiculoEndpoints>();
        }
    }
}

using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;

namespace TechChallenge.Oficina.API.Features.Clientes
{
    public static class ClienteExtensions
    {
        public static RouteGroupBuilder MapClienteEndpoints(
             this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/clientes")
                .WithTags("Clientes");

            group.MapPost(
                string.Empty,
                (
                    ClientEndpoints clientEndpoints,
                    CriarClienteCommand command,
                    CancellationToken cancellationToken
                ) => clientEndpoints.Post(command, cancellationToken))
                .Produces<ClienteViewModel>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            group.MapGet(
                "/{id:guid}",
                (
                    ClientEndpoints clientEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => clientEndpoints.GetById(id, cancellationToken))
                .WithName("GetClienteById")
                .Produces<ClienteViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet(
                string.Empty,
                (
                    ClientEndpoints clientEndpoints,
                    CancellationToken cancellationToken
                ) => clientEndpoints.Get(cancellationToken))
                .Produces<IReadOnlyCollection<ClienteViewModel>>(StatusCodes.Status200OK);

            group.MapPut(
                string.Empty,
                (
                    ClientEndpoints clientEndpoints,
                    AtualizarClienteCommand command,
                    CancellationToken cancellationToken
                ) => clientEndpoints.Put(command, cancellationToken))
                .Produces<ClienteViewModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete(
                "/{id:guid}",
                (
                    ClientEndpoints clientEndpoints,
                    Guid id,
                    CancellationToken cancellationToken
                ) => clientEndpoints.Delete(id, cancellationToken))
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }

        public static void RegisterClienteEndpoints(this IServiceCollection services)
        {
            services.AddScoped<ClientEndpoints>();
        }
    }
}

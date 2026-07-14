using TechChallenge.Oficina.Application.Features.Indicadores.ViewModels;

namespace TechChallenge.Oficina.API.Features.Indicadores
{
    public static class IndicadoresExtensions
    {
        public static RouteGroupBuilder MapIndicadoresEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api/indicadores")
                .WithTags("Indicadores");

            group.MapGet(
                string.Empty,
                (
                    IndicadoresEndpoints indicadoresEndpoints,
                    CancellationToken cancellationToken
                ) => indicadoresEndpoints.Get(cancellationToken))
                .Produces<IndicadorViewModel>(StatusCodes.Status200OK);

            return group;
        }

        public static void RegisterIndicadoresEndpoints(this IServiceCollection services)
        {
            services.AddScoped<IndicadoresEndpoints>();
        }
    }
}

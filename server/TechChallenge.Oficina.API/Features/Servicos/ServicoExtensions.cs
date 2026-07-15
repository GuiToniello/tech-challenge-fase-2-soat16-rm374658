namespace TechChallenge.Oficina.API.Features.Servicos
{
    public static class ServicoExtensions
    {
        public static void RegisterServicoEndpoints(this IServiceCollection services)
        {
            services.AddScoped<ServicoAdapter>();
        }
    }
}

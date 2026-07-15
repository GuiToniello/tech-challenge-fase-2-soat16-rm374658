namespace TechChallenge.Oficina.API.Features.Veiculos
{
    public static class VeiculoExtensions
    {
        public static void RegisterVeiculoEndpoints(this IServiceCollection services)
        {
            services.AddScoped<VeiculoAdapter>();
        }
    }
}

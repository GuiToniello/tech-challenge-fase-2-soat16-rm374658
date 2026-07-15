using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Clientes;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.API.Features.Clientes
{
    public class ClienteAdapter : IClientAdapter
    {
        public object Adapt(ClienteResult<ClienteViewModel, Exception> result)
        {
            if(result.Value != null)
                return TypedResults.CreatedAtRoute(result.Value);

            return CriaErro(result.Error!);
        }

        public object Adapt(ClienteResult<bool, Exception> result)
        {
            if (result.Value)
                return TypedResults.Ok(result.Value);

            return CriaErro(result.Error!);
        }

        public object CriaErro(Exception ex)
        {
            if (ex is DomainException)
                return TypedResults.BadRequest(ex.Message);

            if (ex is KeyNotFoundException)
                return TypedResults.NotFound(ex.Message);

            return TypedResults.Problem(ex?.Message);
        }
    }
}

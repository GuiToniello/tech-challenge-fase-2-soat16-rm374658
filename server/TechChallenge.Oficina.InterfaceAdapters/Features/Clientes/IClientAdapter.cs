using System;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;

namespace TechChallenge.Oficina.Controllers.Features.Clientes
{
    public interface IClientAdapter
    {
        object Adapt(ClienteResult<ClienteViewModel, Exception> result);

        object Adapt(ClienteResult<bool, Exception> result);
    }
}

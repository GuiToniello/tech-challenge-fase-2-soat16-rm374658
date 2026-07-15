using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.Queries;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.Entities.Features.Clientes.VOs;

namespace TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;

public sealed class ClienteUseCases : IClienteUseCases
{
    private readonly IMapper _mapper;
    private readonly IClienteRepository _clienteRepository;

    public ClienteUseCases(IMapper mapper, IClienteRepository clienteRepository)
    {
        _mapper = mapper;
        _clienteRepository = clienteRepository;
    }

    public async Task<ClienteViewModel> CriarAsync(CriarClienteCommand command, CancellationToken cancellationToken = default)
    {
        var identificacao = IdentificacaoCliente.Criar(command.Identificacao);

        await ValidarDuplicidadeAsync(identificacao.Valor, null, cancellationToken);

        var cliente = Cliente.Criar(command.NomeCompleto, identificacao, command.Email);

        await _clienteRepository.AdicionarAsync(cliente, cancellationToken);

        return _mapper.Map<ClienteViewModel>(cliente);
    }

    public async Task<ClienteViewModel> AtualizarAsync(AtualizarClienteCommand command, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterClienteExistenteAsync(command.Id, cancellationToken);
        var identificacao = IdentificacaoCliente.Criar(command.Identificacao);

        await ValidarDuplicidadeAsync(identificacao.Valor, cliente.Id, cancellationToken);

        cliente.AtualizarNomeCompleto(command.NomeCompleto);
        cliente.AtualizarIdentificacao(identificacao);
        cliente.AtualizarEmail(command.Email);

        await _clienteRepository.AtualizarAsync(cliente, cancellationToken);

        return _mapper.Map<ClienteViewModel>(cliente);
    }

    public async Task<ClienteViewModel> ObterPorIdAsync(ObterClientePorIdQuery query, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterClienteExistenteAsync(query.Id, cancellationToken);
        return _mapper.Map<ClienteViewModel>(cliente);
    }

    public async Task<IReadOnlyCollection<ClienteViewModel>> ListarAsync(ListarClientesQuery query, CancellationToken cancellationToken = default)
    {
        var clientes = await _clienteRepository.ListarAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<ClienteViewModel>>(clientes);
    }

    public async Task ExcluirAsync(ExcluirClienteCommand command, CancellationToken cancellationToken = default)
    {
        var cliente = await ObterClienteExistenteAsync(command.Id, cancellationToken);
        await _clienteRepository.RemoverAsync(cliente, cancellationToken);
    }

    private async Task<Cliente> ObterClienteExistenteAsync(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id, cancellationToken);

        if (cliente is null)
        {
            throw new KeyNotFoundException("Cliente não encontrado.");
        }

        return cliente;
    }

    private async Task ValidarDuplicidadeAsync(string identificacao, Guid? clienteId, CancellationToken cancellationToken)
    {
        if (await _clienteRepository.ExisteComIdentificacaoAsync(identificacao, clienteId, cancellationToken))
        {
            throw new DomainException("Já existe um cliente cadastrado com a identificação informada.");
        }
    }
}

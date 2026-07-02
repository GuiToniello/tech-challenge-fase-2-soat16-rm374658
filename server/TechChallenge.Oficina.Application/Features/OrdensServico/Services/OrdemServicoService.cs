using AutoMapper;
using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Application.Features.OrdensServico.Queries;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;
using TechChallenge.Oficina.Domain.Features.Clientes;
using TechChallenge.Oficina.Domain.Features.OrdensServico;
using TechChallenge.Oficina.Domain.Features.Servicos;
using TechChallenge.Oficina.Domain.Features.Veiculos;

namespace TechChallenge.Oficina.Application.Features.OrdensServico.Services;

public sealed class OrdemServicoService : IOrdemServicoService
{
    private readonly IMapper _mapper;
    private readonly IOrdemServicoRepository _ordemServicoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IOrdemServicoServicesFacade _ordemServicoServicesFacade;

    public OrdemServicoService(IMapper mapper, IOrdemServicoRepository ordemServicoRepository, IClienteRepository clienteRepository, IVeiculoRepository veiculoRepository, IServicoRepository servicoRepository, IOrdemServicoServicesFacade ordemServicoServicesFacade)
    {
        _mapper = mapper;
        _ordemServicoRepository = ordemServicoRepository;
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _servicoRepository = servicoRepository;
        _ordemServicoServicesFacade = ordemServicoServicesFacade;
    }

    public async Task<OrdemServicoViewModel> CriarAsync(CriarOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        await ValidarClienteExistenteAsync(command.ClienteId, cancellationToken);
        var veiculo = await ObterVeiculoExistenteAsync(command.VeiculoId, cancellationToken);
        ValidarVeiculoPertenceAoCliente(veiculo, command.ClienteId);
        var servicos = await ObterServicosAsync(command.ServicoIds, cancellationToken);

        var ordemServico = OrdemServico.Criar(command.ClienteId, command.VeiculoId, servicos);

        await _ordemServicoRepository.AdicionarAsync(ordemServico, cancellationToken);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> AtualizarAsync(AtualizarOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);

        await ValidarClienteExistenteAsync(command.ClienteId, cancellationToken);
        var veiculo = await ObterVeiculoExistenteAsync(command.VeiculoId, cancellationToken);
        ValidarVeiculoPertenceAoCliente(veiculo, command.ClienteId);
        var servicos = await ObterServicosAsync(command.ServicoIds, cancellationToken);

        ordemServico.AtualizarClienteId(command.ClienteId);
        ordemServico.AtualizarVeiculoId(command.VeiculoId);
        ordemServico.DefinirServicos(servicos);

        await _ordemServicoRepository.AtualizarAsync(ordemServico, cancellationToken);

        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> ObterPorIdAsync(ObterOrdemServicoPorIdQuery query, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(query.Id, cancellationToken);
        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<IReadOnlyCollection<OrdemServicoViewModel>> ListarAsync(ListarOrdensServicoQuery query, CancellationToken cancellationToken = default)
    {
        var ordensServico = await _ordemServicoRepository.ListarAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<OrdemServicoViewModel>>(ordensServico);
    }

    public async Task ExcluirAsync(ExcluirOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        await _ordemServicoRepository.RemoverAsync(ordemServico, cancellationToken);
    }

    public async Task<AcompanhamentoOrdemServicoViewModel> ObterAcompanhamentoAsync(ObterAcompanhamentoOrdemServicoPorIdQuery query, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(query.Id, cancellationToken);
        return _mapper.Map<AcompanhamentoOrdemServicoViewModel>(ordemServico);
    }

    public async Task<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>> ListarPorClienteAsync(ListarOrdensServicoPorClienteQuery query, CancellationToken cancellationToken = default)
    {
        await ValidarClienteExistenteAsync(query.ClienteId, cancellationToken);

        var ordensServico = await _ordemServicoRepository.ListarPorClienteAsync(query.ClienteId, cancellationToken);
        return _mapper.Map<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>>(ordensServico);
    }

    public async Task<OrdemServicoViewModel> AlterarStatusParaEmDiagnosticoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        ordemServico.AlterarParaEmDiagnostico();
        await _ordemServicoRepository.AtualizarAsync(ordemServico, cancellationToken);
        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> GerarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        await _ordemServicoServicesFacade.EstoqueService.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, cancellationToken);
        ordemServico.GerarOrcamento();
        await _ordemServicoRepository.AtualizarAsync(ordemServico, cancellationToken);
        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task EnviarOrcamentoPorEmailAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);

        if (ordemServico.Orcamento is null)
        {
            throw new DomainException("A ordem de servico informada nao possui orcamento gerado.");
        }

        var cliente = await _clienteRepository.ObterPorIdAsync(ordemServico.ClienteId, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");

        if (string.IsNullOrWhiteSpace(cliente.Email))
        {
            throw new DomainException("O cliente da ordem de servico nao possui email cadastrado.");
        }

        await _ordemServicoServicesFacade.OrcamentoEmailSender.EnviarOrcamentoAsync(ordemServico, cliente.Email, cancellationToken);
    }

    public async Task<OrdemServicoViewModel> AprovarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        await _ordemServicoServicesFacade.EstoqueService.VerificarDisponibilidadeParaOrcamentoAsync(ordemServico.Servicos, cancellationToken);
        await _ordemServicoServicesFacade.EstoqueService.DebitarEstoqueParaOrdemServicoAsync(ordemServico.Servicos, cancellationToken);
        ordemServico.AprovarOrcamento();
        await _ordemServicoRepository.AtualizarAsync(ordemServico, cancellationToken);
        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> AlterarStatusParaEmExecucaoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        ordemServico.AlterarParaEmExecucao();
        await _ordemServicoRepository.AtualizarAsync(ordemServico, cancellationToken);
        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> AlterarStatusParaFinalizadaAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        ordemServico.AlterarParaFinalizada();
        await _ordemServicoRepository.AtualizarAsync(ordemServico, cancellationToken);
        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    public async Task<OrdemServicoViewModel> AlterarStatusParaEntregueAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default)
    {
        var ordemServico = await ObterOrdemServicoExistenteAsync(command.Id, cancellationToken);
        ordemServico.AlterarParaEntregue();
        await _ordemServicoRepository.AtualizarAsync(ordemServico, cancellationToken);
        await _ordemServicoServicesFacade.IndicadorService.AtualizarAsync(cancellationToken);
        return _mapper.Map<OrdemServicoViewModel>(ordemServico);
    }

    private async Task<OrdemServico> ObterOrdemServicoExistenteAsync(Guid id, CancellationToken cancellationToken)
    {
        var ordemServico = await _ordemServicoRepository.ObterPorIdAsync(id, cancellationToken);

        if (ordemServico is null)
        {
            throw new KeyNotFoundException("Ordem de servico nao encontrada.");
        }

        return ordemServico;
    }

    private async Task ValidarClienteExistenteAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(clienteId, cancellationToken);

        if (cliente is null)
        {
            throw new KeyNotFoundException("Cliente não encontrado.");
        }
    }

    private async Task<Veiculo> ObterVeiculoExistenteAsync(Guid veiculoId, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.ObterPorIdAsync(veiculoId, cancellationToken);

        if (veiculo is null)
        {
            throw new KeyNotFoundException("Veículo não encontrado.");
        }

        return veiculo;
    }

    private static void ValidarVeiculoPertenceAoCliente(Veiculo veiculo, Guid clienteId)
    {
        if (veiculo.ClienteId != clienteId)
        {
            throw new DomainException("O veiculo informado deve estar vinculado ao cliente da ordem de servico.");
        }
    }

    private async Task<IReadOnlyCollection<Servico>> ObterServicosAsync(IReadOnlyCollection<Guid> servicoIds, CancellationToken cancellationToken)
    {
        if (servicoIds is null || servicoIds.Count == 0)
        {
            throw new DomainException("A ordem de servico deve possuir ao menos um servico.");
        }

        if (servicoIds.Any(id => id == Guid.Empty))
        {
            throw new DomainException("Todos os servicos informados devem possuir identificador valido.");
        }

        var ids = servicoIds.Distinct().ToArray();
        var servicos = new List<Servico>(ids.Length);

        foreach (var servicoId in ids)
        {
            var servico = await _servicoRepository.ObterPorIdAsync(servicoId, cancellationToken);

            if (servico is null)
            {
                throw new KeyNotFoundException("Servico nao encontrado.");
            }

            servicos.Add(servico);
        }

        return servicos;
    }
}

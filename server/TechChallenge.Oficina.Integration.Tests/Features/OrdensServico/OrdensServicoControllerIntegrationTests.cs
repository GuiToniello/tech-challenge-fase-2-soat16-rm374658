using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Application.Features.Veiculos.Commands;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;
using Xunit;

namespace TechChallenge.Oficina.Integration.Tests.Features.OrdensServico;

public sealed class OrdensServicoControllerIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public OrdensServicoControllerIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    private async Task<Guid> CriarClienteComEmailAsync()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        var cpf = _clienteCpfIndex++ == 0 ? "529.982.247-25" : "123.456.789-09";
        var result = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await endpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente OS", Identificacao = cpf, Email = $"os{cpf[..3]}@email.com" },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private int _clienteCpfIndex;

    private async Task<Guid> CriarVeiculoAsync(Guid clienteId)
    {
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var result = (CreatedAtRoute<VeiculoViewModel>)(await endpoints.Post(
            new CriarVeiculoCommand { Placa = "ABC1D23", Marca = "Ford", Modelo = "Ka", Ano = 2022, Renavam = "12345678901", ClienteId = clienteId },
            CancellationToken.None)).Result!;
        return result.Value.Id;
    }

    private async Task<Guid> CriarInsumoAsync()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var result = (CreatedAtRoute<InsumoViewModel>)(await endpoints.Post(
            new CriarInsumoCommand { Nome = "Vela de Ignicao", Fabricante = "NGK", QuantidadeDisponivel = 20, ValorUnitario = 15m },
            CancellationToken.None)).Result!;
        return result.Value.Id;
    }

    private async Task<Guid> CriarServicoAsync(Guid insumoId)
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var result = (CreatedAtRoute<ServicoViewModel>)(await endpoints.Post(
            new CriarServicoCommand
            {
                Nome = "Troca de Vela",
                Descricao = "Substituicao das velas de ignicao",
                ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 4 }]
            },
            CancellationToken.None)).Result!;
        return result.Value.Id;
    }

    private async Task<(Guid clienteId, Guid veiculoId, Guid servicoId)> CriarContextoCompletoAsync()
    {
        var clienteId = await CriarClienteComEmailAsync();
        var veiculoId = await CriarVeiculoAsync(clienteId);
        var insumoId = await CriarInsumoAsync();
        var servicoId = await CriarServicoAsync(insumoId);
        return (clienteId, veiculoId, servicoId);
    }

    [Fact]
    public async Task Post_OrdemServicoValida_DeveRetornar201ComViewModel()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(resultado.Result);
        var viewModel = Assert.IsType<OrdemServicoViewModel>(created.Value);
        Assert.NotEqual(Guid.Empty, viewModel.Id);
        Assert.Equal(clienteId, viewModel.ClienteId);
        Assert.Equal(veiculoId, viewModel.VeiculoId);
    }

    [Fact]
    public async Task Post_ClienteInexistente_DeveRetornar404()
    {
        var (_, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new CriarOrdemServicoCommand { ClienteId = Guid.NewGuid(), VeiculoId = veiculoId, ServicoIds = [servicoId] };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
    }

    [Fact]
    public async Task Post_SemServicos_DeveRetornar400()
    {
        var (clienteId, veiculoId, _) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [] };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
    }

    [Fact]
    public async Task Post_VeiculoNaoPertenceAoCliente_DeveRetornar400()
    {
        var clientEndpoints = _fixture.CriarClientesEndpoints();
        var cliente1Result = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await clientEndpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente Dono", Identificacao = "529.982.247-25", Email = "dono@email.com" },
            CancellationToken.None));
        var clienteDono = cliente1Result.Value!.Id;

        var cliente2Result = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await clientEndpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente Ordem", Identificacao = "123.456.789-09", Email = "ordem@email.com" },
            CancellationToken.None));
        var clienteOrdem = cliente2Result.Value!.Id;

        var veiculoEndpoints = _fixture.CriarVeiculosEndpoints();
        var veiculoResult = (CreatedAtRoute<VeiculoViewModel>)(await veiculoEndpoints.Post(
            new CriarVeiculoCommand { Placa = "QQQ1Q11", Marca = "Fiat", Modelo = "Uno", Ano = 2020, Renavam = "00011122233", ClienteId = clienteDono },
            CancellationToken.None)).Result!;
        var veiculoDoCliente1 = veiculoResult.Value.Id;

        var insumoEndpoints = _fixture.CriarInsumosEndpoints();
        var insumoResult = (CreatedAtRoute<InsumoViewModel>)(await insumoEndpoints.Post(
            new CriarInsumoCommand { Nome = "Pastilha de Freio", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 40m },
            CancellationToken.None)).Result!;
        var insumoId = insumoResult.Value.Id;

        var servicoEndpoints = _fixture.CriarServicosEndpoints();
        var servicoResult = (CreatedAtRoute<ServicoViewModel>)(await servicoEndpoints.Post(
            new CriarServicoCommand { Nome = "Troca Freio", Descricao = "Troca pastilha de freio", ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 2 }] },
            CancellationToken.None)).Result!;
        var servicoId = servicoResult.Value.Id;

        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new CriarOrdemServicoCommand { ClienteId = clienteOrdem, VeiculoId = veiculoDoCliente1, ServicoIds = [servicoId] };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
    }

    [Fact]
    public async Task GetById_OrdemExistente_DeveRetornar200ComViewModel()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = (CreatedAtRoute<OrdemServicoViewModel>)(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Result!;
        var id = created.Value.Id;

        var resultado = await endpoints.GetById(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(resultado.Result);
        Assert.Equal(id, ok.Value.Id);
    }

    [Fact]
    public async Task GetById_OrdemInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        var resultado = await endpoints.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
    }

    [Fact]
    public async Task Get_ComOrdensServico_DeveRetornar200ComLista()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);

        var resultado = await endpoints.Get(CancellationToken.None);

        var lista = Assert.IsAssignableFrom<IEnumerable<OrdemServicoViewModel>>(resultado.Value);
        Assert.NotEmpty(lista);
    }

    [Fact]
    public async Task GetAcompanhamento_OrdemExistente_DeveRetornar200()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = (CreatedAtRoute<OrdemServicoViewModel>)(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Result!;
        var id = created.Value.Id;

        var resultado = await endpoints.GetAcompanhamento(id, CancellationToken.None);

        Assert.IsType<Ok<AcompanhamentoOrdemServicoViewModel>>(resultado.Result);
    }

    [Fact]
    public async Task GetByCliente_ClienteExistente_DeveRetornar200ComLista()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);

        var resultado = await endpoints.GetByCliente(clienteId, CancellationToken.None);

        var ok = Assert.IsType<Ok<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>>>(resultado.Result);
        Assert.NotEmpty(ok.Value);
    }

    [Fact]
    public async Task AlterarParaEmDiagnostico_OrdemRecebida_DeveRetornar200ComStatusAtualizado()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = (CreatedAtRoute<OrdemServicoViewModel>)(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Result!;
        var id = created.Value.Id;

        var resultado = await endpoints.AlterarParaEmDiagnostico(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(resultado.Result);
        Assert.Equal(2, ok.Value.Status);
    }

    [Fact]
    public async Task GerarOrcamento_OrdemEmDiagnostico_DeveRetornar200()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = (CreatedAtRoute<OrdemServicoViewModel>)(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Result!;
        var id = created.Value.Id;

        await endpoints.AlterarParaEmDiagnostico(id, CancellationToken.None);
        var resultado = await endpoints.GerarOrcamento(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(resultado.Result);
        Assert.NotNull(ok.Value.Orcamento);
    }

    [Fact]
    public async Task FluxoCompleto_DevePassarPorTodosOsStatus()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = (CreatedAtRoute<OrdemServicoViewModel>)(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Result!;
        var id = created.Value.Id;

        await endpoints.AlterarParaEmDiagnostico(id, CancellationToken.None);
        await endpoints.GerarOrcamento(id, CancellationToken.None);
        await endpoints.AprovarOrcamento(id, CancellationToken.None);
        await endpoints.AlterarParaEmExecucao(id, CancellationToken.None);
        await endpoints.AlterarParaFinalizada(id, CancellationToken.None);
        var entregue = await endpoints.AlterarParaEntregue(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(entregue.Result);
        Assert.Equal(6, ok.Value.Status);
    }

    [Fact]
    public async Task Delete_OrdemExistente_DeveRetornar204()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = (CreatedAtRoute<OrdemServicoViewModel>)(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Result!;
        var id = created.Value.Id;

        var resultado = await endpoints.Delete(id, CancellationToken.None);

        Assert.IsType<NoContent>(resultado.Result);
    }

    [Fact]
    public async Task Delete_OrdemInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarOrdensServicoEndpoints();

        var resultado = await endpoints.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
    }

    [Fact]
    public async Task Put_OrdemExistente_DeveRetornar200()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = (CreatedAtRoute<OrdemServicoViewModel>)(await endpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Result!;
        var id = created.Value.Id;
        var command = new AtualizarOrdemServicoCommand { Id = id, ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] };

        var resultado = await endpoints.Put(command, CancellationToken.None);

        Assert.IsType<Ok<OrdemServicoViewModel>>(resultado.Result);
    }

    [Fact]
    public async Task Put_OrdemInexistente_DeveRetornar404()
    {
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var endpoints = _fixture.CriarOrdensServicoEndpoints();
        var command = new AtualizarOrdemServicoCommand { Id = Guid.NewGuid(), ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] };

        var resultado = await endpoints.Put(command, CancellationToken.None);

        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
    }
}

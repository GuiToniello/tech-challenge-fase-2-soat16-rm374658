# ADR-006: Mapeamento com AutoMapper na Application (UseCases)

**Status**: Aceita

**Contexto**:
Necessidade de transformar entidades do domínio em DTOs de resposta, evitando que adaptadores de interface (controllers da arquitetura) façam mapeamento manual e mantendo lógica centralizada.

**Decisão**:
- AutoMapper é registrado apenas no projeto **Application (UseCases)** (não em Infra ou API).
- Profiles de mapeamento ficam em `Application/Features/{Feature}/Mappings/`.
- O `ClienteUseCases` (Application) faz o mapeamento antes de retornar ao adaptador de interface.
- Controllers da arquitetura nunca instanciam `IMapper`; sempre consomem ViewModels já mapeadas.
- Mapeamento complexo (ex: `Identificacao.Valor` para flat property) é feito no Profile.

**Consequências**:
- ✅ Adaptadores de interface sem lógica de transformação.
- ✅ Mapeamento centralizado e reutilizável.
- ✅ Fácil testar profiles isoladamente.
- ✅ API desacoplada de detalhes internos do domínio.

**Exemplo**:
```csharp
// ClienteProfile.cs (Application)
CreateMap<Cliente, ClienteViewModel>()
    .ForMember(destino => destino.Identificacao, 
               origem => origem.MapFrom(cliente => cliente.Identificacao.Valor))
    .ForMember(destino => destino.TipoIdentificacao, 
               origem => origem.MapFrom(cliente => cliente.Identificacao.Tipo.ToString()));
```

# ADR-006: Mapeamento com AutoMapper na Camada Application

**Status**: Aceita

**Contexto**:
Necessidade de transformar entidades do domínio em DTOs de resposta, evitando que controllers façam mapeamento manual e mantendo lógica centralizada.

**Decisão**:
- AutoMapper é registrado apenas na **Application** (não em Infra ou API).
- Profiles de mapeamento ficam em `Application/Features/{Feature}/Mappings/`.
- O `ClienteService` (Application) faz o mapeamento antes de retornar ao controller.
- Controllers nunca instanciam `IMapper`; sempre consomem ViewModels já mapeadas.
- Mapeamento complexo (ex: `Identificacao.Valor` para flat property) é feito no Profile.

**Consequências**:
- ✅ Controllers sem lógica de transformação.
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

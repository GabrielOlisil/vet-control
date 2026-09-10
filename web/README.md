

# AGENT INSTRUCTION: Fix Backend Build and Refactor Application Layer (.NET)

O domínio (Models e EF Configurations) já foi atualizado e a entidade `CartaoVacina` foi removida.
O projeto `Api` atualmente não compila. Seu objetivo é refatorar exclusivamente DTOs, Mappers, Services e Controllers na pasta `Api/` até que `dotnet build Api/Api.csproj` passe com 0 erros e 0 warnings.
Ignore completamente a pasta `./web`.

## 1. DTOs (`Api/DTOs/`)

### 1.1 Exclusão
- Excluir o arquivo e diretório `Api/DTOs/CartoesVacina/CartaoVacinaDtos.cs`.

### 1.2 `Api/DTOs/Especies/EspecieDtos.cs`
- Adicionar `PorteAnimal PortePadrao` e `string IconeKey` nos DTOs:
  - `EspecieCreateDto(string Nome, string? NomeCientifico, PorteAnimal PortePadrao, string IconeKey)`
  - `EspecieUpdateDto(string Nome, string? NomeCientifico, PorteAnimal PortePadrao, string IconeKey)`
  - `EspecieDetailResponseDto`: expor `PorteAnimal PortePadrao` e `string IconeKey`.

### 1.3 `Api/DTOs/Animals/AnimalDtos.cs`
- Criar DTOs para identificadores:
  ```csharp
  public record IdentificadorCreateDto(TipoIdentificador Tipo, string Valor, bool EhPrincipal = false);
  public record IdentificadorResponseDto(Guid Id, TipoIdentificador Tipo, string Valor, bool EhPrincipal);

```

* Atualizar `AnimalCreateDto`:
* `string? Name` (opcional)
* `DateOnly DataNascimento`
* `bool DataNascimentoAproximada`
* `Guid? RacaId`
* `SexoAnimal Sexo`
* `OrigemAnimal Origem`
* `string? LoteOuPasto`
* `List<IdentificadorCreateDto> Identificadores` (ao menos 1 obrigatório)
* Remover `CartaoVacinaId` e `PictureUpload`.


* Atualizar `AnimalUpdateDto`:
* `string? Name`
* `DateOnly DataNascimento`
* `bool DataNascimentoAproximada`
* `Guid? RacaId`
* `SexoAnimal Sexo`
* `OrigemAnimal Origem`
* `string? LoteOuPasto`
* `List<IdentificadorCreateDto>? Identificadores`
* Remover `CartaoVacinaId` e `PictureUpload`.


* Atualizar `AnimalDetailResponseDto`:
* Incluir `string? Name`, `SexoAnimal Sexo`, `OrigemAnimal Origem`, `string? LoteOuPasto`, `bool DataNascimentoAproximada`.
* Incluir `List<IdentificadorResponseDto> Identificadores`.
* Incluir `IdentificadorResponseDto? IdentificadorPrincipal`.
* Expor `RacaDetailResponseDto? Raca` (que já traz a Espécie com `IconeKey`).
* Remover `CartaoVacinaDetailResponseDto? CartaoVacina` e `PictureUpload`.



### 1.4 `Api/DTOs/AplicacoesVacina/AplicacaoVacinaDtos.cs`

* Atualizar `AplicacaoVacinaCreateDto`:
* `Guid AnimalId` (obrigatório, substitui `CartaoVacinaId`)
* `Guid VacinaId`
* `DateOnly DataAplicacao`
* `DateOnly? DataProximaDose` (opcional; se nulo, calculado via `Vacina.ReaplicarEmXDias`)
* `string NumeroLote`
* `string? LaboratorioFabricante`
* `decimal? DoseMl`
* `string VeterinarioResponsavel`
* `string? Aplicador`
* `string? Observacoes`


* Atualizar `AplicacaoVacinaUpdateDto`:
* Manter os mesmos campos clínicos de edição (exceto `AnimalId` e `VacinaId`).


* Atualizar `AplicacaoVacinaDetailResponseDto`:
* Conter todos os novos campos clínicos: `AnimalId`, `DataProximaDose`, `NumeroLote`, `LaboratorioFabricante`, `DoseMl`, `VeterinarioResponsavel`, `Aplicador`, `Observacoes`, `StatusComprovanteVacina StatusComprovante`, `bool TemComprovanteAnexo`.
* Remover qualquer menção a `CartaoVacinaId`.



---

## 2. Mappers (`Api/Mappers/`)

### 2.1 `Api/Mappers/EspecieMapper.cs`

* Mapear `PortePadrao` e `IconeKey` nas conversões de `CreateDto`, `UpdateDto` e `ToDetailResponseDto`.

### 2.2 `Api/Mappers/AnimalMapper.cs`

* Remover mapeamento de `CartaoVacina` e `PictureUpload`.
* No mapeamento para entidade (`ToEntity`), mapear `Sexo`, `Origem`, `LoteOuPasto`, `DataNascimentoAproximada` e converter a lista de `IdentificadorCreateDto` para instâncias de `IdentificadorAnimal`.
* Se nenhum identificador vier marcado como `EhPrincipal = true`, marcar o primeiro como principal por padrão.
* No `ToDetailResponseDto`, mapear a coleção de `Identificadores` e resolver `IdentificadorPrincipal = Identificadores.FirstOrDefault(i => i.EhPrincipal) ?? Identificadores.FirstOrDefault()`.

### 2.3 `Api/Mappers/AplicacaoVacinaMapper.cs`

* Atualizar `ToEntity` para associar `AnimalId` e os campos clínicos (`DataProximaDose`, `NumeroLote`, `VeterinarioResponsavel`, etc.).
* Atualizar `ToDetailResponseDto` para incluir os novos campos e a flag `TemComprovanteAnexo = !string.IsNullOrEmpty(entity.ComprovanteDocumentoPath)`.

---

## 3. Services (`Api/Services/`)

### 3.1 `Api/Services/Animals/AnimalService.cs` e `IAnimalService.cs`

* **Criação (`CreateAsync`)**:
* Validar se a lista de `Identificadores` contém pelo menos um item.
* Se nenhum tiver `EhPrincipal = true`, definir o primeiro como `true`.
* Adicionar ao `_context.Animals`.


* **Listagem e Busca (`ListAsync` / `SearchAsync`)**:
* Carregar `Include(a => a.Raca).ThenInclude(r => r.Especie)` e `Include(a => a.Identificadores)`.
* No filtro de busca textual, buscar por trigram/contains tanto em `a.Name` quanto em `a.Identificadores.Any(i => i.Valor.Contains(term))`.


* **Buscar por Id (`GetByIdAsync`)**:
* Fazer `.Include(a => a.Identificadores)`.


* **Prontuário (`GetProntuarioAsync`)**:
* Criar método na interface e service: `Task<AnimalProntuarioResponseDto?> GetProntuarioAsync(Guid animalId, CancellationToken ct)`.
* Retornar os detalhes do animal juntamente com a lista de suas `AplicacoesVacina` ordenadas por `DataAplicacao DESC`.



### 3.2 `Api/Services/AplicacoesVacina/AplicacaoVacinaService.cs` e `IAplicacaoVacinaService.cs`

* **Remover dependência de `CartaoVacina**`:
* Ao criar aplicação, validar se `AnimalId` existe no banco.
* Se `dto.DataProximaDose` não for informada, buscar a vacina (`Vacinas.FindAsync(dto.VacinaId)`) e calcular:
`dataProximaDose = dto.DataAplicacao.AddDays((int)vacina.ReaplicarEmXDias);`


* **Gestão de Comprovante**:
* Criar métodos na interface e service:
* `Task<bool> UploadComprovanteAsync(Guid aplicacaoId, Stream streamArquivo, string contentType, CancellationToken ct)`:
Salvar o PDF na pasta `Api/wwwroot/comprovantes/{aplicacaoId}.pdf`, salvar o caminho em `ComprovanteDocumentoPath`, atualizar `StatusComprovante = StatusComprovanteVacina.Assinado` e salvar no banco.
* `Task<(byte[] Bytes, string ContentType)?> GetComprovanteAsync(Guid aplicacaoId, CancellationToken ct)`:
Ler o arquivo do disco e retornar os bytes e o content-type.





---

## 4. Controllers (`Api/Controllers/`)

### 4.1 Exclusão

* Se existir `CartaoVacinaController.cs`, excluir imediatamente.

### 4.2 `Api/Controllers/AnimalController.cs`

* Adicionar endpoint:
```csharp
[HttpGet("{id:guid}/prontuario")]
public async Task<ActionResult<AnimalProntuarioResponseDto>> GetProntuario(Guid id, CancellationToken ct)

```


* Atualizar os métodos `Post` e `Put` para aceitar os novos `AnimalCreateDto` e `AnimalUpdateDto`.

### 4.3 `Api/Controllers/AplicacaoVacinaController.cs`

* Atualizar endpoints CRUD para os novos DTOs (sem referências a CartaoVacina).
* Adicionar endpoints para o comprovante:
```csharp
[HttpPost("{id:guid}/comprovante")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadComprovante(Guid id, IFormFile file, CancellationToken ct)
{
    if (file == null || file.Length == 0) return BadRequest("Arquivo inválido.");
    if (file.ContentType != "application/pdf") return BadRequest("Apenas arquivos PDF são aceitos.");
    using var stream = file.OpenReadStream();
    var ok = await _service.UploadComprovanteAsync(id, stream, file.ContentType, ct);
    return ok ? NoContent() : NotFound();
}

[HttpGet("{id:guid}/comprovante")]
public async Task<IActionResult> DownloadComprovante(Guid id, CancellationToken ct)
{
    var result = await _service.GetComprovanteAsync(id, ct);
    if (result == null) return NotFound();
    return File(result.Value.Bytes, result.Value.ContentType, $"comprovante-{id}.pdf");
}

```



---

## 5. Critério de Aceite e Validação

1. Execute no terminal: `dotnet build Api/Api.csproj`.
2. O build deve compilar com **0 Erros**.
3. Nenhuma classe deve importar ou referenciar `CartaoVacina`.

```

```
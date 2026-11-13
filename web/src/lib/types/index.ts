/**
 * Centralização dos tipos gerados a partir do contrato OpenAPI (./contract.d.ts)
 */
export type * from './contract';
import type { components, paths } from './contract';

// Schemas OpenAPI
export type Schemas = components['schemas'];

export type PorteAnimal = Schemas['PorteAnimal'];
export type SexoAnimal = Schemas['SexoAnimal'];
export type OrigemAnimal = Schemas['OrigemAnimal'];
export type StatusComprovanteVacina = Schemas['StatusComprovanteVacina'];
export type TipoIdentificador = Schemas['TipoIdentificador'];

// ── Query Params ─────────────────────────────────────────────────────────────
export type AnimalQueryParams = NonNullable<paths['/api/v1/animais']['get']['parameters']['query']>;
export type AnimalCountQueryParams = NonNullable<paths['/api/v1/animais/count']['get']['parameters']['query']>;
export type VacinaQueryParams = NonNullable<paths['/api/v1/vacinas']['get']['parameters']['query']>;
export type VacinaCountQueryParams = NonNullable<paths['/api/v1/vacinas/count']['get']['parameters']['query']>;
export type RacaQueryParams = NonNullable<paths['/api/v1/racas']['get']['parameters']['query']>;
export type RacaCountQueryParams = NonNullable<paths['/api/v1/racas/count']['get']['parameters']['query']>;
export type EspecieQueryParams = NonNullable<paths['/api/v1/especies']['get']['parameters']['query']>;
export type EspecieCountQueryParams = NonNullable<paths['/api/v1/especies/count']['get']['parameters']['query']>;

// ── Animal ──────────────────────────────────────────────────────────────────
export type AnimalCreateDto = Schemas['AnimalCreateDto'];
export type AnimalDetailResponseDto = Schemas['AnimalDetailResponseDto'];
export type AnimalPatchDto = Schemas['AnimalPatchDto'];
export type AnimalShortResponseDto = Schemas['AnimalShortResponseDto'];
export type AnimalReadResponseDto = Schemas['AnimalReadResponseDto'];
export type AnimalProntuarioResponseDto = Schemas['AnimalProntuarioResponseDto'];

// ── Identificador ────────────────────────────────────────────────────────────
export type IdentificadorCreateDto = Schemas['IdentificadorCreateDto'];
export type IdentificadorResponseDto = Schemas['IdentificadorResponseDto'];

// ── Aplicacao Vacina ─────────────────────────────────────────────────────────
export type AplicacaoVacinaCreateDto = Schemas['AplicacaoVacinaCreateDto'] & {
    animalProtocoloVacinalId?: string;
    protocoloVacinalDoseId?: string;
};
export type AplicacaoVacinaDetailResponseDto = Schemas['AplicacaoVacinaDetailResponseDto'] & {
    animalProtocoloVacinalId?: string | null;
    protocoloVacinalDoseId?: string | null;
};
export type AplicacaoVacinaPatchDto = Schemas['AplicacaoVacinaPatchDto'] & {
    animalProtocoloVacinalId?: string;
    protocoloVacinalDoseId?: string;
};
export type AplicacaoVacinaReadResponseDto = Schemas['AplicacaoVacinaReadResponseDto'] & {
    animalProtocoloVacinalId?: string | null;
    protocoloVacinalDoseId?: string | null;
};
export type AplicacaoVacinaLoteCreateDto = Schemas['AplicacaoVacinaLoteCreateDto'];
export type AplicacaoVacinaAssinaturaDto = Schemas['AplicacaoVacinaAssinaturaDto'];
export type AssinaturaValidacaoResponseDto = Schemas['AssinaturaValidacaoResponseDto'];
export type ValidacaoPublicaVacinaResponseDto = Schemas['ValidacaoPublicaVacinaResponseDto'];
export type ComprovanteVacinasDadosResponseDto = Schemas['ComprovanteVacinasDadosResponseDto'];
export type ItemAnimalLoteDto = Schemas['ItemAnimalLoteDto'];

// ── Especie ──────────────────────────────────────────────────────────────────
export type EspecieCreateDto = Schemas['EspecieCreateDto'];
export type EspecieDetailResponseDto = Schemas['EspecieDetailResponseDto'];
export type EspeciePatchDto = Schemas['EspeciePatchDto'];
export type EspecieReadResponseDto = Schemas['EspecieReadResponseDto'];
export type EspecieShortResponseDto = Schemas['EspecieShortResponseDto'];

// ── Raca ─────────────────────────────────────────────────────────────────────
export type RacaCreateDto = Schemas['RacaCreateDto'];
export type RacaDetailResponseDto = Schemas['RacaDetailResponseDto'];
export type RacaPatchDto = Schemas['RacaPatchDto'];
export type RacaReadResponseDto = Schemas['RacaReadResponseDto'];
export type RacaShortResponseDto = Schemas['RacaShortResponseDto'];

// ── Vacina ───────────────────────────────────────────────────────────────────
export type VacinaCreateDto = Schemas['VacinaCreateDto'];
export type VacinaDetailResponseDto = Schemas['VacinaDetailResponseDto'];
export type VacinaPatchDto = Schemas['VacinaPatchDto'];
export type VacinaReadResponseDto = Schemas['VacinaReadResponseDto'];
export type VacinaShortResponseDto = Schemas['VacinaShortResponseDto'];

// ── Protocolo vacinal e movimentação ────────────────────────────────────────
export type StatusPendenciaVacinal = Schemas['StatusPendenciaVacinal'];
export type TipoMovimentacaoAnimal = Schemas['TipoMovimentacaoAnimal'];
export type ProtocoloVacinalDoseCreateDto = Schemas['ProtocoloVacinalDoseCreateDto'];
export type ProtocoloVacinalCreateDto = Schemas['ProtocoloVacinalCreateDto'];
export type ProtocoloVacinalPatchDto = Schemas['ProtocoloVacinalPatchDto'];
export type ProtocoloVacinalDoseResponseDto = Schemas['ProtocoloVacinalDoseResponseDto'];
export type ProtocoloVacinalReadResponseDto = Schemas['ProtocoloVacinalReadResponseDto'];
export type ProtocoloVacinalDetailResponseDto = Schemas['ProtocoloVacinalDetailResponseDto'];
export type AnimalProtocoloVacinalCreateDto = Schemas['AnimalProtocoloVacinalCreateDto'];
export type AnimalProtocoloVacinalPatchDto = Schemas['AnimalProtocoloVacinalPatchDto'];
export type AnimalProtocoloVacinalDoseResponseDto = Schemas['AnimalProtocoloVacinalDoseResponseDto'];
export type AnimalProtocoloVacinalReadResponseDto = Schemas['AnimalProtocoloVacinalReadResponseDto'];
export type ProtocoloVacinalPendenciaResponseDto = Schemas['ProtocoloVacinalPendenciaResponseDto'];
export type MovimentacaoAnimalCreateDto = Schemas['MovimentacaoAnimalCreateDto'];
export type MovimentacaoAnimalLoteCreateDto = Schemas['MovimentacaoAnimalLoteCreateDto'];
export type MovimentacaoAnimalResponseDto = Schemas['MovimentacaoAnimalResponseDto'];
export type ProtocoloVacinalSearchParams = NonNullable<paths['/api/v1/protocolos-vacinais']['get']['parameters']['query']>;
export type MovimentacaoAnimalSearchParams = NonNullable<paths['/api/v1/movimentacoes-animais']['get']['parameters']['query']>;
export type AgendaVacinalItemDto = Schemas['AgendaVacinalItemDto'];
export type AgendaVacinalSearchParams = NonNullable<paths['/api/v1/agenda-vacinal']['get']['parameters']['query']>;
export type CancelamentoVacinaDto = Schemas['CancelamentoVacinaDto'];
export type RetificacaoVacinaDto = Schemas['RetificacaoVacinaDto'];
export type MigracaoProtocoloVacinalDto = Schemas['MigracaoProtocoloVacinalDto'];
export type SituacaoRegistroVacina = Schemas['SituacaoRegistroVacina'];

// ── Helpers de nome ──────────────────────────────────────────────────────────
export function getAnimalName(animal?: { identificadorPrincipal?: any; name?: string | null } | null): string {
    if (!animal) return 'Sem nome';
    if (animal.name) return animal.name;
    if (typeof animal.identificadorPrincipal === 'string') return animal.identificadorPrincipal;
    if (animal.identificadorPrincipal?.valor) return animal.identificadorPrincipal.valor;
    return 'Sem nome';
}

export function getVacinaName(vacina?: { name?: string } | null): string {
    if (!vacina) return '';
    return vacina.name || '';
}

// ── Re-exportar helpers de enums ─────────────────────────────────────────────
export {
    SexoAnimalLabels,
    OrigemAnimalLabels,
    PorteAnimalLabels,
    TipoIdentificadorLabels,
    StatusComprovanteLabels,
    StatusPendenciaVacinalLabels,
    rotuloStatusDoseVacinal,
    TipoMovimentacaoAnimalLabels,
    calcularIdade,
    formatarPeriodo,
    hoje,
} from './enums';

/**
 * Centralização dos tipos gerados a partir do contrato OpenAPI (./contract.d.ts)
 */
export type * from './contract';
import type { components } from './contract';

// Schemas OpenAPI
export type Schemas = components['schemas'];

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
export type AplicacaoVacinaCreateDto = Schemas['AplicacaoVacinaCreateDto'];
export type AplicacaoVacinaDetailResponseDto = Schemas['AplicacaoVacinaDetailResponseDto'];
export type AplicacaoVacinaPatchDto = Schemas['AplicacaoVacinaPatchDto'];
export type AplicacaoVacinaReadResponseDto = Schemas['AplicacaoVacinaReadResponseDto'];
export type AplicacaoVacinaLoteCreateDto = Schemas['AplicacaoVacinaLoteCreateDto'];
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
    calcularIdade,
    formatarPeriodo,
    hoje,
} from './enums';

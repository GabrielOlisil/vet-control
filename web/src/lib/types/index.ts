/**
 * Centralização dos tipos gerados a partir do contrato OpenAPI (./contract.d.ts)
 */
export type * from './contract';
import type { components, paths } from './contract';

export type { components, paths };

// Schemas OpenAPI
export type Schemas = components['schemas'];

// Animal DTOs
export type AnimalCreateDto = Schemas['AnimalCreateDto'];
export type AnimalDetailResponseDto = Schemas['AnimalDetailResponseDto'];
export type AnimalPatchDto = Schemas['AnimalPatchDto'];
export type AnimalShortResponseDto = Schemas['AnimalShortResponseDto'];
export type AnimaReadResponseDto = Schemas['AnimaReadResponseDto'];

// Aplicacao Vacina DTOs
export type AplicacaoVacinaCreateDto = Schemas['AplicacaoVacinaCreateDto'];
export type AplicacaoVacinaDetailResponseDto = Schemas['AplicacaoVacinaDetailResponseDto'];
export type AplicacaoVacinaPatchDto = Schemas['AplicacaoVacinaPatchDto'];
export type AplicacaoVacinaReadResponseDto = Schemas['AplicacaoVacinaReadResponseDto'];
export type AplicacaoVacinaShortResponseDto = Schemas['AplicacaoVacinaShortResponseDto'];

// Cartao Vacina DTOs
export type CartaoVacinaCreateDto = Schemas['CartaoVacinaCreateDto'];
export type CartaoVacinaDetailResponseDto = Schemas['CartaoVacinaDetailResponseDto'];
export type CartaoVacinaPatchDto = Schemas['CartaoVacinaPatchDto'];
export type CartaoVacinaReadResponseDto = Schemas['CartaoVacinaReadResponseDto'];
export type CartaoVacinaShortResponseDto = Schemas['CartaoVacinaShortResponseDto'];

// Especie DTOs
export type EspecieCreateDto = Schemas['EspecieCreateDto'];
export type EspecieDetailResponseDto = Schemas['EspecieDetailResponseDto'];
export type EspeciePatchDto = Schemas['EspeciePatchDto'];
export type EspecieReadResponseDto = Schemas['EspecieReadResponseDto'];
export type EspecieShortResponseDto = Schemas['EspecieShortResponseDto'];

// Raca DTOs
export type RacaCreateDto = Schemas['RacaCreateDto'];
export type RacaDetailResponseDto = Schemas['RacaDetailResponseDto'];
export type RacaPatchDto = Schemas['RacaPatchDto'];
export type RacaReadResponseDto = Schemas['RacaReadResponseDto'];
export type RacaShortResponseDto = Schemas['RacaShortResponseDto'];

// Vacina DTOs
export type VacinaCreateDto = Schemas['VacinaCreateDto'];
export type VacinaDetailResponseDto = Schemas['VacinaDetailResponseDto'];
export type VacinaPatchDto = Schemas['VacinaPatchDto'];
export type VacinaReadResponseDto = Schemas['VacinaReadResponseDto'];
export type VacinaShortResponseDto = Schemas['VacinaShortResponseDto'];

// Aliases ergonômicos baseados no contrato OpenAPI
export type Animal = (AnimaReadResponseDto | AnimalDetailResponseDto) & {
    name?: string;
    nome?: string;
    pictureUpload?: null | string;
    cartaoVacina?: null | CartaoVacinaShortResponseDto | CartaoVacinaDetailResponseDto;
};
export type AnimalCreate = AnimalCreateDto;
export type AnimalPatch = AnimalPatchDto;

export type Vacina = (VacinaReadResponseDto | VacinaDetailResponseDto) & {
    name?: string;
    nome?: string;
    reaplicarEmXDias?: number | string;
    criadoEm?: string;
};
export type VacinaCreate = VacinaCreateDto;
export type VacinaPatch = VacinaPatchDto;

export type Raca = (RacaReadResponseDto | RacaDetailResponseDto) & {
    nome?: string;
    especie?: EspecieShortResponseDto | null;
    animalCount?: number | string;
};
export type RacaCreate = RacaCreateDto;
export type RacaPatch = RacaPatchDto;

export type Especie = (EspecieReadResponseDto | EspecieDetailResponseDto) & {
    nome?: string;
    nomeCientifico?: string;
    raceCount?: number | string;
};
export type EspecieCreate = EspecieCreateDto;
export type EspeciePatch = EspeciePatchDto;

export type CartaoVacina = CartaoVacinaReadResponseDto;
export type CartaoVacinaCreate = CartaoVacinaCreateDto;
export type CartaoVacinaPatch = CartaoVacinaPatchDto;

export type AplicacaoVacina = AplicacaoVacinaReadResponseDto & {
    vacinaName?: string;
    reaplicarEmXDias?: number | string;
    proximaAplicacao?: string;
};
export type AplicacaoVacinaCreate = AplicacaoVacinaCreateDto;
export type AplicacaoVacinaPatch = AplicacaoVacinaPatchDto;

// Helpers para compatibilidade entre DTOs com variações nos nomes de atributos
export function getAnimalName(animal?: { name?: string; nome?: string } | null): string {
    if (!animal) return '';
    return animal.name || animal.nome || 'Sem nome';
}

export function getVacinaName(vacina?: { name?: string; nome?: string } | null): string {
    if (!vacina) return '';
    return vacina.name || vacina.nome || 'Sem nome';
}

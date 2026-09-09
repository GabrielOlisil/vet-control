// Especie
export interface EspecieShortResponseDto {
    id: string;
    fullName: string;
}

export interface EspecieReadResponseDto {
    id: string;
    nome: string;
    nomeCientifico: string;
}

export interface EspecieDetailResponseDto {
    id: string;
    nome: string;
    nomeCientifico: string;
    raceCount?: number;
}

export interface EspecieCreateDto {
    nome: string;
    nomeCientifico: string;
}

export interface EspeciePatchDto {
    nome?: string | null;
    nomeCientifico?: string | null;
}

// Aliases para compatibilidade de código existente
export type Especie = EspecieReadResponseDto & { racas?: RacaShortResponseDto[] };
export type EspecieCreate = EspecieCreateDto;
export type EspeciePatch = EspeciePatchDto;

// Raca
export interface RacaShortResponseDto {
    id: string;
    nome: string;
}

export interface RacaReadResponseDto {
    id: string;
    nome: string;
    especie?: EspecieShortResponseDto | null;
}

export interface RacaDetailResponseDto {
    id: string;
    nome: string;
    especie: EspecieShortResponseDto;
    animalCount?: number;
}

export interface RacaCreateDto {
    nome: string;
    especieId: string;
}

export interface RacaPatchDto {
    nome?: string | null;
}

// Aliases para compatibilidade
export type Raca = RacaReadResponseDto & { especie?: EspecieShortResponseDto | any; animais?: AnimalSummary[] };
export type RacaCreate = RacaCreateDto;
export type RacaPatch = RacaPatchDto;

// Animal
export interface AnimalSummary {
    id: string;
    name?: string;
    nome?: string;
    dataNascimento?: string;
}

export interface AnimaReadResponseDto {
    id: string;
    name: string;
    raca?: RacaShortResponseDto | null;
    dataNascimento: string;
}

export interface AnimalDetailResponseDto {
    id: string;
    nome: string;
    name?: string;
    dataNascimento: string;
    pictureUpload?: string | null;
    raca?: RacaShortResponseDto | null;
    cartaoVacina?: CartaoVacinaShortResponseDto | null;
}

export interface AnimalCreateDto {
    name: string;
    dataNascimento?: string;
    pictureUpload?: string | null;
    racaId?: string | null;
    cartaoVacinaId?: string | null;
}

export interface AnimalPatchDto {
    name?: string | null;
    dataNascimento?: string | null;
    pictureUpload?: string | null;
    racaId?: string | null;
    cartaoVacinaId?: string | null;
}

export type Animal = (AnimaReadResponseDto | AnimalDetailResponseDto) & {
    nome?: string;
    name?: string;
    pictureUpload?: string | null;
    cartaoVacina?: CartaoVacinaShortResponseDto | CartaoVacinaDetailResponseDto | null;
};
export type AnimalCreate = AnimalCreateDto;
export type AnimalPatch = AnimalPatchDto;

// Helper para obter nome consistente do animal
export function getAnimalName(animal: { name?: string; nome?: string } | null | undefined): string {
    if (!animal) return '';
    return animal.name || animal.nome || 'Sem nome';
}

// Vacina
export interface VacinaShortResponseDto {
    id: string;
    name: string;
}

export interface VacinaReadResponseDto {
    id: string;
    name: string;
    reaplicarEmXDias: number;
}

export interface VacinaDetailResponseDto {
    id: string;
    nome: string;
    name?: string;
    reaplicarEmXDias: number;
    criadoEm?: string;
}

export interface VacinaCreateDto {
    name: string;
    reaplicarEmXDias?: number;
}

export interface VacinaPatchDto {
    name?: string | null;
    reaplicarEmXDias?: number | null;
}

export type Vacina = (VacinaReadResponseDto | VacinaDetailResponseDto) & {
    nome?: string;
    name?: string;
    criadoEm?: string;
};
export type VacinaCreate = VacinaCreateDto;
export type VacinaPatch = VacinaPatchDto;

export function getVacinaName(vacina: { name?: string; nome?: string } | null | undefined): string {
    if (!vacina) return '';
    return vacina.name || vacina.nome || 'Sem nome';
}

// CartaoVacina
export interface CartaoVacinaShortResponseDto {
    id: string;
    numVacinas: number;
}

export interface CartaoVacinaReadResponseDto {
    id: string;
    vacinasAplicadas: AplicacaoVacinaShortResponseDto[];
}

export interface CartaoVacinaDetailResponseDto {
    id: string;
    vacinasAplicadas: AplicacaoVacinaDetailResponseDto[];
}

export interface CartaoVacinaCreateDto {
    vacinasAplicadasIds?: string[] | null;
}

export interface CartaoVacinaPatchDto {
    vacinasAplicadasIds?: string[] | null;
}

export type CartaoVacina = CartaoVacinaDetailResponseDto;
export type CartaoVacinaCreate = CartaoVacinaCreateDto;
export type CartaoVacinaPatch = CartaoVacinaPatchDto;

// AplicacaoVacina
export interface AplicacaoVacinaShortResponseDto {
    id: string;
    vacina: VacinaShortResponseDto;
    dataAplicacao?: string;
}

export interface AplicacaoVacinaReadResponseDto {
    id: string;
    vacina: VacinaShortResponseDto;
    cartaoVacinaId?: string | null;
    dataAplicacao?: string;
}

export interface AplicacaoVacinaDetailResponseDto {
    id: string;
    vacinaName: string;
    reaplicarEmXDias: number;
    cartaoVacinaId?: string | null;
    dataAplicacao?: string;
    proximaAplicacao?: string;
}

export interface AplicacaoVacinaCreateDto {
    vacinaId: string;
    dataAplicacao?: string;
    cartaoVacinaId?: string | null;
}

export interface AplicacaoVacinaPatchDto {
    vacinaId?: string | null;
    dataAplicacao?: string | null;
    cartaoVacinaId?: string | null;
}

export type AplicacaoVacina = AplicacaoVacinaReadResponseDto & {
    vacinaName?: string;
    reaplicarEmXDias?: number;
    proximaAplicacao?: string;
};
export type AplicacaoVacinaCreate = AplicacaoVacinaCreateDto;
export type AplicacaoVacinaPatch = AplicacaoVacinaPatchDto;

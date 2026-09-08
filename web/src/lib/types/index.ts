// Especie
export interface Especie {
    id: string;
    nome: string;
    nomeCientifico: string;
    racas?: RacaResumo[];
}

export interface EspecieCreate {
    nome: string;
    nomeCientifico: string;
}

export interface EspeciePatch {
    nome?: string;
    nomeCientifico?: string;
}

// Raca
export interface RacaResumo {
    id: string;
    nome: string;
}

export interface Raca {
    id: string;
    nome: string;
    especie: {
        id: string;
        nome: string;
        nomeCientifico: string;
    };
    animais?: AnimalResumo[];
}

export interface RacaCreate {
    nome: string;
    especieId: string;
}

export interface RacaPatch {
    nome?: string;
}

// Animal
export interface AnimalResumo {
    id: string;
    name: string;
    dataNascimento?: string;
}

export interface Animal {
    id: string;
    name: string;
    dataNascimento?: string;
    pictureUpload?: string;
    raca?: {
        id: string;
        nome: string;
        especieNome: string;
    };
    cartaoVacina?: CartaoVacinaInfo;
}

export interface AnimalCreate {
    name: string;
    dataNascimento?: string;
    pictureUpload?: string;
    racaId?: string;
    cartaoVacinaId?: string;
}

export interface AnimalPatch {
    name?: string;
    dataNascimento?: string;
    pictureUpload?: string;
    racaId?: string;
    cartaoVacinaId?: string;
}

// Vacina
export interface Vacina {
    id: string;
    name: string;
    reaplicarEmXDias: number;
    criadoEm?: string;
}

export interface VacinaCreate {
    name: string;
    reaplicarEmXDias?: number;
}

export interface VacinaPatch {
    name?: string;
    reaplicarEmXDias?: number;
}

// CartaoVacina
export interface CartaoVacinaInfo {
    id: string;
    vacinasAplicadas: AplicacaoVacinaInfo[];
}

export interface CartaoVacina {
    id: string;
    vacinasAplicadas: AplicacaoVacina[];
}

export interface CartaoVacinaCreate {
    vacinasAplicadasIds?: string[];
}

export interface CartaoVacinaPatch {
    vacinasAplicadasIds?: string[];
}

// AplicacaoVacina
export interface AplicacaoVacinaInfo {
    id: string;
    vacinaName: string;
    reaplicarEmXDias: number;
    dataAplicacao?: string;
}

export interface AplicacaoVacina {
    id: string;
    vacina: {
        id: string;
        nome: string;
        reaplicarEmXDias: number;
    };
    cartaoVacinaId?: string;
    dataAplicacao?: string;
    proximaAplicacao?: string;
}

export interface AplicacaoVacinaCreate {
    vacinaId: string;
    dataAplicacao?: string;
    cartaoVacinaId?: string;
}

export interface AplicacaoVacinaPatch {
    vacinaId?: string;
    dataAplicacao?: string;
    cartaoVacinaId?: string;
}

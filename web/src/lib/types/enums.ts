/**
 * Mapeamentos legíveis para os enums numéricos do schema OpenAPI v1.
 * Uso: SexoAnimalLabels[animal.sexo] => "Macho"
 */

import type { StatusComprovanteVacina } from ".";

export const SexoAnimalLabels: Record<string | number, string> = {
    0: 'Macho',
    1: 'Fêmea',
    2: 'Indefinido',
    Macho: 'Macho',
    Femea: 'Fêmea',
    Indefinido: 'Indefinido',
};

export const OrigemAnimalLabels: Record<string | number, string> = {
    0: 'Interno (Campus/Fazenda Escola)',
    1: 'Externo (Produtor/Comunidade)',
    Interno: 'Interno (Campus/Fazenda Escola)',
    Externo: 'Externo (Produtor/Comunidade)',
};

export const PorteAnimalLabels: Record<string | number, string> = {
    0: 'Pequeno',
    1: 'Médio',
    2: 'Grande',
    Pequeno: 'Pequeno',
    Medio: 'Médio',
    Grande: 'Grande',
};

export const TipoIdentificadorLabels: Record<string | number, string> = {
    0: 'Brinco Visual',
    1: 'Brinco Eletrônico',
    2: 'SISBOV',
    3: 'Tatuagem',
    4: 'Microchip',
    5: 'Registro Genealógico',
    6: 'Outro',
    BrincoVisual: 'Brinco Visual',
    BrincoEletronico: 'Brinco Eletrônico',
    Sisbov: 'SISBOV',
    Microchip: 'Microchip',
    RegistroAssociado: 'Registro Genealógico',
    Tatuagem: 'Tatuagem',
    NomeUnico: 'Nome Único / Outro',
};

export const StatusComprovanteLabels: Record<
    StatusComprovanteVacina, string> = {
    NaoEmitido: 'badge-ghost',
    PendenteAssinatura: 'badge-warning',
    Assinado: 'badge-success',
};

/**
 * Calcula a idade em anos/meses a partir de uma data ISO (YYYY-MM-DD).
 */
export function calcularIdade(dataNascimento?: string): string {
    if (!dataNascimento) return '—';
    const nasc = new Date(dataNascimento);
    const hoje = new Date();
    const anos = hoje.getFullYear() - nasc.getFullYear();
    const meses = hoje.getMonth() - nasc.getMonth();
    const totalMeses = anos * 12 + meses;
    if (totalMeses < 1) return '< 1 mês';
    if (totalMeses < 12) return `${totalMeses} ${totalMeses === 1 ? 'mês' : 'meses'}`;
    const anosCalc = Math.floor(totalMeses / 12);
    const mesesRest = totalMeses % 12;
    if (mesesRest === 0) return `${anosCalc} ${anosCalc === 1 ? 'ano' : 'anos'}`;
    return `${anosCalc}a ${mesesRest}m`;
}

/**
 * Formata um número de dias em rótulo periódico legível.
 */
export function formatarPeriodo(dias?: number | string): string {
    const d = Number(dias);
    if (!d) return '—';
    if (d === 365) return 'Anual';
    if (d === 180) return 'Semestral';
    if (d === 90) return 'Trimestral';
    if (d === 30) return 'Mensal';
    if (d === 21) return '21 dias';
    if (d === 14) return 'Quinzenal';
    if (d === 7) return 'Semanal';
    return `${d} dias`;
}

/**
 * Retorna a data de hoje em formato YYYY-MM-DD.
 */
export function hoje(): string {
    const d = new Date();
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}


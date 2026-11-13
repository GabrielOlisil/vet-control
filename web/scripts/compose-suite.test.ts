import { describe, expect, it, vi } from 'vitest';
import { runSuite } from './compose-suite.mjs';

const project = 'vet-control-test-0123456789abcdef';
const ports = { api: 62000, auth: 62001 };

function fixture(fail: string[] = [], cancelled = () => false) {
    const commands: { tool: string; args: string[]; env?: Record<string, string> }[] = [];
    const run = vi.fn(async (tool: string, args: string[], options?: { env?: Record<string, string> }) => {
        commands.push({ tool, args, env: options?.env });
        const key = args.includes('down') ? 'down' : args.at(-1)!;
        if (fail.includes(key)) throw new Error(`${key} falhou`);
        if (args.includes('port')) return '127.0.0.1:54321\r\n';
        if (args.includes('ps')) return 'b'.repeat(64) + '\r\n';
        return '';
    });
    return { commands, run, cancelled };
}

describe('suíte descartável do Compose', () => {
    it('usa o mesmo projeto explícito para subir e limpar, com a conexão do PostgreSQL do próprio projeto', async () => {
        const state = fixture();
        await runSuite({ project, ports, artifacts: 'artifacts/test-compose/exemplo', ...state });
        const docker = state.commands.filter(command => command.tool === 'docker');
        expect(docker.every(command => command.args.slice(0, 7).join(' ') ===
            `compose -p ${project} -f compose.yaml -f compose.test.yml`)).toBe(true);
        expect(docker.at(-1)?.args.slice(7)).toEqual(['down', '--volumes', '--remove-orphans']);
        const [backend] = state.commands.filter(command => command.args[0] === 'test' && command.tool === 'dotnet');
        expect(backend?.env?.VET_TEST_POSTGRES).toContain('Port=54321;Database=postgres;');
        expect(backend?.env?.VET_TEST_PG_CONTAINER).toBe('b'.repeat(64));
        expect(state.commands.filter(command => command.tool === 'npm').map(command => command.args.at(-1)))
            .toEqual(['ci', 'check', 'test', 'build', 'chromium', 'test:e2e', 'test:compose']);
        const smoke = state.commands.find(command => command.args.includes('test:compose'));
        expect(smoke?.env).toMatchObject({ VET_TEST_COMPOSE_PROJECT: project,
            VET_COMPOSE_API_PORT: '62000', VET_COMPOSE_AUTH_PORT: '62001' });
    });

    it('executa as demais suítes e remove os volumes quando um teste falha', async () => {
        const state = fixture(['test:e2e']);
        await expect(runSuite({ project, ports, artifacts: 'reports', ...state })).rejects.toThrow(/test:e2e falhou/);
        expect(state.commands.some(command => command.args.includes('test:compose'))).toBe(true);
        expect(state.commands.at(-1)?.args).toContain('--volumes');
    });

    it('limpa o projeto mesmo quando a primeira subida falha e informa falha de limpeza', async () => {
        const state = fixture(['db', 'down']);
        await expect(runSuite({ project, ports, artifacts: 'reports', ...state })).rejects.toThrow(/db falhou.*down falhou/s);
        expect(state.commands.at(-1)?.args).toContain('--volumes');
        expect(state.commands.some(command => command.args.includes('test:e2e'))).toBe(false);
    });

    it('interrompe o agendamento após cancelamento e ainda remove o projeto', async () => {
        const state = fixture();
        await expect(runSuite({ project, ports, artifacts: 'reports', ...state,
            cancelled: () => state.commands.some(command => command.args.at(-1) === 'test:e2e'),
        })).rejects.toThrow(/interrompid/i);
        expect(state.commands.some(command => command.args.includes('test:compose'))).toBe(false);
        expect(state.commands.at(-1)?.args).toContain('--volumes');
    });
});

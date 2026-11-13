import { spawn } from 'node:child_process';
import { randomBytes } from 'node:crypto';
import { mkdir } from 'node:fs/promises';
import { createServer } from 'node:net';
import { resolve } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const root = fileURLToPath(new URL('../../', import.meta.url));
const web = fileURLToPath(new URL('../', import.meta.url));

/**
 * @typedef {{ cwd?: string, env?: Record<string, string>, capture?: boolean, cleanup?: boolean }} CommandOptions
 * @typedef {(tool: string, args: string[], options?: CommandOptions) => Promise<string>} RunCommand
 */

/**
 * @param {{ project: string, ports: {api: number, auth: number}, artifacts: string,
 *     run: RunCommand, cancelled?: () => boolean }} options
 */
export async function runSuite({ project, ports, artifacts, run, cancelled = () => false }) {
    if (!/^vet-control-test-[a-f0-9]{16}$/.test(project)) throw new Error('Nome inválido para o projeto descartável.');

    const composeEnv = {
        VET_TEST_COMPOSE_PROJECT: project,
        VET_COMPOSE_API_PORT: String(ports.api),
        VET_COMPOSE_AUTH_PORT: String(ports.auth),
    };
    /** @param {string[]} args @param {CommandOptions} options */
    const compose = (args, options = {}) => run('docker',
        ['compose', '-p', project, '-f', 'compose.yaml', '-f', 'compose.test.yml', ...args],
        { cwd: root, env: composeEnv, ...options });
    const failures = [];

    try {
        if (cancelled()) throw new Error('Execução interrompida.');
        await compose(['up', '-d', '--wait', '--wait-timeout', '90', 'db']);
        const address = (await compose(['port', 'db', '5432'], { capture: true })).trim();
        const port = /^127\.0\.0\.1:(\d+)$/.exec(address)?.[1];
        const container = (await compose(['ps', '-q', 'db'], { capture: true })).trim();
        if (!port || !/^[a-f0-9]{12,64}$/.test(container)) throw new Error('Não foi possível identificar o PostgreSQL do projeto.');
        const testEnv = {
            VET_TEST_POSTGRES: `Host=127.0.0.1;Port=${port};Database=postgres;Username=postgres;Password=vet-control-local-only`,
            VET_TEST_PG_CONTAINER: container,
        };

        if (cancelled()) throw new Error('Execução interrompida.');
        await run('dotnet', ['restore', 'VetControl.slnx', '--locked-mode'], { cwd: root });
        if (cancelled()) throw new Error('Execução interrompida.');
        await run('npm', ['ci'], { cwd: web });

        /** @param {() => Promise<unknown>} action */
        async function runCheck(action) {
            if (cancelled()) throw new Error('Execução interrompida.');
            try { await action(); }
            catch (error) { failures.push(message(error)); }
        }

        await runCheck(() => run('dotnet', ['test', 'VetControl.slnx', '--no-restore', '-m:1',
            '--logger', 'trx', '--results-directory', resolve(root, artifacts, 'xunit')], { cwd: root, env: testEnv }));
        for (const check of ['check', 'test', 'build']) {
            await runCheck(() => run('npm', ['run', check], { cwd: web }));
        }
        await runCheck(async () => {
            await run('npm', ['exec', '--', 'playwright', 'install', 'chromium'], { cwd: web });
            await run('npm', ['run', 'test:e2e'], { cwd: web, env: { ...testEnv,
                VET_TEST_PLAYWRIGHT_OUTPUT: resolve(root, artifacts, 'playwright') } });
        });
        await runCheck(async () => {
            await compose(['up', '-d', '--build', '--wait', '--wait-timeout', '120', 'api']);
            await run('npm', ['run', 'test:compose'], { cwd: web, env: { ...composeEnv,
                VET_TEST_PLAYWRIGHT_OUTPUT: resolve(root, artifacts, 'compose') } });
        });
        if (cancelled()) throw new Error('Execução interrompida.');
    } catch (error) {
        failures.push(message(error));
    } finally {
        if (failures.length) {
            try { await compose(['logs', '--no-color', '--tail', '80'], { cleanup: true }); }
            catch { /* A falha de limpeza é registrada abaixo. */ }
        }
        try { await compose(['down', '--volumes', '--remove-orphans'], { cleanup: true }); }
        catch (error) { failures.push(`Falha ao limpar ${project}: ${message(error)}`); }
    }
    if (failures.length) throw new Error(failures.join('\n'));
}

/** @param {unknown} error */
function message(error) {
    return error instanceof Error ? error.message : String(error);
}

async function demoPorts() {
    const servers = [createServer(), createServer()];
    try {
        const [api, auth] = await Promise.all(servers.map(server => new Promise((ready, reject) => {
            server.once('error', reject);
            server.listen(0, '127.0.0.1', () => {
                const address = server.address();
                if (address && typeof address !== 'string') ready(address.port);
                else reject(new Error('Não foi possível selecionar uma porta local.'));
            });
        })));
        return { api, auth };
    } finally {
        await Promise.all(servers.filter(server => server.listening).map(server =>
            new Promise(resolveClose => server.close(resolveClose))));
    }
}

async function main() {
    const project = `vet-control-test-${randomBytes(8).toString('hex')}`;
    const artifacts = resolve(root, 'artifacts', 'test-compose', project);
    const npmEntry = process.env.npm_execpath;
    if (!npmEntry) throw new Error('Execute pela raiz com npm --prefix web run test:all.');

    /** @type {import('node:child_process').ChildProcess | undefined} */
    let child;
    let interrupted = false;
    let cleaning = false;
    function interrupt() {
        if (interrupted || cleaning) return;
        interrupted = true;
        console.error('Interrompendo os testes e limpando o projeto…');
        if (!child?.pid) return;
        if (process.platform === 'win32') {
            const kill = spawn('taskkill', ['/pid', String(child.pid), '/T', '/F'], { stdio: 'ignore', windowsHide: true });
            kill.on('error', () => child?.kill());
        } else {
            try { process.kill(-child.pid, 'SIGTERM'); }
            catch { /* O comando já pode ter terminado. */ }
        }
    }
    process.on('SIGINT', interrupt);
    process.on('SIGTERM', interrupt);

    /** @type {RunCommand} */
    const run = (tool, args, { env = {}, cwd = root, capture = false, cleanup = false } = {}) => {
        if (interrupted && !cleanup) return Promise.reject(new Error('Execução interrompida.'));
        cleaning = cleanup;
        console.log(`\n> ${tool} ${args.join(' ')}`);
        return new Promise((done, reject) => {
            const command = tool === 'npm' ? [process.execPath, npmEntry, ...args] : [tool, ...args];
            const [file, ...commandArgs] = command;
            child = spawn(file, commandArgs, {
                cwd, env: { ...process.env, ...env }, windowsHide: true,
                detached: process.platform !== 'win32',
                stdio: ['ignore', capture ? 'pipe' : 'inherit', 'inherit'],
            });
            let output = '';
            child.stdout?.setEncoding('utf8').on('data', chunk => { output += chunk; });
            child.once('error', reject);
            child.once('close', (code, signal) => {
                child = undefined;
                if (code === 0) done(output);
                else reject(new Error(`${tool} ${args.join(' ')} terminou com ${signal ?? code}.`));
            });
        });
    };

    try {
        await mkdir(artifacts, { recursive: true });
        console.log(`Projeto de testes: ${project}\nRelatórios: ${artifacts}`);
        await runSuite({ project, ports: await demoPorts(), artifacts, run, cancelled: () => interrupted });
    } finally {
        process.off('SIGINT', interrupt);
        process.off('SIGTERM', interrupt);
    }
}

if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
    await main().catch(error => {
        console.error(message(error));
        process.exitCode = 1;
    });
}

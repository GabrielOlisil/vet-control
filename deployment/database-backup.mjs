import { spawn, spawnSync } from 'node:child_process';
import { createReadStream, createWriteStream } from 'node:fs';
import { readFile, writeFile, stat } from 'node:fs/promises';
import { pipeline } from 'node:stream/promises';
import { createHash } from 'node:crypto';
import { resolve } from 'node:path';

// Uses the PostgreSQL tools from the running container: no local pg_dump installation.
const [operation, ...args] = process.argv.slice(2);
const options = {};
for (let i = 0; i < args.length; i += 2) {
    if (!args[i]?.startsWith('--') || !args[i + 1]) throw new Error('Use --opcao valor.');
    options[args[i].slice(2)] = args[i + 1];
}
const container = options.container;
const database = options.database;
const username = options.username ?? 'postgres';
if (!['backup', 'restore'].includes(operation) || !/^[a-zA-Z0-9_.-]+$/.test(container ?? '') ||
    !/^[a-zA-Z][a-zA-Z0-9_]{0,62}$/.test(database ?? '') || !/^[a-zA-Z][a-zA-Z0-9_]*$/.test(username)) {
    throw new Error('Uso: node deployment/database-backup.mjs backup|restore --container ID --database BASE --file ARQUIVO [--expected-database BASE].');
}
if (!options.file) throw new Error('Informe --file.');
const file = resolve(options.file);
function sql(query) {
    const result = spawnSync('docker', ['exec', container, 'psql', '-X', '-U', username, '-d', database,
        '-v', 'ON_ERROR_STOP=1', '-Atc', query], { encoding: 'utf8' });
    if (result.error || result.status !== 0) throw new Error(result.error?.message ?? result.stderr);
    return result.stdout.trim();
}
async function hashFile() {
    const hash = createHash('sha256');
    for await (const part of createReadStream(file)) hash.update(part);
    return hash.digest('hex');
}
async function command(commandArgs, direction) {
    const child = spawn('docker', commandArgs, { stdio: [direction === 'in' ? 'pipe' : 'ignore', direction === 'out' ? 'pipe' : 'ignore', 'pipe'] });
    let stderr = '';
    child.stderr.on('data', data => { stderr = (stderr + data).slice(-16000); });
    const exited = new Promise((ok, fail) => {
        child.once('error', fail);
        child.once('exit', code => code === 0 ? ok() : fail(new Error(stderr || `docker terminou com código ${code}`)));
    });
    const stream = direction === 'out'
        ? pipeline(child.stdout, createWriteStream(file, { flags: 'wx', mode: 0o600 }))
        : pipeline(createReadStream(file), child.stdin);
    await Promise.all([exited, stream]);
}
if (operation === 'backup') {
    const serverVersion = sql('SHOW server_version');
    await command(['exec', container, 'pg_dump', '-U', username, '-d', database, '--format=custom', '--no-owner', '--no-acl'], 'out');
    const manifest = { database, serverVersion, createdAt: new Date().toISOString(), sha256: await hashFile(), bytes: (await stat(file)).size };
    await writeFile(`${file}.json`, JSON.stringify(manifest, null, 2) + '\n', { flag: 'wx', mode: 0o600 });
    console.log(`Backup concluído: ${file}. Guarde o arquivo e seu manifesto em armazenamento protegido.`);
} else {
    if (options['expected-database'] !== database || ['postgres', 'template0', 'template1'].includes(database.toLowerCase()))
        throw new Error('Restauração exige --expected-database idêntico ao destino e uma base de aplicação dedicada.');
    const manifest = JSON.parse(await readFile(`${file}.json`, 'utf8'));
    if (await hashFile() !== manifest.sha256) throw new Error('O hash do backup difere do manifesto. Restauração recusada.');
    // Refuses destructive restores. Operator creates an empty target explicitly; never uses --clean.
    if (sql("SELECT count(*) FROM pg_class c JOIN pg_namespace n ON n.oid=c.relnamespace WHERE n.nspname NOT IN ('pg_catalog','information_schema') AND n.nspname NOT LIKE 'pg_toast%' AND c.relkind IN ('r','p','v','m','S','f')") !== '0')
        throw new Error('O destino contém dados ou objetos. Restaure em uma base nova e vazia.');
    await command(['exec', '-i', container, 'pg_restore', '-U', username, '-d', database, '--no-owner', '--no-acl', '--exit-on-error', '--single-transaction'], 'in');
    console.log(`Restauração concluída em ${database}. Confira migrations, vínculos e assinaturas antes de liberar o uso.`);
}

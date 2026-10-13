import { mkdir, writeFile, rename } from 'node:fs/promises';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { resolve } from 'node:path';
import openapiTS, { astToString } from 'openapi-typescript';

// This script is deliberately independent from types:g (the clinic API contract).
const source = new URL('/openapi/v1.json', process.env.AUTH_API_URL || 'http://localhost:8081');
const output = process.env.AUTH_CONTRACT_OUTPUT
    ? pathToFileURL(resolve(process.env.AUTH_CONTRACT_OUTPUT))
    : new URL('../src/lib/types/auth-contract.d.ts', import.meta.url);
try {
    const response = await fetch(source, { signal: AbortSignal.timeout(15_000) });
    if (!response.ok) throw new Error(`OpenAPI respondeu HTTP ${response.status}.`);
    const document = await response.json();
    if (!document.openapi || !document.paths?.['/api/auth/session']) throw new Error('O documento não é o OpenAPI da API Auth.');
    const declarations = astToString(await openapiTS(document));
    await mkdir(new URL('.', output), { recursive: true });
    const temporary = new URL(`${output.href}.tmp`);
    await writeFile(temporary, `/** Gerado do OpenAPI da API Auth. Execute npm run types:g:auth; não edite manualmente. */\n${declarations}`, 'utf8');
    await rename(temporary, output);
    console.log(`Contrato Auth gerado: ${fileURLToPath(output)}`);
} catch (error) {
    console.error(`Não foi possível gerar o contrato Auth. Inicie a API na porta 8081 e tente novamente.\n${error.message}`);
    process.exitCode = 1;
}

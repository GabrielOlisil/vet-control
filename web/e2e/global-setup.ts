import { spawn } from 'node:child_process';
import { fileURLToPath } from 'node:url';

export default async function globalSetup() {
    const root = fileURLToPath(new URL('../../', import.meta.url));
    const host = spawn('dotnet', ['run', '--no-build', '--no-launch-profile', '--project',
        'Testing/BrowserHost/VetControl.BrowserHost.csproj'], {
        cwd: root,
        env: { ...process.env, VET_E2E_HOST: '1' },
        stdio: ['pipe', 'pipe', 'pipe'],
        windowsHide: true,
    });
    let output = '';
    const exited = new Promise<number | null>((resolve) => host.once('close', resolve));
    host.stdin.on('error', () => { /* The startup error is reported with the host's output below. */ });
    const stop = async () => {
        if (host.pid && host.exitCode === null && !host.stdin.destroyed) host.stdin.end('stop\n');
        const code = await exited;
        if (code !== 0) throw new Error(`Test API exited with ${code}: ${output}`);
    };
    try {
        await new Promise<void>((resolve, reject) => {
            const timeout = setTimeout(() => reject(new Error(`Test API startup timed out: ${output}`)), 60_000);
            const ready = () => { clearTimeout(timeout); resolve(); };
            host.stdout.on('data', (chunk: Buffer) => {
                output += chunk.toString();
                if (output.includes('VET_E2E_READY')) ready();
            });
            host.stderr.on('data', (chunk: Buffer) => { output += chunk.toString(); });
            host.once('error', (error) => { clearTimeout(timeout); reject(error); });
            host.once('exit', (code) => { clearTimeout(timeout); reject(new Error(`Test API exited with ${code}: ${output}`)); });
        });
        return stop;
    } catch (error) {
        // A graceful stop lets the .NET host clean up its database even on startup failures.
        await stop().catch(() => {});
        throw error;
    }
}

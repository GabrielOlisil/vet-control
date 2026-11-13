import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
    testDir: './e2e',
    outputDir: process.env.VET_TEST_PLAYWRIGHT_OUTPUT ?? './test-results',
    globalSetup: './e2e/global-setup.ts',
    fullyParallel: false,
    workers: 1,
    forbidOnly: !!process.env.CI,
    retries: 0,
    reporter: process.env.CI ? 'github' : 'list',
    use: { baseURL: 'http://localhost:5187', trace: 'retain-on-failure' },
    projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
    webServer: {
        command: 'npm run dev -- --host localhost --port 5187 --strictPort',
        url: 'http://localhost:5187/validar-vacinacao',
        reuseExistingServer: false,
        timeout: 60_000,
        env: { VITE_API_URL: 'http://localhost:5188', VITE_IDENTITY_URL: 'http://localhost:5189' },
    },
});

import { defineConfig, devices } from '@playwright/test';

// Exercises the actual Docker image: no Vite server, fake auth or replacement API.
export default defineConfig({
    testDir: './compose-e2e',
    outputDir: process.env.VET_TEST_PLAYWRIGHT_OUTPUT ?? './test-results',
    workers: 1,
    retries: 0,
    use: { baseURL: `http://localhost:${process.env.VET_COMPOSE_API_PORT ?? 8080}`, trace: 'retain-on-failure' },
    projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});

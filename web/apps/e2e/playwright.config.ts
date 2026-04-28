import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: Boolean(process.env.CI),
  retries: 0,
  workers: process.env.CI ? 1 : undefined,
  outputDir: 'test-results',
  reporter: [
    ['list'],
    ['html', { open: 'never', outputFolder: 'playwright-report' }],
    ['junit', { outputFile: 'test-results/junit.xml' }],
  ],
  use: {
    actionTimeout: 10_000,
    navigationTimeout: 15_000,
    screenshot: 'only-on-failure',
    trace: 'retain-on-failure',
    video: {
      mode: process.env.PLAYWRIGHT_RECORD_VIDEO === '1' ? 'on' : 'retain-on-failure',
      size: { width: 1280, height: 720 },
    },
  },
  projects: [
    {
      name: 'management-desktop',
      testMatch: /management-.*\.spec\.ts/,
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://127.0.0.1:4173',
      },
    },
    {
      name: 'customer-desktop',
      testMatch: /customer-.*\.spec\.ts/,
      use: {
        ...devices['Desktop Chrome'],
        baseURL: 'http://127.0.0.1:4174',
      },
    },
  ],
  webServer: [
    {
      command: 'pnpm --filter @alcopilot/management-portal dev --host 127.0.0.1',
      url: 'http://127.0.0.1:4173',
      reuseExistingServer: true,
      timeout: 120_000,
    },
    {
      command: 'pnpm --filter @alcopilot/web-portal dev --host 127.0.0.1',
      url: 'http://127.0.0.1:4174',
      reuseExistingServer: true,
      timeout: 120_000,
    },
  ],
});

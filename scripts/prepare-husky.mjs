import { existsSync } from 'node:fs';
import { spawnSync } from 'node:child_process';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

if (process.env.HUSKY === '0') {
  process.exit(0);
}

const root = dirname(dirname(fileURLToPath(import.meta.url)));
const huskyBin = join(
  root,
  'node_modules',
  '.bin',
  process.platform === 'win32' ? 'husky.cmd' : 'husky',
);

if (!existsSync(huskyBin)) {
  console.log('husky prepare skipped: root dev dependencies are not installed.');
  process.exit(0);
}

const result = spawnSync(huskyBin, {
  cwd: root,
  stdio: 'inherit',
  shell: process.platform === 'win32',
});

process.exit(result.status ?? 1);

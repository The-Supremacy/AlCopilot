function shellQuote(value) {
  return `'${value.replaceAll("'", "'\\''")}'`;
}

export default {
  '*.{md,mdx,json,yml,yaml,html}': ['prettier --write'],
  '*.{js,mjs,cjs}': ['prettier --write'],
  'web/apps/web-portal/**/*.{ts,tsx,js,jsx,css}': (files) => [
    `prettier --write ${files.map(shellQuote).join(' ')}`,
    'pnpm --dir web/apps/web-portal lint',
  ],
  'web/apps/management-portal/**/*.{ts,tsx,js,jsx,css}': (files) => [
    `prettier --write ${files.map(shellQuote).join(' ')}`,
    'pnpm --dir web/apps/management-portal lint',
  ],
  'web/packages/**/*.{ts,tsx,js,jsx,css}': (files) => [
    `prettier --write ${files.map(shellQuote).join(' ')}`,
    'pnpm -r --filter "./web/apps/**" lint',
  ],
  'server/**/*.cs': (files) => {
    if (files.length === 0) {
      return [];
    }

    return `bash scripts/husky/dotnet-format-staged.sh ${files.map(shellQuote).join(' ')}`;
  },
};

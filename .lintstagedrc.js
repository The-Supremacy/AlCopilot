export default {
  '*.{ts,tsx,js,jsx}': ['prettier --write', 'eslint --fix'],
  '*.{json,css,md,yaml,yml,html}': ['prettier --write'],
  '*.cs': (filenames) => {
    const files = filenames.map((f) => `--include ${f}`).join(' ');
    return [`dotnet format server/AlCopilot.slnx --no-restore ${files}`];
  },
};

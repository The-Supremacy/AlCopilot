export default {
  '*.{ts,tsx,js,jsx}': (filenames) => {
    const cmds = [`prettier --write ${filenames.join(' ')}`];
    const portalFiles = filenames.filter((f) => f.includes('web/'));
    if (portalFiles.length > 0) {
      cmds.push(`pnpm --filter @alcopilot/portal exec eslint --fix ${portalFiles.join(' ')}`);
    }
    return cmds;
  },
  '*.{json,css,md,yaml,yml,html}': ['prettier --write'],
  '*.cs': (filenames) => {
    const files = filenames.map((f) => `--include ${f}`).join(' ');
    return [`dotnet format server/AlCopilot.slnx --no-restore ${files}`];
  },
};

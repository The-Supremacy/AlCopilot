# Documentation Conventions

## Standards

- Use ATX-style headings (`#`, `##`, `###`)
- One sentence per line in source (renders as paragraphs, easier diffs)
- Use `---` horizontal rules to separate major sections
- Tables for structured comparisons
- Code blocks with language tags (`csharp`, `bash`, `typescript`, etc.)

## Architecture Docs

- `docs/constitution.md`, `docs/architecture.md`, and `docs/testing.md` are thin root indexes
- Detailed human-readable guidance should live in area docs under `docs/constitution/`, `docs/architecture/`, and `docs/testing/`
- `docs/adr/` records architectural decisions and superseded choices over time
- `docs/adr/README.md` defines ADR structure, statuses, and sync expectations
- `docs/ai/llm.md` covers local LLM and vector database setup
- `docs/ai/llm.md` also covers the recommended local subagent workflow for bounded review and exploration tasks
- `docs/ai/embedding.md` is the distilled architecture-level embedding guide
- New documentation goes in `docs/` unless it's module-specific (then it lives in the module)

## Information Architecture

- Keep root docs concise and navigational
- Do not duplicate area-specific detail between root docs and sub-docs
- Prefer linking to a detailed sub-doc over restating its guidance

## Decision Records

- When documenting "Why Not X" decisions, include: the alternative considered, why it was rejected, and what we chose instead
- Keep decisions honest and specific — avoid vague "it's better" statements

## Diagrams

- Prefer **Mermaid** for diagrams — renders natively in GitHub and most tools
- Keep diagrams close to the text that references them

# Documentation Conventions

## Standards

- Use ATX-style headings (`#`, `##`, `###`)
- One sentence per line in source (renders as paragraphs, easier diffs)
- Use `---` horizontal rules to separate major sections
- Tables for structured comparisons
- Code blocks with language tags (`csharp`, `bash`, `typescript`, etc.)

## Architecture Docs

- `docs/constitution.md` is the source of truth for project-wide governance and workflow rules
- `docs/architecture.md` is the source of truth for design decisions
- `docs/llm.md` covers local LLM and vector database setup
- New documentation goes in `docs/` unless it's module-specific (then it lives in the module)

## Decision Records

- When documenting "Why Not X" decisions, include: the alternative considered, why it was rejected, and what we chose instead
- Keep decisions honest and specific — avoid vague "it's better" statements

## Diagrams

- Prefer **Mermaid** for diagrams — renders natively in GitHub and most tools
- Keep diagrams close to the text that references them

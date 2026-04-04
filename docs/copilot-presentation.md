# GitHub Copilot for Effective Development — Presentation Plan

Workshop-style presentation.
Goal: show the team how to use GitHub Copilot effectively — not just autocomplete, but a controlled, layered system for AI-assisted development.
Context: we will use this to build a new product together during a follow-up workshop.

---

## Slide 1 — Title

**On the slide:**

> **GitHub Copilot for Effective Development**
> Beyond autocomplete — a controlled, layered approach to AI-assisted engineering

**What to say:**

- This is not "let the AI write your code" — it's about building a system around the AI so it works _with_ you, not _instead_ of you.
- By the end you'll have a setup you can apply to any project, and we'll use it in a hands-on workshop to build something real.

---

## Slide 2 — The Problem with "Vibe Coding"

**On the slide:**

> **Vibe coding** = prompting the AI, pasting whatever it gives you, fixing errors until it runs.
>
> - No plan → no coherence
> - No conventions → inconsistent code
> - No boundaries → AI decides your architecture
> - No review → bugs ship faster too

**What to say:**

- Copilot is incredibly powerful, but without structure it produces code that _looks_ right and _compiles_ — yet doesn't follow your patterns, uses wrong libraries, or quietly introduces design problems.
- The fix isn't to stop using AI — it's to control it.
- The key insight: **you control the tool, the tool doesn't control you.**

---

## Slide 3 — Copilot Customization Layers

**On the slide:**

```
┌─────────────────────────────────────────┐
│  1. copilot-instructions.md             │  ← Project-wide rules
│  2. Custom instructions (per-path)      │  ← Context-specific conventions
│  3. Custom agents                       │  ← Role-based AI modes
│  4. SKILL files                         │  ← Repeatable generation templates
│  5. MCP servers                         │  ← Extended context & tools
│  6. Agentic workflows                   │  ← CI/CD with AI executor
└─────────────────────────────────────────┘
```

**What to say:**

- These are the building blocks.
  Each layer adds specificity — start broad, narrow down.
- We'll go through each one.

---

## Slide 4 — copilot-instructions.md

**On the slide:**

> **`.github/copilot-instructions.md`** — global rules applied to every Copilot interaction.
>
> Example rules:
>
> - "No Vibe Coding" — SKILL gate for code generation
> - Plan-first: present a plan before implementing anything non-trivial
> - Reference `docs/architecture.md` — don't contradict documented decisions
> - Semantic commits only
> - Repo structure overview so Copilot knows where things live

**What to say:**

- This is the first thing to set up.
  It's your contract with Copilot: "these are the rules, always."
- In our project, we use a "SKILL gate" — Copilot cannot generate implementation code unless a matching SKILL file exists.
  Without it, it explains the approach instead.
- This forces the developer to build the first instance manually, understand it, and _then_ templatize.

---

## Slide 5 — Custom Instructions (Per-Path)

**On the slide:**

> **`.github/instructions/*.instructions.md`** — scoped rules that auto-activate by file path.
>
> ```yaml
> ---
> applyTo: 'server/**'
> ---
> ```
>
> Examples:
> | File | Applies to | Key rules |
> |---|---|---|
> | `server.instructions.md` | `server/**` | Sealed classes, Mediator not MediatR, NSubstitute not Moq |
> | `web.instructions.md` | `web/**` | TanStack Router, named exports, Zustand for state |
> | `ci.instructions.md` | `.github/workflows/**` | Reusable workflows, no secrets in logs |
> | `deploy.instructions.md` | `deploy/**` | Terraform conventions, Flux GitOps |
> | `docs.instructions.md` | `docs/**` | ATX headings, one sentence per line |

**What to say:**

- You don't want your React conventions polluting .NET suggestions and vice versa.
- Per-path instructions activate automatically when Copilot touches files matching the glob — zero effort from the developer.
- This is where you encode "Why Not X" decisions.
  Example: we use Mediator (MIT, source-gen), not MediatR (commercial v13+).
  Without this instruction, Copilot defaults to MediatR every time because it's more popular in training data.

---

## Slide 6 — Custom Agents

**On the slide:**

> **`.github/agents/*.agent.md`** — specialized AI modes with different tools and instructions.
>
> | Agent         | Purpose                                              | Can edit code? |
> | ------------- | ---------------------------------------------------- | -------------- |
> | `@architect`  | Plan features, validate boundaries, check tech stack | No             |
> | `@scaffolder` | Generate code following SKILL files                  | Yes (only one) |
> | `@reviewer`   | Check conventions, review PRs                        | No             |
>
> Key concept: **tool restrictions**
>
> ```yaml
> tools: ['read', 'search', 'agent', 'todo'] # architect — read-only
> ```
>
> Scaffolder has no restrictions — full access.

**What to say:**

- Instead of one general-purpose AI that can do everything, you create specialized roles.
- The architect _cannot_ edit files — it can only read, search, and plan.
  This is a hard technical restriction, not just a prompt suggestion.
- The scaffolder is the _only_ agent that can write code, and it's gated by SKILL files.
- Agents can delegate to each other as subagents — the architect can spawn a reviewer to check conventions before finalizing a plan.
- You switch between agents via the mode dropdown in VS Code chat.

---

## Slide 7 — SKILL Files

**On the slide:**

> **`.github/skills/*.md`** — step-by-step recipes for repeatable code generation.
>
> Workflow:
>
> 1. Developer builds the first instance manually
> 2. Developer writes a SKILL file capturing the pattern
> 3. Scaffolder agent follows the SKILL for subsequent instances
>
> Example SKILL: `new-endpoint.md`
>
> - Create request/response DTOs in Contracts project
> - Create handler in module with `sealed` class
> - Register via `Add{Module}Module()` extension
> - Create integration test with TestContainers

**What to say:**

- SKILLs are the bridge between "I don't trust the AI" and "I need the AI to be productive."
- You prove the pattern works by building it once.
  Then you encode it so Copilot can replicate it exactly.
- This is the opposite of vibe coding: the human designs, the AI replicates.
- SKILLs accumulate over time — your project gets more AI-assisted as you build more patterns.

---

## Slide 8 — MCP Servers

**On the slide:**

> **Model Context Protocol** — give Copilot access to external tools and data sources.
>
> Public MCP servers:
> | Server | What it provides |
> |---|---|
> | GitHub | PR diffs, issues, repo search |
> | Context7 | Up-to-date library documentation |
>
> Custom MCP servers (your domain):
> | Server | What it provides |
> |---|---|
> | Application logs | Agent reads prod/staging logs to diagnose issues |
> | Database access | Agent queries schema or reference data |
> | Internal APIs | Agent calls staging endpoints to verify behavior |
>
> **Note:** the focus here is developer experience, not building an AI product.
> MCP makes the _developer's_ Copilot smarter, not the end-user's app.

**What to say:**

- Out of the box, Copilot only sees your code.
  MCP servers extend what it can access.
- Context7 is a game-changer: it fetches _current_ library docs instead of relying on training data.
  No more suggestions using deprecated APIs.
- Custom MCP servers are where it gets really interesting for enterprise teams.
  Imagine: "why is this endpoint returning 500 in staging?" — Copilot reads the actual logs, finds the stack trace, and points you to the bug.
- These are just stdio processes — you can build one in an afternoon with any language.

---

## Slide 9 — Agentic Workflows

**On the slide:**

> **GitHub Agentic Workflows** — Copilot as a CI/CD executor.
>
> ```
> Markdown file → gh aw compile → .lock.yml → GitHub Actions
> ```
>
> Instead of shell scripts, Copilot coding agent executes tasks:
>
> - Understands codebase context
> - Follows your copilot-instructions
> - Creates PRs with changes
>
> Use cases:
> | Trigger | Workflow |
> |---|---|
> | New issue labeled `bug` | Agent investigates, proposes fix, opens PR |
> | Dependency update PR | Agent runs tests, checks breaking changes, summarizes |
> | Weekly | Agent audits TODOs, stale branches, test coverage |
> | New PR | Agent runs convention review before human reviewer |

**What to say:**

- This extends Copilot beyond the editor into your CI/CD pipeline.
- The workflow is defined in markdown — natural language — and compiled to GitHub Actions.
- The AI agent runs as the executor, so it has the same context awareness as your editor Copilot — it reads your instructions, follows your conventions.
- Practical example: label an issue as `bug`, the agent reads the issue, investigates the codebase, proposes a fix, and opens a PR.
  You review the PR just like any human contribution.
- We haven't deployed these yet but plan to start with the convention review use case — run `@reviewer` checks automatically on every PR.

---

## Slide 10 — Hooks

**On the slide:**

> **Copilot Hooks** — event-driven shell scripts that fire during coding agent and Copilot CLI sessions.
>
> Location: `.github/hooks/*.json`
>
> | Event                        | When                     | Can block?         |
> | ---------------------------- | ------------------------ | ------------------ |
> | `sessionStart`               | Agent session begins     | No                 |
> | `userPromptSubmitted`        | User sends a prompt      | No                 |
> | **`preToolUse`**             | **Before any tool call** | **Yes — the gate** |
> | `postToolUse`                | After tool completes     | No                 |
> | `agentStop` / `subagentStop` | Agent finishes           | No                 |
> | `errorOccurred`              | Error during execution   | No                 |
>
> The **`preToolUse`** hook is the key — it can approve or deny any tool:
>
> ```bash
> # Block edits outside src/ and test/
> if [[ ! "$PATH_ARG" =~ ^(src/|test/) ]]; then
>   echo '{"permissionDecision":"deny","permissionDecisionReason":"Can only edit src/ or test/"}'
> fi
> ```
>
> Planned use cases:
> | Hook | What it does |
> |---|---|
> | `preToolUse` | Block edits to `docs/architecture.md` (source of truth, human-only) |
> | `preToolUse` | Deny destructive commands (`rm -rf`, `DROP TABLE`, `force push`) |
> | `preToolUse` | Run linter before allowing file edits — deny if it fails |
> | `postToolUse` | Log all tool calls for compliance audit trail |
> | `sessionStart` | Log session start with timestamp and initial prompt |
>
> **Note:** hooks apply to Copilot coding agent (GitHub.com) and Copilot CLI — not VS Code chat agents.
> For VS Code agents, tool restrictions are set in `.agent.md` files.

**What to say:**

- Hooks are middleware for the AI — same concept as HTTP middleware but for agent tool calls.
- The `preToolUse` hook is the most powerful — it receives the tool name and arguments before execution, and can deny the action.
- Think of it as a security gate: the agent wants to run `rm -rf dist` — your hook script checks it, decides it's too dangerous, returns `deny`, and the agent has to find another way.
- You can also enforce code quality — run your linter before any file edit, and if it fails, block the edit.
  The agent has to fix the code before it can save it.
- I haven't deployed these yet, but the gate pattern is the first thing I plan to set up — block edits to architecture docs and deny destructive commands.
- Important: hooks are for the Copilot coding agent on GitHub and Copilot CLI — they don't apply to VS Code chat.
  For VS Code, we use tool restrictions in agent files instead.

---

## Slide 11 — Guidance vs Enforcement

**On the slide:**

> **Most AI setup is guidance. Only two things actually enforce.**
>
> | Layer                                  | Type            | Where it works         | Can the agent ignore it?                 |
> | -------------------------------------- | --------------- | ---------------------- | ---------------------------------------- |
> | `copilot-instructions.md`              | Guidance        | Everywhere             | Yes — it's a prompt                      |
> | Per-path instructions                  | Guidance        | Everywhere             | Yes — it's a prompt                      |
> | SKILL files                            | Guidance        | Everywhere             | Yes — it's a prompt                      |
> | **Custom agent `tools:` restrictions** | **Enforcement** | **VS Code only**       | **No — tool is physically unavailable**  |
> | **Hooks (`preToolUse` deny)**          | **Enforcement** | **Coding agent + CLI** | **No — action is blocked at tool level** |
>
> Instructions tell the agent what it _should_ do.
> Tool restrictions and hooks control what it _can_ do.
>
> **This is why "No Vibe Coding" matters.**
> If you vibe-code, you have no instructions, no enforcement, no audit trail.
> The agent decides everything — architecture, libraries, patterns — and you discover the problems later.

**What to say:**

- This is the most important slide.
  Everything else we've covered is layered on top of this insight.
- Instructions, skills, per-path files — they're all prompt-level guidance.
  The agent _should_ follow them, and usually does, but nothing _physically prevents_ it from ignoring them.
- Only two mechanisms actually enforce: tool restrictions in custom agents (VS Code only) and hooks (coding agent + CLI).
- In VS Code, your `@architect` agent literally cannot call the edit tool — the tool doesn't exist in its context.
  In the coding agent on GitHub, a `preToolUse` hook can deny any action at the tool level — no prompt trick bypasses it.
- This is also the strongest argument for No Vibe Coding.
  If you skip the setup — no instructions, no agents, no hooks — the AI has zero constraints.
  It picks its own architecture, its own libraries, its own patterns.
  You only find out it was wrong when you're debugging in production.
- The setup takes effort upfront, but it converts "hope the AI does the right thing" into "the AI cannot do the wrong thing."

---

## Slide 12 — Copilot Extensions & Cookbook

**On the slide:**

> **Copilot Extensions** — build your own Copilot-powered tools.
>
> The **Copilot Cookbook** (github.com/copilot/cookbook) is an SDK + samples for building applications that use Copilot as a backend:
>
> - Chat extensions for GitHub.com
> - Copilot-powered internal tools
> - Custom AI assistants using GitHub's infrastructure
>
> **This is for building AI products, not for dev workflow.**
> Relevant when: you want to build something like "ask our docs" or "internal support bot" powered by Copilot.

**What to say:**

- The Cookbook is a separate concern from everything else in this presentation.
- Everything we've covered so far is about making _your development_ better.
  The Cookbook is about building AI-powered _products_ for end users.
- It's the Copilot SDK — you build extensions that run on GitHub's infrastructure.
- Worth exploring if your team wants to build internal AI tools, but it's a different track from what we're focused on today.

---

## Slide 13 — My Setup

**On the slide:**

> **AlCopilot project — layered AI setup**
>
> ```
> .github/
> ├── copilot-instructions.md        "No Vibe Coding" + SKILL gate
> ├── instructions/
> │   ├── server.instructions.md     .NET conventions, library choices
> │   ├── web.instructions.md        React/TS patterns, TanStack, Zustand
> │   ├── ci.instructions.md         GitHub Actions conventions
> │   ├── deploy.instructions.md     Terraform + Flux conventions
> │   └── docs.instructions.md       Documentation standards
> ├── skills/                        (empty — created after first manual impl)
> ├── agents/
> │   ├── architect.agent.md         Read-only planning (tools: read, search, agent, todo)
> │   ├── scaffolder.agent.md        Full access, SKILL-gated code generation
> │   └── reviewer.agent.md          Read-only convention checking
> └── hooks/
>     ├── copilot-hooks.json         Hook definitions
>     └── scripts/
>         ├── security-gate.sh       Blocks destructive commands, protects arch docs
>         ├── audit-log.sh           Logs all tool calls to JSONL
>         └── log-session.sh         Logs session start
> ```
>
> Key principle: **I build it first. Then I teach Copilot how to replicate it.**

**What to say:**

- This is a real project — an AI-powered drinks suggestion platform built as a modular monolith.
- The setup took about a day to think through and configure.
  The conventions and "Why Not X" decisions were already in my head — I just had to write them down.
- The SKILL directory is empty on purpose.
  I haven't built the first endpoint yet — when I do, I'll do it manually, then write a SKILL so the scaffolder can create the next ones.
- The agents have hard tool restrictions.
  The architect literally cannot edit a file — it's not a suggestion, it's enforced.
- The per-path instructions mean I never have to think about context switching.
  Touch a server file → .NET rules kick in.
  Touch a web file → React rules kick in.
  Automatic.

---

## Slide 14 — Easy Wins

**On the slide:**

> **Low effort, high value, low risk — start here.**
>
> | Use case                            | Why it's safe                                                                                 | Setup needed                         |
> | ----------------------------------- | --------------------------------------------------------------------------------------------- | ------------------------------------ |
> | **Writing tests for existing code** | Tests must pass without changing source code — green or it's wrong                            | None — just ask Copilot              |
> | **First-line code review**          | Agent finds surface issues (naming, patterns, missing checks) — human reviewer still required | `reviewer` agent or just inline chat |
> | **Boilerplate scaffolding**         | Folder structure, empty classes, config files from templates — predictable output             | SKILL file for the pattern           |
> | **Commit messages**                 | Reads the diff, suggests semantic commit — worst case you edit it                             | Built-in                             |
> | **Explaining unfamiliar code**      | Read-only — zero risk of breaking anything                                                    | None                                 |
>
> Risk is never zero — but these are the safest starting points.
> Every output still requires human review.

**What to say:**

- If you're skeptical, start here.
  These are the use cases where the downside is minimal and the upside is significant.
- Tests: tell Copilot "write tests for this class."
  The constraint is powerful — if the tests don't pass without changing the source, they're wrong.
  You verify by running them.
  This alone can save hours per week.
- First-line review: the agent catches the things humans miss because they're boring — inconsistent naming, wrong assertion library, missing `sealed` keyword.
  It doesn't replace human review, it _surfaces the easy stuff_ so the human can focus on logic and design.
- Boilerplate: the most tedious part of development — creating 5 files with the right namespaces, registrations, and structure.
  After you've done it once, a SKILL file makes this instant.
- None of these require trusting the AI with critical decisions.
  They're all verifiable.

---

## Slide 15 — Extra Thoughts

**On the slide:**

> **Things I've learned the hard way**
>
> - **Agent output can NEVER be trusted 100%.** Always double-check anything important.
>   The more confident the AI sounds, the more carefully you should verify.
> - **Plan first, always.** If you can't explain what you want in a sentence, Copilot can't build it right.
>   A 5-minute plan saves an hour of fixing AI-generated spaghetti.
> - **Don't let the tool control you.** Copilot suggests — you decide.
>   If it pushes back or goes in a different direction, that's the AI's agenda, not yours.
>   Reset, re-prompt, or do it manually.
> - **"No Vibe Coding" is a discipline, not a restriction.** The SKILL gate feels slow at first.
>   Then you realize you understand every line in your codebase because _you_ wrote the first instance.
> - **Guidance is not enforcement.** Instructions tell the agent what it _should_ do.
>   Hooks and tool restrictions tell it what it _can_ do. If something truly must not happen, don't ask nicely — enforce it.
> - **Copilot's training data has opinions.** It will default to the most popular library, not the right one.
>   Without per-path instructions, it'll use MediatR, Jest, React Router — because those have more training examples.
>   Your instructions override its defaults.
> - **Start small, expand deliberately.** `copilot-instructions.md` → per-path → agents → skills.
>   Don't set up everything on day one.
>   Add layers as you discover where Copilot needs more guidance.

**What to say:**

- These are personal observations, not official guidance.
- The "never 100% trusted" point is especially important for junior developers.
  The AI generates plausible code — it compiles, it might even pass tests — but it can be subtly wrong in ways that only show up later.
- "Don't let the tool control you" — this sounds abstract but it's very practical.
  If you ask Copilot to refactor a function and it rewrites half the file, _stop_.
  You asked for a refactor, not a rewrite.
  Control the scope.
- The training data point is key for enterprise teams.
  Your tech stack choices are deliberate — the AI doesn't know that unless you tell it.

---

## Slide 16 — Workshop Preview

**On the slide:**

> **Next step: hands-on workshop**
>
> We'll build a new product together using this setup:
>
> 1. Set up `copilot-instructions.md` with project rules
> 2. Create per-path instructions for the tech stack
> 3. Build the first feature manually
> 4. Write a SKILL file from the pattern
> 5. Use `@scaffolder` to generate the second feature
> 6. Run `@reviewer` to check conventions
> 7. Set up an agentic workflow for automated PR review
>
> Bring your VS Code + GitHub Copilot subscription.

**What to say:**

- Everything in this presentation was theory — the workshop is where we apply it.
- We'll start from scratch, make deliberate tech choices, and encode them so Copilot follows them.
- By the end, you'll have a working product with a full AI-assisted workflow — from planning to code generation to automated review.

---

## Appendix — Useful References

| Resource                 | URL                                                                                                               |
| ------------------------ | ----------------------------------------------------------------------------------------------------------------- |
| Custom agents docs       | `https://docs.github.com/en/copilot/customizing-copilot/custom-agents`                                            |
| Custom instructions docs | `https://docs.github.com/en/copilot/customizing-copilot/adding-repository-custom-instructions-for-github-copilot` |
| Agent workflows          | `https://docs.github.com/en/copilot/using-github-copilot/using-agentic-workflows`                                 |
| MCP in VS Code           | `https://code.visualstudio.com/docs/copilot/chat/mcp-servers`                                                     |
| Context7 MCP             | `https://github.com/upstash/context7`                                                                             |
| Hooks reference          | `https://docs.github.com/en/copilot/reference/hooks-configuration`                                                |
| Copilot CLI              | `https://github.com/github/copilot-cli`                                                                           |
| Copilot Cookbook/SDK     | `https://github.com/copilot/cookbook`                                                                             |
| Awesome Copilot          | `https://github.com/pchunduri6/awesome-copilot`                                                                   |

# AI-Assisted Development & Spec-Driven Development — Presentation

30-minute presentation + 30-minute discussion.
Goal: Share practical AI experience, introduce SDD as a structured approach, and propose a PoC effort for the team's brownfield project.
Audience: Team with 2–3 weeks of AI coding experience.

---

## PART 1 — Practical AI Insights (~12–15 min)

---

### Slide 1 — Title

**On the slide:**

> **AI-Assisted Development: What I've Learned & Where We Should Go**
> Practical experience, honest observations, and a proposal for structured AI adoption

**What to say:**

- This is a two-part talk. First, I'll share what I've learned using AI intensively over the past ~6 months, with some honest observations that go beyond the usual "write good prompts" advice.
- Second, I'll introduce Spec-Driven Development (SDD) — a structured approach that addresses the core problems of AI-assisted coding — and propose we allocate effort for a proof of concept.

---

### Slide 2 — My Experience

**On the slide:**

> **Two projects, two approaches**
>
> **ProperTea** (~6 months)
>
> - Learning modern cloud tooling: Kubernetes + ArgoCD on self-hosted Talos cluster (3 VMs), .NET microservices, Wolverine, Postgres + Marten (event sourcing), Grafana o11y stack
> - GitHub Copilot with custom instructions (heavy `applyTo` usage), SKILL files.
> - AI was a **learning accelerator** — explaining unfamiliar concepts, scaffolding boilerplate, reviewing decisions
>
> **AlCopilot** (starting)
>
> - AI-powered drinks suggestion platform — modular monolith (.NET 10 + Aspire)
> - Focus: Spec-Driven Development through OpenSpec, CI/CD pipelines from day one (semantic commits, quality gates, Release Please)
> - Engineering process is the primary experiment, not just the product

**What to say:**

- ProperTea was about learning technologies I hadn't used before. AI helped enormously as a **learning companion** — not generating code blindly, but explaining concepts, suggesting approaches, and helping me understand unfamiliar ecosystems.
- AlCopilot is where I'm experimenting with SDD. It's early, but the structured approach already feels different from "just asking Copilot to implement things."
- Both are non-commercial. I'm sharing what worked and what didn't, not selling a proven methodology.

---

### Slide 3 — What AI Is Genuinely Good At

**On the slide:**

> **High-value, high-confidence use cases**
>
> | Use case                                     | Why it works well                                                                                                     |
> | -------------------------------------------- | --------------------------------------------------------------------------------------------------------------------- |
> | **Interactive reasoning ("rubber ducking")** | You explain your thinking, AI gives intelligent feedback. Like a colleague who's always available and never impatient |
> | **Documentation**                            | Consistently well-structured, articulate, properly formatted. AI writes better docs than most of us                   |
> | **Boilerplate with examples**                | "Create another table + DAL + REST like this existing one" — saves a day of mechanical work                           |
> | **First-line code review**                   | Catches obvious issues, code smells, naming inconsistencies. Frees the human reviewer for logic and design            |
> | **Writing tests for existing code**          | Clear constraint: tests must pass without changing source. Wrong test = obviously wrong                               |
> | **Explaining unfamiliar code**               | Read-only, zero risk, high value for onboarding or working with legacy code                                           |

**What to say:**

- These are the areas where I've consistently gotten value. The common thread: either the output is easily verifiable (tests, boilerplate), or the AI is in read-only mode (explaining, reviewing) where mistakes have near-zero cost.
- The rubber-ducking point deserves emphasis. Even when AI doesn't give you the answer, **the act of structuring your thoughts for the prompt** often reveals the solution. It's a thinking aid, not just a code generator.
- The boilerplate point matters for us specifically. Everyone on the team knows how to create a new table, build the data access layer, add REST endpoints. But it still takes time. AI eliminates the mechanical part so you can focus on the interesting decisions.

---

### Slide 4 — What AI Struggles With

**On the slide:**

> **Real problems, not hypothetical risks**
>
> | Problem                          | What actually happens                                                                                                                                                                              |
> | -------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
> | **Hallucinations are confident** | AI states non-existent library features as fact. You believe it because it sounds authoritative. Especially dangerous during planning — selecting a library based on capabilities it doesn't have  |
> | **Shortcutting over solving**    | When a test fails, AI may modify or delete the test instead of fixing the code. "I couldn't make this pass, so I changed the assertion"                                                            |
> | **Knowledge cutoff is real**     | Example: multiple agents recommended MassTransit despite my explicit "no commercial products" rule. MassTransit's licensing change was recent enough to be after training data                     |
> | **Training data biases**         | AI defaults to the most popular library, not the right one. Without explicit instructions, it will suggest MediatR over Mediator, Jest over Vitest, React Router over TanStack Router — every time |
> | **Scope creep**                  | Ask AI to refactor a function, it rewrites half the file. Ask for a bugfix, it "improves" adjacent code. The more it does, the more review burden on you                                           |

**What to say:**

- These aren't edge cases — they happen regularly. The hallucination problem is the most insidious because the AI sounds equally confident whether it's right or wrong. I've caught it inventing API methods that don't exist, suggesting library features that were never implemented.
- The shortcutting behaviour is what you get when AI optimises for "green tests" rather than "working software." This is why reviewing AI-generated code matters more, not less — the AI will take shortcuts a human wouldn't.
- The knowledge cutoff is a practical concern for us. Our dependencies, licensing decisions, internal conventions — the AI knows nothing about these unless we tell it explicitly.
- **The common thread: AI is a tool that optimises for completing the task, not for correctness.** Our job is to set up guardrails that keep the optimisation aligned with what we actually want.

---

### Slide 5 — Non-Obvious Observations & Tips

**On the slide:**

> **Things that aren't in the "how to use AI" tutorials**
>
> 1. **Prompts are encouragements, not enforcement.**
>    "Use library X" in your instructions is a suggestion. The AI will _try_, but nothing prevents it from ignoring the instruction if the context gets complex.
> 2. **"This model is better" is not a permanent truth.**
>    Providers update and throttle models weekly. What works best today may not next week. Share experiences as a team continuously.
> 3. **"Lost in the middle" is real.**
>    AI attention is strongest at the start and end of context. Instructions buried in the middle get forgotten. Keep critical rules at the top.
> 4. **Context is everything — and less is more.**
>    Only load what's relevant. No 500-line AGENTS.md covering every convention. Scoped, per-area instructions that activate only when needed.
> 5. **Steering > correcting.**
>    It's easier to guide AI in the right direction upfront than to fix its output after. Plan first. Constrain the scope. Provide examples.
> 6. **Long conversations degrade.**
>    Context windows compress. Core instructions get diluted. Start fresh conversations for new tasks rather than continuing endlessly.

**What to say:**

- Point 1 is crucial. Many teams set up instruction files and assume the AI will always follow them. It usually does — but "usually" isn't "always." True enforcement requires technical constraints (tool restrictions in custom agents, hooks), not just better prompts.
- Point 3 — "lost in the middle" — is documented in AI research. The practical implication: structure your instructions so the most important rules are at the top of the file, and keep files short.
- Point 4 — this was one of my biggest learnings. Early on I had massive instruction files. The AI would follow some rules and ignore others seemingly at random. When I split into scoped, per-path files that only activate for relevant code, compliance improved dramatically.
- Point 6 — you may have noticed that quality degrades after several back-and-forths. This isn't imaginary. The tool compresses earlier messages to fit the context window, and your earlier instructions can get summarized away. Fresh context = better results.

---

## PART 2 — Spec-Driven Development (~12–15 min)

---

### Slide 6 — The Problem SDD Solves

**On the slide:**

> **What happens without specs?**
>
> ```
> Developer: "Add user authentication"
>     ↓
> AI: generates code
>     ↓
> Developer: reviews... looks OK... ships
>     ↓
> Next developer: "Why is it built this way?"
> Next change: breaks assumptions from first change
> Three months later: inconsistent patterns everywhere
> ```
>
> The issue isn't that AI generates bad code.
> The issue is that **intent lives in chat history that disappears.**
>
> - No record of _why_ this approach was chosen
> - No agreement on _what_ the feature should do
> - No way to verify _if_ the implementation matches intent
> - No way for the _next_ change to understand prior decisions

**What to say:**

- This is the core problem. Even with good instructions, every AI session is an independent event. The AI doesn't remember what you agreed on last week. Conventions help with _how_ code should look, but they don't capture _what_ you're building and _why_.
- In our brownfield project, this matters even more. With 8 years of accumulated decisions (and undocumented assumptions), adding AI to the mix without structure just accelerates the chaos.
- SDD addresses this by creating a **persistent specification layer** that survives between sessions, captures intent, and gives the AI (and the next developer) the context it needs.

---

### Slide 7 — SDD Tiers: Finding Our Level

**On the slide:**

> **Three levels of spec-driven development**
>
> | Tier              | Definition                                                                                                | When to use                                                          | Example                                                                    |
> | ----------------- | --------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- | -------------------------------------------------------------------------- |
> | **Spec-sourced**  | Extract specs from existing code/behaviour. Reverse-engineer what the system does today.                  | Starting SDD on a brownfield codebase with no existing documentation | "Before we change auth, let's document how it currently works"             |
> | **Spec-anchored** | Specs exist for key areas. New changes reference them. Not everything is specced, but critical paths are. | Brownfield with incremental improvement. Our sweet spot.             | "New features get specs. Existing features get specced when we touch them" |
> | **Spec-first**    | All work begins with specs. Code is generated from specs. Specs are the source of truth.                  | Greenfield or mature SDD adoption                                    | "No code without a spec. Changes are spec deltas, not code patches"        |
>
> **Note:** If you use AI agents to plan-then-implement (rather than vibe-coding), you're already doing informal spec-first — your specs just live in chat and vanish when the session ends.

**What to say:**

- These tiers aren't rigid categories — they're a spectrum. The point is to find the right entry point for your situation.
- **Spec-sourced**: Start by documenting what exists. This is the hardest tier because you're reverse-engineering intent from code that may not clearly express it. But it's the necessary first step for legacy areas you plan to modify.
- **Spec-anchored**: This is where I'd recommend we aim for our project. We don't spec everything — that would be months of effort with no code changes. Instead, we spec what we're changing, and specs accumulate over time.
- **Spec-first**: The ideal endpoint. Every new feature starts with specs. Changes are expressed as spec modifications, not code patches. AI implements from specs, and specs persist as documentation.
- The last point matters: if your team already uses "plan mode" or asks AI to "plan first, then implement" — you're doing spec-first with ephemeral specs. SDD just makes those specs persistent and reviewable.

---

### Slide 8 — Tools: OpenSpec vs Spec Kit

**On the slide:**

> **Two main open-source SDD tools**
>
> |                    | **OpenSpec** (Fission AI)                                                                        | **Spec Kit** (GitHub)                                                 |
> | ------------------ | ------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------- |
> | Stars              | ~37k                                                                                             | ~85k                                                                  |
> | Language           | TypeScript / Node.js                                                                             | Python                                                                |
> | Philosophy         | Lightweight, fluid, brownfield-first                                                             | Comprehensive, structured, phase-driven                               |
> | Spec format        | Delta-based (ADDED / MODIFIED / REMOVED)                                                         | Full specs per feature branch                                         |
> | Workflow           | `propose → specs → design → tasks → implement → verify → archive`                                | `constitution → specify → plan → tasks → implement`                   |
> | Key differentiator | Changes as folders with delta specs. Multiple parallel changes. Explore mode for unspecced code. | Constitutional principles. Extensions/presets ecosystem. Phase gates. |
> | Brownfield fit     | Excellent — delta specs let you spec only what you change                                        | Good — brownfield walkthroughs exist, but heavier ceremony            |
> | Setup              | `npm install -g @fission-ai/openspec && openspec init`                                           | `uv tool install specify-cli && specify init --ai copilot`            |
> | Agent support      | 20+ tools (Copilot, Codex, Claude, Cursor...)                                                    | 25+ agents                                                            |
>
> **My recommendation: OpenSpec for us.**

**What to say:**

- Both tools are MIT-licensed, actively maintained, and support our tooling (Copilot + Codex).
- I recommend OpenSpec for us for three reasons:
  1. **Delta-based specs** — in a brownfield project, you don't want to write a complete spec for the entire auth system before you can change the login flow. OpenSpec lets you spec _just the change_ using ADDED/MODIFIED/REMOVED sections.
  2. **Explore mode** — when you encounter unspecced code and need to understand it before changing it, `/explore` is designed exactly for this. It's the spec-sourced tier built into the tool.
  3. **Lower ceremony** — Spec Kit is powerful (extensions, presets, constitutions), but for a team new to SDD, OpenSpec's lighter approach is less likely to feel like overhead.
- Spec Kit is worth revisiting if we outgrow OpenSpec or want tighter CI/CD integration. But for a PoC, start lean.

---

### Slide 9 — OpenSpec Core Concepts

**On the slide:**

> **How OpenSpec organises work**
>
> ```
> openspec/
> ├── specs/                          ← Source of truth: how the system works now
> │   ├── auth/spec.md
> │   ├── payments/spec.md
> │   └── ...
> └── changes/                        ← Active work: proposed modifications
>     ├── add-two-factor-auth/
>     │   ├── proposal.md             ← Why and what (intent + scope)
>     │   ├── design.md               ← How (technical approach)
>     │   ├── tasks.md                ← Implementation checklist
>     │   └── specs/                  ← Delta specs (ADDED/MODIFIED/REMOVED)
>     │       └── auth/spec.md
>     └── archive/                    ← Completed changes (preserved history)
>         └── 2026-03-15-fix-login/
> ```
>
> **Key concepts:**
>
> - **Specs** = source of truth for current system behaviour
> - **Changes** = proposed modifications, self-contained folders
> - **Delta specs** = what's changing relative to current specs (not full rewrites)
> - **Artifacts** = proposal → specs → design → tasks (the "thinking chain" for each change)
> - **Archive** = when done, deltas merge into main specs, change is preserved for history

**What to say:**

- The mental model: **specs describe what exists**, **changes describe what's being modified**.
- Each change is self-contained in a folder. You can have multiple changes in progress simultaneously without conflicts — they're like lightweight feature branches for specifications.
- Delta specs are the key innovation for brownfield. Instead of writing "here's the entire auth spec," you write "here's what's ADDED, MODIFIED, or REMOVED." This means you can start speccing changes _today_ even if the full system isn't documented.
- When a change is complete and implemented, you archive it — the deltas merge into the main specs (updating the source of truth) and the change folder moves to archive (preserving the _why_ behind the change).
- This creates a virtuous cycle: every change you make adds to the system's specification. Over time, your spec coverage grows organically.

---

### Slide 10 — OpenSpec Workflow (What a Typical Session Looks Like)

**On the slide:**

> **Scenario 1: New feature**
>
> ```
> /opsx:propose add-password-reset
>   → Creates change folder with proposal, specs, design, tasks
>   → Review and refine artifacts
> /opsx:apply
>   → AI implements tasks, checking them off
> /opsx:verify
>   → Verify implementation matches specs
> /opsx:archive
>   → Delta specs merge into main specs
> ```
>
> **Scenario 2: Modify an existing specced feature**
>
> ```
> /opsx:propose change-session-timeout
>   → Proposal references existing auth/spec.md
>   → Delta spec: MODIFIED session timeout from 30 to 15 min
>   → Design + tasks generated with existing context
> ```
>
> **Scenario 3: Modify unspecced code (our reality today)**
>
> ```
> /opsx:explore
>   → "How does login currently work in our codebase?"
>   → AI investigates, produces understanding
>   → Use findings to create an initial spec (spec-sourced)
> /opsx:propose improve-login-validation
>   → Now the change has context (the spec we just created)
> ```
>
> **Scenario 4: Mid-implementation changes**
>
> ```
> During /opsx:apply, requirements change
>   → Update the proposal/design/specs artifacts
>   → Continue implementation with updated context
>   → No rigid phase gates — update any artifact anytime
> ```

**What to say:**

- These four scenarios cover what I expect to be our real workflow.
- Scenario 1 is the textbook case — clean, new feature, full workflow. This is where SDD shines brightest.
- Scenario 2 shows the power of delta specs — you're modifying existing behaviour, and the change precisely documents _what_ changed and _why_.
- Scenario 3 is the most relevant for us today. When you need to modify code that has no spec, start with `/explore` to understand it. This produces an initial spec that captures current behaviour. Then make your change against that baseline. It's spec-sourced → spec-anchored in one session.
- Scenario 4 addresses the "isn't this waterfall?" concern. OpenSpec is explicitly _not_ phase-gated. You can update any artifact at any time. Requirements changed mid-implementation? Update the spec, adjust the design, continue. The specs reflect reality, not a frozen plan.

---

### Slide 11 — What This Means For Us (The Proposal)

**On the slide:**

> **PoC proposal: 2–3 sprints of structured experimentation**
>
> **What we'd do:**
>
> 1. Set up OpenSpec in our repository
> 2. Pick 2–3 upcoming features of varying complexity
> 3. Use the full SDD workflow: explore existing code → create specs → propose changes → implement → archive
> 4. Compare against our current approach (effort, quality, regression rate, knowledge transfer)
>
> **What we're NOT doing:**
>
> - Spec the entire existing system (that's months of work for uncertain value)
> - Force every change through SDD (focus on new features + high-risk modifications)
> - Commit to a specific tool permanently (if OpenSpec doesn't fit, we evaluate alternatives)
>
> **What I'd need from the team:**
>
> - Willingness to try the workflow on selected features
> - Honest feedback: what felt useful, what felt like overhead
> - Brief weekly sync to share learnings

**What to say:**

- I want to be honest: my experience with SDD is limited. I've set it up, I've run through the workflow, and the approach _makes sense_ — but I haven't proven it at scale. That's exactly why I'm proposing a PoC, not a wholesale adoption.
- The value proposition is straightforward: SDD addresses the problems we all know we have. Requirements that live in Slack threads and get lost. Changes that break assumptions from previous changes. New team members who can't understand _why_ code is the way it is.
- The risk is low: we try it on a few features, measure the results, and decide as a team whether to continue. If it doesn't work, we've lost a small investment of effort. If it does, we have a workflow that compounds in value over time.
- The PoC will also help us understand what tier makes sense for our project. Maybe spec-anchored is enough. Maybe we find that spec-first for new features is worth the extra upfront effort. We'll know after the PoC.

---

### Slide 12 — Demo Preview

**On the slide:**

> **[Live demo — ~5 min]**
>
> We'll walk through in our actual codebase:
>
> 1. **Create a new feature** — full `propose → apply → archive` workflow
> 2. **Modify the newly created feature** — show delta spec in action
> 3. **Approach unspecced code** — use `/explore` to understand, then create spec
> 4. **Handle mid-change requirements shift** — update artifacts during implementation
>
> _Demo will be on [project name] repo._

**What to say:**

- [Run the demo live using the team's actual codebase. Keep it short — 1-2 minutes per scenario.]
- [Narrate as you go: "I'm starting with `/explore` because this code has no spec. The AI is reading the codebase and producing an understanding of how login currently works. Now I can create a change against that baseline..."]
- [Emphasis throughout: the specs persist. After the demo, the team can open the `openspec/` folder and see exactly what was proposed, designed, and changed. No chat history archaeology.]

---

### Slide 13 — Summary & Discussion

**On the slide:**

> **Key takeaways**
>
> 1. AI is a powerful tool — but without structure, it amplifies chaos as well as productivity
> 2. Context is everything: scoped instructions, relevant examples, and persistent specs beat massive prompts
> 3. SDD makes AI-generated plans and decisions **persistent, reviewable, and referenceable**
> 4. For our brownfield project: start spec-anchored, new features get specs, existing code gets specced when touched
> 5. Proposal: allocate effort for a 2–3 sprint PoC with OpenSpec
>
> **Discussion:**
>
> - Does this workflow feel like it would fit our team?
> - What concerns do you have?
> - Which upcoming features would be good candidates for the PoC?

**What to say:**

- The core message: AI adoption isn't just about which tool to use or which model is best. It's about **having a structure that keeps the AI aligned with your intent** and **capturing decisions so they don't get lost**.
- SDD is one answer to that. I'm not claiming it's the only answer, or that OpenSpec is the perfect tool. But the problem — ephemeral context, lost decisions, inconsistent AI outputs — is real, and we should invest in solving it.
- Open for discussion. Tell me what resonates, what doesn't, and what you'd want to see in the PoC.

---

## Appendix — References

| Resource                   | URL                                                                            |
| -------------------------- | ------------------------------------------------------------------------------ |
| OpenSpec (Fission AI)      | `https://github.com/Fission-AI/OpenSpec`                                       |
| OpenSpec concepts          | `https://github.com/Fission-AI/OpenSpec/blob/main/docs/concepts.md`            |
| Spec Kit (GitHub)          | `https://github.com/github/spec-kit`                                           |
| SDD methodology (Spec Kit) | `https://github.com/github/spec-kit/blob/main/spec-driven.md`                  |
| OpenSpec website           | `https://openspec.dev`                                                         |
| "Lost in the middle" paper | Liu et al., 2023 — "Lost in the Middle: How Language Models Use Long Contexts" |

## Appendix — Detailed Comparison Notes (Not in slides, for Q&A)

### On "Copilot quality degraded"

This perception is common and partially grounded. What's likely happening:

- **Model rotations**: providers update models frequently, sometimes with regressions for specific tasks.
- **Context management**: the tool (VS Code chat) manages the context window, compressing earlier messages. After several exchanges, your initial instructions may be summarized or truncated. This is the tool's behaviour, not the model's.
- **Token limits**: newer pricing tiers introduce rate limits and throttling that can affect response quality under load.
- **Practical advice**: start fresh conversations for new tasks. Don't try to do everything in one session.

### On model selection

- Claude models (Opus 4.6) tend to have stronger reasoning and better context preservation in longer conversations. The compression algorithms seem more effective at retaining core context.
- Codex (OpenAI's agent) is optimised for code implementation tasks — it can feel "better" for straight coding because that's its primary optimisation target.
- Neither is universally better. Use Claude for planning, design, and review. Use Codex for implementation and bulk code changes.
- This will change. Have a standing team conversation (monthly or biweekly) to share observations.

### On OpenSpec vs Spec Kit — deeper detail

**OpenSpec strengths for brownfield:**

- Delta specs (ADDED/MODIFIED/REMOVED) mean you don't need to write a complete spec before making changes
- `/explore` mode is purpose-built for understanding unspecced code
- Changes-as-folders allow parallel work without conflicts
- Lightweight — no Python dependency, just Node.js
- Config is a single YAML file with project context

**Spec Kit strengths (relevant if we outgrow OpenSpec):**

- Constitution concept — immutable architectural principles that gate all work
- Extensive extension ecosystem (Jira integration, V-Model testing, code review, etc.)
- Preset system for customizing workflows per project
- More prescriptive — which helps teams that want stricter guardrails
- Backed by GitHub directly

**Why not Spec Kit for PoC:**

- Python dependency + `uv` tool management adds complexity to onboarding
- Heavier ceremony (constitution before first spec)
- Extension ecosystem is powerful but overwhelming for a first evaluation
- Feature-branch model (one feature per branch) is less flexible than OpenSpec's folder-per-change model for our workflow

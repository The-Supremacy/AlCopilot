# AI-Assisted Development & Spec-Driven Development — Presentation

30-minute presentation + 30-minute discussion.
Goal: Share practical AI experience, introduce SDD as a structured approach, and propose allocating effort for a PoC.
Audience: Team with 2–3 weeks of AI coding experience.

---

## PART 1 — Practical AI Insights (~12–15 min)

---

### Slide 1 — Title

**On the slide:**

> **AI-Assisted Development: What I've Learned & Ways to Go**
> Practical experience, honest observations, and a structured approach to AI adoption

**What to say:**

- This is a two-part talk. First, I'll share what I've learned using AI intensively over the past ~6 months — some honest observations that go beyond the usual "write good prompts" advice.
- Second, I'll introduce Spec-Driven Development — a structured approach to working with AI — and explain why I think it's worth our time to evaluate.
- Before I start — I know some of the things I'll mention here are already familiar to you. Please bear with me where that's the case. Also, I'd genuinely appreciate it if you point out anything I say that's wrong or outdated. Things change fast in this space, AI is a black box in many regards, and my personal experience may not represent the full picture. Let's treat this as a conversation, not a lecture.

---

### Slide 2 — My Experience

**On the slide:**

> **Two projects, different focus**
>
> **Project 1** (~6 months)
>
> - Learning modern cloud tooling from scratch, including self-hosted Kubernetes cluster (3 VMs, Talos + ArgoCD), .NET microservices, event sourcing (Postgres + Marten), full o11y stack (Grafana)
> - AI wasn't just chat + code editing — it helped me debug live through kubectl logs, ArgoCD sync failures, networking issues. Things I had zero prior experience with
> - GitHub Copilot with layered setup: custom instructions (heavy `applyTo` scoping), SKILL files for repeatable patterns
>
> **Project 2** (starting)
>
> - .NET 10 modular monolith with Aspire
> - Focus on engineering process: SDD with OpenSpec, full CI/CD from day one (semantic commits, quality gates, Release Please)
> - Also exploring working with LLMs directly — locally through Ollama via Aspire, cloud through Azure AI Foundry

**What to say:**

- Project 1 was about learning technologies I hadn't touched before. I want to emphasise: AI helped me build a self-hosted Kubernetes cluster with 3 nodes without any prior k8s knowledge. Not just writing Helm charts — live debugging through kubectl, reading ArgoCD logs, diagnosing networking issues between nodes. This goes well beyond code editing.
- AI was a **learning accelerator** — explaining unfamiliar concepts, scaffolding boilerplate, and reviewing my decisions against established practices. It's not just a code generator.
- Project 2 is where I'm experimenting with Spec-Driven Development through OpenSpec. It's early, but the structured approach already feels different from "just asking Copilot to implement things." I'll explain the SDD idea behind it later.
- For Project 2 I also want to explore working with LLMs more directly — running them locally, using them through cloud services. The domain itself is a bit of a joke between friends, but the engineering setup is serious.
- Both are non-commercial. I'm sharing what worked and what didn't, not selling a proven methodology.

---

### Slide 3 — My AI Workflow (Project 1)

**On the slide:**

> **How I work with AI — a phased approach**
>
> | Phase                       | Model                     | What happens                                                                                       |
> | --------------------------- | ------------------------- | -------------------------------------------------------------------------------------------------- |
> | **1. Reasoning & Planning** | Opus 4.6 (Claude)         | Architecture decisions, domain modelling, feature description documents. AI as a thinking partner. |
> | **2. Development Plan**     | Opus 4.6 or Sonnet 4.6    | Break down into implementation steps. Design doc, sequence of changes, dependencies.               |
> | **3. Implementation**       | Sonnet 4.6 or Codex (GPT) | Actual code generation from the plan. Mechanical translation, not creative decisions.              |
>
> **Why separate phases?**
>
> - Reasoning models (Opus) are better at planning and design — they consider trade-offs, alternatives, edge cases
> - Implementation models (Codex/Sonnet) are better at following a clear plan efficiently
> - Separating phases means you can verify the plan _before_ committing to code

**What to say:**

- This was my personal workflow for Project 1, which evolved through trial and error. The key insight: **different models have different strengths**, and using the right model for the right phase produces better results than using one model for everything.
- Phase 1 — reasoning and planning — is where Opus shines. I'd describe my intent, and we'd iterate on the architecture, discuss trade-offs, consider alternatives. The output is a document, not code.
- Phase 2 — development planning — can use either Opus or Sonnet. This is about breaking the design into concrete implementation steps. Less creative, more structured.
- Phase 3 — implementation — is where Codex or Sonnet models work well. They're following a clear, reviewed plan at this point. The creative decisions were already made and validated.
- For our actual project, I'd suggest a similar phased approach. Plan and design with a reasoning model, then hand off to an implementation model. SDD, which I'll cover later, formalizes this separation.
- One personal observation on models: Claude's (Anthropic) compression algorithms seem better at preserving core context in longer conversations, which makes Opus better for extended planning sessions. Codex models feel stronger at steering — they follow explicit instructions more reliably during implementation. But this changes frequently. What matters is having the team share observations regularly, not committing to "model X is always better."

---

### Slide 4 — What AI Is Genuinely Good At

**On the slide:**

> **High-value, high-confidence use cases**
>
> | Use case                                     | Why it works well                                                                                                                                                |
> | -------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
> | **Interactive reasoning ("rubber ducking")** | You explain your thinking, AI gives intelligent feedback. Like a colleague who's always available and never impatient                                            |
> | **Documentation**                            | Consistently well-structured, articulate, properly formatted. Also produces docs optimised for AI consumption — which matters for instructions and skills files  |
> | **Boilerplate with examples**                | "Create another table + DAL + REST like this existing one" — saves a day of mechanical work                                                                      |
> | **First-line code review**                   | Catches obvious issues, code smells, naming inconsistencies. Frees the human reviewer for logic and design                                                       |
> | **Writing tests for existing code**          | Clear constraint: tests must pass without changing source. Wrong test = obviously wrong. Tests are boilerplate too — you write one, then need a dozen variations |
> | **Explaining unfamiliar code**               | Read-only, zero risk, high value for onboarding or working with legacy code                                                                                      |

**What to say:**

- These are the areas where I've consistently gotten value. The common thread: either the output is easily verifiable (tests, boilerplate), or the AI is in read-only mode (explaining, reviewing) where mistakes have near-zero cost.
- The rubber-ducking point deserves emphasis. Even when AI doesn't give you the answer, **the act of structuring your thoughts for the prompt** often reveals the solution. It's a thinking aid, not just a code generator.
- On documentation — this is worth calling out specifically. AI doesn't just write good docs for humans. It also knows how to structure documentation that's optimised for AI to read later. If you write a custom instructions file or a SKILL file yourself, it's always worth asking an AI agent to review and optimise it. The formatting, keyword placement, and structure all affect how well future AI sessions pick up on those rules.
- The boilerplate point matters for us specifically. Everyone on the team knows how to create a new table, build the data access layer, add REST endpoints. But it still takes time. AI eliminates the mechanical part.
- On tests — in practice, tests are boilerplate too. You write one well-crafted test for a method, then you need a dozen variations for different inputs and edge cases. Data-driven tests aren't always the right choice because you often want separate test methods for easier future modification. AI excels at this kind of "write twelve variations of this pattern" work.

---

### Slide 5 — What AI Struggles With

**On the slide:**

> **Real problems, not hypothetical risks**
>
> | Problem                              | What actually happens                                                                                                                                          |
> | ------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------- |
> | **Hallucinations are confident**     | AI states non-existent library features as fact. Especially dangerous during planning — selecting a library based on capabilities it doesn't have              |
> | **Task completion over correctness** | When a test fails, AI may modify or delete the test instead of fixing the code. It optimises for "done," not "right"                                           |
> | **Knowledge cutoff**                 | Multiple agents recommended MassTransit despite explicit "no commercial products." Its licensing change was too recent for training data                       |
> | **Training data biases**             | Defaults to the most popular library, not the right one. Without instructions: MediatR over Mediator, Jest over Vitest — every time                            |
> | **Scope creep**                      | Ask to refactor a function, it rewrites half the file. The more it does, the more review burden on you                                                         |
> | **UI building is mostly blind**      | Backend: write tests, verify output. Frontend: AI can't see the rendered result. You describe what's wrong, provide screenshots, iterate                       |
> | **Confirmation bias**                | If you lean toward an option, AI will happily confirm it's the correct one. It optimises for agreement, not truth                                              |
> | **Silent tool failures**             | If a CLI tool isn't available, AI may silently implement functionality itself instead of flagging the issue. You only catch it if you read the reasoning trace |

**What to say:**

- These aren't edge cases — they happen regularly.
- The hallucination problem is the most insidious because the AI sounds equally confident whether it's right or wrong. I've caught it inventing API methods that don't exist.
- "Task completion over correctness" — this is the broader version of the "deletes the test" problem. AI is fundamentally optimising for completing the task, not for providing the correct result. Every output needs verification.
- The UI point is worth acknowledging. Backend work has a tight feedback loop — tests pass or they don't. Frontend work lacks that. AI can't see what the page looks like. There's promising work happening with visual feedback tools and screenshot-aware agents, but today it's still a gap.
- Confirmation bias is subtle but dangerous. If you're evaluating two approaches and you phrase your question as "I'm thinking option A is better because X," the AI will almost certainly agree. Present options neutrally, or explicitly ask for counterarguments.
- The silent tool failure is something I encountered with more complex setups — custom instructions, SKILL files, specialised agents. If a tool or CLI command isn't accessible, the AI doesn't necessarily tell you. It may just decide to scaffold things manually. The only way to catch this is to read the reasoning trace, which is often a collapsed section in the chat, not something you'd naturally review.
- **The common thread: AI optimises for completing the task, not for correctness.** Our job is to set up guardrails that keep the optimisation aligned with what we actually want.

---

### Slide 6 — Non-Obvious Observations & Tips

**On the slide:**

> **Things that aren't in the "how to use AI" tutorials**
>
> 1. **Prompts are encouragements, not enforcement.**
>    AGENTS.md, instruction files, SKILL files — the AI will _try_ to follow them. But nothing physically prevents it from ignoring them, especially as context fills up.
> 2. **Context utilisation changes past ~50% capacity.**
>    Research ("Lost in the Middle," Liu et al. 2023) shows LLMs attend more strongly to the beginning and end of context. Past the halfway mark, instructions in the middle get less attention. This compounds with point 1.
> 3. **Context is everything — and less is more.**
>    Only load what's relevant. No monolithic AGENTS.md. Scoped, per-area instructions that activate only when needed. Keep critical rules at the top of files.
> 4. **Steering > correcting.**
>    Codex/GPT models seem particularly responsive to explicit steering ("use subagents for distinct parts," "follow this plan step by step"). Claude excels at maintaining context in longer reasoning sessions.
> 5. **Long conversations degrade differently by model.**
>    Claude compresses by summarising but tends to preserve core instructions longer. GPT models compress more aggressively — earlier context can lose fidelity faster. Both degrade; start fresh for new tasks.
> 6. **"This model is better" changes weekly.**
>    Providers update and throttle models constantly. Build a team habit of sharing observations, not a fixed model preference.
> 7. **Subagent prompting may help.**
>    Adding "feel free to use subagents" or "use subagents for distinct parts" in prompts can increase the probability the agent delegates complex work. It often does this on its own, but explicit encouragement seems to help. (Anecdotal — treat as a tip to try, not a proven technique.)
> 8. **Custom agents: useful but not urgent.**
>    Custom agents (specialised modes with restricted tools) help for recurring tasks that need specific tooling. Worth exploring once your workflow stabilises, not something to rush into.

**What to say:**

- Point 1 is crucial. A common misconception is that AGENTS.md or instruction files are "hard rules" the AI must follow. They're not — they're prompt-level guidance. The AI _should_ follow them, and usually does, but especially as context fills up (point 2), compliance drops. True enforcement requires technical constraints: tool restrictions in custom agents, hooks that block actions.
- Point 2 — the "Lost in the Middle" paper by Liu et al. demonstrated that when LLMs process long contexts, they attend most to the beginning and end, and significantly less to the middle. The practical implication: don't put your most important rules in the middle of a long file. Keep instruction files short. This also means that as your conversation grows and context utilisation climbs past ~50%, your early instructions — the ones at the "top" of the middle — get less attention. This is why point 3 matters so much.
- Point 3 was one of my biggest learnings. Early on I had massive instruction files. The AI would follow some rules and ignore others seemingly at random. When I split into scoped, per-path files that only activate for relevant code, compliance improved dramatically.
- Point 4 — steering vs correcting. It's easier to guide AI in the right direction upfront than to fix its output after. Codex models seem particularly responsive to explicit steering instructions. Plan first, constrain the scope, provide examples.
- Point 5 — the practical takeaway: for extended planning sessions, Claude models hold up better. For implementation tasks where you want the model to follow a clear plan, Codex/GPT models can be more reliable. Don't try to do everything in one session regardless.

---

## PART 2 — Spec-Driven Development (~12–15 min)

---

### Slide 7 — What Is SDD?

**On the slide:**

> **Spec-Driven Development (SDD)**
>
> Write a structured spec _before_ writing code with AI.
> The spec becomes the source of truth for the human and the AI.
>
> — Birgitta Böckeler, [Thoughtworks / Martin Fowler](https://martinfowler.com/articles/exploring-gen-ai/sdd-3-tools.html)
>
> **Why this matters — with or without AI:**
>
> - Intent captured in persistent, reviewable documents — not chat history or Slack threads
> - Every change has a _why_, not just a _what_
> - The next developer (or AI session) can understand prior decisions
> - Specs accumulate over time into system documentation
>
> **We're not starting from a broken AI workflow that needs fixing.**
> We're establishing AI workflows from scratch. SDD is about starting them right.

**What to say:**

- SDD is the idea that you write a structured specification before writing code with AI. The spec becomes the source of truth that both humans and AI reference.
- I want to be clear about framing: this isn't about "AI without specs produces bad code" — that applies to human-written code without documentation too. We all know what happens when requirements live only in Slack threads and people's heads.
- The point is: we're establishing AI workflows from scratch right now. We have a choice about how structured that process is. SDD is one answer that makes intent persist beyond a single AI session.
- What this looks like in practice: before you ask AI to implement something, you agree on what you're building, why, and how. That agreement lives in files in the repo, not in ephemeral chat. When the next person (or the next AI session) touches that code, the context is right there.

---

### Slide 8 — SDD Tiers: Finding Our Level

**On the slide:**

> **Three levels of SDD** (per [Böckeler, Thoughtworks](https://martinfowler.com/articles/exploring-gen-ai/sdd-3-tools.html))
>
> | Tier               | During feature creation                                                     | During evolution & maintenance                                                                           |
> | ------------------ | --------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
> | **Spec-first**     | A spec is written first, then used in the AI-assisted workflow for the task | Spec may be discarded after the task. A new spec is created for the next change.                         |
> | **Spec-anchored**  | Same as spec-first                                                          | Spec is _kept_ after creation and continues to be used for evolution and maintenance of that feature     |
> | **Spec-as-source** | Same as spec-anchored                                                       | Spec is the _only_ artifact edited by humans. Code is generated from specs. Humans don't touch the code. |
>
> Each level builds on the previous one.
>
> **My suggestion for us: aim for spec-anchored.**
>
> - New features and significant changes get specs (spec-first at minimum)
> - Specs are kept and maintained as features evolve (spec-anchored)
> - Spec-as-source is an aspirational future — not our starting point

**What to say:**

- These tiers come from Birgitta Böckeler's analysis of SDD tools at Thoughtworks. They represent increasing levels of commitment to specs as the primary artifact.
- **Spec-first** is the baseline: you write a spec before implementing, and AI uses it during the task. But after the task is done, the spec may be discarded. The next change to that feature starts a new spec from scratch. If you already use "plan mode" or ask AI to "plan first, then implement" — you're informally doing spec-first. Your specs just live in chat and vanish when the session ends.
- **Spec-anchored** keeps the spec alive. When you need to modify a feature later, you update the existing spec rather than starting from scratch. Over time, your specs become a living documentation layer. This is where I think we should aim.
- **Spec-as-source** is the most ambitious: humans only edit specs, never code. Code is always generated. Tessl is exploring this approach — it's interesting but experimental. Not our starting point.
- For our project — 8 years old, low documentation, code quality challenges — spec-anchored gives us the best balance. We document what we change, and those specs accumulate into system documentation over time. We don't need to spec the entire system upfront.

---

### Slide 9 — Tools: OpenSpec vs Spec Kit

**On the slide:**

> **Two main open-source SDD tools**
>
> |                    | **OpenSpec** (Fission AI)                                                               | **Spec Kit** (GitHub)                                                 |
> | ------------------ | --------------------------------------------------------------------------------------- | --------------------------------------------------------------------- |
> | Stars              | ~37k                                                                                    | ~85k                                                                  |
> | Language           | TypeScript / Node.js                                                                    | Python                                                                |
> | Philosophy         | Lightweight, fluid, brownfield-first                                                    | Comprehensive, structured, phase-driven                               |
> | Spec format        | Delta-based (ADDED / MODIFIED / REMOVED)                                                | Full specs per feature branch                                         |
> | Workflow           | `propose → specs → design → tasks → implement → verify → archive`                       | `constitution → specify → plan → tasks → implement`                   |
> | Key differentiator | Changes as folders with delta specs. Parallel changes. Explore mode for unspecced code. | Constitutional principles. Extensions/presets ecosystem. Phase gates. |
> | Brownfield fit     | Excellent — delta specs let you spec only what you change                               | Walkthroughs exist (ASP.NET CMS, Java runtime) but heavier ceremony   |
> | Setup              | `npm install -g @fission-ai/openspec && openspec init`                                  | `uv tool install specify-cli && specify init --ai copilot`            |
>
> **My recommendation: OpenSpec for the PoC.**

**What to say:**

- Both tools are MIT-licensed, actively maintained, and support Copilot + Codex.
- I recommend OpenSpec for three reasons:
  1. **Delta-based specs** — in a brownfield project, you don't want to write a complete spec for the entire auth system before you can change the login flow. OpenSpec lets you spec _just the change_ using ADDED/MODIFIED/REMOVED sections.
  2. **Explore mode** — when you encounter unspecced code and need to understand it before changing it, `/explore` is designed exactly for this.
  3. **Lower ceremony** — OpenSpec is lighter and more fluid. Spec Kit is powerful (extensions, presets, constitutions) but demands more ceremony upfront.
- Now, I want to address something: some of you might see Spec Kit's stricter, more structured flow as a _benefit_ for faster team adoption — and that's fair. A more opinionated workflow can reduce uncertainty about "what do I do next." But my concern for a PoC is that too much structure too early becomes overhead that obscures the actual value. We can always layer more structure on later.
- Spec Kit does have brownfield walkthroughs — there's an [ASP.NET CMS example](https://github.com/mnriem/spec-kit-aspnet-brownfield-demo) (~307k lines, adding Docker Compose + REST API) and a [Java runtime example](https://github.com/mnriem/spec-kit-java-brownfield-demo) (~420k lines, adding an admin console). Worth examining if you want to compare approaches.
- There's also an honest critique to consider: Birgitta Böckeler at Thoughtworks [noted that spec-kit creates a lot of markdown to review](https://martinfowler.com/articles/exploring-gen-ai/sdd-3-tools.html), which can feel like overhead — "I'd rather review code than all these markdown files." OpenSpec's lighter approach addresses this concern directly.

---

### Slide 10 — OpenSpec Core Concepts

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
> - **Artifacts** = proposal → specs → design → tasks (the "thinking chain" for a change)
> - **Archive** = deltas merge into main specs, change folder preserved for history

**What to say:**

- The mental model: **specs describe what exists**, **changes describe what's being modified**.
- Each change is self-contained in a folder. You can have multiple changes in progress simultaneously without conflicts — they're like lightweight feature branches for specifications.
- Delta specs are the key innovation for brownfield. Instead of writing "here's the entire auth spec," you write "here's what's ADDED, MODIFIED, or REMOVED." You can start speccing changes _today_ even if the full system isn't documented.
- When a change is complete, you archive it — the deltas merge into the main specs (updating the source of truth) and the change folder moves to archive (preserving the _why_).
- This creates a virtuous cycle: every change adds to the system's specification. Over time, your spec coverage grows organically.

---

### Slide 11 — OpenSpec Workflow

**On the slide:**

> **Two workflow modes**
>
> |                   | **Core** (quick path)                          | **Expanded** (step-by-step)                                                      |
> | ----------------- | ---------------------------------------------- | -------------------------------------------------------------------------------- |
> | Commands          | `propose → apply → archive`                    | `new → continue/ff → apply → verify → archive`                                   |
> | Best for          | Clear scope, want to move fast                 | Complex changes, want to review each artifact before proceeding                  |
> | Artifact creation | All artifacts created at once during `propose` | One artifact at a time with `continue`, or all at once with `ff`                 |
> | Feedback loop     | Review all artifacts together, then implement  | Review and refine each artifact (proposal → specs → design → tasks) individually |
>
> **Expanded mode reduces rework** — you give feedback at each stage instead of reworking all artifacts when something isn't right.
>
> Switch with: `openspec config profile` → select expanded → `openspec update`

**What to say:**

- OpenSpec offers two workflow modes. The core profile is a quick path: `/opsx:propose` creates all planning artifacts at once, you review them, then `/opsx:apply` implements and `/opsx:archive` finalises.
- The expanded profile gives you step-by-step control: `/opsx:new` creates the change scaffold, then `/opsx:continue` creates one artifact at a time — first proposal, then specs, then design, then tasks. You review and provide feedback at each stage.
- Alternatively, `/opsx:ff` (fast-forward) creates all remaining artifacts at once — useful when you know the scope is clear and want to skip the step-by-step.
- I'd recommend the expanded profile for us, at least initially. The step-by-step flow reduces the need to redo entire documents when feedback changes something fundamental. You catch issues at the proposal stage before they propagate into design and tasks. This is especially important given our brownfield codebase where assumptions about existing code may be wrong.

---

### Slide 12 — Realistic Workflow Scenarios

**On the slide:**

> **Scenario 1: New feature (with iteration)**
>
> ```
> /opsx:new add-password-reset
> /opsx:continue                    → Creates proposal.md
>   Review → "Scope is too broad, split email and SMS"
>   → Update proposal with feedback
> /opsx:continue                    → Creates specs (delta)
>   Review → "Missing edge case: expired reset link"
>   → Refine specs
> /opsx:continue                    → Creates design.md
> /opsx:continue                    → Creates tasks.md
> /opsx:apply                       → Implements tasks
>   Mid-implementation: "Also need rate limiting on reset endpoint"
>   → Update specs + design + tasks artifacts
>   → Continue /opsx:apply with updated context
> /opsx:verify → /opsx:archive
> ```
>
> **Scenario 2: Modifying unspecced code (our day-1 reality)**
>
> ```
> /opsx:explore                     → "How does login validation work today?"
>   AI investigates codebase, produces understanding
>   → Create initial spec from findings (spec-sourced → spec-anchored)
> /opsx:new improve-login-validation
> /opsx:continue                    → Proposal referencing the new spec
>   Review → "The existing code also has X dependency we didn't know about"
>   → Update proposal + specs
> /opsx:ff                          → create remaining artifacts
> /opsx:apply → /opsx:verify → /opsx:archive
> ```
>
> **Scenario 3: Changing an already-specced/archived feature**
>
> ```
> /opsx:new change-session-timeout
> /opsx:continue                    → Proposal references existing auth spec
>   → Delta spec: MODIFIED session timeout from 30 to 15 min
>   Review → "Also need to update the 'remember me' logic"
>   → Expand scope in proposal + add to delta spec
> /opsx:ff → /opsx:apply → /opsx:verify → /opsx:archive
>   → Archive merges delta into main auth spec (source of truth updated)
> ```

**What to say:**

- These three scenarios represent what I expect our real workflow to look like. Notice that every scenario includes iteration and course correction — that's realistic, especially for a brownfield codebase with undocumented assumptions.
- Scenario 1 shows the full expanded flow with feedback at multiple stages. Key point: when requirements change _during_ implementation, you update the artifacts and continue. OpenSpec has no rigid phase gates — any artifact can be updated at any time.
- Scenario 2 is our day-1 reality. We have code with zero specs. `/explore` is purpose-built for this: understand what exists, document it as a spec, then make your change against that baseline. This is the spec-sourced → spec-anchored transition happening in one workflow.
- Scenario 3 shows the payoff of keeping specs: when you need to change a feature that already has a spec from a previous change, you're working with context, not starting blind. The delta spec precisely documents what changed and why, and after archiving, the main spec reflects the new reality.
- The common element in all scenarios: changes at any stage are normal and expected. The value is that those changes are captured in the artifacts, not lost in chat.

---

### Slide 13 — Demo Preview

**On the slide:**

> **[Live demo — ~5 min]**
>
> We'll walk through in our actual codebase:
>
> 1. **Create a new feature** — step-by-step with feedback at each artifact
> 2. **Modify the newly created feature** — show delta spec in action
> 3. **Approach unspecced code** — `/explore` to understand, then spec, then change
> 4. **Handle mid-change requirements** — update artifacts during implementation
>
> _[Placeholder — demo will be prepared with specific examples from our codebase]_

**What to say:**

- [Run the demo live. Keep it short — 1–2 minutes per scenario.]
- [Narrate as you go: "I'm starting with `/explore` because this code has no spec. The AI is reading the codebase and producing an understanding of how this currently works. Now I can create a change against that baseline..."]
- [Emphasis: the specs persist. After the demo, the team can open the `openspec/` folder and see exactly what was proposed, designed, and changed. No chat history archaeology.]

---

### Slide 14 — What This Means For Us

**On the slide:**

> _[Placeholder — to be filled with concrete proposal after presentation content is finalised]_

**What to say:**

- [This will be a short, informal slide. The real content is the conversation that follows.]

---

### Slide 15 — Summary & Discussion

**On the slide:**

> **Key takeaways**
>
> 1. AI is a powerful tool — but without structure, it amplifies chaos as well as productivity
> 2. Context is everything: scoped instructions, relevant examples, and persistent specs beat massive prompts
> 3. SDD makes AI-generated plans and decisions **persistent, reviewable, and referenceable**
> 4. For our project: start spec-anchored — new features get specs, existing code gets specced when touched
> 5. OpenSpec is lightweight, brownfield-first, and worth evaluating

**What to say:**

- The core message: AI adoption isn't just about which tool to use or which model is best. It's about **having a structure that keeps the AI aligned with your intent** and **capturing decisions so they don't get lost**.
- SDD is one answer. I'm not claiming it's the only answer, or that OpenSpec is the perfect tool. But the problem — ephemeral context, lost decisions, inconsistent AI outputs — is real. We should invest in solving it.

---

## Appendix — References

| Resource                                        | URL                                                                            |
| ----------------------------------------------- | ------------------------------------------------------------------------------ |
| OpenSpec (Fission AI)                           | `https://github.com/Fission-AI/OpenSpec`                                       |
| OpenSpec concepts                               | `https://github.com/Fission-AI/OpenSpec/blob/main/docs/concepts.md`            |
| OpenSpec workflows                              | `https://github.com/Fission-AI/OpenSpec/blob/main/docs/workflows.md`           |
| Spec Kit (GitHub)                               | `https://github.com/github/spec-kit`                                           |
| SDD methodology (Spec Kit)                      | `https://github.com/github/spec-kit/blob/main/spec-driven.md`                  |
| SDD tiers & tool comparison (Böckeler / Fowler) | `https://martinfowler.com/articles/exploring-gen-ai/sdd-3-tools.html`          |
| Spec Kit brownfield: ASP.NET CMS                | `https://github.com/mnriem/spec-kit-aspnet-brownfield-demo`                    |
| Spec Kit brownfield: Java runtime               | `https://github.com/mnriem/spec-kit-java-brownfield-demo`                      |
| Spec Kit brownfield: Go/React dashboard         | `https://github.com/mnriem/spec-kit-go-brownfield-demo`                        |
| OpenSpec website                                | `https://openspec.dev`                                                         |
| "Lost in the Middle" paper                      | Liu et al., 2023 — "Lost in the Middle: How Language Models Use Long Contexts" |

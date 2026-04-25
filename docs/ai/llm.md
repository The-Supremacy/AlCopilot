# Local LLM Development

## Purpose

This document captures the current local LLM direction for AlCopilot recommendation work.
It explains what the main LLM-side settings mean and why the current defaults are conservative.

---

## Current Direction

- Ollama remains the default local model host for recommendation development.
- `gemma4:e4b` remains the default local CPU-oriented recommendation model unless a newer documented default replaces it.
- PostgreSQL remains the canonical source of truth for catalog and recommendation business data.
- Qdrant is the accepted vector store for recommendation semantic retrieval, but it is documented separately in [embedding.md](embedding.md).
- The end-to-end recommendation RAG pipeline is documented separately in [recommendation.md](recommendation.md).

---

## Role Of The LLM

In this system the LLM is the narrator, not the policy engine.

It is responsible for:

- turning grounded recommendation context into a useful conversational response
- using tools when the grounded context is insufficient for a precise answer
- staying aligned with deterministic constraints already prepared by backend code

It is not responsible for:

- owning canonical catalog truth
- deciding hard exclusions
- performing raw vector retrieval

That boundary is the reason many of the LLM defaults are intentionally conservative.

---

## Main Settings

### Provider and model

`Provider`

- Meaning: which chat backend AlCopilot uses for recommendation narration
- Current default: `ollama`
- Why this default: local development should stay cheap, inspectable, and easy to run without external hosted dependencies

`ModelId`

- Meaning: the default chat model used for recommendation narration
- Current default: `gemma4:e4b`
- Why this default: it is a practical local CPU-oriented baseline for conversational narration and tool use experiments

`Endpoint`

- Meaning: where the local LLM host is running
- Why it exists: the model runtime is an operational dependency and may live on another machine in local-network development

### Sampling

These three knobs all affect token selection, but they do different jobs:

- `Temperature` is the boldness knob: how willing the model is to move away from its safest wording
- `TopP` is the "how far down the list do we look?" knob
- `TopK` is the "how many options are even on the table?" knob

So the practical difference is:

- `Temperature` changes how adventurous the model is overall
- `TopP` controls whether the model is allowed to consider only the most likely wording or also some less likely wording just below it
- `TopK` puts a fixed cap on the number of wording options the model may choose from at each step

`Temperature`

- Meaning: how sharply the model favors its highest-probability token over the rest
- Plain-English shortcut: how bold or cautious the wording should be
- What it changes: the shape of the probability distribution itself
- If you increase it: the distribution becomes flatter, so alternatives that were only somewhat plausible become much more competitive
- If you decrease it: the distribution becomes sharper, so the top token dominates more strongly
- Practical effect: this is the broadest "creativity vs stability" knob because it affects the model's overall willingness to depart from the safest wording
- Current default: `0.2`
- Why this default: recommendation narration should stay stable, grounded, and not improvise wildly

`TopP`

- Meaning: cumulative probability cutoff for nucleus sampling
- Plain-English shortcut: how far past the safest wording we are willing to look
- What it changes: how much of the probability tail is allowed to remain eligible after ranking tokens by likelihood
- If you increase it: the sampler includes more of the lower-probability tail, especially in steps where probability is spread across many reasonable next tokens
- If you decrease it: the sampler cuts the tail earlier and keeps only the probability mass near the front
- Practical effect: unlike `Temperature`, this does not make the model more or less bold in general; it decides how far into the "maybe" options the sampler is allowed to go
- Current default: `0.9`
- Why this default: it allows some flexibility in phrasing while still keeping output inside the high-probability region

`TopK`

- Meaning: hard cap on how many top-ranked tokens are eligible for sampling
- Plain-English shortcut: how many candidate wordings can stay in play
- What it changes: the candidate count, not the cumulative probability mass
- If you increase it: the sampler can look farther down the ranked list, even if the extra tokens contribute very little total probability
- If you decrease it: anything below the top `K` ranks is excluded completely, even if the combined probability just below the cutoff is still meaningful
- Practical effect: unlike `TopP`, this is a fixed count rule; it always keeps the same number of options in play
- Why it exists: it is another way to constrain variance when a model/provider supports it
- Current default reasoning: leave it as configured in runtime settings rather than forcing an additional hard constraint unless experiments show a need

### Reasoning controls

`Reasoning.Enabled`

- Meaning: whether to ask the model/provider for explicit reasoning-mode behavior when available
- If you enable it: the provider may spend more compute and time on intermediate reasoning behavior, which can help with harder multi-step tasks, but often adds latency and can be unnecessary for grounded narration
- If you disable it: responses stay lighter and faster, which fits straightforward recommendation turns, but the model has less room for heavy deliberate problem solving
- Current default: `false`
- Why this default: the current recommendation workflow benefits more from grounded concise output than from extra reasoning overhead

`Reasoning.Effort`

- Meaning: how much reasoning work to request from the model when reasoning mode is enabled
- If you increase it: the provider is asked to do more deliberate reasoning work, which may help on ambiguous or tool-heavy tasks, but usually increases latency and cost
- If you decrease it: the provider is asked for a lighter pass, which keeps responses snappier, but gives up some depth on genuinely hard turns
- Current default: `Low`
- Why this default: if reasoning is turned on locally, low effort is the least disruptive starting point

`Reasoning.Output`

- Meaning: what form of reasoning output the model/provider should expose when supported
- If you choose more detailed output: debugging and observability improve, but logs become heavier and noisier for normal development
- If you choose less detailed output: the workflow stays cleaner and lighter, but you lose some visibility into why a hard turn behaved the way it did
- Current default: `Summary`
- Why this default: observability is useful, but full reasoning traces are more noise-prone and heavier than the current workflow needs

### Conversation harness

`MaxHistoryMessages`

- Meaning: how many prior messages from the conversation should be retained in the live narrator context
- If you increase it: the model can remember more earlier conversation detail, which helps long follow-ups, but also raises cost, latency, and the risk that stale context overrides the latest grounded run
- If you decrease it: the model focuses more strongly on the recent turn and fresh backend context, but may forget older user preferences or prior clarifications sooner
- Why it exists: long history increases cost, latency, and confusion risk
- Current default reasoning: keep enough recent context for follow-up questions, but not so much that the narrator starts prioritizing stale turns over the fresh grounded run context

---

## Why These Defaults Are Conservative

The Recommendation module already performs important deterministic work before the LLM speaks:

- intent resolution
- candidate grouping
- exclusion handling
- semantic hint preparation

Because of that, the LLM does not need highly creative defaults.
It needs reliable, readable, grounded narration.

That is why lower-temperature settings and modest reasoning defaults are the current baseline.

---

## Relationship To Embeddings

The LLM settings and embedding settings are related but they solve different problems.

- LLM settings affect narration and tool-using behavior
- embedding settings affect retrieval shape, semantic trust, and vector search behavior

Those concerns are intentionally documented separately so tuning one does not blur into tuning the other.

---

## Related Guidance

- [AI Embedding Guide](embedding.md)
- [Recommendation RAG Pipeline](recommendation.md)
- [Architecture Index](../architecture.md)
- [Server Architecture](../architecture/server.md)
- [ADR 0015](../adr/0015-recommendation-workflows-with-agent-framework.md)
- [ADR 0016](../adr/0016-recommendation-semantic-retrieval-with-qdrant.md)

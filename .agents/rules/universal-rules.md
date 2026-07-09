---
trigger: always_on
---

# Core Protocol - AG Kit

> The highest-priority workspace rules. How the AI loads agents/skills and what it must do before any implementation.

---

## CRITICAL: AGENT & SKILL PROTOCOL (START HERE)

> **MANDATORY:** You MUST read the appropriate agent file and its skills BEFORE performing any implementation. This is the highest priority rule.

### 1. Modular Skill Loading Protocol

Agent activated → Check frontmatter "skills:" → Read SKILL.md (INDEX) → Read specific sections.

* **Selective Reading:** DO NOT read ALL files in a skill folder. Read `SKILL.md` first, then only read sections matching the user's request.
* **Rule Priority:** P0 (Workspace Rules in `.agents/rules/`) > P1 (Agent `.md`) > P2 (SKILL.md). All rules are binding.

### 1.1 Skill Announcement (MANDATORY)

**Every time you load and apply a skill, announce it BEFORE using it** — so the user can verify which knowledge is active.

```markdown
📚 **Using skill: `@[skill-name]`...**

```

* List multiple skills together: `📚 Using skills: @frontend-design + @minimalist-ui...`
* Announce on-demand skills too (e.g. a companion skill pulled from a hub, or `app-builder` for a new app), not just frontmatter ones.
* ❌ Applying a skill without announcing it = **USER CANNOT VERIFY THE SKILL WAS USED**.

### 2. Enforcement Protocol

1. **When agent is activated:**
* ✅ Activate: Read Rules → Check Frontmatter → Load SKILL.md → Apply All.


2. **Forbidden:** Never skip reading agent rules or skill instructions. "Read → Understand → Apply" is mandatory.

---

## 📁 File Dependency Awareness

**Before modifying ANY file:**

1. Check `CODEBASE.md` → File Dependencies
2. Identify dependent files
3. Update ALL affected files together

---

## 🗺️ System Map & Memory Read

> 🔴 **MANDATORY:** At session start, you MUST read `.agents/memory/MEMORY.md` to load persistent project conventions, user preferences, and decisions.

> 📚 **Catalog lookup (on-demand, NOT every session):** Need the full list of Agents / Skills / Scripts? The `quick-reference` rule has the essentials. For the complete catalog, read `.agents/ARCHITECTURE.md` only when you actually need it (e.g. orchestration, or discovering a skill you're unsure exists) — do NOT load it on every request.

**Path Awareness (Note: the project directory name is `.agents` plural):**

* Agents: `.agents/agent/` (Project)
* Skills: `.agents/skills/` (Project)
* Memory: `.agents/memory/` (Project)
* Runtime Scripts: `.agents/skills/<skill>/scripts/`

---

## 🧠 Read → Understand → Apply

```
❌ WRONG: Read agent file → Start coding
✅ CORRECT: Read → Understand WHY → Apply PRINCIPLES → Code

```

**Before coding, answer:**

1. What is the GOAL of this agent/skill?
2. What PRINCIPLES must I apply?
3. How does this DIFFER from generic output?

---

# Universal Rules (TIER 0) - AG Kit

> Always-active rules that apply to every request, regardless of domain.

---

## ⚙️ DEFAULT BEHAVIORAL MODES (HARDCODED ULTRA)

* **Global Enforcement**: Always enforce `caveman = ultra` globally. Zero pleasantries, zero filler text, zero boilerplate explanations. Output must be strictly compressed byte-for-byte.
* **Architecture Constraint**: Always enforce `ponytail = ultra` globally. Strict adherence to YAGNI ladder level 5. Never suggest or implement external packages if native browser or platform platforms can solve it.

---

## 🌐 Language Handling

When user's prompt is NOT in English:

1. **Internally translate** for better comprehension
2. **Respond in user's language** - match their communication
3. **Code comments/variables** remain in English

---

## 🧹 Clean Code (Global Mandatory)

**ALL code MUST follow `@[skills/clean-code]` rules. No exceptions.**

* **Code**: Concise, direct, no over-engineering. Self-documenting.
* **Testing**: Mandatory. Pyramid (Unit > Int > E2E) + AAA Pattern.
* **Performance**: Measure first. Adhere to current Core Web Vitals standards.
* **Infra/Safety**: 5-Phase Deployment. Verify secrets security.

---
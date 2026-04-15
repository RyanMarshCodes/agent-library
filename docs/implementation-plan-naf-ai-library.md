# NAF AI Library Implementation Plan

## ⚠️ Pre-Implementation Clarifications

**Before creating ANY files or scaffolding anything, the implementing agent MUST clarify the following:**

### Required Questions

1. **Organization naming**: What should the library be named? (e.g., `naf-ai-library`, or your org's name)

2. **GitHub Copilot file structure**: GitHub reads `.github/copilot-instructions.md` per repo with no automatic multi-file loading.
   - Option A: Put everything in ONE file per repo (simpler, larger)
   - Option B: Keep organized structure, devs manually reference files
   - Recommendation: ___

3. **Shell script formats needed**: Your team uses mixed shells.
   - [ ] `.sh` (macOS/Linux/bash)
   - [ ] `.ps1` (PowerShell)
   - [ ] `.bat` (CMD)

4. **Team conventions**: Should implementation include your actual team conventions now, or keep files as templates for later customization?

5. **Repo URL**: What will be the target GitHub repo URL? (for documentation references)

6. **Standards customization**: Which standards files should be included initially?
   - [ ] C#
   - [ ] TypeScript
   - [ ] Angular
   - [ ] React
   - [ ] Others: ___

7. **Agents to include**: Which agents should be created?
   - [ ] Planning Agent
   - [ ] Implementation Agent
   - [ ] Review Agent
   - [ ] Testing Agent
   - [ ] Others: ___

8. **Skills to include**: Which skills should be created?
   - [ ] Generate Tests
   - [ ] Explain Code
   - [ ] Troubleshoot
   - [ ] Security Audit
   - [ ] Others: ___

10. **Copilot SDLC Skills** (from requirement):

| Skill | Description
|-------|-----------|
| `/init` | Bootstrap .sdlc/ structure in a new repo |
| `/spec` | Write requirement specs from feature requests |
| `/architect` | Design architecture and break requirements into tasks |
| `/validate` | Validate any SDLC phase output before advancing |
| `/review` | Multi-agent code review |
| `/reflect` | Post-implementation self-review before formal review |
| `/wrapup` | Close out a feature — commit, merge, deploy, update artifacts |
| `/bugfix` | Streamlined bug fix workflow |
| `/status` | Show current state of all SDLC work |
| `/analyze` | Codebase health audit |
| `/optimize` | API cost & performance scanner |

> ⚠️ **Update**: GitHub Copilot in VS Code DOES support MCP servers now (2025+). Configure via `.vscode/mcp.json`.
> - **SDLC skills**: Can be exposed as MCP server tools for `/slash` command functionality
> - **Memory**: Could connect to a memory MCP server for persistent context
> - **Recommendation**: Design skills as both manual prompts AND potential MCP server tools for future

   - [ ] Include all 11 SDLC skills (future-enabled)
   - [ ] Include subset: ___
   - [ ] Skip for now, add later

9. **Delivery preference**:
   - [ ] Copy-per-repo (simplest, won't overwrite user files)
   - [ ] Organization custom instructions only
   - [ ] Hybrid (recommended in plan)
   - Recommendation: ___

10. **Versioning mechanism**: How should devs get updates?
    - Option A: Copy files to `.github/` (won't overwrite user customizations, manual updates)
    - Option B: Symlink from central repo (automatic updates, but can override local files)
    - Option C: Hybrid — copy baseline, symlink for updates
    - Recommendation: ___ (lean toward Option A for simplicity and safety)

11. **CI integration**: Should integrate with existing CI/code review?
    - [ ] Add standards checks to PR pipeline
    - [ ] GitHub Copilot tips in PR templates
    - [ ] None for now

12. **Repo context/onboarding**: How should AI understand the codebase?
    - GitHub Copilot has NO persistent memory — every session starts fresh
    - Option A: Maintain a `REPO_CONTEXT.md` that devs update manually
    - Option B: Prompt templates that tell AI to "read our codebase first"
    - Option C: Both (recommended)

13. **Memory solution**: Since GitHub Copilot doesn't support memory tools:
    - Recommendation: Use manual context file + prompt templates
    - Document where to maintain repo-specific context: ___ (e.g., `.github/REPO_CONTEXT.md`)
    - Update workflow: ___ (e.g., "update when adding new services")

14. **MCP servers**: VS Code supports MCP — which servers should be included?
    - [ ] GitHub MCP server (repository, PR, issues)
    - [ ] Memory server (persistent context)
    - [ ] Playwright (frontend testing)
    - [ ] custom NAF MCP server (team-specific)
    - [ ] None / skip for now

15. **If adding MCP**: Recommended setup approach:
    - Option A: Local `.vscode/mcp.json` per repo
    - Option B: Organization-managed (centrally controlled)
    - Recommendation: ___

### Important Notes for the Implementing Agent

- **DO NOT create any files or folders until questions above are answered**
- **DO NOT assume team conventions — ask or leave as templates**
- **Start with a minimal viable library and iterate**
- **Get feedback from the team before adding more**

---

## Scaffolded Folder Structure

To quickly get started, here's the complete folder structure:

```
naf-ai-library/
├── README.md
├── copy-to-repo.sh
├── copy-to-repo.ps1
│
├── github/                              # GitHub Copilot files
│   ├── copilot-instructions.md         # Main instructions file
│   ├── copilot-prompts.md              # Reusable prompt templates
│   ├── copilot-troubleshooting.md       # When Copilot ignores instructions
│   └── REPO_CONTEXT.md               # Repo-specific context (update per-repo)
│
│   ├── templates/                     # Prompt templates
│   │   ├── analyze-codebase.md      # "Tell me about this codebase"
│   │   └── new-feature-prompt.md      # Standard new feature prompt
│
├── getting-started/                   # Quick-start for new devs
│   ├── getting-started.md            # Day 1 guide
│   └── quickstart.md                 # One-pager for fast onboarding
│
├── agents/                              # AI agent definitions
│   ├── planning-agent.md
│   ├── implementation-agent.md
│   ├── review-agent.md
│   └── testing-agent.md
│
├── skills/                              # Polyglot skills
│   ├── generate-tests.md
│   ├── explain-code.md
│   ├── troubleshoot.md
│   └── security-audit.md
│
├── sdlc-skills/                         # SDLC workflow skills (GitHub Copilot Commands)
│   ├── init.md                        # Bootstrap .sdlc/ structure
│   ├── spec.md                       # Write requirement specs
│   ├── architect.md                 # Design architecture, break into tasks
│   ├── validate.md                # Validate SDLC phase output
│   ├── review.md                  # Multi-agent code review
│   ├── reflect.md                 # Post-implementation self-review
│   ├── wrapup.md                  # Close out feature (commit, merge, deploy)
│   ├── bugfix.md                  # Bug fix workflow
│   ├── status.md                 # Current SDLC state
│   ├── analyze.md                # Codebase health audit
│   └── optimize.md               # API cost & performance
│
├── mcp/                               # MCP server configs (for VS Code)
│   ├── mcp.json.template           # Template for .vscode/mcp.json
│   └── servers/                     # Custom MCP server defs
│       └── naf-memory.md            # Custom memory server (optional)
│
├── standards/                          # Language/framework standards
│   ├── csharp.md
│   ├── typescript.md
│   ├── angular.md
│   ├── react.md
│   └── GENERAL.md
│
└── workflows/                         # How-to guides
    ├── new-feature.md
    ├── debugging.md
    ├── legacy-code.md
    └── code-review.md
```

### Quick Scaffold Command

```bash
# Clone and scaffold
git clone git@github.com:your-org/naf-ai-library.git
cd naf-ai-library
./copy-to-repo.sh  # or copy-to-repo.ps1 on Windows
```

---

## Overview

This document outlines a baseline AI skills/instructions library for organization-wide use with GitHub Copilot Enterprise. Designed to be framework/language agnostic at the core, with language-specific standards layered on top.

**Target users**: NAF developers (new to experienced)
**Delivery**: Private repo that devs clone/copy from, plus org-level custom instructions

---

## Phase 1.5: Prompt Templates & Quick Start

### 1.5.1 Prompt Templates (`github/copilot-prompts.md`)

Copy-pasteable high-value templates:

```
# NAF Prompt Templates

# Code Review
"Review this code for security issues. Check for: hardcoded secrets, SQL injection risks, unvalidated input."
"Review this code for [performance issues/memory leaks/race conditions]."
"Check this PR for [proper error handling/test coverage/logging]."

# Test Generation
"Write tests for this function using [xUnit/NUnit/Jest/Vitest]. Cover happy path, edge cases, and error conditions."
"Generate unit tests for [ClassName]. Use mocking for external dependencies."

# Debugging
"Explain this error message and suggest likely causes. Read [relevant code] first."
"Debug this [exception/crash/behavior]. Expected: [X], Actual: [Y]."

# Refactoring
"Refactor this to follow our [C#/TypeScript/Angular/React] standards."
"Simplify this function while preserving behavior. Keep under 20 lines."

# Documentation
"Document this API with [OpenAPI/TSDoc/JSDoc] format."
"Write a README for this [module/component/service]."
```

### 1.5.2 Quick Start Guide (`getting-started/getting-started.md`)

```
# NAF AI Library - Getting Started

## What is this?
Our team's shared AI tools and standards for Copilot.

## Day 1 Setup

1. Clone this repo
2. Run `./copy-to-repo.sh` (or `.ps1` on Windows)
3. You're done. Copilot will use our standards automatically.

## How to Use

### Agents (for structured work)
- "Use our Planning Agent to break down [feature]"
- "Use our Implementation Agent to build [feature]"

### Skills (for specific tasks)
- "Use our Generate Tests skill to write tests for [code]"
- "Use our Security Audit skill to review [code]"

### Prompt Templates
- Copy prompts from `github/copilot-prompts.md` as needed

## Finding Help
- Standards: `standards/[language].md`
- Workflows: `workflows/*.md`
```

### 1.5.3 One-Pager (`getting-started/quickstart.md`)

```
# Quick Start (30 seconds)

1. Clone
2. Run copy script
3. Use prompts/agents

Main prompts: copy from copilot-prompts.md
```

### 1.5.4 Repo Context Template (`github/templates/analyze-codebase.md`)

Template for onboarding AI to a new codebase:

```
# [Repo Name] — codebase context

## Architecture Overview
- Brief description of what this app/service does
- Main technologies: [list]

## Key Services/Modules
- [Service A]: [what it does]
- [Service B]: [what it does]

## Data Stores
- [Database/API]: [what it stores]

## External Dependencies
- [Service/API]: [purpose]

## Testing Conventions
- Framework: [xUnit/Jest/etc.]
- Location: [tests/ or __tests__/]
- Pattern: [AAA/Arrange-Act-Assert]

## Common Patterns
- [pattern 1]
- [pattern 2]

## Gotchas / Things to Know
- [Note any quirks or common pitfalls]
```

### 1.5.5 New Feature Prompt Template (`github/templates/new-feature-prompt.md`)

Template for starting new feature work:

```
# New Feature Prompt

## Feature Summary
[Brief description]

## Context
- Read REPO_CONTEXT.md first
- Follow [language] standards in standards/[language].md

## Tasks
1. [Task 1]
2. [Task 2]

## Verification
- [How to verify the feature works]
- [Tests to add/update]
```

---

### Memory/Context Update Workflow

Since GitHub Copilot has no memory, devs update `REPO_CONTEXT.md` when:

- New service/module added
- Architecture changes
- New dependencies introduced
- Testing framework changes

**Update prompt** (included in `copilot-instructions.md`):
```
"When making significant changes to this codebase, update REPO_CONTEXT.md to reflect the new architecture/services."
```

---

## Phase 1: Core Library Structure

### Recommended Repo Structure

```
naf-ai-library/
├── README.md                          # Getting started guide
├── copy-to-repo.sh                  # Cross-platform copy script
├── github/
│   ├── copilot-instructions.md      # Org-level instructions (for Enterprise admin)
│   └── copilot-prompts.md           # Reusable prompt templates
├── agents/                         # AI agent definitions
│   ├── plannning-agent.md           # Breaking down requirements
│   ├── implementation-agent.md      # Code generation
│   ├── review-agent.md            # Code review
│   └── testing-agent.md           # Test generation
├── skills/                         # Polyglot skills
│   ├── generate-tests.md            # Test generation skill
│   ├── explain-code.md           # Explain code skill
│   ├── troubleshoot.md           # Debug/troubleshoot skill
│   └── security-audit.md       # Security audit skill
├── standards/                    # Language/framework standards
│   ├── csharp.md                # C# standards
│   ├── typescript.md            # TypeScript standards
│   ├── angular.md              # Angular standards
│   ├── react.md                # React standards
│   └── GENERAL.md              # Language-agnostic standards
└── workflows/                   # How-to guides
    ├── new-feature.md           # New feature workflow
    ├── debugging.md           # Debugging workflow
    ├── legacy-code.md        # Working with legacy code
    └── code-review.md        # AI-assisted review
```

---

## Phase 2: Core Agent Definitions

### 2.1 Planning Agent (`agents/planning-agent.md`)

**Purpose**: Break down requirements into actionable tasks

**Instructions**:

- Analyze requirements and ask clarifying questions first
- Break into smallest possible units
- Identify dependencies between tasks
- Flag edge cases and assumptions
- Output a task list with acceptance criteria per item

**When to use**: Start of any feature/bug work

---

### 2.2 Implementation Agent (`agents/implementation-agent.md`)

**Purpose**: Generate code following team standards

**Instructions**:

- Follow language-specific standards (see `standards/`)
- Write self-documenting code with clear naming
- Include appropriate error handling
- Add minimal necessary comments (prefer self-documenting)
- Handle errors at appropriate layer
- Keep functions <20 lines, classes SRP

**When to use**: Writing new code or refactoring

---

### 2.3 Review Agent (`agents/review-agent.md`)

**Purpose**: AI-assisted code review

**Instructions**:

- Check correctness first, then quality, then security
- Verify error handling is appropriate
- Check for obvious security issues (no secrets, parameterized queries)
- Flag any hardcoded values that should be config
- Verify tests cover happy path and key edge cases
- Keep feedback concise and actionable

**When to use**: Before opening PRs

---

### 2.4 Testing Agent (`agents/testing-agent.md`)

**Purpose**: Generate meaningful tests

**Instructions**:

- Target >90% coverage for new code
- Test edge cases and error conditions, not just happy path
- Use meaningful assertions (verify behavior, not just coverage)
- Mock external dependencies appropriately
- Follow team's testing conventions
- Name tests clearly: `MethodName_Scenario_ExpectedBehavior`

**When to use**: Writing tests for new code

---

## Phase 3: Polyglot Skills

## Phase 3.5: SDLC Workflow Skills (`sdlc-skills/`)

### 3.5.1 `/init` — Bootstrap SDLC Structure

```
Skill: /init

Purpose: Bootstrap .sdlc/ folder structure in a new repo

Instructions:
- Create the following structure:
  - .sdlc/
  -   ├── requirements/        # Feature requirements/docs
  -   ├── specs/            # Technical specifications
  -   ├── architecture/     # Architecture decisions
  -   ├── tasks/           # Task tracking
  -   └── artifacts/       # Generated artifacts
- Create a README.md in .sdlc/ explaining the structure
- Initialize with basic templates appropriate to the project

When to use: New repo setup or starting a new feature branch
```

### 3.5.2 `/spec` — Write Requirement Specs

```
Skill: /spec

Purpose: Write requirement specifications from feature requests

Instructions:
- Ask clarifying questions first
- Write requirements in GHERKIN format (Given/When/Then) or user stories
- Include acceptance criteria
- Note any dependencies or edge cases
- Flag any ambiguous requirements

When to use: Starting new feature work
```

### 3.5.3 `/architect` — Design Architecture

```
Skill: /architect

Purpose: Design architecture and break requirements into tasks

Instructions:
- Follow Clean Architecture principles
- Identify components and their responsibilities
- Define boundaries and interfaces
- Break into smallest possible tasks
- Output task list with dependencies

When to use: Before implementation
```

### 3.5.4 `/validate` — Validate SDLC Phase

```
Skill: /validate

Purpose: Validate any SDLC phase output before advancing

Instructions:
- For specs: Check completeness, consistency, feasibility
- For code: Run tests, linting, security checks
- For reviews: Verify all items addressed
- Output validation report with pass/fail for each item

When to use: Before advancing to next SDLC phase
```

### 3.5.5 `/review` — Code Review

```
Skill: /review

Purpose: Multi-agent code review

Instructions:
- Check correctness, quality, security, performance
- Verify tests cover key scenarios
- Check error handling
- Flag hardcoded values, secrets, code smells
- Provide actionable feedback

When to use: Before opening PR
```

### 3.5.6 `/reflect` — Self-Review

```
Skill: /reflect

Purpose: Post-implementation self-review before formal review

Instructions:
- Review your own changes first
- Check for unintended changes
- Verify tests pass
- Run linter/security checks
- Note any concerns for reviewer

When to use: Before requesting formal review
```

### 3.5.7 `/wrapup` — Feature Closeout

```
Skill: /wrapup

Purpose: Close out a feature — commit, merge, deploy, update artifacts

Instructions:
- Ensure all tests pass
- Verify changes are committed
- Create PR with clear description
- Update any artifacts (docs, spec, tasks)
- Note deployment steps if applicable

When to use: Feature complete, ready to merge
```

### 3.5.8 `/bugfix` — Bug Fix Workflow

```
Skill: /bugfix

Purpose: Streamlined bug fix workflow

Instructions:
- Gather reproduction steps
- Identify root cause
- Write failing test first
- Fix the bug
- Verify fix works
- Check for similar issues elsewhere

When to use: Fixing bugs
```

### 3.5.9 `/status` — SDLC State

```
Skill: /status

Purpose: Show current state of all SDLC work

Instructions:
- Check .sdlc/tasks/ for open items
- Check current branch status
- Note what's in progress, ready for review, blocked
- Summarize in concise format

When to use: Checkpoint/status meetings
```

### 3.5.10 `/analyze` — Codebase Health

```
Skill: /analyze

Purpose: Codebase health audit

Instructions:
- Check code complexity
- Check test coverage
- Check for code smells
- Check dependency health
- Note technical debt
- Provide summary with recommendations

When to use: Periodic health checks
```

### 3.5.11 `/optimize` — Cost & Performance

```
Skill: /optimize

Purpose: API cost & performance scanner

Instructions:
- Check for N+1 queries
- Check for missing caching
- Check for large payload transfers
- Check for expensive computations
- Provide optimization recommendations

When to use: Performance reviews
```

---

## Phase 3: Polyglot Skills

### 3.1 Generate Tests Skill (`skills/generate-tests.md`)

**Framework-agnostic test generation**:

```
Skill: Generate Tests

Instructions:
- Generate tests appropriate to the project's test framework (xUnit, NUnit, Jest, Vitest, etc.)
- If unknown, ask the developer which framework they use
- Follow existing test patterns in the codebase
- Cover: happy path, edge cases, error conditions
- Use AAA pattern: Arrange, Act, Assert
- Verify behavior, not implementation details
```

---

### 3.2 Explain Code Skill (`skills/explain-code.md`)

```
Skill: Explain Code

Instructions:
- Explain in plain English, adapt depth to developer's apparent experience level
- Include what the code does, not just what each line does
- Note any non-obvious decisions or patterns
- Flag potential issues or concerns
- Suggest improvements only if significant
```

---

### 3.3 Troubleshoot Skill (`skills/troubleshoot.md`)

```
Skill: Troubleshoot

Instructions:
- Read the full error message first
- Read relevant code context before proposing solutions
- Propose most likely cause first, explain why
- Suggest minimal fix to test the hypothesis
- If unclear, ask clarifying questions
```

---

### 3.4 Security Audit Skill (`skills/security-audit.md`)

### 3.5 Troubleshooting Copilot Guide (`github/copilot-troubleshooting.md`)

```
# When Copilot Ignores Our Instructions

## Problem: Copilot isn't following our standards

### Fixes (try in order)

1. **Be explicit in EVERY prompt**
   - Bad: "Write a function"
   - Good: "Write a function following our C# standards in standards/csharp.md"

2. **Reference specific files**
   - "Follow standards/csharp.md, especially the error handling section"

3. **Use our prompts template**
   - Copy from copilot-prompts.md for best results

4. **Check .github/copilot-instructions.md exists**
   - Must be in repo root/.github/

5. **Organization custom instructions**
   - Ask admin to verify org-level instructions are set

## Known Limitations
- GitHub Copilot has context limits
- Very long instructions may get truncated
- Keep prompts focused and specific
```

```
Skill: Security Audit

Instructions:
- Check for OWASP Top 10 issues
- No hardcoded secrets/keys/tokens
- Parameterized queries for database operations
- Input validation on untrusted inputs
- Proper error handling (no stack traces to users)
- Appropriate authentication/authorization checks
```

---

## Phase 4: Language/Framework Standards

### 4.1 C# Standards (`standards/csharp.md`)

```
# C# Standards

## Code Style
- Follow Microsoft conventions: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
- Use file-scoped namespaces
- Use primary constructors where appropriate
- Nullable reference types enabled

## Architecture
- Follow Clean Architecture layers
- DI for dependencies, not static
- Interfaces for abstractions
- Record types for DTOs/immutable data

## Error Handling
- Exceptions for exceptional cases
- Result pattern for expected failures
- Always log at appropriate layer

## Testing
- xUnit or NUnit
- Moq for mocking
- FluentAssertions for assertions
```

---

### 4.2 TypeScript Standards (`standards/typescript.md`)

```
# TypeScript Standards

## Code Style
- strict mode enabled
- Explicit return types for functions
- Prefer interfaces over types for object shapes
- Use `type` for unions/intersections

## Architecture
- Feature-based folder structure
- Barrel files for clean imports
- Shared types in central location

## Error Handling
- Custom error classes for domain errors
- Result pattern for expected failures
- Never fail silently

## Testing
- Vitest or Jest
- Testing Library for components
- MSW for API mocking
```

---

### 4.3 Angular Standards (`standards/angular.md`)

```
# Angular Standards

## Architecture
- Standalone components (Angular 15+)
- Signals for state management
- Functional guards/interceptors
- OnPush change detection

## Patterns
- Smart/dumb component separation
- Services for state + API
- Reactive forms for user input

## Code Style
- Strict templates
- Strict mode enabled
- ESLint with angular plugin
```

---

### 4.4 React Standards (`standards/react.md`)

```
# React Standards

## Architecture
- Feature-based folder structure
- Custom hooks for logic reuse
- Context for global state only

## Patterns
- Component composition over inheritance
- Controlled inputs
- Functional components with hooks

## Code Style
- TypeScript strict
- ESLint + Prettier
- No class components

## State Management
- TanStack Query for server state
- Zustand or Context for client state
- Avoid prop drilling
```

---

### 4.5 General Standards (`standards/GENERAL.md`)

```
# General Standards (All Languages)

## Core Principles
- KISS: Keep solutions simple and pragmatic
- DRY: Don't repeat yourself
- SRP: Single responsibility per function/class
- YAGNI: You aren't gonna need it

## Code Quality
- No compiler warnings
- No linter warnings
- Self-documenting code with clear names
- Handle errors at appropriate layer

## Security
- Never expose secrets
- Parameterized queries
- Validate untrusted input
- Proper authentication/authorization

## Version Control
- Atomic, focused commits
- Clear commit messages
- Run tests before push
```

---

## Phase 5: Workflow Guides

### 5.1 New Feature Workflow (`workflows/new-feature.md`)

```
# AI-Assisted New Feature Workflow

1. Planning
   - Define the feature requirements clearly
   - Use Planning Agent to break into tasks
   - Identify dependencies and edge cases

2. Implementation
   - Use Implementation Agent for each task
   - Run tests after each piece
   - Keep PRs small and focused

3. Review
   - Use Review Agent before opening PR
   - Address feedback
   - Ensure tests pass
```

---

### 5.2 Debugging Workflow (`workflows/debugging.md`)

```
# AI-Assisted Debugging Workflow

1. Gather context
   - Copy the full error message
   - Note what you expected vs what happened

2. Troubleshoot
   - Use Troubleshoot skill
   - Read relevant code first
   - Test proposed fix before implementing

3. Verify
   - Confirm the fix works
   - Check for similar issues elsewhere
```

---

### 5.3 Legacy Code Workflow (`workflows/legacy-code.md`)

```
# Working with Legacy Code

1. Understand first
   - Use Explain Code skill
   - Read tests if they exist
   - Note the current behavior

2. Test before change
   - Add tests for current behavior
   - Verify tests pass before modifying

3. Make minimal changes
   - Address only what's needed
   - Don't refactor unless required
   - Add tests for new behavior
```

---

## Phase 6: Versioning & Updates

### Versioning Mechanism (Recommended: Copy-Only)

**Core principle**: Copy files to repo, never overwrite user's customizations.

```
# Update workflow (recommended)

1. Pull latest from library repo
2. Re-run copy script (only copies new/missing files)
3. Any customizations in your repo are preserved
```

**Copy script behavior**:
- Only copies NEW files (skips if target exists)
- OR copies to a SEPARATE folder (e.g., `.github/naf-baseline/`) that users can reference
- This prevents overwriting personalized work

**Symlink alternative** (optional, for teams that want automatic updates):
```
# Only if explicitly requested
ln -s naf-ai-library/github/* .github/
```
⚠️ Warning: This overwrites if files exist. Use only if team agrees.

---

### MCP Server Support

Since VS Code GitHub Copilot supports MCP servers, consider:

```
# MCP server options

## Option A: Use existing public MCP servers
- GitHub MCP server (repo, PR, issues)
- Playwright MCP (testing)
- Context7 (documentation lookup)

## Option B: Build custom NAF MCP server
- Expose team's tools as MCP tools
- Custom memory/persistent context
- Internal service integrations

## Option C: Both (recommended)
- Start with public servers
- Add custom as needed
```

**MCP configuration file**: `.vscode/mcp.json`

---

### Memory & Context Management

Since GitHub Copilot has NO persistent memory, every session starts fresh.

**Solution**: Manual context file + prompt templates

| File | Purpose |
|------|--------|
| `.github/REPO_CONTEXT.md` | Repo-specific context (update per repo) |
| `.github/copilot-prompts.md` | Copy-paste prompt templates |

**REPO_CONTEXT.md** should include:
- Architecture overview
- Key services/modules
- Data stores
- External dependencies
- Testing conventions
- Common patterns
- Gotchas

**When to update REPO_CONTEXT.md**:
- New service/module added
- Architecture changes
- New dependencies
- Testing framework changes

**How AI uses it**:
Included in `copilot-instructions.md`:
```
"At the start of any new work, read REPO_CONTEXT.md first to understand this codebase."
```

---

### CI Integration (Optional)

```
# .github/copilot-review-pr.md (auto-added to PR comments)

## AI Code Review Checklist
- [ ] Standards followed: See `.github/ai-standards/`
- [ ] Tests added: Target >90% coverage
- [ ] Security: No hardcoded secrets
```

---

## Phase 7: Delivery Mechanism

### Option A: Copy per Repo (Simplest)

```
# copy-to-repo.sh
# Run from repo root to copy files

cp -r naf-ai-library/github/* .github/
cp -r naf-ai-library/agents/ .claude/  # or .github/ai-agents/
# etc.
```

**Pros**: Simple, no infrastructure
**Cons**: Manual updates

---

### Option B: Organization Custom Instructions

Set via GitHub Enterprise admin → Organization → Copilot → Custom instructions

**Sample header for org-level instructions**:

```
 NAF Development Standards (Organization Custom Instructions)

- Always follow our standards: See naf-ai-library/standards/[language].md
- Use our testing conventions: See naf-ai-library/agents/testing-agent.md
- Code review checklist: See naf-ai-library/agents/review-agent.md

For workflow guides, see: https://github.com/your-org/naf-ai-library
```

---

### Option C: Hybrid (Recommended)

1. Create the library repo (`naf-ai-library`)
2. Add `copy-to-repo.sh` for easy distribution
3. Set org-level custom instructions pointing to the repo
4. Tell devs: "Copy relevant files to your repo's `.github/` folder"

---

## Getting Started Checklist

- [ ] Create `naf-ai-library` repo (private)
- [ ] Add core files from this plan
- [ ] Add prompt templates to `github/copilot-prompts.md`
- [ ] Create quick-start guide in `getting-started/`
- [ ] Customize for your team's conventions
- [ ] Test copy script on each shell (sh, ps1, bat)
- [ ] Set org-level custom instructions (if Enterprise)
- [ ] Announce to team with quick-start guide
- [ ] Iterate based on feedback

---

## Notes

- GitHub Copilot reads `.github/copilot-instructions.md` per repo
- Organization custom instructions apply everywhere
- These are starting points — customize for your team's actual needs
- Start small, add more as you learn what works

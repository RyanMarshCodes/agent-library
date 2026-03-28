# Agent Configurations

Personal development standards and conventions for AI coding agents.

## Purpose

This repository contains standardized rules, conventions, and best practices that any AI coding agent can use to generate high-quality, consistent code across different languages and frameworks.

## Structure

```
.
├── AGENTS.md                 # Global development standards (root level)
├── CLAUDE.md                 # Claude Code entry point → defers to AGENTS.md
├── GEMINI.md                 # Gemini entry point → defers to AGENTS.md
├── .cursorrules              # Cursor entry point → defers to AGENTS.md
├── .windsurfrules            # Windsurf entry point → defers to AGENTS.md
├── .clinerules               # Cline entry point → defers to AGENTS.md
├── .github/
│   └── copilot-instructions.md  # GitHub Copilot entry point → defers to AGENTS.md
├── global/                   # Language/framework-agnostic cross-cutting standards
│   └── unit-testing.md       # Unit testing guidelines and best practices
├── backend/                  # Backend language-specific conventions
│   └── csharp/               # C# specific standards
│       ├── naming-conventions.md
│       ├── code-style.md
│       ├── async-programming.md
│       ├── class-design.md
│       ├── code-analysis.md
│       ├── error-handling.md
│       ├── security.md
│       ├── logging.md
│       └── dependency-injection.md
├── frontend/                 # Frontend framework-specific conventions
│   └── (e.g. react/, angular/)
└── ai-docs/                  # AI agent long-term memory and session logs
    ├── README.md             # How agents read/write to this folder
    ├── issues-resolutions.md # Persistent log of issues found and resolved
    └── sessions/             # Per-session summaries: YYYY-MM-DD-{agent}.md
```

## Quick Reference

### Global Standards (AGENTS.md)
- >90% test coverage required
- Zero tolerance for warnings and code smells
- Follow SOLID, CLEAN, DRY, KISS, SRP principles
- Security: Never expose secrets, validate all inputs

### Language/Framework-Specific
Organized under `backend/` (by language) and `frontend/` (by framework). Each folder typically contains:
- **Naming Conventions**: Casing, prefixes, naming rules
- **Code Style**: Formatting, style preferences
- **Async Programming**: Async/await patterns
- **Class Design**: OOP principles and patterns
- **Code Analysis**: Linting rules and analyzers
- **Error Handling**: Exception patterns
- **Security**: Security best practices
- **Logging**: Logging standards
- **Dependency Injection**: DI patterns

### AI Agent Memory (ai-docs/)
- Session summaries written after each working session
- Persistent log of issues found and how they were resolved
- Treated as long-term memory across sessions and agents

## Usage

When working on any project, AI agents should:
1. Check for project-specific `AGENTS.md` in the project root
2. Read `ai-docs/` for prior context, decisions, and known issues
3. Apply language/framework-specific conventions from `backend/` or `frontend/`
4. Follow global standards from root `AGENTS.md`
5. Write a session summary to `ai-docs/sessions/` when the session ends

## Contributing

To add new languages/frameworks or update conventions:
1. Create or update the relevant folder under `backend/` or `frontend/`
2. Reference official documentation (Microsoft, React, Angular, etc.)
3. Ensure consistency with global `AGENTS.md` principles
4. Update this README to reflect the new structure

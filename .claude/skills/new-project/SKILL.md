---
name: new-project
description: Scaffold a new project in any supported stack with opinionated best-practice defaults
argument-hint: "[stack] [ProjectName] — stack: dotnet-api | angular | react | vue | nextjs | svelte | analog"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Scaffold a new project. Detect the stack from the arguments and apply the correct opinionated scaffold.

Input: $ARGUMENTS

## Stack Detection

Parse the first argument as the stack identifier. Match case-insensitively:

| Input | Stack | Skill |
|---|---|---|
| `dotnet-api`, `dotnet`, `.net`, `api`, `csharp`, `c#` | .NET 9 Web API (Azure, Docker) | Follow init-dotnet-api instructions |
| `angular`, `ng` | Angular 19 | Follow init-angular instructions |
| `react`, `vite-react` | React 19 + Vite | Follow init-react instructions |
| `vue`, `vue3` | Vue 3.5 + Vite | Follow init-vue instructions |
| `nextjs`, `next`, `next.js` | Next.js 15 App Router | Follow init-nextjs instructions |
| `svelte`, `sveltekit` | SvelteKit 2 + Svelte 5 | Follow init-svelte instructions |
| `analog`, `analogjs` | AnalogJS | Follow init-analog instructions |

## Instructions

1. Parse the stack from the first word of `$ARGUMENTS`. The remaining words are the project name.
2. If the stack is not recognized, list the supported stacks and ask the user to pick one.
3. Read the corresponding skill file for the detected stack:
   - dotnet-api → `D:/Projects/agent-configurations/.claude/skills/init-dotnet-api/SKILL.md`
   - angular → `D:/Projects/agent-configurations/.claude/skills/init-angular/SKILL.md`
   - react → `D:/Projects/agent-configurations/.claude/skills/init-react/SKILL.md`
   - vue → `D:/Projects/agent-configurations/.claude/skills/init-vue/SKILL.md`
   - nextjs → `D:/Projects/agent-configurations/.claude/skills/init-nextjs/SKILL.md`
   - svelte → `D:/Projects/agent-configurations/.claude/skills/init-svelte/SKILL.md`
   - analog → `D:/Projects/agent-configurations/.claude/skills/init-analog/SKILL.md`
4. Follow that skill's instructions exactly, substituting the project name throughout.
5. After scaffolding, run the build verification step from the skill's final checklist.

## Examples

- `/new-project dotnet-api MyOrderService` → scaffolds .NET Clean Architecture API named "MyOrderService"
- `/new-project react TaskTracker` → scaffolds React 19 + Vite app named "TaskTracker"
- `/new-project angular AdminPortal` → scaffolds Angular 19 app named "AdminPortal"
- `/new-project analog MyBlog` → scaffolds AnalogJS app named "MyBlog"

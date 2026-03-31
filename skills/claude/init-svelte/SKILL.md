---
name: init-svelte
description: Scaffold an opinionated SvelteKit 2 + Svelte 5 app — TypeScript strict, Runes, Tailwind v4, Shadcn-svelte, Drizzle ORM, Vitest, Husky
argument-hint: "[ProjectName] [optional: description]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Scaffold a production-grade SvelteKit 2 + Svelte 5 application. Follow every instruction below exactly. Do not skip steps. Do not ask before proceeding — just build it.

## Inputs

Project name: $ARGUMENTS

---

## Opinions (non-negotiable)

- **SvelteKit 2** + **Svelte 5**
- **Runes** (`$state`, `$derived`, `$effect`, `$props`) — no legacy reactive declarations (`$:`)
- **TypeScript** — strictest settings
- **Tailwind CSS v4**
- **Shadcn-svelte** — accessible component primitives
- **Drizzle ORM** + **PostgreSQL** — type-safe database
- **Superforms** + **Zod** — form handling
- **Lucia Auth** — authentication (or Auth.js)
- **Vitest** + **Svelte Testing Library**
- **Playwright** — end-to-end tests
- **ESLint v9** + **Prettier** with svelte plugin
- **Husky** + **lint-staged**

---

## Step 1: Create Project

```bash
npx sv create {name}
# Select: SvelteKit, TypeScript, ESLint, Prettier, Playwright, Vitest
cd {name}
npm install
```

---

## Step 2: TypeScript Strict Config

`tsconfig.json`:
```json
{
  "extends": "./.svelte-kit/tsconfig.json",
  "compilerOptions": {
    "strict": true,
    "noImplicitOverride": true,
    "noUncheckedIndexedAccess": true,
    "exactOptionalPropertyTypes": true,
    "useUnknownInCatchVariables": true,
    "noImplicitReturns": true,
    "forceConsistentCasingInFileNames": true,
    "paths": {
      "$lib": ["src/lib"],
      "$lib/*": ["src/lib/*"]
    }
  }
}
```

---

## Step 3: Install Dependencies

```bash
# Tailwind
npm install tailwindcss @tailwindcss/vite

# UI utilities
npm install class-variance-authority clsx tailwind-merge bits-ui lucide-svelte

# Forms
npm install sveltekit-superforms zod

# Database
npm install drizzle-orm postgres
npm install -D drizzle-kit

# Auth
npm install lucia arctic

# Dev
npm install -D prettier-plugin-svelte prettier-plugin-tailwindcss
npm install -D husky lint-staged
```

---

## Step 4: Vite + Tailwind Config

`vite.config.ts`:
```typescript
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  plugins: [tailwindcss(), sveltekit()],
});
```

`src/app.css`:
```css
@import "tailwindcss";
```

---

## Step 5: Shadcn-svelte

```bash
npx shadcn-svelte@latest init
npx shadcn-svelte@latest add button input label card badge
```

---

## Step 6: ESLint + Prettier

`eslint.config.js`:
```javascript
import js from '@eslint/js';
import ts from 'typescript-eslint';
import svelte from 'eslint-plugin-svelte';
import globals from 'globals';
import svelteParser from 'svelte-eslint-parser';

export default ts.config(
  { ignores: ['build/', '.svelte-kit/', 'node_modules/'] },
  js.configs.recommended,
  ...ts.configs.strictTypeChecked,
  ...svelte.configs['flat/recommended'],
  {
    languageOptions: {
      globals: { ...globals.browser, ...globals.node },
      parserOptions: { project: true, tsconfigRootDir: import.meta.dirname },
    },
  },
  {
    files: ['**/*.svelte'],
    languageOptions: {
      parser: svelteParser,
      parserOptions: { parser: ts.parser },
    },
  },
  {
    rules: {
      '@typescript-eslint/no-explicit-any': 'error',
      'no-console': 'warn',
    },
  },
);
```

`.prettierrc`:
```json
{
  "singleQuote": true,
  "trailingComma": "all",
  "printWidth": 100,
  "tabWidth": 2,
  "semi": true,
  "plugins": ["prettier-plugin-svelte", "prettier-plugin-tailwindcss"],
  "overrides": [{ "files": "*.svelte", "options": { "parser": "svelte" } }]
}
```

---

## Step 7: Husky

```bash
npx husky init
```

`.husky/pre-commit`:
```bash
npx lint-staged
```

`package.json` (add):
```json
{
  "lint-staged": {
    "*.{ts,svelte}": ["eslint --fix", "prettier --write"],
    "*.{css,json,md}": ["prettier --write"]
  }
}
```

---

## Step 8: Folder Structure

```
src/
├── lib/
│   ├── components/
│   │   └── ui/          ← shadcn-svelte components
│   ├── server/
│   │   ├── db/
│   │   │   ├── index.ts ← Drizzle client
│   │   │   └── schema.ts
│   │   └── auth.ts      ← Lucia config
│   ├── utils.ts
│   └── validations/
├── routes/
│   ├── +layout.svelte
│   ├── +layout.server.ts
│   ├── +page.svelte
│   └── (auth)/
│       ├── login/
│       │   └── +page.svelte
│       └── +layout.svelte
└── app.css
```

`src/lib/utils.ts`:
```typescript
import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}
```

`src/lib/server/db/index.ts`:
```typescript
import { drizzle } from 'drizzle-orm/postgres-js';
import postgres from 'postgres';
import * as schema from './schema';

if (!process.env['DATABASE_URL']) {
  throw new Error('DATABASE_URL is not set');
}

const client = postgres(process.env['DATABASE_URL']);
export const db = drizzle(client, { schema });
```

`src/lib/server/db/schema.ts`:
```typescript
import { pgTable, text, timestamp, uuid } from 'drizzle-orm/pg-core';

export const users = pgTable('users', {
  id: uuid('id').primaryKey().defaultRandom(),
  email: text('email').notNull().unique(),
  createdAt: timestamp('created_at').notNull().defaultNow(),
  updatedAt: timestamp('updated_at').notNull().defaultNow(),
});
```

`drizzle.config.ts`:
```typescript
import { defineConfig } from 'drizzle-kit';

if (!process.env['DATABASE_URL']) throw new Error('DATABASE_URL not set');

export default defineConfig({
  schema: './src/lib/server/db/schema.ts',
  out: './drizzle',
  dialect: 'postgresql',
  dbCredentials: { url: process.env['DATABASE_URL'] },
});
```

Root `+layout.svelte`:
```svelte
<script lang="ts">
  import '../app.css';

  const { children } = $props();
</script>

{@render children()}
```

Example component using Runes (`+page.svelte`):
```svelte
<script lang="ts">
  let count = $state(0);
  const doubled = $derived(count * 2);

  function increment(): void {
    count++;
  }
</script>

<main class="container mx-auto p-8">
  <h1 class="text-4xl font-bold">Welcome</h1>
  <p>Count: {count} (doubled: {doubled})</p>
  <button onclick={increment} class="btn">Increment</button>
</main>
```

---

## Step 9: Drizzle Config

`package.json` scripts (add):
```json
{
  "scripts": {
    "db:generate": "drizzle-kit generate",
    "db:migrate": "drizzle-kit migrate",
    "db:push": "drizzle-kit push",
    "db:studio": "drizzle-kit studio"
  }
}
```

`.env` (template):
```
DATABASE_URL=postgresql://postgres:password@localhost:5432/{name}
```

---

## Step 10: Vitest Config

`vitest.config.ts`:
```typescript
import { defineConfig } from 'vitest/config';
import { sveltekit } from '@sveltejs/kit/vite';

export default defineConfig({
  plugins: [sveltekit()],
  test: {
    globals: true,
    environment: 'jsdom',
    include: ['src/**/*.{test,spec}.{js,ts}'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      thresholds: { lines: 80, functions: 80, branches: 80, statements: 80 },
    },
  },
});
```

---

## Final Checklist

- [ ] `npm run build` succeeds with zero errors
- [ ] `npm test` passes
- [ ] `npm run lint` returns zero errors
- [ ] `npm run check` (svelte-check) returns zero errors
- [ ] Runes used throughout — no `$:` reactive declarations
- [ ] `$props()` used instead of `export let` for all component props
- [ ] Drizzle schema and client in place
- [ ] Shadcn-svelte initialized
- [ ] Tailwind v4 working
- [ ] Husky pre-commit hook executable
- [ ] `.env` template committed (no actual secrets)

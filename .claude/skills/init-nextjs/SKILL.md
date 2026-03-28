---
name: init-nextjs
description: Scaffold an opinionated Next.js 15 App Router app — TypeScript strict, Tailwind v4, Shadcn/ui, TanStack Query, React Hook Form + Zod, Prisma, Vitest + Playwright, Husky
argument-hint: "[ProjectName] [optional: description]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Scaffold a production-grade Next.js 15 application. Follow every instruction below exactly. Do not skip steps. Do not ask before proceeding — just build it.

## Inputs

Project name: $ARGUMENTS

---

## Opinions (non-negotiable)

- **Next.js 15** — **App Router only**, never Pages Router
- **TypeScript** — strictest settings
- **Server Components by default** — `'use client'` only when necessary
- **Tailwind CSS v4**
- **Shadcn/ui**
- **TanStack Query v5** — for client-component data fetching
- **React Hook Form v7** + **Zod** — form state and validation
- **Prisma** + **PostgreSQL** — database ORM
- **NextAuth.js v5** (Auth.js) — authentication
- **Vitest** + **React Testing Library** — unit/integration tests
- **Playwright** — end-to-end tests
- **ESLint v9** + **Prettier** — zero tolerance for errors
- **Husky** + **lint-staged**
- **Route groups** for layout partitioning

---

## Step 1: Create Project

```bash
npx create-next-app@latest {name} \
  --typescript \
  --tailwind \
  --eslint \
  --app \
  --src-dir \
  --import-alias "@/*"
cd {name}
```

---

## Step 2: TypeScript Strict Config

`tsconfig.json` — ensure these are set:
```json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitOverride": true,
    "noUncheckedIndexedAccess": true,
    "exactOptionalPropertyTypes": true,
    "useUnknownInCatchVariables": true,
    "noImplicitReturns": true,
    "forceConsistentCasingInFileNames": true
  }
}
```

---

## Step 3: Install Dependencies

```bash
# State and data fetching
npm install @tanstack/react-query @tanstack/react-query-devtools

# Forms and validation
npm install react-hook-form zod @hookform/resolvers

# Auth
npm install next-auth@beta

# Database
npm install prisma @prisma/client
npx prisma init --datasource-provider postgresql

# UI utilities
npm install class-variance-authority clsx tailwind-merge lucide-react

# Dev
npm install -D vitest @vitest/coverage-v8 jsdom
npm install -D @testing-library/react @testing-library/user-event @testing-library/jest-dom
npm install -D @vitejs/plugin-react
npm install -D playwright @playwright/test
npm install -D prettier prettier-plugin-tailwindcss prettier-plugin-organize-imports
npm install -D husky lint-staged
```

---

## Step 4: Shadcn/ui

```bash
npx shadcn@latest init
# Choose: Default style, Zinc color, CSS variables
npx shadcn@latest add button input label card badge form
```

---

## Step 5: ESLint (flat config)

`eslint.config.mjs`:
```javascript
import { dirname } from 'path';
import { fileURLToPath } from 'url';
import { FlatCompat } from '@eslint/eslintrc';
import tseslint from 'typescript-eslint';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const compat = new FlatCompat({ baseDirectory: __dirname });

export default tseslint.config(
  { ignores: ['.next/', 'node_modules/', 'playwright-report/'] },
  ...compat.extends('next/core-web-vitals', 'next/typescript'),
  ...tseslint.configs.strictTypeChecked,
  {
    languageOptions: {
      parserOptions: { project: true, tsconfigRootDir: __dirname },
    },
    rules: {
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/explicit-function-return-type': ['error', { allowExpressions: true }],
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
  "plugins": ["prettier-plugin-tailwindcss", "prettier-plugin-organize-imports"]
}
```

---

## Step 6: Husky

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
    "*.{ts,tsx}": ["eslint --fix", "prettier --write"],
    "*.{css,json,md}": ["prettier --write"]
  }
}
```

---

## Step 7: Folder Structure

```
src/
├── app/
│   ├── (auth)/
│   │   ├── login/
│   │   │   └── page.tsx
│   │   └── layout.tsx
│   ├── (dashboard)/
│   │   ├── dashboard/
│   │   │   └── page.tsx
│   │   └── layout.tsx
│   ├── api/
│   │   └── auth/
│   │       └── [...nextauth]/
│   │           └── route.ts
│   ├── layout.tsx
│   ├── page.tsx
│   └── providers.tsx
├── components/
│   └── ui/                ← shadcn components
├── features/
│   └── dashboard/
│       ├── components/
│       └── actions.ts     ← Server Actions for this feature
├── lib/
│   ├── auth.ts            ← NextAuth config
│   ├── db.ts              ← Prisma client singleton
│   ├── utils.ts           ← cn() and helpers
│   └── validations/
│       └── common.ts
├── types/
│   └── index.ts
└── test-setup.ts
```

`src/lib/db.ts`:
```typescript
import { PrismaClient } from '@prisma/client';

const globalForPrisma = globalThis as unknown as { prisma: PrismaClient | undefined };

export const db =
  globalForPrisma.prisma ??
  new PrismaClient({
    log: process.env['NODE_ENV'] === 'development' ? ['query', 'error', 'warn'] : ['error'],
  });

if (process.env['NODE_ENV'] !== 'production') globalForPrisma.prisma = db;
```

`src/lib/utils.ts`:
```typescript
import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}
```

`src/app/providers.tsx`:
```typescript
'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { useState } from 'react';

export function Providers({ children }: { readonly children: React.ReactNode }): JSX.Element {
  const [queryClient] = useState(
    () => new QueryClient({ defaultOptions: { queries: { staleTime: 60_000 } } })
  );

  return (
    <QueryClientProvider client={queryClient}>
      {children}
      <ReactQueryDevtools />
    </QueryClientProvider>
  );
}
```

`src/app/layout.tsx`:
```typescript
import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import { Providers } from './providers';
import './globals.css';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
  title: '{Name}',
  description: '',
};

export default function RootLayout({
  children,
}: {
  readonly children: React.ReactNode;
}): JSX.Element {
  return (
    <html lang="en">
      <body className={inter.className}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
```

---

## Step 8: Vitest Config

`vitest.config.ts`:
```typescript
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test-setup.ts'],
    exclude: ['node_modules', 'e2e/**'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      thresholds: { lines: 80, functions: 80, branches: 80, statements: 80 },
    },
  },
  resolve: {
    alias: { '@': resolve(__dirname, 'src') },
  },
});
```

`src/test-setup.ts`:
```typescript
import '@testing-library/jest-dom';
```

---

## Step 9: Playwright

`playwright.config.ts`:
```typescript
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  workers: process.env['CI'] ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:3000',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:3000',
    reuseExistingServer: !process.env['CI'],
  },
});
```

Create `e2e/home.spec.ts`:
```typescript
import { test, expect } from '@playwright/test';

test('home page loads', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveTitle(/{Name}/);
});
```

---

## Step 10: Scripts

`package.json`:
```json
{
  "scripts": {
    "dev": "next dev",
    "build": "next build",
    "start": "next start",
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage",
    "test:e2e": "playwright test",
    "test:e2e:ui": "playwright test --ui",
    "lint": "eslint .",
    "lint:fix": "eslint . --fix",
    "format": "prettier --write \"src/**/*.{ts,tsx,css,json}\"",
    "typecheck": "tsc --noEmit",
    "db:generate": "prisma generate",
    "db:push": "prisma db push",
    "db:migrate": "prisma migrate dev",
    "db:studio": "prisma studio"
  }
}
```

---

## Final Checklist

- [ ] `npm run build` succeeds with zero errors
- [ ] `npm test` passes
- [ ] `npm run lint` returns zero errors
- [ ] `npm run typecheck` returns zero errors
- [ ] App Router only — no `pages/` directory
- [ ] Server Components are default — `'use client'` minimized
- [ ] Providers wraps QueryClient in a client component
- [ ] Prisma schema initialized and `db.ts` singleton in place
- [ ] Shadcn/ui initialized with first components
- [ ] Playwright e2e test runs and passes
- [ ] Husky pre-commit hook executable

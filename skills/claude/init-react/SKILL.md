---
name: init-react
description: Scaffold an opinionated React 19 + Vite app — TypeScript strict, TanStack Router, TanStack Query, Zustand, Tailwind v4, Shadcn/ui, Vitest, Husky
argument-hint: "[ProjectName] [optional: description]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Scaffold a production-grade React 19 application. Follow every instruction below exactly. Do not skip steps. Do not ask before proceeding — just build it.

## Inputs

Project name: $ARGUMENTS

---

## Opinions (non-negotiable)

- **React 19** + **Vite 6**
- **TypeScript** — strictest settings possible
- **TanStack Router** (file-based, type-safe) — not React Router
- **TanStack Query v5** — server state, caching, mutations
- **Zustand** — client-side global state
- **Tailwind CSS v4** — utility-first styling
- **Shadcn/ui** — accessible, unstyled component primitives
- **Vitest** + **React Testing Library** — no Jest
- **ESLint v9** (flat config) + **Prettier** — zero tolerance
- **Husky** + **lint-staged** — pre-commit enforces everything
- **Feature-based** folder structure
- **Error boundaries** from day one
- **Path aliases** (`@/` → `src/`)

---

## Step 1: Create Project

```bash
npm create vite@latest {name} -- --template react-ts
cd {name}
npm install
```

---

## Step 2: TypeScript Strict Config

`tsconfig.json`:
```json
{
  "compilerOptions": {
    "target": "ES2022",
    "lib": ["ES2022", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "strict": true,
    "noImplicitOverride": true,
    "noUncheckedIndexedAccess": true,
    "exactOptionalPropertyTypes": true,
    "noPropertyAccessFromIndexSignature": true,
    "useUnknownInCatchVariables": true,
    "noImplicitReturns": true,
    "noFallthroughCasesInSwitch": true,
    "forceConsistentCasingInFileNames": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"],
      "@components/*": ["src/components/*"],
      "@features/*": ["src/features/*"],
      "@hooks/*": ["src/hooks/*"],
      "@lib/*": ["src/lib/*"],
      "@stores/*": ["src/stores/*"]
    }
  },
  "include": ["src"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
```

---

## Step 3: Install Core Dependencies

```bash
# Routing
npm install @tanstack/react-router

# Server state
npm install @tanstack/react-query @tanstack/react-query-devtools

# Client state
npm install zustand immer

# UI
npm install tailwindcss @tailwindcss/vite
npm install class-variance-authority clsx tailwind-merge lucide-react

# Forms & validation
npm install react-hook-form zod @hookform/resolvers

# Dev dependencies
npm install -D @types/react @types/react-dom
npm install -D vitest @vitest/coverage-v8 jsdom
npm install -D @testing-library/react @testing-library/user-event @testing-library/jest-dom
npm install -D @tanstack/router-devtools
npm install -D eslint @eslint/js typescript-eslint eslint-plugin-react eslint-plugin-react-hooks
npm install -D prettier prettier-plugin-tailwindcss
npm install -D husky lint-staged
npm install -D @vitejs/plugin-react
```

---

## Step 4: Vite Config

`vite.config.ts`:
```typescript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import { resolve } from 'path';
import { TanStackRouterVite } from '@tanstack/router-plugin/vite';

export default defineConfig({
  plugins: [
    TanStackRouterVite(),
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      '@components': resolve(__dirname, 'src/components'),
      '@features': resolve(__dirname, 'src/features'),
      '@hooks': resolve(__dirname, 'src/hooks'),
      '@lib': resolve(__dirname, 'src/lib'),
      '@stores': resolve(__dirname, 'src/stores'),
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test-setup.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      thresholds: { lines: 80, functions: 80, branches: 80, statements: 80 },
    },
  },
});
```

---

## Step 5: ESLint (flat config)

`eslint.config.js`:
```javascript
import js from '@eslint/js';
import tseslint from 'typescript-eslint';
import reactPlugin from 'eslint-plugin-react';
import reactHooks from 'eslint-plugin-react-hooks';
import globals from 'globals';

export default tseslint.config(
  { ignores: ['dist', 'node_modules', '.tanstack'] },
  {
    extends: [js.configs.recommended, ...tseslint.configs.strictTypeChecked],
    files: ['**/*.{ts,tsx}'],
    languageOptions: {
      ecmaVersion: 2022,
      globals: globals.browser,
      parserOptions: {
        project: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
    plugins: {
      react: reactPlugin,
      'react-hooks': reactHooks,
    },
    rules: {
      ...reactPlugin.configs.recommended.rules,
      ...reactHooks.configs.recommended.rules,
      'react/react-in-jsx-scope': 'off',
      'react/prop-types': 'off',
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/explicit-function-return-type': ['error', { allowExpressions: true }],
      '@typescript-eslint/no-unused-vars': 'error',
      '@typescript-eslint/prefer-nullish-coalescing': 'error',
      '@typescript-eslint/prefer-optional-chain': 'error',
      'no-console': 'warn',
    },
    settings: { react: { version: 'detect' } },
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
  "plugins": ["prettier-plugin-tailwindcss"]
}
```

---

## Step 6: Husky + lint-staged

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

## Step 7: Tailwind CSS

`src/index.css`:
```css
@import "tailwindcss";
```

---

## Step 8: Folder Structure

```
src/
├── components/
│   └── ui/              ← shadcn components go here
│       ├── button.tsx
│       └── input.tsx
├── features/
│   └── home/
│       ├── index.ts
│       ├── HomeView.tsx
│       └── HomeView.test.tsx
├── hooks/
│   └── useDebounce.ts
├── lib/
│   ├── api.ts           ← axios/fetch client
│   ├── utils.ts         ← cn() helper
│   └── validations/
├── routes/
│   ├── __root.tsx       ← TanStack Router root
│   └── index.tsx
├── stores/
│   └── useAppStore.ts
├── types/
│   └── index.ts
├── App.tsx
├── main.tsx
└── test-setup.ts
```

`src/lib/utils.ts`:
```typescript
import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}
```

`src/routes/__root.tsx`:
```typescript
import { createRootRoute, Outlet } from '@tanstack/react-router';
import { TanStackRouterDevtools } from '@tanstack/router-devtools';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

export const Route = createRootRoute({
  component: () => (
    <>
      <Outlet />
      <TanStackRouterDevtools />
      <ReactQueryDevtools />
    </>
  ),
});
```

`src/routes/index.tsx`:
```typescript
import { createFileRoute } from '@tanstack/react-router';
import { HomeView } from '@features/home';

export const Route = createFileRoute('/')({
  component: HomeView,
});
```

`src/main.tsx`:
```typescript
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { RouterProvider, createRouter } from '@tanstack/react-router';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { routeTree } from './routeTree.gen';
import './index.css';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 60_000, retry: 2 },
  },
});

const router = createRouter({ routeTree, context: { queryClient } });

declare module '@tanstack/react-router' {
  interface Register { router: typeof router; }
}

const root = document.getElementById('root');
if (!root) throw new Error('Root element not found');

createRoot(root).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  </StrictMode>,
);
```

`src/test-setup.ts`:
```typescript
import '@testing-library/jest-dom';
```

---

## Step 9: Initialize Shadcn/ui

```bash
npx shadcn@latest init
# Choose: Default style, Zinc base color, CSS variables
```

Add first components:
```bash
npx shadcn@latest add button input label card badge
```

---

## Step 10: Scripts

`package.json`:
```json
{
  "scripts": {
    "dev": "vite",
    "build": "tsc -b && vite build",
    "preview": "vite preview",
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage",
    "lint": "eslint .",
    "lint:fix": "eslint . --fix",
    "format": "prettier --write \"src/**/*.{ts,tsx,css,json}\"",
    "format:check": "prettier --check \"src/**/*.{ts,tsx,css,json}\"",
    "typecheck": "tsc --noEmit"
  }
}
```

---

## Final Checklist

- [ ] `npm run build` succeeds with zero errors
- [ ] `npm test` passes
- [ ] `npm run lint` returns zero errors
- [ ] `npm run typecheck` returns zero errors
- [ ] TanStack Router file-based routing is configured
- [ ] TanStack Query client is set up with sane defaults
- [ ] Tailwind v4 is working (test with a utility class)
- [ ] Shadcn/ui components are initialized
- [ ] Path aliases work (`@/`, `@features/`, etc.)
- [ ] Husky pre-commit hook is executable
- [ ] No `any` types in source files

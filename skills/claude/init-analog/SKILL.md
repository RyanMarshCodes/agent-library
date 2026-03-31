---
name: init-analog
description: Scaffold an opinionated AnalogJS app — Angular meta-framework, TypeScript strict, file-based routing, SSR, Nitro API routes, Tailwind v4, Signals, Vitest
argument-hint: "[ProjectName] [optional: description]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Scaffold a production-grade AnalogJS application. Follow every instruction below exactly. Do not skip steps. Do not ask before proceeding — just build it.

## What is AnalogJS?

AnalogJS is a full-stack Angular meta-framework built on Vite and Nitro. It provides:
- File-based routing (like Next.js but for Angular)
- Server-side rendering (SSR) by default
- API routes via Nitro (`.server.ts` files)
- Angular Signal support throughout
- Vite-powered dev server (fast HMR)

## Inputs

Project name: $ARGUMENTS

---

## Opinions (non-negotiable)

- **AnalogJS** (latest) on top of **Angular 19**
- **TypeScript** — strictest settings
- **Standalone components** — no NgModules, ever
- **Signals** for all reactive state
- **`inject()`** for all DI — no constructor injection
- **File-based routing** — all routes in `src/app/pages/`
- **API routes** via Nitro in `src/server/routes/`
- **SSR enabled** by default
- **Tailwind CSS v4** via Vite plugin
- **Angular Material 19** for UI components
- **Vitest** for testing
- **ESLint** (angular-eslint) + **Prettier**
- **Husky** + **lint-staged**

---

## Step 1: Create Project

```bash
npx create-analog@latest {name}
# Select: TypeScript, Angular Material, Tailwind (if offered), SSR
cd {name}
npm install
```

If the CLI doesn't offer all options, continue with manual setup below.

---

## Step 2: Install Dependencies

```bash
# Tailwind v4
npm install tailwindcss @tailwindcss/vite

# Angular Material
ng add @angular/material

# Testing
npm install -D vitest @vitest/coverage-v8 jsdom @analogjs/vite-plugin-angular

# Quality
npm install -D eslint @angular-eslint/eslint-plugin @angular-eslint/eslint-plugin-template
npm install -D @typescript-eslint/eslint-plugin @typescript-eslint/parser
npm install -D prettier husky lint-staged

# Utilities
npm install zod
```

---

## Step 3: Vite Config

`vite.config.ts`:
```typescript
import { defineConfig } from 'vite';
import analog from '@analogjs/platform';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig(({ mode }) => ({
  plugins: [
    analog({
      ssr: true,
      nitro: {
        preset: 'node-server',
      },
    }),
    tailwindcss(),
  ],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test-setup.ts'],
    include: ['src/**/*.spec.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      thresholds: { lines: 80, functions: 80, branches: 80, statements: 80 },
    },
  },
}));
```

---

## Step 4: TypeScript Strict Config

`tsconfig.json`:
```json
{
  "compileOnSave": false,
  "compilerOptions": {
    "strict": true,
    "noImplicitOverride": true,
    "noPropertyAccessFromIndexSignature": true,
    "noImplicitReturns": true,
    "noFallthroughCasesInSwitch": true,
    "exactOptionalPropertyTypes": true,
    "useUnknownInCatchVariables": true,
    "forceConsistentCasingInFileNames": true,
    "skipLibCheck": true,
    "esModuleInterop": true,
    "experimentalDecorators": true,
    "moduleResolution": "bundler",
    "target": "ES2022",
    "module": "ES2022",
    "lib": ["ES2022", "dom"],
    "paths": {
      "@/*": ["src/*"],
      "@core/*": ["src/app/core/*"],
      "@shared/*": ["src/app/shared/*"]
    }
  },
  "angularCompilerOptions": {
    "strictInjectionParameters": true,
    "strictInputAccessModifiers": true,
    "strictTemplates": true
  }
}
```

---

## Step 5: Tailwind CSS

`src/styles.css`:
```css
@import "tailwindcss";
```

---

## Step 6: ESLint + Prettier

`.eslintrc.json`:
```json
{
  "root": true,
  "overrides": [
    {
      "files": ["*.ts"],
      "extends": [
        "plugin:@typescript-eslint/strict-type-checked",
        "plugin:@angular-eslint/recommended",
        "plugin:@angular-eslint/template/process-inline-templates"
      ],
      "parserOptions": {
        "project": ["tsconfig.json"]
      },
      "rules": {
        "@angular-eslint/prefer-standalone": "error",
        "@typescript-eslint/no-explicit-any": "error",
        "@typescript-eslint/explicit-function-return-type": "error",
        "no-console": "warn"
      }
    },
    {
      "files": ["*.html"],
      "extends": [
        "plugin:@angular-eslint/template/recommended",
        "plugin:@angular-eslint/template/accessibility"
      ]
    }
  ]
}
```

`.prettierrc`:
```json
{
  "singleQuote": true,
  "trailingComma": "all",
  "printWidth": 100,
  "tabWidth": 2,
  "semi": true
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
    "*.{ts,html}": ["eslint --fix", "prettier --write"],
    "*.{css,json,md}": ["prettier --write"]
  }
}
```

---

## Step 8: Folder Structure

```
src/
├── app/
│   ├── core/
│   │   ├── interceptors/
│   │   │   └── auth.interceptor.ts
│   │   └── services/
│   │       └── auth.service.ts
│   ├── shared/
│   │   └── components/
│   ├── pages/               ← file-based routing
│   │   ├── index.page.ts    ← route: /
│   │   ├── about.page.ts    ← route: /about
│   │   └── (auth)/
│   │       └── login.page.ts
│   └── app.config.ts
├── server/
│   └── routes/              ← Nitro API routes
│       └── api/
│           └── v1/
│               └── health.get.ts
├── test-setup.ts
└── styles.css
```

**File-based routing rules:**
- `index.page.ts` → `/`
- `about.page.ts` → `/about`
- `[id].page.ts` → `/:id` (dynamic)
- `(group)/page.page.ts` → route group (no URL segment)

`src/app/pages/index.page.ts`:
```typescript
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <main class="container mx-auto p-8">
      <h1 class="text-4xl font-bold">Welcome to {Name}</h1>
      <p class="mt-4">Count: {{ count() }}</p>
      <button (click)="increment()" class="mt-4 btn">Increment</button>
    </main>
  `,
})
export default class HomePageComponent {
  readonly count = signal(0);

  increment(): void {
    this.count.update((c) => c + 1);
  }
}
```

**API route (Nitro)** `src/server/routes/api/v1/health.get.ts`:
```typescript
export default defineEventHandler(() => ({
  status: 'ok',
  timestamp: new Date().toISOString(),
}));
```

`src/app/app.config.ts`:
```typescript
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideClientHydration } from '@angular/platform-browser';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideFileRouter } from '@analogjs/router';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideFileRouter(),
    provideClientHydration(),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    ),
  ],
};
```

---

## Step 9: Test Setup

`src/test-setup.ts`:
```typescript
import '@angular/compiler';
import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

beforeEach(() => {
  TestBed.configureTestingModule({
    providers: [provideNoopAnimations()],
  });
});
```

---

## Step 10: Scripts

`package.json`:
```json
{
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview",
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage",
    "lint": "eslint src --ext .ts,.html",
    "lint:fix": "eslint src --ext .ts,.html --fix",
    "format": "prettier --write \"src/**/*.{ts,html,css,json}\"",
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
- [ ] SSR is enabled in vite.config.ts
- [ ] File-based routing working — `index.page.ts` renders at `/`
- [ ] At least one Nitro API route exists and returns JSON
- [ ] Standalone components only — no NgModules
- [ ] `inject()` used throughout — no constructor injection
- [ ] Signals used for all reactive state
- [ ] Tailwind v4 working
- [ ] Husky pre-commit hook executable

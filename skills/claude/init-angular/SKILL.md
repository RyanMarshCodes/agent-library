---
name: init-angular
description: Scaffold an opinionated Angular 19 app — standalone components, Signals, Angular Material, Tailwind, strict TypeScript, Vitest, Husky
argument-hint: "[ProjectName] [optional: description]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Scaffold a production-grade Angular 19 application. Follow every instruction below exactly. Do not skip steps. Do not ask before proceeding — just build it.

## Inputs

Project name: $ARGUMENTS

---

## Opinions (non-negotiable)

- Angular 19 (latest)
- **Standalone components only** — no NgModules, ever
- **Signals** for all reactive state — no RxJS except where Angular internals require it
- **`inject()`** function for all dependency injection — no constructor injection
- **Strict TypeScript**: `strict`, `strictTemplates`, `noImplicitOverride`, `exactOptionalPropertyTypes`
- **Angular Material 19** for UI components
- **Tailwind CSS v4** for utility styling
- **ESLint** (angular-eslint) + Prettier — zero tolerance for linting errors
- **Vitest** for testing — no Karma, no Jasmine
- **Husky** + lint-staged — pre-commit enforces lint + format
- **Feature-based folder structure** — not type-based
- **Lazy-loaded routes** for everything except the shell
- **Functional HTTP interceptors** for auth, error handling, and loading state
- **Environment** configuration from day one

---

## Step 1: Create Project

```bash
npx @angular/cli@latest new {name} \
  --standalone \
  --routing \
  --style scss \
  --strict \
  --skip-git \
  --package-manager npm

cd {name}
```

---

## Step 2: Add Tailwind CSS v4

```bash
npm install tailwindcss @tailwindcss/vite --save-dev
```

Add to `vite.config.ts` (create if needed for Vitest, see Step 4):
```typescript
import tailwindcss from '@tailwindcss/vite';
```

Add to `src/styles.scss`:
```scss
@import "tailwindcss";
```

---

## Step 3: Add Angular Material

```bash
ng add @angular/material
# Choose: Indigo/Pink theme, set up typography, include animations
```

---

## Step 4: Replace Karma with Vitest

```bash
npm uninstall karma karma-chrome-launcher karma-coverage karma-jasmine karma-jasmine-html-reporter jasmine-core @types/jasmine
npm install vitest @vitest/coverage-v8 @analogjs/vite-plugin-angular jsdom --save-dev
```

Create `vitest.config.ts`:
```typescript
import { defineConfig } from 'vitest/config';
import angular from '@analogjs/vite-plugin-angular';

export default defineConfig({
  plugins: [angular()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test-setup.ts'],
    include: ['src/**/*.spec.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: ['node_modules/', 'src/test-setup.ts'],
      thresholds: {
        lines: 80,
        functions: 80,
        branches: 80,
        statements: 80,
      },
    },
  },
});
```

Create `src/test-setup.ts`:
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

Update `package.json` scripts:
```json
{
  "scripts": {
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage"
  }
}
```

---

## Step 5: ESLint + Prettier

```bash
ng add @angular-eslint/schematics
npm install prettier prettier-plugin-organize-imports --save-dev
```

`.eslintrc.json` (replace generated):
```json
{
  "root": true,
  "ignorePatterns": ["projects/**/*"],
  "overrides": [
    {
      "files": ["*.ts"],
      "extends": [
        "eslint:recommended",
        "plugin:@typescript-eslint/strict-type-checked",
        "plugin:@angular-eslint/recommended",
        "plugin:@angular-eslint/template/process-inline-templates"
      ],
      "parserOptions": {
        "project": ["tsconfig.json"],
        "createDefaultProgram": true
      },
      "rules": {
        "@angular-eslint/directive-selector": ["error", { "type": "attribute", "prefix": "app", "style": "camelCase" }],
        "@angular-eslint/component-selector": ["error", { "type": "element", "prefix": "app", "style": "kebab-case" }],
        "@angular-eslint/prefer-standalone": "error",
        "@typescript-eslint/explicit-function-return-type": "error",
        "@typescript-eslint/no-explicit-any": "error",
        "@typescript-eslint/no-unused-vars": "error",
        "@typescript-eslint/prefer-readonly": "error",
        "no-console": "warn"
      }
    },
    {
      "files": ["*.html"],
      "extends": [
        "plugin:@angular-eslint/template/recommended",
        "plugin:@angular-eslint/template/accessibility"
      ],
      "rules": {}
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
  "semi": true,
  "plugins": ["prettier-plugin-organize-imports"]
}
```

`.prettierignore`:
```
dist/
.angular/
node_modules/
```

---

## Step 6: Husky + lint-staged

```bash
npm install husky lint-staged --save-dev
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
    "*.{scss,css,json,md}": ["prettier --write"]
  }
}
```

---

## Step 7: TypeScript Config (strictest)

`tsconfig.json`:
```json
{
  "compileOnSave": false,
  "compilerOptions": {
    "outDir": "./dist/out-tsc",
    "forceConsistentCasingInFileNames": true,
    "strict": true,
    "noImplicitOverride": true,
    "noPropertyAccessFromIndexSignature": true,
    "noImplicitReturns": true,
    "noFallthroughCasesInSwitch": true,
    "exactOptionalPropertyTypes": true,
    "useUnknownInCatchVariables": true,
    "skipLibCheck": true,
    "esModuleInterop": true,
    "sourceMap": true,
    "declaration": false,
    "experimentalDecorators": true,
    "moduleResolution": "bundler",
    "importHelpers": true,
    "target": "ES2022",
    "module": "ES2022",
    "lib": ["ES2022", "dom"],
    "paths": {
      "@core/*": ["src/app/core/*"],
      "@features/*": ["src/app/features/*"],
      "@shared/*": ["src/app/shared/*"],
      "@env/*": ["src/environments/*"]
    }
  },
  "angularCompilerOptions": {
    "enableI18nLegacyMessageIdFormat": false,
    "strictInjectionParameters": true,
    "strictInputAccessModifiers": true,
    "strictTemplates": true
  }
}
```

---

## Step 8: Folder Structure

Create this structure:

```
src/app/
├── core/
│   ├── interceptors/
│   │   ├── auth.interceptor.ts
│   │   ├── error.interceptor.ts
│   │   └── loading.interceptor.ts
│   ├── guards/
│   │   └── auth.guard.ts
│   └── services/
│       └── auth.service.ts
├── shared/
│   ├── components/
│   ├── directives/
│   ├── pipes/
│   └── models/
├── features/
│   └── home/
│       ├── home.component.ts
│       ├── home.component.html
│       ├── home.component.scss
│       └── home.component.spec.ts
├── layout/
│   ├── shell/
│   │   └── shell.component.ts
│   ├── header/
│   │   └── header.component.ts
│   └── footer/
│       └── footer.component.ts
├── app.component.ts
├── app.config.ts
└── app.routes.ts
```

`app.config.ts`:
```typescript
import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding, withViewTransitions } from '@angular/router';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding(), withViewTransitions()),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor, errorInterceptor])
    ),
    provideAnimationsAsync(),
  ],
};
```

`app.routes.ts`:
```typescript
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/home/home.component').then((m) => m.HomeComponent),
  },
  { path: '**', redirectTo: '' },
];
```

`core/interceptors/auth.interceptor.ts`:
```typescript
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.token();

  if (token) {
    const authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`),
    });
    return next(authReq);
  }

  return next(req);
};
```

`core/interceptors/error.interceptor.ts`:
```typescript
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      console.error(`HTTP ${error.status}: ${error.message}`);
      return throwError(() => error);
    })
  );
};
```

`core/services/auth.service.ts`:
```typescript
import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly token = signal<string | null>(null);
  readonly isAuthenticated = signal(false);

  setToken(token: string): void {
    this.token.set(token);
    this.isAuthenticated.set(true);
  }

  clearToken(): void {
    this.token.set(null);
    this.isAuthenticated.set(false);
  }
}
```

---

## Step 9: Environment Files

`src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:8080',
};
```

`src/environments/environment.prod.ts`:
```typescript
export const environment = {
  production: true,
  apiUrl: '',
};
```

---

## Step 10: Update Scripts in package.json

```json
{
  "scripts": {
    "start": "ng serve",
    "build": "ng build",
    "build:prod": "ng build --configuration production",
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage",
    "lint": "ng lint",
    "lint:fix": "ng lint --fix",
    "format": "prettier --write \"src/**/*.{ts,html,scss,json}\"",
    "format:check": "prettier --check \"src/**/*.{ts,html,scss,json}\""
  }
}
```

---

## Final Checklist

- [ ] `ng build` succeeds with zero errors or warnings
- [ ] `npm test` passes
- [ ] `npm run lint` passes with zero errors
- [ ] No NgModules anywhere — fully standalone
- [ ] `inject()` used everywhere — no constructor injection
- [ ] Signals used for all reactive state
- [ ] Husky pre-commit hook is executable and blocks bad commits
- [ ] Path aliases configured in tsconfig.json
- [ ] Feature routing is lazy-loaded
- [ ] Auth, error interceptors are functional (not class-based)
- [ ] Environment files in place

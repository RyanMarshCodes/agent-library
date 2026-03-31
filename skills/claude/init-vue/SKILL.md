---
name: init-vue
description: Scaffold an opinionated Vue 3.5 + Vite app — TypeScript strict, Composition API, Pinia, Vue Router, Tailwind v4, Shadcn-vue, VueUse, Vitest, Husky
argument-hint: "[ProjectName] [optional: description]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Scaffold a production-grade Vue 3 application. Follow every instruction below exactly. Do not skip steps. Do not ask before proceeding — just build it.

## Inputs

Project name: $ARGUMENTS

---

## Opinions (non-negotiable)

- **Vue 3.5** + **Vite 6**
- **TypeScript** — strictest settings
- **`<script setup>`** on every single file component — no Options API, ever
- **Composition API** throughout — no mixins
- **Pinia** for state management
- **Vue Router 4** with lazy-loaded routes
- **Tailwind CSS v4**
- **Shadcn-vue** (port of shadcn/ui for Vue)
- **VueUse** — composable utilities
- **Zod** + **VeeValidate** — form validation
- **Vitest** + **Vue Test Utils** — no Jest
- **ESLint v9** (flat config) + **Prettier** with vue plugin
- **Husky** + **lint-staged**
- **Path aliases** (`@/` → `src/`)

---

## Step 1: Create Project

```bash
npm create vue@latest {name}
# Select: TypeScript, Router, Pinia, Vitest, ESLint, Prettier
cd {name}
npm install
```

---

## Step 2: TypeScript Strict Config

`tsconfig.app.json` — add to compilerOptions:
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

## Step 3: Install Additional Dependencies

```bash
# UI and styling
npm install tailwindcss @tailwindcss/vite
npm install class-variance-authority clsx tailwind-merge radix-vue lucide-vue-next

# Utilities
npm install @vueuse/core @vueuse/components

# Forms and validation
npm install vee-validate zod @vee-validate/zod

# Dev
npm install -D @vue/eslint-config-typescript @vue/eslint-config-prettier
npm install -D eslint-plugin-vue
npm install -D prettier-plugin-tailwindcss
npm install -D husky lint-staged
```

---

## Step 4: Vite Config

`vite.config.ts`:
```typescript
import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import vueDevTools from 'vite-plugin-vue-devtools';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  plugins: [vue(), vueDevTools(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      '@components': fileURLToPath(new URL('./src/components', import.meta.url)),
      '@features': fileURLToPath(new URL('./src/features', import.meta.url)),
      '@stores': fileURLToPath(new URL('./src/stores', import.meta.url)),
      '@composables': fileURLToPath(new URL('./src/composables', import.meta.url)),
      '@lib': fileURLToPath(new URL('./src/lib', import.meta.url)),
    },
  },
});
```

---

## Step 5: Tailwind CSS

`src/assets/main.css`:
```css
@import "tailwindcss";
```

---

## Step 6: ESLint (flat config) + Prettier

`eslint.config.js`:
```javascript
import pluginVue from 'eslint-plugin-vue';
import { defineConfigWithVueTs, vueTsConfigs } from '@vue/eslint-config-typescript';

export default defineConfigWithVueTs(
  { ignores: ['dist/', 'node_modules/'] },
  pluginVue.configs['flat/recommended'],
  vueTsConfigs.strictTypeChecked,
  {
    rules: {
      'vue/component-api-style': ['error', ['script-setup']],
      'vue/define-macros-order': ['error', { order: ['defineOptions', 'defineProps', 'defineEmits', 'defineExpose'] }],
      'vue/no-undef-components': 'error',
      'vue/no-unused-vars': 'error',
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
  "plugins": ["prettier-plugin-tailwindcss"]
}
```

---

## Step 7: Husky + lint-staged

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
    "*.{ts,vue}": ["eslint --fix", "prettier --write"],
    "*.{css,json,md}": ["prettier --write"]
  }
}
```

---

## Step 8: Folder Structure

```
src/
├── assets/
│   └── main.css
├── components/
│   └── ui/              ← shadcn-vue components
├── composables/         ← reusable composables
│   └── useApi.ts
├── features/
│   └── home/
│       ├── HomeView.vue
│       └── HomeView.test.ts
├── layouts/
│   └── DefaultLayout.vue
├── lib/
│   ├── api.ts
│   └── utils.ts
├── router/
│   └── index.ts
├── stores/
│   └── useAppStore.ts
├── types/
│   └── index.ts
├── App.vue
└── main.ts
```

`src/lib/utils.ts`:
```typescript
import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}
```

`src/router/index.ts`:
```typescript
import { createRouter, createWebHistory } from 'vue-router';

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('@features/home/HomeView.vue'),
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/',
    },
  ],
});
```

`src/stores/useAppStore.ts`:
```typescript
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export const useAppStore = defineStore('app', () => {
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  const hasError = computed(() => error.value !== null);

  function setLoading(value: boolean): void {
    isLoading.value = value;
  }

  function setError(message: string | null): void {
    error.value = message;
  }

  function clearError(): void {
    error.value = null;
  }

  return { isLoading, error, hasError, setLoading, setError, clearError };
});
```

`src/main.ts`:
```typescript
import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import { router } from './router';
import './assets/main.css';

const app = createApp(App);
app.use(createPinia());
app.use(router);
app.mount('#app');
```

---

## Step 9: Initialize Shadcn-vue

```bash
npx shadcn-vue@latest init
# Choose: Default style, Zinc color, CSS variables
npx shadcn-vue@latest add button input label card badge
```

---

## Step 10: Vitest Config

`vitest.config.ts`:
```typescript
import { defineConfig } from 'vitest/config';
import vue from '@vitejs/plugin-vue';
import { resolve } from 'path';

export default defineConfig({
  plugins: [vue()],
  test: {
    globals: true,
    environment: 'jsdom',
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

---

## Step 11: Scripts

`package.json`:
```json
{
  "scripts": {
    "dev": "vite",
    "build": "vue-tsc -b && vite build",
    "preview": "vite preview",
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage",
    "lint": "eslint .",
    "lint:fix": "eslint . --fix",
    "format": "prettier --write \"src/**/*.{ts,vue,css,json}\"",
    "typecheck": "vue-tsc --noEmit"
  }
}
```

---

## Final Checklist

- [ ] `npm run build` succeeds with zero errors
- [ ] `npm test` passes
- [ ] `npm run lint` returns zero errors
- [ ] `npm run typecheck` returns zero errors
- [ ] Every SFC uses `<script setup>` — no Options API
- [ ] Vue Router routes are all lazy-loaded
- [ ] Pinia store uses setup syntax (not options syntax)
- [ ] Shadcn-vue initialized and first components added
- [ ] Tailwind v4 working
- [ ] Husky pre-commit hook executable

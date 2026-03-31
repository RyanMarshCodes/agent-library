# Migration Checklist for Central Agent Library

## Step 1: Copy Library

1. Copy docs/agent-library into your target MCP project.
2. Verify all agent files exist under agent-library/agents.

## Step 2: Wire to Runtime

1. Configure your runtime to load agent definitions from agent-library/agents.
2. Validate each agent is discoverable by name.

## Step 3: Validate Agent Behavior

1. Run a dry test for TechDebtAnalysisAgent (`tech-debt-analysis.agent.md`).
2. Run a dry test for CodeSimplifierAgent (`code-simplifier.agent.md`).
3. Run a dry test for SecurityCheckAgent (`security-check.agent.md`).
4. If you have a verify agent, run a dry test for it (`verify.agent.md` or as named in your catalog).

## Step 4: Secret and Environment Hygiene

1. Keep tokens and credentials in environment variables or local secret storage.
2. Do not store secrets in agent markdown files.

## Step 5: Versioning

1. Update catalog.json version after any agent change.
2. Add release notes entry in your MCP project changelog.

## Step 6: Optional Generalization

1. Keep current LenderLink-specific agents as-is.
2. Add generic variants if needed for broader org usage.
3. Document scope in catalog.json tags and description.

---
name: proceed
description: "Run end-to-end ADLC-compatible feature pipeline with validation gates."
---

Use `/proceed` for one-shot execution.

Sequence:
/context -> /spec -> /validate -> /architect -> /validate -> /implement -> /test -> /reflect -> /review -> /commit -> /wrapup

Do not advance gates while blockers exist.

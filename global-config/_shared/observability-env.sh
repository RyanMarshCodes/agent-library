# Agent Observability Configuration
# Export telemetry to Aspire dashboard OTEL endpoint
# Check your Aspire dashboard for the actual OTEL collector endpoint
# (typically http://localhost:4317 for gRPC or http://localhost:4318 for HTTP)

# ── OpenTelemetry ──────────────────────────────────────────────────────────────

# Find your OTEL endpoint in Aspire dashboard: Dashboard → Settings → Telemetry
export OTEL_EXPORTER_OTLP_ENDPOINT=${OTEL_EXPORTER_OTLP_ENDPOINT:-http://localhost:4317}
export OTEL_EXPORTER_OTLP_PROTOCOL=grpc

# Enable telemetry (Claude Code, etc.)
export CLAUDE_CODE_ENABLE_TELEMETRY=1

# Enable exporters
export OTEL_METRICS_EXPORTER=otlp
export OTEL_LOGS_EXPORTER=otlp
export OTEL_TRACES_EXPORTER=otlp

# Export intervals (milliseconds) - reduce for faster visibility during debugging
# export OTEL_METRIC_EXPORT_INTERVAL=10000  # 10 seconds
# export OTEL_LOGS_EXPORT_INTERVAL=5000   # 5 seconds

# ── OpenCode OTEL Plugin ───────────────────────────────────────────────────────

# Enable OpenCode telemetry plugin (install: npm install @devtheops/opencode-plugin-otel)
# export OPENCODE_ENABLE_TELEMETRY=1
# export OPENCODE_OTLP_ENDPOINT=http://localhost:4317

# ── Resource Attributes ────────────────────────────────────────────────────────

# Optional: override service name for clarity in Aspire dashboard
# export OTEL_SERVICE_NAME=my-agent-name

# Multi-stage Dockerfile for Cafe1316 Backend
# Using Bun runtime with Alpine for minimal image size

# Stage 1: Dependencies
FROM oven/bun:1-alpine AS deps
WORKDIR /app

# Copy dependency files
COPY package.json bun.lockb* ./

# Install dependencies
RUN bun install --frozen-lockfile --production

# Stage 2: Builder
FROM oven/bun:1-alpine AS builder
WORKDIR /app

# Copy dependency files
COPY package.json bun.lockb* ./

# Install all dependencies (including dev)
RUN bun install --frozen-lockfile

# Copy source code
COPY . .

# Build step (if needed in future)
# RUN bun run build

# Stage 3: Runner (Production)
FROM oven/bun:1-alpine AS runner
WORKDIR /app

# Set production environment
ENV NODE_ENV=production
ENV PATH=/usr/local/bin:$PATH

# Create non-root user for security
RUN addgroup --system --gid 1001 bunjs \
    && adduser --system --uid 1001 bunjs \
    && chown -R bunjs:bunjs /app

# Copy production dependencies from deps stage
COPY --from=deps --chown=bunjs:bunjs /app/node_modules ./node_modules

# Copy application code
COPY --chown=bunjs:bunjs . .

# Switch to non-root user
USER bunjs

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD bun --eval "fetch('http://localhost:8080').then(r => r.ok ? process.exit(0) : process.exit(1)).catch(() => process.exit(1))"

# Start application
CMD ["bun", "run", "start"]
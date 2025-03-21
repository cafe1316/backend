# Use an official Bun runtime as a base image
FROM oven/bun:1

# Set the working directory in the container
WORKDIR /app

# Copy the application files into the container
COPY . .

# Install dependencies
RUN bun install

# Expose the port your Bun app listens on (e.g., 8080)
EXPOSE 8080

# Command to run your Bun app
# bun run --watch src/index.ts
CMD ["bun", "run", "index.ts"]
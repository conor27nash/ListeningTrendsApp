# Use the official Node.js image as the base image
FROM node:20-alpine AS build

# Set the working directory
WORKDIR /app

# Set environment variable to indicate Docker environment
ENV DOCKER=true

# Copy package.json and package-lock.json
COPY package.json package-lock.json ./

# Ensure Node.js is installed
RUN apk add --no-cache nodejs npm

# Install dependencies
RUN npm install

# Verify Node.js installation
RUN node --version && npm --version

# Copy the rest of the application files
COPY . .

# Build the React app
RUN npm run build

# Replace Nginx with Vite development server
CMD ["npm", "run", "dev", "--", "--host"]

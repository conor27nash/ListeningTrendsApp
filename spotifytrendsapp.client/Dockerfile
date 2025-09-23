# Use the official Node.js image as the base image
FROM node:20-alpine AS build

# Set the working directory
WORKDIR /app

# Copy package.json and package-lock.json
COPY package.json package-lock.json ./

# Install dependencies
RUN npm ci --only=production

# Copy the rest of the application files
COPY . .

# Build the React app for production
RUN npm run build

# Production stage with nginx
FROM nginx:alpine AS production

# Copy the build output to nginx
COPY --from=build /app/dist /usr/share/nginx/html

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]

# Development stage (for docker-compose)
FROM node:20-alpine AS development

WORKDIR /app

# Set environment variable to indicate Docker environment
ENV DOCKER=true

# Copy package files
COPY package.json package-lock.json ./

# Install all dependencies (including dev)
RUN npm install

# Copy source code
COPY . .

# Expose port 5173 for Vite dev server
EXPOSE 5173

# Start development server
CMD ["npm", "run", "dev", "--", "--host"]

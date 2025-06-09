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

# Use the official Nginx image to serve the built app
FROM nginx:alpine

# Set permissions for the Nginx directory
RUN chmod -R 755 /usr/share/nginx/html

# Copy the built files to the Nginx container
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]

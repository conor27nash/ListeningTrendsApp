#!/bin/bash

# Spotify Trends App Setup Script
# This script helps you set up the necessary environment variables securely

echo "🎵 Spotify Trends App Setup"
echo "=========================="
echo ""

# Check if .env file exists
if [ -f ".env" ]; then
    echo "⚠️  .env file already exists. Backup created as .env.backup"
    cp .env .env.backup
fi

# Copy template
cp .env.example .env

echo "✅ Created .env file from template"
echo ""

# Generate JWT secret
JWT_SECRET=$(openssl rand -base64 32)
echo "🔐 Generated secure JWT secret key"

# Update .env file with generated JWT secret
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    sed -i '' "s/your_very_secure_jwt_secret_key_here_minimum_32_characters/$JWT_SECRET/g" .env
else
    # Linux
    sed -i "s/your_very_secure_jwt_secret_key_here_minimum_32_characters/$JWT_SECRET/g" .env
fi

echo "✅ Updated .env with secure JWT key"
echo ""

echo "📝 Next steps:"
echo "1. Get your Spotify credentials from https://developer.spotify.com/"
echo "2. Edit .env file and add your Spotify Client ID and Client Secret"
echo "3. Ensure .env is never committed to version control"
echo ""

echo "🔒 Security reminder:"
echo "- Never commit .env files to version control"
echo "- Keep your Spotify credentials secure"
echo "- Rotate your JWT secret periodically"
echo ""

echo "Setup complete! 🎉"

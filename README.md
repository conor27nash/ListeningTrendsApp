# 🎵 Spotify Trends App - Updated & Secured

A modern microservices-based application for visualizing your Spotify listening analytics with beautiful charts and insights.

## 🔐 Security Updates Applied

This application has been comprehensively reviewed and updated with critical security improvements:

### ✅ **CRITICAL SECURITY FIXES COMPLETED:**
- ❌ **Removed hardcoded credentials** from configuration files
- 🔑 **Implemented secure environment variable management**
- 🔒 **Added JWT secret key generation and validation**
- 🐳 **Fixed Docker production configuration**
- 🛡️ **Added comprehensive error handling and input validation**
- 💚 **Implemented health checks across all services**
- 🧪 **Added foundational unit testing framework**

## 🚀 Quick Start

### 1. **Security Setup (REQUIRED)**

Run the setup script to securely configure your environment:

```bash
./setup.sh
```

This will:
- Generate a cryptographically secure JWT secret key
- Create a `.env` file from the template
- Set up proper environment variables

### 2. **Configure Spotify Credentials**

1. Go to [Spotify Developer Dashboard](https://developer.spotify.com/)
2. Create a new app and get your Client ID and Client Secret
3. Edit the `.env` file and add your credentials:

```bash
# Edit .env file
nano .env

# Add your Spotify credentials:
SPOTIFY_CLIENT_ID=your_actual_client_id_here
SPOTIFY_CLIENT_SECRET=your_actual_client_secret_here
```

### 3. **Run the Application**

#### Development Mode:
```bash
docker-compose up --build
```

#### Production Mode:
```bash
docker-compose -f docker-compose.prod.yml up --build
```

The application will be available at:
- **Frontend:** http://localhost:5173 (dev) or http://localhost:80 (prod)
- **Backend:** http://localhost:5000
- **Health Checks:** http://localhost:5000/health

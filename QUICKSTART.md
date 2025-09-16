# 🚀 Quick Start Guide

Get your Personalized Assistant up and running in minutes!

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/downloads)
- [MongoDB](https://www.mongodb.com/try/download/community) (Alternative: Use Docker container)

> **Note**: If you encounter Docker Hub connectivity issues, you can install MongoDB locally using Homebrew (macOS) or the official MongoDB installer.

## 🏃‍♂️ Quick Setup (5 minutes)

### 1. Clone and Setup
```bash
# Clone the repository
git clone <your-repo-url>
cd PersonalizedAssistant

# Run the setup script
./scripts/setup.sh
```

### 2. Configure Google API (Required)
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable Gmail API and Google Drive API
4. Create OAuth 2.0 credentials
5. Update `src/PersonalizedAssistant.API/appsettings.Development.json`:
   ```json
   {
     "GoogleApi": {
       "ClientId": "your-google-client-id",
       "ClientSecret": "your-google-client-secret"
     }
   }
   ```

### 3. Start the Application

#### Option A: Using Docker (Recommended)
```bash
# Start all services
./scripts/run-dev.sh

# Or manually with Docker Compose
docker-compose up -d
```

#### Option B: Using Local Services (If Docker Hub issues)
```bash
# Install MongoDB locally (macOS)
brew tap mongodb/brew
brew install mongodb-community

# Install monitoring tools locally
brew install grafana prometheus

# Start all services
brew services start mongodb/brew/mongodb-community
brew services start grafana
brew services start prometheus

# Start API service directly
cd src/PersonalizedAssistant.API
dotnet run
```

### 4. Verify Installation
- **API Documentation**: http://localhost:8080 (Swagger UI)
- **Health Check**: http://localhost:8080/health
- **Grafana Monitoring**: http://localhost:3000 
  - Docker: admin/admin123
  - Local: admin/admin (first login)
- **Prometheus Metrics**: http://localhost:9090

#### Configuration Verification
```bash
# Test API health
curl http://localhost:8080/health
# Should return: Healthy

# Test MongoDB connection
mongosh --eval "db.runCommand('ping')"
# Should return: { ok: 1 }

# Test Swagger UI
curl -I http://localhost:8080/
# Should return: HTTP/1.1 200 OK
```

## 🧪 Test the API

### 1. Health Check
```bash
curl http://localhost:8080/health
```

### 2. Authentication (Example)
```bash
# Note: You'll need a valid Google OAuth token
curl -X POST http://localhost:8080/api/auth/authenticate \
  -H "Content-Type: application/json" \
  -d '{"googleToken": "your-google-oauth-token"}'
```

### 3. Chat with the Assistant
```bash
# Create a chat session
curl -X POST http://localhost:8080/api/chat/session \
  -H "Authorization: Bearer your-jwt-token"

# Send a message
curl -X POST http://localhost:8080/api/chat/message \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{"message": "Who am I?", "sessionId": "your-session-id"}'
```

## 📊 Monitor Your Application

### Grafana Dashboards
1. Open http://localhost:3000
2. Login credentials:
   - **Docker version**: admin/admin123
   - **Local version**: admin/admin (change on first login)
3. View service metrics and performance

### Prometheus Metrics
1. Open http://localhost:9090
2. Query metrics like `http_requests_total`
3. Set up alerts and monitoring

### Local Monitoring Setup
If using local installation:
```bash
# Check service status
brew services list | grep -E "(grafana|prometheus)"

# Restart services if needed
brew services restart grafana
brew services restart prometheus

# View service logs
brew services info grafana
brew services info prometheus
```

## 🔧 Development

### Run Individual Services
```bash
# API Gateway
cd src/PersonalizedAssistant.API
dotnet run

# Auth Service
cd services/PersonalizedAssistant.AuthService
dotnet run

# Monitoring Services (if using local installation)
brew services start grafana
brew services start prometheus
brew services start mongodb/brew/mongodb-community
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
```

### Stop Services
```bash
docker-compose down
```

## 🐛 Troubleshooting

### Common Issues

#### 1. MongoDB Connection Failed

**Docker Container Issues:**
```bash
# Check if MongoDB is running
docker ps | grep mongodb

# Restart MongoDB
docker-compose restart mongodb
```

**Docker Hub Connectivity Issues:**
If you see errors like "failed to resolve reference docker.io/library/mongo:6.0":
```bash
# Option 1: Install all services locally (macOS)
brew tap mongodb/brew
brew install mongodb-community grafana prometheus

# Start all services
brew services start mongodb/brew/mongodb-community
brew services start grafana
brew services start prometheus

# Option 2: Use modified docker-compose (excludes MongoDB)
# The project includes a backup docker-compose.yml.backup
# Use the modified version that connects to local MongoDB
```

#### 2. Monitoring Services Not Accessible

**Grafana/Prometheus Not Loading:**
```bash
# Check if services are running
brew services list | grep -E "(grafana|prometheus)"

# Start services if not running
brew services start grafana
brew services start prometheus

# Check service status
curl -I http://localhost:3000  # Grafana
curl -I http://localhost:9090  # Prometheus
```

**Grafana Login Issues:**
- Docker version: admin/admin123
- Local version: admin/admin (change password on first login)

**MongoDB Configuration Issues:**
If you see errors like "The connection string '' is not valid" or "MongoDbSettings ambiguous reference":

```bash
# Check if MongoDB is running
mongosh --eval "db.runCommand('ping')"

# Verify configuration file
cat src/PersonalizedAssistant.API/appsettings.json | grep -A 3 "MongoDb"

# Check for compilation errors
cd src/PersonalizedAssistant.API
dotnet build

# If you see "MongoDbSettings ambiguous reference" errors:
# This has been fixed in the latest version by removing duplicate class definitions
# The MongoDbSettings class is now only defined in Configuration/AppSettings.cs

# Restart API service
pkill -f "dotnet run"
cd src/PersonalizedAssistant.API
dotnet run --urls="http://localhost:8080"
```

**Configuration Fix Applied:**
- ✅ Removed duplicate `MongoDbSettings` class definition from `MongoDbContext.cs`
- ✅ Fixed namespace conflicts in `ServiceCollectionExtensions.cs`
- ✅ MongoDB connection string now properly binds from `appsettings.json`

#### 3. Google API Authentication Failed
- Verify your Google API credentials
- Check that Gmail and Drive APIs are enabled
- Ensure OAuth consent screen is configured

#### 4. Services Not Starting
```bash
# Check service logs
docker-compose logs service-name

# Rebuild containers
docker-compose build --no-cache
docker-compose up -d
```

#### 4. Port Conflicts
If ports are already in use, update `docker-compose.yml`:
```yaml
ports:
  - "5001:8080"  # Change 5001 to available port
```

### Reset Everything
```bash
# Stop and remove all containers
docker-compose down -v

# Remove all images
docker system prune -a

# Start fresh
./scripts/setup.sh
```

## 📚 Next Steps

### 1. Explore the API
- Visit http://localhost:8080 (Swagger UI)
- Try different endpoints
- Test authentication flow

### 2. Connect Your Data
- Grant permissions for Gmail and Google Drive
- Sync your contacts and calendar
- Start chatting with your assistant

### 3. Customize the Assistant
- Modify the chat responses in `ChatAgentService`
- Add new data sources
- Implement custom ML algorithms

### 4. Deploy to Production
- Set up Azure resources
- Configure production secrets
- Deploy using Azure Container Instances or AKS

## 🆘 Need Help?

- Check the [README.md](README.md) for detailed documentation
- Review [ARCHITECTURE.md](ARCHITECTURE.md) for system design
- Create an issue in the repository
- Contact the development team

## 🎉 You're Ready!

Your Personalized Assistant is now running! Start exploring the API and building amazing personalized experiences.

Happy coding! 🚀

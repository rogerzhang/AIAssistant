# 🚀 Quick Start Guide

Get your Personalized Assistant up and running in minutes!

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/downloads)

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
```bash
# Start all services
./scripts/run-dev.sh

# Or manually with Docker Compose
docker-compose up -d
```

### 4. Verify Installation
- **API Documentation**: http://localhost:8080 (Swagger UI)
- **Health Check**: http://localhost:8080/health
- **Grafana Monitoring**: http://localhost:3000 (admin/admin123)
- **Prometheus Metrics**: http://localhost:9090

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
2. Login with admin/admin123
3. View service metrics and performance

### Prometheus Metrics
1. Open http://localhost:9090
2. Query metrics like `http_requests_total`
3. Set up alerts and monitoring

## 🔧 Development

### Run Individual Services
```bash
# API Gateway
cd src/PersonalizedAssistant.API
dotnet run

# Auth Service
cd services/PersonalizedAssistant.AuthService
dotnet run
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
```bash
# Check if MongoDB is running
docker ps | grep mongodb

# Restart MongoDB
docker-compose restart mongodb
```

#### 2. Google API Authentication Failed
- Verify your Google API credentials
- Check that Gmail and Drive APIs are enabled
- Ensure OAuth consent screen is configured

#### 3. Services Not Starting
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

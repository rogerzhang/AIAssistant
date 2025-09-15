# Personalized Online Assistant (POC)

A proof-of-concept microservices-based backend system that leverages user data from Google services (Gmail, Google Drive) and iOS (Contacts, Calendar) to create a personalized online assistant using machine learning algorithms.

## 🏗️ Architecture

### Platform Services Architecture

- **Target User Base**: Individual users seeking personalized insights from their data
- **Estimated RPS**: 100-1000 requests per second (scalable design)
- **Hosting**: Azure-optimized (cost-efficient for OpenAI services integration)
- **Architecture**: Microservices-based with service redundancy for scalability

### Technology Stack

- **Backend Language**: .NET 9.0
- **Database**: MongoDB (NoSQL)
- **Inter-service Communication**: gRPC
- **Monitoring**: Grafana + Prometheus
- **Containerization**: Docker + Docker Compose

## 🏛️ Microservices Architecture

### Core Services

1. **API Gateway** (`PersonalizedAssistant.API`)
   - REST API endpoints
   - Authentication & authorization
   - Request routing to microservices

2. **Authentication Service** (`PersonalizedAssistant.AuthService`)
   - Google Sign-In integration
   - User management
   - Permission management

3. **Data Collection Service** (`PersonalizedAssistant.DataCollectionService`)
   - Gmail data collection
   - Google Drive data collection
   - iOS Contacts & Calendar integration

4. **Data Processing Service** (`PersonalizedAssistant.DataProcessingService`)
   - ML-based data analysis
   - User preference extraction
   - Insight generation

5. **Chat Agent Service** (`PersonalizedAssistant.ChatAgentService`)
   - Natural language processing
   - Personalized responses
   - Chat session management

## ✅ Current Status

### 🎯 Project Status
- ✅ **Solution Structure**: Complete microservices architecture
- ✅ **API Gateway**: Running on http://localhost:8080
- ✅ **Swagger UI**: Available at http://localhost:8080
- ✅ **Health Check**: Available at http://localhost:8080/health
- ✅ **All Projects**: Successfully compiled with .NET 9.0
- ⚠️ **MongoDB**: Not running (health check shows "Unhealthy")
- ⚠️ **Microservices**: Individual services not yet started

### 🔧 Recent Updates
- Upgraded from .NET 8.0 to .NET 9.0 for compatibility
- Fixed Swagger UI configuration for all environments
- Resolved all compilation errors
- Updated package versions for security and compatibility
- Configured API to run on port 8080

## 🚀 Quick Start

### Prerequisites

- .NET 9.0 SDK
- Docker & Docker Compose
- MongoDB (if running locally)

### Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd PersonalizedAssistant
   ```

2. **Configure environment variables**
   ```bash
   cp src/PersonalizedAssistant.API/appsettings.Development.json.example src/PersonalizedAssistant.API/appsettings.Development.json
   # Edit the configuration file with your Google API credentials
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run with Docker Compose**
   ```bash
   docker-compose up -d
   ```

### Services Endpoints

- **API Gateway**: http://localhost:8080
- **Swagger UI**: http://localhost:8080 (API documentation)
- **Health Check**: http://localhost:8080/health
- **Auth Service**: http://localhost:5001
- **Data Collection Service**: http://localhost:5002
- **Data Processing Service**: http://localhost:5003
- **Chat Agent Service**: http://localhost:5004
- **Grafana**: http://localhost:3000 (admin/admin123)
- **Prometheus**: http://localhost:9090

## 📊 Data Sources

### Google Services Integration

- **Gmail**: Email content, subjects, senders, labels
- **Google Drive**: Files, folders, sharing information

### iOS Integration

- **Contacts**: Names, emails, phones, organizations
- **Calendar**: Events, meetings, appointments

## 🤖 Chat Agent Capabilities

The chat agent can answer questions such as:

- "Who am I?" - Personal profile and interests
- "What do I like to eat?" - Food preferences from data analysis
- "What are my passions?" - Extracted interests and hobbies
- "What are my doctors?" - Medical contacts and appointments
- "Who are my friends?" - Relationship analysis
- "What tasks should I do?" - Task recommendations from calendar

## 🔧 Configuration

### Google API Setup

1. Create a Google Cloud Project
2. Enable Gmail API and Google Drive API
3. Create OAuth 2.0 credentials
4. Update `appsettings.json` with your credentials

### MongoDB Configuration

The system uses MongoDB for data storage. Configure the connection string in `appsettings.json`:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "PersonalizedAssistant"
  }
}
```

## 📈 Monitoring & Observability

### Grafana Dashboards

- Service health monitoring
- Performance metrics
- User activity analytics
- Data processing statistics

### Prometheus Metrics

- Request rates and response times
- Error rates
- Resource utilization
- Custom business metrics

## 🔒 Security

- JWT-based authentication
- Google OAuth 2.0 integration
- Data encryption at rest
- Secure inter-service communication
- User data privacy controls

## 🧪 Testing

### Unit Tests
```bash
dotnet test
```

### Integration Tests
```bash
dotnet test --filter Category=Integration
```

### Load Testing
```bash
# Using Apache Bench
ab -n 1000 -c 10 http://localhost:8080/health
```

## 📦 Deployment

### Docker Deployment

```bash
# Build all services
docker-compose build

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f
```

### Azure Deployment

1. Create Azure Container Instances or Azure Kubernetes Service
2. Configure Azure Database for MongoDB
3. Set up Application Insights for monitoring
4. Deploy using Azure DevOps or GitHub Actions

## 🔄 API Documentation

### Authentication Endpoints

- `POST /api/auth/authenticate` - Authenticate with Google token
- `POST /api/auth/permissions/grant` - Grant data permissions
- `GET /api/auth/permissions` - Get user permissions
- `GET /api/auth/profile` - Get user profile

### Data Collection Endpoints

- `POST /api/datacollection/gmail` - Collect Gmail data
- `POST /api/datacollection/google-drive` - Collect Google Drive data
- `POST /api/datacollection/ios-contacts` - Collect iOS contacts
- `POST /api/datacollection/ios-calendar` - Collect iOS calendar
- `GET /api/datacollection/data` - Get collected data

### Chat Endpoints

- `POST /api/chat/message` - Send chat message
- `POST /api/chat/session` - Create chat session
- `GET /api/chat/sessions` - Get user sessions
- `GET /api/chat/suggestions` - Get suggested questions

### Insights Endpoints

- `GET /api/insights/preferences` - Get user preferences
- `GET /api/insights/insights` - Get generated insights
- `POST /api/insights/process` - Process user data

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation wiki

## 🔮 Future Enhancements

- OpenAI GPT integration for advanced chat capabilities
- Real-time data synchronization
- Advanced ML models for better insights
- Mobile app integration
- Voice assistant capabilities
- Multi-language support

# Personalized Assistant - System Architecture

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Client Applications                        │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │   Web App   │  │  Mobile App │  │   Desktop   │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
└─────────────────────┬───────────────────────────────────────────┘
                      │ HTTPS/REST API
┌─────────────────────▼───────────────────────────────────────────┐
│                    API Gateway                                  │
│              (PersonalizedAssistant.API)                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │   Auth      │  │   Data      │  │    Chat     │            │
│  │ Controller  │  │ Collection  │  │ Controller  │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
└─────────────────────┬───────────────────────────────────────────┘
                      │ gRPC
┌─────────────────────▼───────────────────────────────────────────┐
│                    Microservices Layer                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────┐│
│  │    Auth     │  │    Data     │  │    Data     │  │  Chat   ││
│  │  Service    │  │ Collection  │  │ Processing  │  │ Agent   ││
│  │             │  │  Service    │  │  Service    │  │ Service ││
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────┘│
└─────────────────────┬───────────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────────┐
│                    Data Layer                                   │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │   MongoDB    │  │  External   │  │   File      │            │
│  │  Database   │  │    APIs     │  │  Storage    │            │
│  │             │  │ (Google,    │  │             │            │
│  │             │  │   iOS)      │  │             │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
└─────────────────────────────────────────────────────────────────┘
```

## Microservices Architecture

### 1. API Gateway (PersonalizedAssistant.API)
- **Port**: 8080
- **Purpose**: Entry point for all client requests
- **Responsibilities**:
  - Request routing and load balancing
  - Authentication and authorization
  - API documentation (Swagger)
  - Rate limiting and throttling
  - Request/response transformation

### 2. Authentication Service (PersonalizedAssistant.AuthService)
- **Port**: 5001
- **Purpose**: User authentication and authorization
- **Responsibilities**:
  - Google OAuth 2.0 integration
  - JWT token management
  - User profile management
  - Permission management
  - Session handling

### 3. Data Collection Service (PersonalizedAssistant.DataCollectionService)
- **Port**: 5002
- **Purpose**: Collect data from external sources
- **Responsibilities**:
  - Gmail data collection
  - Google Drive data collection
  - iOS Contacts integration
  - iOS Calendar integration
  - Data validation and storage

### 4. Data Processing Service (PersonalizedAssistant.DataProcessingService)
- **Port**: 5003
- **Purpose**: Process and analyze collected data
- **Responsibilities**:
  - Machine learning algorithms
  - User preference extraction
  - Insight generation
  - Data categorization
  - Pattern recognition

### 5. Chat Agent Service (PersonalizedAssistant.ChatAgentService)
- **Port**: 5004
- **Purpose**: Natural language processing and chat functionality
- **Responsibilities**:
  - Message processing
  - Intent recognition
  - Response generation
  - Chat session management
  - Context maintenance

## Data Flow

### 1. User Authentication Flow
```
Client → API Gateway → Auth Service → Google OAuth → MongoDB
```

### 2. Data Collection Flow
```
Client → API Gateway → Data Collection Service → External APIs → MongoDB
```

### 3. Data Processing Flow
```
Data Collection Service → Data Processing Service → MongoDB (Processed Data)
```

### 4. Chat Interaction Flow
```
Client → API Gateway → Chat Agent Service → Data Processing Service → MongoDB
```

## Technology Stack

### Backend
- **.NET 9.0**: Primary development framework
- **ASP.NET Core**: Web API framework
- **gRPC**: Inter-service communication
- **MongoDB**: NoSQL database
- **JWT**: Authentication tokens

### External Integrations
- **Google APIs**: Gmail, Google Drive, OAuth
- **iOS APIs**: Contacts, Calendar (via client apps)

### Monitoring & Observability
- **Grafana**: Metrics visualization
- **Prometheus**: Metrics collection
- **Serilog**: Structured logging
- **Health Checks**: Service health monitoring

### Infrastructure
- **Docker**: Containerization
- **Docker Compose**: Local development orchestration
- **Azure**: Cloud deployment target

## Database Schema

### Collections
1. **users**: User profiles and authentication data
2. **data_collections**: Raw collected data
3. **gmail_data**: Processed Gmail data
4. **google_drive_data**: Processed Google Drive data
5. **ios_contact_data**: iOS contacts data
6. **ios_calendar_data**: iOS calendar data
7. **chat_sessions**: Chat conversation history

### Key Indexes
- User lookup by Google ID
- Data collections by user and source
- Chat sessions by user and activity
- Time-based indexes for analytics

## Security Architecture

### Authentication
- Google OAuth 2.0 for user authentication
- JWT tokens for API access
- Refresh token mechanism

### Authorization
- Role-based access control
- Data source permissions
- API endpoint protection

### Data Protection
- Encryption at rest (MongoDB)
- Encryption in transit (HTTPS/gRPC)
- User data isolation
- GDPR compliance considerations

## Configuration Management

### Configuration Structure
- **appsettings.json**: Main configuration file
- **appsettings.Development.json**: Development-specific settings
- **Environment Variables**: Production secrets and overrides

### Key Configuration Areas
- **MongoDB Settings**: Connection string and database name
- **JWT Settings**: Secret key, issuer, audience, and expiration
- **Google API Settings**: Client ID, client secret, and OAuth scopes
- **gRPC Settings**: Service URLs for inter-service communication
- **Logging Settings**: Log levels, paths, and output formats

### Configuration Binding
- Uses `IOptions<T>` pattern for strongly-typed configuration
- Automatic binding from JSON to C# classes
- Environment variable override support
- Validation and error handling for required settings

### Recent Fixes Applied
- ✅ **MongoDB Configuration**: Fixed namespace conflicts and binding issues
- ✅ **Duplicate Class Definitions**: Removed duplicate `MongoDbSettings` class
- ✅ **Service Registration**: Updated dependency injection configuration

## Scalability Considerations

### Horizontal Scaling
- Stateless microservices
- Load balancer ready
- Database sharding support
- Container orchestration ready

### Performance Optimization
- Connection pooling
- Caching strategies
- Async processing
- Batch operations

### Monitoring & Alerting
- Service health checks
- Performance metrics
- Error tracking
- Resource utilization

## Deployment Architecture

### Development Environment
- Docker Compose for local development
- MongoDB container
- All services in containers
- Grafana and Prometheus for monitoring

### Production Environment (Azure)
- Azure Container Instances or AKS
- Azure Database for MongoDB
- Application Insights
- Azure Key Vault for secrets
- Azure Monitor for observability

## API Design

### RESTful Endpoints
- `/api/auth/*` - Authentication endpoints
- `/api/datacollection/*` - Data collection endpoints
- `/api/chat/*` - Chat functionality endpoints
- `/api/insights/*` - Insights and analytics endpoints

### gRPC Services
- AuthService - User authentication
- DataCollectionService - Data collection operations
- ChatAgentService - Chat functionality

## Error Handling

### Global Error Handling
- Centralized exception handling
- Structured error responses
- Logging and monitoring integration

### Service-Level Error Handling
- Retry mechanisms
- Circuit breaker patterns
- Graceful degradation

## Testing Strategy

### Unit Tests
- Service layer testing
- Business logic validation
- Mock external dependencies

### Integration Tests
- API endpoint testing
- Database integration testing
- Service communication testing

### Load Testing
- Performance benchmarking
- Scalability testing
- Stress testing

## Future Enhancements

### Planned Features
- OpenAI GPT integration
- Real-time data synchronization
- Advanced ML models
- Mobile app integration
- Voice assistant capabilities

### Technical Improvements
- Event-driven architecture
- Message queues
- Advanced caching
- Multi-region deployment

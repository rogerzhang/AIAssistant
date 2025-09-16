#!/bin/bash

# Personalized Assistant Setup Script
# This script sets up the development environment

echo "🚀 Setting up Personalized Assistant Development Environment..."

# Check if .NET 8.0 is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 8.0 SDK is not installed. Please install it from https://dotnet.microsoft.com/download"
    exit 1
fi

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install it from https://www.docker.com/get-started"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install it from https://docs.docker.com/compose/install/"
    exit 1
fi

echo "✅ Prerequisites check passed"

# Create development configuration if it doesn't exist
if [ ! -f "src/PersonalizedAssistant.API/appsettings.Development.json" ]; then
    echo "📝 Creating development configuration..."
    cp src/PersonalizedAssistant.API/appsettings.Development.json.example src/PersonalizedAssistant.API/appsettings.Development.json
    echo "⚠️  Please update src/PersonalizedAssistant.API/appsettings.Development.json with your Google API credentials"
fi

# Restore NuGet packages
echo "📦 Restoring NuGet packages..."
dotnet restore

# Build the solution
echo "🔨 Building the solution..."
dotnet build

if [ $? -eq 0 ]; then
    echo "✅ Build successful"
else
    echo "❌ Build failed"
    exit 1
fi

# Create logs directory
mkdir -p logs

# Set up MongoDB with Docker
echo "🐳 Starting MongoDB with Docker..."
docker-compose up -d mongodb

# Wait for MongoDB to be ready
echo "⏳ Waiting for MongoDB to be ready..."
sleep 10

# Check if MongoDB is running
if docker ps | grep -q "personalized-assistant-mongodb"; then
    echo "✅ MongoDB is running"
else
    echo "❌ Failed to start MongoDB"
    exit 1
fi

echo ""
echo "🎉 Setup completed successfully!"
echo ""
echo "Next steps:"
echo "1. Update src/PersonalizedAssistant.API/appsettings.Development.json with your Google API credentials"
echo "2. Run 'docker-compose up' to start all services"
echo "3. Visit http://localhost:5000 for the API documentation"
echo "4. Visit http://localhost:3000 for Grafana monitoring (admin/admin123)"
echo ""
echo "Happy coding! 🚀"


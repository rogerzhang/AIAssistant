#!/bin/bash

# Test Build Script for Personalized Assistant
echo "🧪 Testing Personalized Assistant Build..."

# Check if .NET 8.0 is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 8.0 SDK is not installed. Please install it from https://dotnet.microsoft.com/download"
    exit 1
fi

echo "✅ .NET 8.0 SDK found"

# Clean and restore packages
echo "🧹 Cleaning and restoring packages..."
dotnet clean
dotnet restore

if [ $? -ne 0 ]; then
    echo "❌ Package restore failed"
    exit 1
fi

echo "✅ Packages restored successfully"

# Build the solution
echo "🔨 Building the solution..."
dotnet build --configuration Release --no-restore

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    echo ""
    echo "🎉 All projects compiled successfully!"
    echo ""
    echo "Project Summary:"
    echo "• PersonalizedAssistant.Shared - ✅"
    echo "• PersonalizedAssistant.Infrastructure - ✅"
    echo "• PersonalizedAssistant.API - ✅"
    echo "• PersonalizedAssistant.AuthService - ✅"
    echo "• PersonalizedAssistant.DataCollectionService - ✅"
    echo "• PersonalizedAssistant.DataProcessingService - ✅"
    echo "• PersonalizedAssistant.ChatAgentService - ✅"
    echo ""
    echo "Next steps:"
    echo "1. Configure Google API credentials in appsettings.Development.json"
    echo "2. Run 'docker-compose up' to start all services"
    echo "3. Visit http://localhost:5000/swagger for API documentation"
    echo ""
    echo "Happy coding! 🚀"
else
    echo "❌ Build failed"
    exit 1
fi


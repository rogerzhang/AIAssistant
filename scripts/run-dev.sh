#!/bin/bash

# Personalized Assistant Development Run Script
# This script runs the application in development mode

echo "🚀 Starting Personalized Assistant in Development Mode..."

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "❌ Docker is not running. Please start Docker first."
    exit 1
fi

# Start all services with Docker Compose
echo "🐳 Starting all services with Docker Compose..."
docker-compose up -d

# Wait for services to be ready
echo "⏳ Waiting for services to be ready..."
sleep 15

# Check service health
echo "🔍 Checking service health..."

# Check API Gateway
if curl -s http://localhost:5000/health > /dev/null; then
    echo "✅ API Gateway is running at http://localhost:5000"
else
    echo "⚠️  API Gateway might not be ready yet"
fi

# Check Grafana
if curl -s http://localhost:3000 > /dev/null; then
    echo "✅ Grafana is running at http://localhost:3000 (admin/admin123)"
else
    echo "⚠️  Grafana might not be ready yet"
fi

# Check Prometheus
if curl -s http://localhost:9090 > /dev/null; then
    echo "✅ Prometheus is running at http://localhost:9090"
else
    echo "⚠️  Prometheus might not be ready yet"
fi

echo ""
echo "🎉 All services are starting up!"
echo ""
echo "Available endpoints:"
echo "• API Gateway: http://localhost:5000"
echo "• API Documentation: http://localhost:5000/swagger"
echo "• Auth Service: http://localhost:5001"
echo "• Data Collection Service: http://localhost:5002"
echo "• Data Processing Service: http://localhost:5003"
echo "• Chat Agent Service: http://localhost:5004"
echo "• Grafana: http://localhost:3000 (admin/admin123)"
echo "• Prometheus: http://localhost:9090"
echo ""
echo "To view logs: docker-compose logs -f"
echo "To stop services: docker-compose down"
echo ""
echo "Happy coding! 🚀"

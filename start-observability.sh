#!/bin/bash

# ECommerce with Observability Quick Start Script

echo "🔧 Starting ECommerce Application with Observability..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}❌ Docker is not running. Please start Docker and try again.${NC}"
    exit 1
fi

# Start the full application stack
echo -e "${BLUE}🚀 Starting ECommerce application with observability...${NC}"
docker-compose up -d

# Wait for services to be ready
echo -e "${YELLOW}⏳ Waiting for services to initialize...${NC}"
sleep 15

# Check service status
echo -e "${BLUE}📊 Checking service status...${NC}"

services=("ecommerce.webapi" "ecommerce.authserver" "ecommerce.jaeger" "ecommerce.prometheus" "ecommerce.grafana" "ecommerce.seq")
all_healthy=true

for service in "${services[@]}"; do
    if docker-compose ps | grep -q "$service.*Up"; then
        echo -e "${GREEN}✅ $service is running${NC}"
    else
        echo -e "${RED}❌ $service is not running${NC}"
        all_healthy=false
    fi
done

if [ "$all_healthy" = true ]; then
    echo -e "${GREEN}🎉 All services are running!${NC}"
    echo ""
    echo -e "${BLUE}🌐 Application URLs:${NC}"
    echo -e "  • WebAPI:       ${YELLOW}http://localhost:4000${NC}"
    echo -e "  • AuthServer:   ${YELLOW}https://localhost:5002${NC}"
    echo -e "  • API Metrics:  ${YELLOW}http://localhost:4000/metrics${NC}"
    echo ""
    echo -e "${BLUE}📊 Observability URLs:${NC}"
    echo -e "  • Jaeger UI:    ${YELLOW}http://localhost:16686${NC}"
    echo -e "  • Prometheus:   ${YELLOW}http://localhost:9090${NC}"
    echo -e "  • Grafana:      ${YELLOW}http://localhost:3000${NC} (admin/admin)"
    echo -e "  • Seq:          ${YELLOW}http://localhost:5341${NC}"
    echo ""
    echo -e "${BLUE}🗄️  Database URLs:${NC}"
    echo -e "  • PostgreSQL:   ${YELLOW}localhost:5432${NC} (postgres/postgres)"
    echo -e "  • PgAdmin:      ${YELLOW}http://localhost:8082${NC} (admin@example.com/admin)"
    echo -e "  • Redis:        ${YELLOW}localhost:6379${NC}"
    echo ""
    echo -e "${GREEN}✨ Everything is ready! Start using the application!${NC}"
else
    echo -e "${RED}⚠️  Some services failed to start. Check logs with:${NC}"
    echo -e "  ${YELLOW}docker-compose logs${NC}"
fi

echo ""
echo -e "${BLUE}💡 Useful commands:${NC}"
echo -e "  Stop all services:    ${YELLOW}docker-compose down${NC}"
echo -e "  View logs:            ${YELLOW}docker-compose logs -f${NC}"
echo -e "  Restart services:     ${YELLOW}docker-compose restart${NC}"
echo -e "  Build and start:      ${YELLOW}docker-compose up --build -d${NC}"
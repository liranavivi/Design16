# Flow Architecture System

A microservices-based architecture for managing data flows, processing chains, and task scheduling.

## Project Structure

The solution is organized into the following layers:

### Core Layer
- **FlowArchitecture.Core**: Core interfaces and abstractions
- **FlowArchitecture.Common**: Common utilities and shared components

### Service Layer
- **FlowArchitecture.Services.Abstractions**: Service interfaces
- **FlowArchitecture.Services.Importers**: Importer service implementations
- **FlowArchitecture.Services.Processors**: Processor service implementations
- **FlowArchitecture.Services.Exporters**: Exporter service implementations
- **FlowArchitecture.Services.Managers**: Manager service implementations

### Protocol Layer
- **FlowArchitecture.Protocols.Abstractions**: Protocol interfaces
- **FlowArchitecture.Protocols.Implementations**: Protocol implementations

### Entity Layer
- **FlowArchitecture.Entities.Abstractions**: Entity interfaces
- **FlowArchitecture.Entities.Implementations**: Entity implementations

### Infrastructure Layer
- **FlowArchitecture.Infrastructure.Data**: Data access components (MongoDB)
- **FlowArchitecture.Infrastructure.Messaging**: Messaging components (MassTransit)
- **FlowArchitecture.Observability**: Observability components (OpenTelemetry)

### Hosting Layer
- **FlowArchitecture.Worker.Importer**: Worker service for importers
- **FlowArchitecture.Worker.Processor**: Worker service for processors
- **FlowArchitecture.Worker.Exporter**: Worker service for exporters
- **FlowArchitecture.Worker.Orchestrator**: Worker service for orchestration
- **FlowArchitecture.Api.Gateway**: API gateway
- **FlowArchitecture.Api.Admin**: Admin API
- **FlowArchitecture.Api.EntityManagement**: Entity management API

### UI Layer
- **FlowArchitecture.Admin.UI**: Admin UI (Blazor)

### Test Layer
- **FlowArchitecture.Tests.Unit**: Unit tests
- **FlowArchitecture.Tests.Integration**: Integration tests
- **FlowArchitecture.Tests.System**: System tests

## Technology Stack

- **.NET 9.0**: Base framework
- **MongoDB**: Data persistence
- **Hazelcast**: Distributed caching
- **MassTransit**: Messaging
- **Quartz.NET**: Scheduling
- **OpenTelemetry**: Observability

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- MongoDB
- Docker (optional)

### Building the Solution

```bash
dotnet build
```

### Running the Tests

```bash
dotnet test
```

## Development Guidelines

- Follow the microservices architecture principles
- Use appropriate service types for each component
- Build and test incrementally
- Write tests before or alongside implementation
- Follow the dependency rules (dependencies point inward)

## License

This project is licensed under the MIT License - see the LICENSE file for details.

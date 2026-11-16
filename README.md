
<p align="center">
  <img width="300" height="72" alt="image" src="https://github.com/user-attachments/assets/83bddcc4-32da-46f1-a59d-5ada4c2e0679" />
</p>

<h1 align="center">InsightFlow</h1>
<h3 align="center">Intelligent Sales Analytics and Predictive Forecasting Platform</h3>

<p align="center">
  <b>Transform raw business data into actionable intelligence.</b>
</p>

<p align="center">
  <!-- 🔗 BADGES: replace YOUR_USERNAME / YOUR_REPO and add/remove as needed -->
  <a href="https://github.com/YOUR_USERNAME/YOUR_REPO/actions"><img src="https://img.shields.io/github/actions/workflow/status/YOUR_USERNAME/YOUR_REPO/YOUR_WORKFLOW.yml?label=build" alt="Build Status" /></a>
  <img src="https://img.shields.io/badge/.NET-10-512BD4" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Angular-22-DD0031" alt="Angular 22" />
  <img src="https://img.shields.io/badge/Python-FastAPI-009688" alt="FastAPI" />
  <a href="YOUR_LICENSE_LINK"><img src="https://img.shields.io/badge/license-YOUR_LICENSE-blue" alt="License" /></a>
</p>

<p align="center">
  <a href="YOUR_LIVE_FRONTEND_URL">🌐 Live Demo</a> •
  <a href="YOUR_BACKEND_SWAGGER_URL">📘 API Docs (Swagger)</a> •
  <a href="YOUR_DEMO_VIDEO_URL">🎥 Demo Video</a> •
  <a href="YOUR_PORTFOLIO_OR_LINKEDIN_URL">👤 Author</a>
</p>

<!-- 🖼️ HERO IMAGE / GIF: a dashboard screenshot or short walkthrough GIF -->
<p align="center">
  <img src="docs/images/hero-dashboard.png" alt="InsightFlow AI Dashboard" width="90%" />
</p>

---

InsightFlow AI is a full-stack business intelligence and predictive analytics platform designed to help businesses understand historical performance, identify trends, forecast future outcomes, and interact with their data using natural language.

The platform combines traditional business analytics, machine learning-based forecasting, and Generative AI architecture to create an intelligent decision-support system.

Rather than relying exclusively on static dashboards, InsightFlow aims to provide an interactive analytical experience where users can upload datasets, explore business metrics, generate predictions, and eventually delegate analytical tasks to AI agents.

---

## 📑 Table of Contents

1. [Project Overview](#1-project-overview)
2. [Live Links](#2-live-links)
3. [Screenshots](#3-screenshots)
4. [Key Features](#4-key-features)
5. [Technology Stack](#5-technology-stack)
6. [System Architecture](#6-system-architecture)
7. [Backend Architecture](#7-backend-architecture)
8. [Database Architecture](#8-database-architecture)
9. [Forecasting Microservice Architecture](#9-forecasting-microservice-architecture)
10. [Agentic AI Architecture](#10-agentic-ai-architecture)
11. [API Architecture](#11-api-architecture)
12. [Deployment Architecture](#12-deployment-architecture)
13. [Repository Structure](#13-repository-structure)
14. [Local Development Setup](#14-local-development-setup)
15. [Security Considerations](#15-security-considerations)
16. [Development Roadmap](#16-development-roadmap)
17. [Engineering Concepts Demonstrated](#17-engineering-concepts-demonstrated)
18. [Project Information](#18-project-information)
19. [License](#19-license)
20. [Author](#20-author)

---

# 1. Project Overview

Businesses generate large volumes of sales data, but extracting meaningful insights often requires manual analysis, technical expertise, and separate analytics tools.

InsightFlow AI addresses this challenge through a unified platform that integrates:

- **Data Engineering:** Dataset ingestion, validation, profiling, and management.
- **Business Intelligence:** Automated KPIs, analytical summaries, and interactive dashboards.
- **Predictive Analytics:** Time-series forecasting using Python-based statistical and machine learning models.
- **Generative AI:** Natural-language interaction with business data.
- **Agentic AI:** An extensible architecture for intelligent agents, tool execution, and multi-step analytical workflows.

### Primary Objectives

1. Simplify sales data analysis.
2. Automate the discovery of business performance metrics.
3. Predict future sales and revenue trends.
4. Enable natural-language interaction with datasets.
5. Build an extensible foundation for autonomous AI-powered business intelligence.

---

# 2. Live Links

| Resource | Link |
|---|---|
| 🌐 Frontend (Production) | [YOUR_FRONTEND_URL](YOUR_FRONTEND_URL) |
| ⚙️ Backend API (Production) | [YOUR_BACKEND_URL](YOUR_BACKEND_URL) |
| 📘 Swagger / OpenAPI Docs | [YOUR_SWAGGER_URL](YOUR_SWAGGER_URL) |
| 🐍 Forecasting Service (Production) | [YOUR_FORECASTING_SERVICE_URL](YOUR_FORECASTING_SERVICE_URL) |
| ❤️ Health Check | [YOUR_HEALTH_CHECK_URL](YOUR_HEALTH_CHECK_URL) |
| 🎥 Demo Video | [YOUR_DEMO_VIDEO_URL](YOUR_DEMO_VIDEO_URL) |
| 📂 Sample Dataset | [YOUR_SAMPLE_DATASET_LINK](YOUR_SAMPLE_DATASET_LINK) |

> **Demo credentials (optional):** `YOUR_DEMO_EMAIL` / `YOUR_DEMO_PASSWORD`
> Remove this note if you do not want to provide a public demo account.

---

# 3. Screenshots

<!-- 🖼️ Add one screenshot per module. Suggested size: 1280x720 or similar. -->

| Landing Page | Dashboard |
|---|---|
| ![Landing Page](docs/images/landing.png) | ![Dashboard](docs/images/dashboard.png) |

| Dataset Upload | Dataset Details |
|---|---|
| ![Dataset Upload](docs/images/upload.png) | ![Dataset Details](docs/images/dataset-details.png) |

| Analytics Dashboard | Forecasting |
|---|---|
| ![Analytics](docs/images/analytics.png) | ![Forecasting](docs/images/forecasting.png) |

| AI Analyst | Login / Register |
|---|---|
| ![AI Analyst](docs/images/ai-analyst.png) | ![Auth](docs/images/auth.png) |

---

# 4. Key Features

## 4.1 Authentication & User Management

- User registration and login.
- ASP.NET Core Identity.
- JWT-based authentication.
- Protected API endpoints.
- User-specific dataset management.
- Authenticated backend operations.

<!-- 🖼️ Screenshot: login / registration page -->
![Authentication](docs/images/feature-auth.png)

## 4.2 Dataset Management

Users can upload and manage their business datasets.

Supported functionality:

- CSV file upload.
- Excel file upload.
- File validation.
- Dataset metadata management.
- Automatic column discovery.
- Column data-type identification.
- Dataset profiling and processing.
- Dataset ownership and isolation.

Supported analytical columns include:
- Date and time.
- Numerical values.
- Categorical attributes.
- Revenue and sales-related metrics.

<!-- 🖼️ Screenshot: upload page and dataset list -->
![Dataset Management](docs/images/feature-datasets.png)

## 4.3 Business Analytics Dashboard

The analytics module transforms uploaded datasets into meaningful business information.

Capabilities include:

- Dataset summaries.
- Total revenue analysis.
- Sales performance metrics.
- Numerical and categorical analysis.
- Product and regional breakdowns.
- Interactive charts.
- Historical trend analysis.
- Dataset-specific analytics.

Visualization is powered by Chart.js and integrated into Angular through ng2-charts.

<!-- 🖼️ Screenshot: analytics dashboard with charts -->
![Analytics Dashboard](docs/images/feature-analytics.png)

## 4.4 Predictive Analytics & Forecasting

InsightFlow AI incorporates an independent Python forecasting service.

The objective is to allow users to select a dataset, identify a date column and numerical target, configure a prediction horizon, and generate future forecasts.

Forecasting configuration includes:

- Dataset selection.
- Date column selection.
- Target column selection.
- Forecast frequency.
- Prediction horizon.
- Model configuration.

The forecasting architecture supports:

- Statistical time-series models.
- Machine learning-based forecasting.
- Historical versus predicted value comparison.
- Forecast evaluation metrics.
- Prediction intervals.
- Forecast execution history.

Forecasting is separated from the primary .NET backend through a Python FastAPI microservice.

<!-- 🖼️ Screenshot: forecast configuration + historical vs predicted chart -->
![Forecasting](docs/images/feature-forecasting.png)

## 4.5 AI Analyst — Generative & Agentic AI

The AI Analyst is envisioned as an intelligent analytical assistant that allows users to interact with their business data using natural language.

Instead of manually navigating multiple dashboards, users can ask questions about their datasets and receive contextual explanations.

The architecture explores:

- LLM-powered conversational interaction.
- Semantic Kernel orchestration.
- Context-aware conversations.
- Function calling and application tool integration.
- Dataset-aware analytical assistance.
- Persistent conversation history.
- Agent execution tracking.

The underlying design supports extending the assistant into a specialized agentic business intelligence system.

<!-- 🖼️ Screenshot or GIF: AI Analyst chat interface -->
![AI Analyst](docs/images/feature-ai-analyst.png)

## 4.6 Multi-Agent Architecture — Future Extension

The platform is designed to accommodate specialized agents with distinct responsibilities.

Potential agents include:

| Agent | Responsibility |
|---|---|
| Data Analyst Agent | Understands datasets and analytical requirements |
| Analytics Agent | Executes analytical operations |
| Forecasting Agent | Coordinates forecasting requests |
| Insight Generation Agent | Identifies important business patterns |
| Recommendation Agent | Generates actionable business recommendations |
| Orchestrator Agent | Coordinates specialized agents and workflows |

These are proposed agent responsibilities, not a claim that every specialized agent is currently implemented.

---

# 5. Technology Stack

## Frontend

| Technology | Usage |
|---|---|
| Angular 22 | Frontend framework |
| TypeScript | Application logic |
| HTML5 | UI structure |
| CSS3 | Styling and responsive layouts |
| Chart.js | Data visualization |
| ng2-charts | Angular chart integration |
| Angular Router | Navigation |
| Angular Services | API communication |
| RxJS | Reactive programming |
| HttpClient | Backend communication |

## Backend

| Technology | Usage |
|---|---|
| C# | Backend programming language |
| .NET 10 | Application framework |
| ASP.NET Core Web API | REST API development |
| Entity Framework Core | ORM and database access |
| ASP.NET Core Identity | Authentication and user management |
| JWT | Token-based authorization |
| Swagger / OpenAPI | API documentation |
| Dependency Injection | Service management |

## Database

| Technology | Usage |
|---|---|
| Microsoft SQL Server | Relational database |
| SQL Server LocalDB | Local development |
| Azure SQL Database | Cloud database |
| Entity Framework Core | Database access |
| Database-First Approach | Existing schema integration |

## AI & Generative AI

| Technology | Usage |
|---|---|
| Microsoft Semantic Kernel | LLM orchestration architecture |
| Large Language Models | Natural-language understanding and generation |
| Function Calling | Application tool integration |
| Conversational Context | Contextual interactions |
| Retrieval-Augmented Generation (RAG) | Planned retrieval-grounded analytical assistance |
| Multi-Agent Systems | Planned specialized agent collaboration |

## Machine Learning & Forecasting

| Technology | Usage |
|---|---|
| Python | Forecasting implementation |
| FastAPI | Forecasting REST API |
| Pandas | Data processing |
| NumPy | Numerical computation |
| Uvicorn | Python application server |
| Statistical Time-Series Models | Forecast generation |

## Cloud & DevOps

| Technology | Usage |
|---|---|
| Microsoft Azure | Cloud infrastructure |
| Azure SQL Database | Managed database |
| Render | Application hosting |
| Docker | Containerization |
| Git | Version control |
| GitHub | Source code management |
| CI/CD | Deployment automation |

## Development Tools

- Visual Studio 2026 / Visual Studio 2022.
- Visual Studio Code.
- SQL Server Management Studio.
- PowerShell.
- GitHub.
- Render Dashboard.
- Azure Portal.
- Swagger UI.
- npm.
- NuGet.
- Docker.

---

# 6. System Architecture

InsightFlow follows a modular architecture with an Angular frontend, a .NET backend, a relational database, and an independent forecasting service.

<!-- 🖼️ ARCHITECTURE DIAGRAM: export a clean diagram (draw.io / Excalidraw / Lucidchart)
     and place it here. The text diagram below can stay as a fallback. -->
![System Architecture Diagram](docs/images/architecture-diagram.png)

```text
                       USER
                         |
                         v
                 ANGULAR FRONTEND
                    Angular 22
                         |
                         | HTTP / REST
                         v
                 ASP.NET CORE WEB API
                      .NET 10
                         |
          +--------------+--------------+
          |              |              |
          v              v              v
    AUTHENTICATION    ANALYTICS       AI ANALYST
    Identity + JWT    SERVICES      AI ORCHESTRATION
          |              |              |
          |              |              v
          |              |        LLM / AI MODEL
          |              |              |
          |              |              v
          |              |        APPLICATION TOOLS
          |              |              |
          +--------------+--------------+
                         |
              +----------+----------+
              |                     |
              v                     v
          AZURE SQL           PYTHON FASTAPI
          DATABASE             FORECASTING
                                    |
                                    v
                             TIME-SERIES MODELS
                                    |
                                    v
                              PREDICTIONS
                                    |
                                    v
                               AZURE SQL
```

### Architectural Responsibilities

**Angular Frontend**
- User interface.
- Authentication.
- Dataset uploads.
- Dataset management.
- Analytics dashboards.
- Forecasting configuration.
- AI Analyst interface.

**ASP.NET Core Backend**
- Authentication and authorization.
- Dataset management.
- Business analytics.
- Database operations.
- Forecasting orchestration.
- AI orchestration and tool execution.

**Python Forecasting Service**
- Data preprocessing.
- Time-series processing.
- Model execution.
- Forecast generation.
- Evaluation and prediction outputs.

**Azure SQL Database**
- User records.
- Dataset metadata.
- Dataset column information.
- Forecast history.
- Forecast prediction points.
- AI conversation and tool-execution records.

---

# 7. Backend Architecture

The backend is developed using ASP.NET Core Web API with .NET 10.

The solution contains two primary .NET projects.

```text
Solution1/
│
├── InsightFlow.Api/
│   ├── Controllers/
│   ├── Services/
│   ├── Program.cs
│   ├── appsettings.json
│   └── InsightFlow.Api.csproj
│
├── InsightFlow.Core/
│   ├── Models/
│   ├── Services/
│   ├── Repositories/
│   └── InsightFlow.Core.csproj
│
└── Solution1.slnx
```

### InsightFlow.Api

Responsible for:

- HTTP endpoints.
- Request and response handling.
- Authentication middleware.
- Authorization.
- Dependency injection.
- API configuration.
- Service orchestration.

### InsightFlow.Core

Responsible for:

- Domain models.
- Business logic.
- Application services.
- Database-related abstractions.
- Core application functionality.

### Backend Design Principles

- Separation of concerns.
- Dependency injection.
- RESTful API design.
- Reusable services.
- User-specific data access.
- Modular application development.

---

# 8. Database Architecture

The project uses SQL Server for relational data storage and Entity Framework Core for database operations.

The development database uses SQL Server LocalDB, while the production database uses Azure SQL Database.

<!-- 🖼️ ER DIAGRAM: export from SSMS / dbdiagram.io / draw.io -->
![Entity Relationship Diagram](docs/images/er-diagram.png)

### Major Entities

| Entity | Responsibility |
|---|---|
| Application Users | User accounts and authentication |
| Datasets | Uploaded dataset metadata |
| DatasetColumns | Dataset column definitions |
| ForecastRuns | Forecast execution records |
| ForecastPoints | Predicted values and intervals |
| Conversations | AI conversation records |
| ChatMessages | Individual conversation messages |
| AgentToolExecutions | Tool invocation and execution records |

### Forecasting Data Model

A forecast execution stores information such as:

- Dataset ID.
- User ID.
- Target column.
- Date column.
- Forecast frequency.
- Prediction horizon.
- Model version.
- Configuration.
- Evaluation metrics.
- Execution status.
- Timestamps.

Forecast points store:

- Forecast date.
- Predicted value.
- Lower prediction bound.
- Upper prediction bound.

### Data Security

The application uses authenticated user context to control access to datasets and associated operations.

---

# 9. Forecasting Microservice Architecture

Forecasting is implemented as a separate Python service instead of placing all machine learning logic inside the .NET backend.

This separation provides:

- Independent model development.
- Easier Python library integration.
- Independent deployment.
- Separation of application and ML responsibilities.
- Future flexibility to introduce additional forecasting algorithms.

### Communication Flow

1. User selects a dataset and forecasting parameters.
2. Angular sends a request to the ASP.NET Core API.
3. The backend validates the user and dataset.
4. The backend coordinates the forecasting operation.
5. Python FastAPI processes the forecasting request.
6. The forecasting engine generates predictions.
7. Results are returned to the backend.
8. Forecast records and prediction points are persisted.
9. Angular displays the results.

<!-- 🖼️ SEQUENCE DIAGRAM (optional): Angular → .NET API → FastAPI → Azure SQL -->
![Forecasting Sequence Diagram](docs/images/forecasting-sequence.png)

> 🔗 **Forecasting service repository / folder:** [YOUR_FORECASTING_SERVICE_REPO_LINK](YOUR_FORECASTING_SERVICE_REPO_LINK)

---

# 10. Agentic AI Architecture

The long-term AI architecture is based on an orchestrated system where an LLM can interact with application capabilities.

### Proposed Workflow

```text
User Query
    |
    v
AI Analyst Interface
    |
    v
AI Orchestration Layer
    |
    v
LLM Reasoning
    |
    v
Intent / Tool Selection
    |
    v
Application Tool Execution
    |
    v
Tool Result
    |
    v
LLM Response Generation
    |
    v
Contextual Response
```

<!-- 🖼️ AGENT WORKFLOW DIAGRAM (optional): a visual version of the flow above -->
![Agentic AI Workflow](docs/images/agent-workflow.png)

### Why Agentic AI?

A traditional chatbot primarily generates text responses.

An agentic system can additionally:

- Interpret user goals.
- Select available tools.
- Invoke application functions.
- Process returned results.
- Coordinate multi-step workflows.
- Generate responses based on application data.

### Future AI Capabilities

- Multi-agent collaboration.
- Dataset-aware reasoning.
- RAG-based contextual retrieval.
- Automated business insights.
- Natural-language analytics.
- Intelligent anomaly detection.
- Predictive recommendations.
- Conversational forecasting.

---

# 11. API Architecture

The ASP.NET Core backend exposes REST APIs for frontend communication.

Major API modules include:

| Module | Responsibility |
|---|---|
| Auth | Registration and login |
| Datasets | Dataset upload and management |
| Analytics | Dataset summaries and analytical results |
| Forecasting | Forecast execution and history |
| AI Analyst | Conversational analytics and agent interactions |

📘 **Full API documentation:** [YOUR_SWAGGER_URL](YOUR_SWAGGER_URL)

<!-- 🖼️ Screenshot: Swagger UI -->
![Swagger UI](docs/images/swagger.png)

### Python Forecasting API

The forecasting microservice exposes independent endpoints for forecast processing and service health checks.

The exact endpoint contracts are maintained by the Python service implementation.

📘 **Forecasting API docs (FastAPI `/docs`):** [YOUR_FORECASTING_DOCS_URL](YOUR_FORECASTING_DOCS_URL)

---

# 12. Deployment Architecture

The project uses a combination of Microsoft Azure and Render.

| Component | Deployment Platform | Link |
|---|---|---|
| Angular Frontend | Render | [YOUR_FRONTEND_URL](YOUR_FRONTEND_URL) |
| ASP.NET Core Backend | Render | [YOUR_BACKEND_URL](YOUR_BACKEND_URL) |
| Python Forecasting Service | Render | [YOUR_FORECASTING_SERVICE_URL](YOUR_FORECASTING_SERVICE_URL) |
| Relational Database | Azure SQL Database | N/A (private) |
| Source Code | GitHub | [YOUR_GITHUB_REPOSITORY_URL](YOUR_GITHUB_REPOSITORY_URL) |

<!-- 🖼️ DEPLOYMENT DIAGRAM (optional): Render services + Azure SQL -->
![Deployment Architecture](docs/images/deployment-diagram.png)

### Frontend Deployment

- Angular production build.
- Environment-specific API configuration.
- Static website hosting.

### Backend Deployment

- Docker-based hosting.
- ASP.NET Core application.
- Production configuration through environment variables.
- Cloud database connectivity.

### Database Deployment

- Azure SQL Database.
- Production connection configuration.
- Separate local development environment.

**Deployment status:** The individual frontend and backend services have been deployed. End-to-end production authentication and service integration are still undergoing verification.

---

# 13. Repository Structure

The repository is organized around the main application components.

```text
InsightFlow/
│
├── InsightFlow.Api/
│
├── InsightFlow.Core/
│
├── InsightFlow-ui/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/
│   │   │   ├── features/
│   │   │   │   ├── auth/
│   │   │   │   ├── landing/
│   │   │   │   ├── dashboard/
│   │   │   │   ├── datasets/
│   │   │   │   ├── dataset-details/
│   │   │   │   ├── upload/
│   │   │   │   ├── analytics/
│   │   │   │   ├── forecasting/
│   │   │   │   └── ai-analyst/
│   │   │   └── app.routes.ts
│   │   │
│   │   └── environments/
│   │
│   └── package.json
│
├── Solution1.slnx
│
├── Dockerfile
│
└── README.md
```

The exact Python forecasting service location and deployment configuration are maintained separately from the two-project .NET solution.

---

# 14. Local Development Setup

### Prerequisites

Install:

- .NET 10 SDK.
- Visual Studio with ASP.NET Core development tools.
- Node.js and npm.
- Angular CLI.
- Python.
- SQL Server or SQL Server LocalDB.
- Git.

### Step 1: Clone the Repository

```bash
git clone YOUR_GITHUB_REPOSITORY_URL
```

### Step 2: Open the Backend Solution

Open:

```text
Solution1.slnx
```

The solution contains:

- InsightFlow.Api
- InsightFlow.Core

### Step 3: Configure the Database

Configure the SQL Server connection string in the local configuration.

Do not commit database passwords or production credentials.

### Step 4: Restore and Build the Backend

```bash
dotnet restore
dotnet build
```

### Step 5: Run the Backend

```bash
dotnet run --project InsightFlow.Api
```

### Step 6: Configure the Angular Frontend

```bash
cd InsightFlow-ui
npm install
```

### Step 7: Run Angular

```bash
ng serve
```

The frontend development environment uses the local backend API configuration.

### Step 8: Run the Forecasting Service

Navigate to the forecasting service directory, install its Python dependencies, configure the required environment variables, and start the FastAPI application using Uvicorn.

> 🔗 **Detailed forecasting setup:** [YOUR_FORECASTING_SETUP_DOC_LINK](YOUR_FORECASTING_SETUP_DOC_LINK)

### Environment Variables

<!-- 📝 Fill this table in. Never put real secrets here. -->

| Variable | Service | Purpose | Required | Example |
|---|---|---|---|---|
| `ConnectionStrings__DefaultConnection` | Backend | SQL Server connection | Yes | `YOUR_EXAMPLE_VALUE` |
| `Jwt__Key` | Backend | JWT signing key | Yes | `YOUR_EXAMPLE_VALUE` |
| `Jwt__Issuer` | Backend | JWT issuer | Yes | `YOUR_EXAMPLE_VALUE` |
| `ForecastingService__BaseUrl` | Backend | URL of the FastAPI service | Yes | `http://localhost:8000` |
| `YOUR_VARIABLE` | YOUR_SERVICE | YOUR_PURPOSE | YOUR_REQUIRED | `YOUR_EXAMPLE_VALUE` |

---

# 15. Security Considerations

The application incorporates:

- JWT authentication.
- ASP.NET Core Identity.
- Protected API endpoints.
- User-specific dataset access.
- CORS configuration.
- Environment-specific application settings.
- Externalized secrets and connection strings.
- Authenticated backend operations.

Future security improvements include:

- Role-based access control.
- Agent-level permissions.
- API rate limiting.
- Audit logging.
- SQL query validation.
- Production secret management.
- More comprehensive automated security testing.

🔐 **Report a vulnerability:** YOUR_SECURITY_CONTACT_EMAIL_OR_LINK

---

# 16. Development Roadmap

### Phase 1 — Core Application

- [x] .NET backend foundation.
- [x] Angular frontend.
- [x] SQL Server integration.
- [x] User registration and login implementation.
- [x] JWT authentication.
- [x] Dataset upload.
- [x] CSV and Excel processing.
- [x] Dataset management.
- [x] User-specific data isolation.

### Phase 2 — Business Analytics

- [x] Analytics API foundation.
- [x] Dataset summary endpoints.
- [x] Angular analytics dashboard.
- [x] Interactive charts.
- [ ] Further performance optimization.
- [ ] Advanced analytical insights.

### Phase 3 — Predictive Analytics

- [x] Forecasting architecture and database entities.
- [x] Python-based forecasting service foundation.
- [ ] Complete forecasting workflow verification.
- [ ] Forecast visualization improvements.
- [ ] Forecast integration with analytics dashboards.
- [ ] Advanced model evaluation.

### Phase 4 — Generative & Agentic AI

- [x] AI conversation data model.
- [x] Agent tool-execution data model.
- [ ] Complete AI Analyst workflow.
- [ ] Dataset-aware AI tools.
- [ ] Persistent conversational experience.
- [ ] Multi-agent orchestration.
- [ ] RAG integration.
- [ ] Automated insight generation.

### Phase 5 — Production & Scalability

- [x] GitHub version control.
- [x] Frontend cloud deployment.
- [x] Backend cloud deployment.
- [x] Azure SQL infrastructure.
- [ ] End-to-end production authentication verification.
- [ ] Cloud file-storage integration.
- [ ] Monitoring and observability.
- [ ] CI/CD improvements.
- [ ] Automated integration testing.

📋 **Issue tracker / project board:** [YOUR_PROJECT_BOARD_LINK](YOUR_PROJECT_BOARD_LINK)
📝 **Changelog:** [CHANGELOG.md](YOUR_CHANGELOG_LINK)

---

# 17. Engineering Concepts Demonstrated

InsightFlow AI brings together several areas of modern software engineering.

**Full-Stack Development**
- Angular.
- TypeScript.
- C#.
- ASP.NET Core.
- REST API development.

**Backend Engineering**
- Layered architecture.
- Dependency injection.
- Entity Framework Core.
- Authentication and authorization.
- Database-first development.

**Cloud Engineering**
- Azure SQL Database.
- Docker.
- Render deployment.
- Environment configuration.
- Git-based deployment workflows.

**Machine Learning Engineering**
- Python.
- FastAPI.
- Time-series forecasting.
- Statistical modelling.
- ML service integration.

**Generative & Agentic AI**
- LLM integration.
- Semantic Kernel architecture.
- Function calling.
- AI tool execution.
- Conversational context.
- Multi-agent system design.
- Retrieval-Augmented Generation.

---

# 18. Project Information

**Project:** InsightFlow AI

**Category:** Full-Stack Development | Business Intelligence | Predictive Analytics | Agentic AI

**Frontend:** Angular 22

**Backend:** ASP.NET Core Web API (.NET 10)

**Database:** Microsoft SQL Server / Azure SQL Database

**Forecasting:** Python / FastAPI

**AI Architecture:** Semantic Kernel and LLM-based orchestration

**Cloud:** Microsoft Azure and Render

**Version Control:** Git and GitHub

---

# 19. License

This project is licensed under the **YOUR_LICENSE_NAME** license. See the [LICENSE](YOUR_LICENSE_FILE_LINK) file for details.

---

# 20. Author

<p>
  <!-- 🖼️ PROFILE PHOTO (optional) -->
  <img src="docs/images/author.jpg" alt="Tisha Yadav" width="100" style="border-radius:50%;" />
</p>

**Tisha Yadav**

Digital Specialist Engineer | .NET Full-Stack Development | Generative AI | Agentic AI

| | |
|---|---|
| 💼 LinkedIn | [YOUR_LINKEDIN_URL](YOUR_LINKEDIN_URL) |
| 🐙 GitHub | [YOUR_GITHUB_PROFILE_URL](YOUR_GITHUB_PROFILE_URL) |
| 🌐 Portfolio | [YOUR_PORTFOLIO_URL](YOUR_PORTFOLIO_URL) |
| 📧 Email | [YOUR_EMAIL](mailto:YOUR_EMAIL) |

---

<p align="center">

**InsightFlow AI**

*From raw data to intelligent decisions.*

</p>

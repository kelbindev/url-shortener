# URL Shortener

A .NET 8 Minimal API URL shortener hosted on Azure, with PostgreSQL for storage and Azure Key Vault for secrets management.

## Features

- Shorten long URLs to compact, shareable links
- JWT authentication for the shorten endpoint (sliding token & HTTP-only cookie)
- PostgreSQL database backend
- Azure Key Vault for secrets management
- CI/CD with GitHub Actions

## Tech Stack

- **.NET 8** Minimal API
- **PostgreSQL** database
- **Azure Key Vault** for secrets
- **Azure Container Apps** for hosting
- **Azure Container Registry (ACR)** for Docker images

---

## CI/CD — GitHub Actions Deployment

The workflow file at [`.github/workflows/deploy-aca.yml`](.github/workflows/deploy-aca.yml) automatically builds the Docker image, pushes it to Azure Container Registry, and deploys the updated image to an Azure Container App.

### Workflow Triggers

| Trigger | Description |
|---------|-------------|
| Push to `main` | Automatically builds and deploys on every merge |
| `workflow_dispatch` | Manual trigger from the GitHub Actions UI |

### Workflow Steps

1. **Build & Push** — Builds the Docker image from the repository root `Dockerfile` and pushes two tags to ACR:
   - `kelbinacr01.azurecr.io/url-shortener:latest`
   - `kelbinacr01.azurecr.io/url-shortener:<commit-sha>`
2. **Deploy** — Logs in to Azure and updates the Container App to run the new SHA-tagged image, triggering a new revision.

---

## Required GitHub Variables

Configure the following in **Settings → Secrets and variables → Actions → Variables**:

| Variable | Description |
|----------|-------------|
| `ACR_LOGIN_SERVER` | ACR login server hostname, e.g. `kelbinacr01.azurecr.io` |
| `ACR_USERNAME` | ACR admin username (or service principal app ID) |
| `ACR_PASSWORD` | ACR admin password (or service principal password) |
| `AZURE_RESOURCE_GROUP` | Resource group containing the Container App |
| `AZURE_CONTAINER_APP` | Name of the Azure Container App |

## Required GitHub Secrets

Configure the following in **Settings → Secrets and variables → Actions → Secrets**:

| Secret | Description |
|--------|-------------|
| `AZURE_CREDENTIALS` | Azure service principal JSON (see format below) |

### `AZURE_CREDENTIALS` format

```json
{
  "clientId": "<service-principal-app-id>",
  "clientSecret": "<service-principal-password>",
  "subscriptionId": "<azure-subscription-id>",
  "tenantId": "<azure-tenant-id>"
}
```

Create the service principal with the Azure CLI:

```bash
az ad sp create-for-rbac \
  --name "url-shortener-github-actions" \
  --role contributor \
  --scopes /subscriptions/<subscription-id>/resourceGroups/<resource-group> \
  --sdk-auth
```

Copy the entire JSON output as the value of the `AZURE_CREDENTIALS` secret.

---

## Local Development

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)
- PostgreSQL instance

### Run with Docker Compose

```bash
# Development
docker compose up

# Production-like
docker compose -f docker-compose.prod.yml up
```

### Environment Variables

Copy `.env.example` to `.env` and fill in the values:

```bash
cp .env.example .env
```

| Variable | Description |
|----------|-------------|
| `AZURE_KEYVAULT_URL` | URL of your Azure Key Vault |
| `AZURE_SECRET_NAME_CONNECTIONSTRING` | Key Vault secret name for the DB connection string |
| `AZURE_CLIENT_ID` | Azure service principal client ID |
| `AZURE_CLIENT_SECRET` | Azure service principal client secret |
| `AZURE_TENANT_ID` | Azure tenant ID |
| `JWT_SECRET` | Secret key for JWT signing |
| `JWT_ISSUER` | JWT issuer |
| `JWT_AUDIENCE` | JWT audience |

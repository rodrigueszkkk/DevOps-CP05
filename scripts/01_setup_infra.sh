#!/bin/bash
# ============================================================================
# DISCIPLINA: DevOps Tools & Cloud Computing (Prof. João Menk)
# SCRIPT: 01_setup_infra.sh - Automação Total de Infraestrutura na Azure
# ============================================================================

set -e

RM="rm561760"
# Região de provisionamento permitida pela assinatura (ex: centralus, canadacentral, eastus)
LOCATION="${1:-centralus}"
RESOURCE_GROUP_NAME="rg-safeshelter-${RM}"
SQL_SERVER_NAME="sql-safeshelter-${RM}-${LOCATION}"
SQL_DATABASE_NAME="db-safeshelter"
SQL_ADMIN_USER="admin_dimdim"
SQL_ADMIN_PASSWORD="Fiap@2tdsvms2026!"
APP_INSIGHTS_NAME="ai-safeshelter-${RM}"
APP_SERVICE_PLAN="plan-safeshelter-${RM}"
WEBAPP_NAME="app-safeshelter-${RM}"
DOTNET_RUNTIME="DOTNETCORE:8.0"

echo "=================================================================="
echo "Iniciando provisionamento na Azure para o RM: ${RM} em ${LOCATION}"
echo "=================================================================="

# Provedores
echo ">> Registrando Resource Providers..."
az provider register --namespace Microsoft.Web
az provider register --namespace Microsoft.Sql
az provider register --namespace Microsoft.Insights
az provider register --namespace Microsoft.OperationalInsights
az extension add --name application-insights --yes 2>/dev/null || true

# Resource Group
echo ">> Criando Resource Group: ${RESOURCE_GROUP_NAME}..."
az group create --name "${RESOURCE_GROUP_NAME}" --location "${LOCATION}"

# Azure SQL Server PaaS
echo ">> Criando Azure SQL Server PaaS: ${SQL_SERVER_NAME}..."
az sql server create \
  --name "${SQL_SERVER_NAME}" \
  --resource-group "${RESOURCE_GROUP_NAME}" \
  --location "${LOCATION}" \
  --admin-user "${SQL_ADMIN_USER}" \
  --admin-password "${SQL_ADMIN_PASSWORD}" \
  --enable-public-network true

# Banco SQL PaaS
echo ">> Criando Banco de Dados: ${SQL_DATABASE_NAME} (Camada Basic)..."
az sql db create \
  --resource-group "${RESOURCE_GROUP_NAME}" \
  --server "${SQL_SERVER_NAME}" \
  --name "${SQL_DATABASE_NAME}" \
  --service-objective Basic \
  --backup-storage-redundancy Local \
  --zone-redundant false

# Regras de Firewall (com pausa para propagação no ARM)
echo ">> Aguardando sincronização do Azure Resource Manager (15s)..."
sleep 15
echo ">> Configurando regras de firewall..."
az sql server firewall-rule create \
  --resource-group "${RESOURCE_GROUP_NAME}" \
  --server "${SQL_SERVER_NAME}" \
  --name "AllowAzureServices" \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

az sql server firewall-rule create \
  --resource-group "${RESOURCE_GROUP_NAME}" \
  --server "${SQL_SERVER_NAME}" \
  --name "liberaGeral" \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 255.255.255.255

# Application Insights
echo ">> Criando Application Insights: ${APP_INSIGHTS_NAME}..."
az monitor app-insights component create \
  --app "${APP_INSIGHTS_NAME}" \
  --location "${LOCATION}" \
  --resource-group "${RESOURCE_GROUP_NAME}" \
  --application-type web

APP_INSIGHTS_CONNECTION_STRING=$(az monitor app-insights component show \
  --app "${APP_INSIGHTS_NAME}" \
  --resource-group "${RESOURCE_GROUP_NAME}" \
  --query connectionString \
  --output tsv)

# App Service Plan F1 Linux
echo ">> Criando App Service Plan Linux (F1): ${APP_SERVICE_PLAN}..."
az appservice plan create \
  --name "${APP_SERVICE_PLAN}" \
  --resource-group "${RESOURCE_GROUP_NAME}" \
  --location "${LOCATION}" \
  --sku F1 \
  --is-linux

# Web App
echo ">> Criando Web App .NET 8 Linux: ${WEBAPP_NAME}..."
az webapp create \
  --name "${WEBAPP_NAME}" \
  --resource-group "${RESOURCE_GROUP_NAME}" \
  --plan "${APP_SERVICE_PLAN}" \
  --runtime "${DOTNET_RUNTIME}"

az resource update \
  --resource-group "${RESOURCE_GROUP_NAME}" \
  --namespace Microsoft.Web \
  --resource-type basicPublishingCredentialsPolicies \
  --name scm \
  --parent "sites/${WEBAPP_NAME}" \
  --set properties.allow=true

# App Settings
SQL_CONNECTION_STRING="Server=tcp:${SQL_SERVER_NAME}.database.windows.net,1433;Initial Catalog=${SQL_DATABASE_NAME};User ID=${SQL_ADMIN_USER};Password=${SQL_ADMIN_PASSWORD};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az webapp config appsettings set \
  --name "${WEBAPP_NAME}" \
  --resource-group "${RESOURCE_GROUP_NAME}" \
  --settings \
    APPLICATIONINSIGHTS_CONNECTION_STRING="${APP_INSIGHTS_CONNECTION_STRING}" \
    ApplicationInsightsAgent_EXTENSION_VERSION="~3" \
    XDT_MicrosoftApplicationInsights_Mode="Recommended" \
    XDT_MicrosoftApplicationInsights_PreemptSdk="1" \
    ConnectionStrings__DefaultConnection="${SQL_CONNECTION_STRING}" \
    ASPNETCORE_ENVIRONMENT="Production"

az monitor app-insights component connect-webapp \
  --app "${APP_INSIGHTS_NAME}" \
  --web-app "${WEBAPP_NAME}" \
  --resource-group "${RESOURCE_GROUP_NAME}"

az webapp restart --name "${WEBAPP_NAME}" --resource-group "${RESOURCE_GROUP_NAME}"

echo "=================================================================="
echo "PROVISIONAMENTO CONCLUÍDO COM SUCESSO!"
echo "URL do Web App: https://${WEBAPP_NAME}.azurewebsites.net"
echo "SQL Server: ${SQL_SERVER_NAME}.database.windows.net"
echo "Application Insights: ${APP_INSIGHTS_NAME}"
echo "=================================================================="

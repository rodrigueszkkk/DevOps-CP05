# ============================================================================
# DISCIPLINA: DevOps Tools & Cloud Computing (Prof. João Menk)
# SCRIPT: 01_setup_infra.ps1 - Automação de Infraestrutura na Azure (PowerShell)
# ============================================================================

$ErrorActionPreference = "Stop"

$RM = "rm561760"
$LOCATION = if ($args[0]) { $args[0] } else { "centralus" }
$RESOURCE_GROUP_NAME = "rg-safeshelter-$RM"
$SQL_SERVER_NAME = "sql-safeshelter-$RM-$LOCATION"
$SQL_DATABASE_NAME = "db-safeshelter"
$SQL_ADMIN_USER = "admin_dimdim"
$SQL_ADMIN_PASSWORD = "Fiap@2tdsvms2026!"
$APP_INSIGHTS_NAME = "ai-safeshelter-$RM"
$APP_SERVICE_PLAN = "plan-safeshelter-$RM"
$WEBAPP_NAME = "app-safeshelter-$RM"
$DOTNET_RUNTIME = "DOTNETCORE:8.0"

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "Provisionando Infraestrutura Azure para $RM em $LOCATION" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan

# 1. Provedores
az provider register --namespace Microsoft.Web
az provider register --namespace Microsoft.Sql
az provider register --namespace Microsoft.Insights
az provider register --namespace Microsoft.OperationalInsights
az extension add --name application-insights --yes 2>$null

# 2. Resource Group
az group create --name $RESOURCE_GROUP_NAME --location $LOCATION

# 3. SQL Server PaaS
az sql server create `
  --name $SQL_SERVER_NAME `
  --resource-group $RESOURCE_GROUP_NAME `
  --location $LOCATION `
  --admin-user $SQL_ADMIN_USER `
  --admin-password $SQL_ADMIN_PASSWORD `
  --enable-public-network true

# 4. Banco SQL
az sql db create `
  --resource-group $RESOURCE_GROUP_NAME `
  --server $SQL_SERVER_NAME `
  --name $SQL_DATABASE_NAME `
  --service-objective Basic `
  --backup-storage-redundancy Local `
  --zone-redundant false

# 5. Firewall
Write-Host ">> Aguardando sincronização do Azure Resource Manager (15s)..." -ForegroundColor Cyan
Start-Sleep -Seconds 15
az sql server firewall-rule create `
  --resource-group $RESOURCE_GROUP_NAME `
  --server $SQL_SERVER_NAME `
  --name "AllowAzureServices" `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0

az sql server firewall-rule create `
  --resource-group $RESOURCE_GROUP_NAME `
  --server $SQL_SERVER_NAME `
  --name "liberaGeral" `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 255.255.255.255

# 6. Application Insights
az monitor app-insights component create `
  --app $APP_INSIGHTS_NAME `
  --location $LOCATION `
  --resource-group $RESOURCE_GROUP_NAME `
  --application-type web

$APP_INSIGHTS_CONNECTION_STRING = az monitor app-insights component show `
  --app $APP_INSIGHTS_NAME `
  --resource-group $RESOURCE_GROUP_NAME `
  --query connectionString `
  --output tsv

# 7. App Service Plan Linux F1
az appservice plan create `
  --name $APP_SERVICE_PLAN `
  --resource-group $RESOURCE_GROUP_NAME `
  --location $LOCATION `
  --sku F1 `
  --is-linux

# 8. Web App .NET 8
az webapp create `
  --name $WEBAPP_NAME `
  --resource-group $RESOURCE_GROUP_NAME `
  --plan $APP_SERVICE_PLAN `
  --runtime $DOTNET_RUNTIME

az resource update `
  --resource-group $RESOURCE_GROUP_NAME `
  --namespace Microsoft.Web `
  --resource-type basicPublishingCredentialsPolicies `
  --name scm `
  --parent "sites/$WEBAPP_NAME" `
  --set properties.allow=true

# 9. App Settings e Conexão
$SQL_CONNECTION_STRING = "Server=tcp:$SQL_SERVER_NAME.database.windows.net,1433;Initial Catalog=$SQL_DATABASE_NAME;User ID=$SQL_ADMIN_USER;Password=$SQL_ADMIN_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az webapp config appsettings set `
  --name $WEBAPP_NAME `
  --resource-group $RESOURCE_GROUP_NAME `
  --settings `
    APPLICATIONINSIGHTS_CONNECTION_STRING="$APP_INSIGHTS_CONNECTION_STRING" `
    ApplicationInsightsAgent_EXTENSION_VERSION="~3" `
    XDT_MicrosoftApplicationInsights_Mode="Recommended" `
    XDT_MicrosoftApplicationInsights_PreemptSdk="1" `
    ConnectionStrings__DefaultConnection="$SQL_CONNECTION_STRING" `
    ASPNETCORE_ENVIRONMENT="Production"

az monitor app-insights component connect-webapp `
  --app $APP_INSIGHTS_NAME `
  --web-app $WEBAPP_NAME `
  --resource-group $RESOURCE_GROUP_NAME

az webapp restart --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP_NAME

Write-Host "==================================================================" -ForegroundColor Green
Write-Host "SUCESSO! Web App online em: https://$WEBAPP_NAME.azurewebsites.net" -ForegroundColor Green
Write-Host "==================================================================" -ForegroundColor Green

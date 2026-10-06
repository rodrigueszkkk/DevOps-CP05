# ============================================================================
# SCRIPT: 02_deploy_app.ps1 - Deploy Automatizado via az webapp deploy (PowerShell)
# ============================================================================

$ErrorActionPreference = "Stop"

$RM = "rm561760"
$RESOURCE_GROUP_NAME = "rg-safeshelter-$RM"
$WEBAPP_NAME = "app-safeshelter-$RM"
$PROJECT_PATH = "./src/SafeShelter.API"
$PUBLISH_DIR = "./src/SafeShelter.API/bin/Release/net8.0/publish"
$ZIP_FILE = "./publish_app.zip"

Write-Host ">> 1. Gerando publicação Release do .NET 8..." -ForegroundColor Cyan
dotnet publish "$PROJECT_PATH/SafeShelter.API.csproj" -c Release -o $PUBLISH_DIR

Write-Host ">> 2. Compactando pacote ZIP..." -ForegroundColor Cyan
Compress-Archive -Path "$PUBLISH_DIR/*" -DestinationPath $ZIP_FILE -Force

Write-Host ">> 3. Executando az webapp deploy..." -ForegroundColor Cyan
az webapp deploy `
  --resource-group $RESOURCE_GROUP_NAME `
  --name $WEBAPP_NAME `
  --src-path $ZIP_FILE `
  --type zip

Write-Host ">> 4. Reiniciando Web App..." -ForegroundColor Cyan
az webapp restart --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP_NAME

Remove-Item -Path $ZIP_FILE -Force -ErrorAction SilentlyContinue

Write-Host "==================================================================" -ForegroundColor Green
Write-Host "DEPLOY FINALIZADO COM SUCESSO: https://$WEBAPP_NAME.azurewebsites.net" -ForegroundColor Green
Write-Host "==================================================================" -ForegroundColor Green

#!/bin/bash
# ============================================================================
# SCRIPT: 02_deploy_app.sh - Deploy Automatizado via az webapp deploy
# ============================================================================

set -e

RM="rm561760"
RESOURCE_GROUP_NAME="rg-safeshelter-${RM}"
WEBAPP_NAME="app-safeshelter-${RM}"
ROOT_DIR="$(pwd)"
PUBLISH_DIR="${ROOT_DIR}/publish_output"
ZIP_FILE="${ROOT_DIR}/publish_app.zip"

echo ">> 1. Executando build e publish do projeto .NET 8..."
dotnet publish "${ROOT_DIR}/src/SafeShelter.API/SafeShelter.API.csproj" -c Release -o "${PUBLISH_DIR}"

echo ">> 2. Compactando arquivos de publicação..."
cd "${PUBLISH_DIR}"
zip -r "${ZIP_FILE}" .
cd "${ROOT_DIR}"

echo ">> 3. Efetuando deploy automatizado via Azure CLI (az webapp deploy)..."
az webapp deploy \
  --resource-group "${RESOURCE_GROUP_NAME}" \
  --name "${WEBAPP_NAME}" \
  --src-path "${ZIP_FILE}" \
  --type zip

echo ">> 4. Reiniciando o Web App..."
az webapp restart --name "${WEBAPP_NAME}" --resource-group "${RESOURCE_GROUP_NAME}"

rm -rf "${PUBLISH_DIR}" "${ZIP_FILE}"

echo "=================================================================="
echo "DEPLOY REALIZADO COM SUCESSO!"
echo "Acesse: https://${WEBAPP_NAME}.azurewebsites.net"
echo "=================================================================="

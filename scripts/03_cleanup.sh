#!/bin/bash
# ============================================================================
# SCRIPT: 03_cleanup.sh - Exclusão de Recursos na Azure
# ============================================================================

RM="rm561760"
RESOURCE_GROUP_NAME="rg-safeshelter-${RM}"

echo ">> Excluindo Resource Group ${RESOURCE_GROUP_NAME}..."
az group delete --name "${RESOURCE_GROUP_NAME}" --yes --no-wait
echo "Exclusão iniciada com sucesso em segundo plano."

# ============================================================================
# SCRIPT: 03_cleanup.ps1 - Exclusão de Recursos na Azure (PowerShell)
# ============================================================================

$RM = "rm561760"
$RESOURCE_GROUP_NAME = "rg-safeshelter-$RM"

Write-Host ">> Excluindo Resource Group $RESOURCE_GROUP_NAME..." -ForegroundColor Yellow
az group delete --name $RESOURCE_GROUP_NAME --yes --no-wait
Write-Host "Exclusão solicitada com sucesso." -ForegroundColor Green

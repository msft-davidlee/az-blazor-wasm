param(
    [string]$funcName,
    [string]$authority,
    [string]$clientId,
    [string]$scope,
    [string]$accountName,
    [string]$rgName)

# Publish API
dotnet publish MyTodo.API\MyTodo.API.csproj -c Release -o outapi
Compress-Archive outapi\* -DestinationPath api.zip -Force
az functionapp deployment source config-zip -g $rgName -n $funcName --src api.zip

$appSettings = @{ "AzureAdB2C" = @{ "Authority" = "$authority"; "ClientId" = "$clientId"; "ValidateAuthority" = $false; }; "Scope" = "$scope"; "ServiceEndpoint" = "https://$funcName.azurewebsites.net" }      
Set-Content -Path MyTodo.Client\wwwroot\appsettings.json -Value ($appSettings | ConvertTo-Json -Depth 10)
dotnet publish MyTodo.Client\MyTodo.Client.csproj -c Release -o outcli

$end = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")
$start = (Get-Date).ToString("yyyy-MM-dd")
$sas = (az storage container generate-sas -n `$web --account-name $accountName --permissions racwl --expiry $end --start $start --https-only | ConvertFrom-Json)
azcopy_v10 sync outcli\wwwroot "https://$accountName.blob.core.windows.net/`$web?$sas" --delete-destination=true
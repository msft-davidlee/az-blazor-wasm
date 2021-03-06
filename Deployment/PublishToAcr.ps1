param(
    [string]$BUILD_ENV,
    [string]$VERSION)

$ErrorActionPreference = "Stop"

$platformRes = (az resource list --tag stack-name=platform --tag stack-environment=prod | ConvertFrom-Json)
if (!$platformRes) {
    throw "Unable to find eligible platform resources!"
}
if ($platformRes.Length -eq 0) {
    throw "Unable to find 'ANY' eligible platform resources!"
}

$acr = ($platformRes | Where-Object { $_.type -eq "Microsoft.ContainerRegistry/registries" })
if (!$acr) {
    throw "Unable to find eligible platform container registry!"
}
$acrName = $acr.Name

# Login to ACR
az acr login --name $AcrName
if ($LastExitCode -ne 0) {
    throw "An error has occured. Unable to login to acr."
}

$list = az acr repository list --name $AcrName | ConvertFrom-Json
if ($LastExitCode -ne 0) {
    throw "An error has occured. Unable to list from repository"
}

# https://docs.microsoft.com/en-us/azure/container-registry/container-registry-image-tag-version#unique-tags
$appVersion = "my-todo-api:1.0"
if ($BUILD_ENV -eq 'dev') {
    $imageName = "$appVersion.$VERSION-$BUILD_ENV"
}
else {
    $imageName = $appVersion
}


if (!$list -or !$list.Contains($imageName)) {

    $path = "/MyTodo.Api"
    az acr build --image $imageName -r $AcrName --file ./$path/Dockerfile .

    if ($LastExitCode -ne 0) {
        throw "An error has occured. Unable to build image."
    }
}
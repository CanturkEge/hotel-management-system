$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    dotnet restore HotelManagement.sln
    if ($LASTEXITCODE -ne 0) { throw 'Paket yukleme basarisiz.' }
    dotnet build HotelManagement.sln --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Derleme basarisiz.' }
    dotnet run --project tests/HotelManagement.Tests --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Is kurali testleri basarisiz.' }
    Write-Host 'Derleme ve is kurali testleri basarili. Gercek DB/Identity testleri icin TESTLER.md dosyasini izleyin.'
} finally { Pop-Location }

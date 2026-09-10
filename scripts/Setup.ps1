param([switch]$SkipSecrets)
$ErrorActionPreference = 'Stop'
$previousOutputEncoding = $OutputEncoding
$OutputEncoding = [System.Text.UTF8Encoding]::new($false)
$projectRoot = Split-Path $PSScriptRoot -Parent
Push-Location $projectRoot
function Invoke-Dotnet {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet komutu basarisiz. Sonraki adim calistirilmadi." }
}
function Read-PrivateText {
    param([string]$Prompt)
    $secureValue = Read-Host $Prompt -AsSecureString
    return [System.Net.NetworkCredential]::new('', $secureValue).Password
}
try {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw '.NET 10 SDK kurulu degil.' }
    Invoke-Dotnet -Arguments @('--version')
    Write-Host '1/5 Paketler ve yerel EF araci hazirlaniyor...'
    Invoke-Dotnet -Arguments @('restore','HotelManagement.sln')
    Invoke-Dotnet -Arguments @('tool','restore')
    Write-Host '2/5 Derleme ve is kurali testleri...'
    Invoke-Dotnet -Arguments @('build','HotelManagement.sln','--no-restore')
    Invoke-Dotnet -Arguments @('run','--project','tests/HotelManagement.Tests','--no-build')
    if (-not $SkipSecrets) {
        Write-Host '3/5 Yalnizca yeni/ayri Supabase projenizin bilgilerini girin. Sifreler ekrana yazilmaz.'
        $dbHost = Read-Host 'Connect > Session pooler > Host'
        $dbUser = Read-Host 'Username (ornegin postgres.PROJE_REFERANSI)'
        $dbPassword = Read-PrivateText 'Supabase veritabani sifresi'
        $superEmail = Read-Host 'Super admin e-postasi'
        $superPassword = Read-PrivateText 'Super admin sifresi (12+, buyuk/kucuk harf, rakam, sembol)'
        $adminEmail = Read-Host 'Oda yoneticisi e-postasi (farkli olmali)'
        $adminPassword = Read-PrivateText 'Oda yoneticisi sifresi (12+, buyuk/kucuk harf, rakam, sembol)'
        if ([string]::IsNullOrWhiteSpace($dbHost) -or [string]::IsNullOrWhiteSpace($dbUser) -or [string]::IsNullOrWhiteSpace($dbPassword)) { throw 'Veritabani alanlari bos olamaz.' }
        if ($adminEmail.Trim() -eq $superEmail.Trim()) { throw 'Iki yonetici icin farkli e-posta girin.' }
        $connection = [System.Data.Common.DbConnectionStringBuilder]::new()
        $connection['Host'] = $dbHost.Trim()
        $connection['Port'] = 5432
        $connection['Database'] = 'postgres'
        $connection['Username'] = $dbUser.Trim()
        $connection['Password'] = $dbPassword
        $connection['SSL Mode'] = 'VerifyFull'
        $connection['Maximum Pool Size'] = 10
        $connection['Timeout'] = 15
        $secrets = @{
            'ConnectionStrings:HotelDatabase' = $connection.ConnectionString
            'Seed:SuperAdmin:Email' = $superEmail.Trim()
            'Seed:SuperAdmin:Password' = $superPassword
            'Seed:Admin:Email' = $adminEmail.Trim()
            'Seed:Admin:Password' = $adminPassword
        }
        $secrets | ConvertTo-Json | & dotnet user-secrets set --project src/HotelManagement.Web
        if ($LASTEXITCODE -ne 0) { throw 'User Secrets kaydedilemedi.' }
        $dbPassword=$null; $superPassword=$null; $adminPassword=$null; $secrets=$null; $connection=$null
    }
    Write-Host '4/5 Migration hazirlaniyor...'
    $snapshots = @(Get-ChildItem 'src/HotelManagement.Infrastructure/Migrations' -Filter '*ModelSnapshot.cs' -ErrorAction SilentlyContinue)
    if ($snapshots.Count -eq 0) {
        Invoke-Dotnet -Arguments @('ef','migrations','add','InitialCreate','--project','src/HotelManagement.Infrastructure','--startup-project','src/HotelManagement.Web','--output-dir','Migrations')
    } else {
        Write-Host 'Mevcut migration korundu; yeniden olusturulmuyor.'
    }
    Write-Host '5/5 Migration uygulanacak; hotel semasi ve ilk hesaplar olusturulacak.'
    Write-Host 'Eski SQLite dosyalariniz veya public/auth/storage semalariniz silinmez.'
    $answer = Read-Host 'Baglanti hedefini kontrol ettiniz mi? Devam icin EVET yazin'
    if ($answer -cne 'EVET') { throw 'Veritabani kurulumu kullanici tarafindan durduruldu.' }
    Invoke-Dotnet -Arguments @('run','--project','src/HotelManagement.Web','--launch-profile','http','--','--setup')
    Write-Host 'Kurulum tamamlandi. Siteyi baslatmak icin:'
    Write-Host 'dotnet run --project src/HotelManagement.Web --launch-profile http'
} finally { $OutputEncoding = $previousOutputEncoding; Pop-Location }

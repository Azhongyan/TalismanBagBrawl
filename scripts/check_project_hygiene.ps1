[CmdletBinding()]
param(
    [string]$ProjectRoot = "",
    [string[]]$ScopePath = @(),
    [switch]$Strict
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = [System.IO.Path]::GetFullPath(
        (Join-Path $PSScriptRoot ".."))
}

$warnings = [System.Collections.Generic.List[string]]::new()
$info = [System.Collections.Generic.List[string]]::new()

function Resolve-ProjectPath {
    param([string]$PathValue)

    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return [System.IO.Path]::GetFullPath($PathValue)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $ProjectRoot $PathValue))
}

function Get-RelativeProjectPath {
    param([string]$FullPath)

    $rootPrefix = [System.IO.Path]::GetFullPath($ProjectRoot).TrimEnd('\', '/') +
        [System.IO.Path]::DirectorySeparatorChar
    $normalized = [System.IO.Path]::GetFullPath($FullPath)
    if ($normalized.StartsWith(
        $rootPrefix,
        [System.StringComparison]::OrdinalIgnoreCase)) {
        return $normalized.Substring($rootPrefix.Length)
    }

    return $normalized
}

function Get-TextFiles {
    param(
        [string[]]$Roots,
        [string[]]$Extensions
    )

    $result = [System.Collections.Generic.List[System.IO.FileInfo]]::new()
    foreach ($root in $Roots) {
        if (-not (Test-Path -LiteralPath $root)) {
            continue
        }

        $item = Get-Item -LiteralPath $root
        if (-not $item.PSIsContainer) {
            if ($Extensions -contains $item.Extension) {
                $result.Add($item)
            }
            continue
        }

        Get-ChildItem -LiteralPath $item.FullName -Recurse -File | Where-Object {
            $Extensions -contains $_.Extension
        } | ForEach-Object {
            $result.Add($_)
        }
    }

    return $result | Sort-Object FullName -Unique
}

$requiredPaths = @(
    "Docs/CURRENT/CLEANROOM_GOVERNANCE_BASELINE_V1.md",
    "Docs/CURRENT/PROJECT_CLEANROOM_COMPONENT_LEDGER.md",
    "Docs/TEMPLATES/TASK_CONTRACT_V2.md"
)

foreach ($relativePath in $requiredPaths) {
    $fullPath = Resolve-ProjectPath $relativePath
    if (-not (Test-Path -LiteralPath $fullPath)) {
        $warnings.Add("Required governance carrier missing: $relativePath")
    }
}

$formalRoots = @(
    "Assets/_Game/Scripts/TalismanBag/UnifiedBattle",
    "Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal",
    "Assets/_Game/Scripts/TalismanBag/Items/Canonical",
    "Assets/_Game/Scripts/TalismanBag/V04/RewardDrop"
) | ForEach-Object { Resolve-ProjectPath $_ }

$formalFiles = Get-TextFiles -Roots $formalRoots -Extensions @(".cs")
$sandboxDependencyPattern = [regex]'(?m)^\s*using\s+TalismanBag\.(BuildSandbox|ItemSandbox|BattleSandbox)\s*;|TalismanBag\.(BuildSandbox|ItemSandbox|BattleSandbox)\.'
$devResourcePattern = [regex]'Resources\.Load[^\r\n]*(DevOnly|Sandbox)'

foreach ($file in $formalFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    if ($sandboxDependencyPattern.IsMatch($content)) {
        $relative = Get-RelativeProjectPath $file.FullName
        $warnings.Add("Formal runtime references Sandbox namespace: $relative")
    }
    if ($devResourcePattern.IsMatch($content)) {
        $relative = Get-RelativeProjectPath $file.FullName
        $warnings.Add("Formal runtime loads DevOnly/Sandbox resource: $relative")
    }
}

$projectScriptRoot = Resolve-ProjectPath "Assets/_Game/Scripts/TalismanBag"
$projectScriptFiles = Get-TextFiles -Roots @($projectScriptRoot) -Extensions @(".cs")
$forbiddenDigestPattern = [regex]'System\.Security\.Cryptography|\bSHA256\b'
$forbiddenDigestFiles = [System.Collections.Generic.List[string]]::new()
foreach ($file in $projectScriptFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    if ($forbiddenDigestPattern.IsMatch($content)) {
        $relative = Get-RelativeProjectPath $file.FullName
        $forbiddenDigestFiles.Add($relative)
    }
}
if ($forbiddenDigestFiles.Count -gt 0) {
    $digestGroups = $forbiddenDigestFiles | ForEach-Object {
        $suffix = $_ -replace '^Assets[\\/]_Game[\\/]Scripts[\\/]TalismanBag[\\/]', ''
        ($suffix -split '[\\/]')[0]
    } | Group-Object | Sort-Object Name | ForEach-Object {
        "$($_.Name)=$($_.Count)"
    }
    $warnings.Add(
        "Project code contains forbidden digest gates: files=$($forbiddenDigestFiles.Count) groups=$($digestGroups -join ',')")
}

$serializedRoots = @(
    Resolve-ProjectPath "Assets/_Game/Scenes",
    Resolve-ProjectPath "Assets/_Game/Prefabs"
)
$serializedFiles = Get-TextFiles -Roots $serializedRoots -Extensions @(".unity", ".prefab")
foreach ($file in $serializedFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw
    if ($content -match 'm_Script:\s*\{fileID:\s*0') {
        $relative = Get-RelativeProjectPath $file.FullName
        $warnings.Add("Serialized Missing Script marker: $relative")
    }
}

$ledgerPath = Resolve-ProjectPath "Docs/CURRENT/PROJECT_CLEANROOM_COMPONENT_LEDGER.md"
if (Test-Path -LiteralPath $ledgerPath) {
    $ledger = Get-Content -LiteralPath $ledgerPath -Raw
    $replaceCount = ([regex]::Matches($ledger, '\bREPLACE\b')).Count
    $quarantineCount = ([regex]::Matches($ledger, '\bQUARANTINE\b')).Count
    $deleteCandidateCount = ([regex]::Matches($ledger, 'DELETE-CANDIDATE')).Count
    $info.Add("Ledger debt mentions: REPLACE=$replaceCount QUARANTINE=$quarantineCount DELETE-CANDIDATE=$deleteCandidateCount")
}

if ($ScopePath.Count -gt 0) {
    $scopeRoots = $ScopePath | ForEach-Object { Resolve-ProjectPath $_ }
    $scopeFiles = Get-TextFiles -Roots $scopeRoots -Extensions @(".cs", ".md", ".asset", ".prefab", ".unity")
    $ownerTypePattern = [regex]'\b(class|struct|interface)\s+\w*(Catalog|Database|Authority|Session|Manager|Provider|Repository|Bridge|Adapter|RuntimeState|Save)\b'
    $temporaryPattern = [regex]'\b(Temporary|Fallback|Compatibility|Sidecar)\b'

    foreach ($file in $scopeFiles) {
        $content = Get-Content -LiteralPath $file.FullName -Raw
        $relative = Get-RelativeProjectPath $file.FullName
        if ($ownerTypePattern.IsMatch($content)) {
            $info.Add("Owner review required for owner-level declaration in scope: $relative")
        }
        if ($temporaryPattern.IsMatch($content) -and $content -notmatch '\bDeleteWhen\b') {
            $warnings.Add("Temporary structure term without DeleteWhen in scope: $relative")
        }
    }
}

$roomStatus = if ($warnings.Count -eq 0) { "CLEAN" } else { "WARNING" }

Write-Output "PROJECT_HYGIENE_GATE"
Write-Output "ProjectRoot: $ProjectRoot"
Write-Output "AutoFix: DISABLED"
foreach ($line in $info) {
    Write-Output "INFO: $line"
}
foreach ($line in $warnings) {
    Write-Output "HYGIENE_WARNING: $line"
}
Write-Output "RoomStatus: $roomStatus"

if ($Strict -and $warnings.Count -gt 0) {
    exit 2
}

exit 0

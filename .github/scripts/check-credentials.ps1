# PowerShell 7 / GitHub Actions preflight. Exit code 1 from git grep means
# "no matches" and must not leak into the job's own final exit status.
$ErrorActionPreference = 'Stop'

$forbidden = @(git ls-files | Where-Object {
    $_ -match '(?i)(^|/)(\.env(\.[^/]+)?|id_rsa|id_ed25519|secrets\.json|credentials\.json|[^/]+\.(pfx|p12|pem|key|snk))$'
})
if ($LASTEXITCODE -ne 0) {
    throw "git ls-files failed with exit code $LASTEXITCODE"
}
if ($forbidden.Count -gt 0) {
    throw "Sensitive filenames tracked in Git: $($forbidden -join ', ')"
}

# Disable PowerShell's native-command error conversion just around git grep.
# Never print the candidate secret itself in a job log.
$previousPreference = $PSNativeCommandUseErrorActionPreference
$PSNativeCommandUseErrorActionPreference = $false
try {
    $matches = @(git grep --cached -I -l -E -e '-----BEGIN [A-Z ]*PRIVATE KEY-----' -e 'ghp_[A-Za-z0-9]{36}' -e 'github_pat_[A-Za-z0-9_]{70,}' -- .)
    $grepCode = $LASTEXITCODE
}
finally {
    $PSNativeCommandUseErrorActionPreference = $previousPreference
}

if ($grepCode -notin @(0, 1)) {
    throw "Credential scan failed with exit code $grepCode"
}
if ($grepCode -eq 0 -and $matches.Count -gt 0) {
    throw "Potential credentials detected in tracked files: $($matches -join ', ')"
}

# GitHub's pwsh wrapper appends an "exit $LASTEXITCODE" at the end of a run
# step. Without this reset, a clean git grep (exit 1) fails the workflow.
$global:LASTEXITCODE = 0
Write-Host 'Credential preflight passed; clean git grep status normalized to zero.'

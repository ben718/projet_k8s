# PowerShell script to add entry to Windows hosts file for dotnetgigs.local

$hostsPath = "$env:SystemRoot\System32\drivers\etc\hosts"
$entry = "127.0.0.1 dotnetgigs.local"

# Check if entry already exists
$hostsContent = Get-Content $hostsPath -ErrorAction Stop
if ($hostsContent -contains $entry) {
    Write-Output "Entry already exists in hosts file."
} else {
    # Add entry with elevated privileges
    try {
        Add-Content -Path $hostsPath -Value $entry -ErrorAction Stop
        Write-Output "Entry added to hosts file successfully."
    } catch {
        Write-Error "Failed to add entry to hosts file. Please run this script as Administrator."
    }
}

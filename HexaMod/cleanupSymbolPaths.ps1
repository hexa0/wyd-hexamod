param(
    [Parameter(Mandatory=$true)]
    [string]$MdbPath
)

$bytes = [System.IO.File]::ReadAllBytes($MdbPath)
$modified = $false
$dotSlashBytes = [byte[]](0x2E, 0x2F) # ASCII for './'

for ($i = 0; $i -lt $bytes.Length - 1; $i++) {
    if ($bytes[$i] -eq $dotSlashBytes[0] -and $bytes[$i+1] -eq $dotSlashBytes[1]) {
        $nullTerminatorIndex = -1
        for ($j = $i + 2; $j -lt $bytes.Length; $j++) {
            if ($bytes[$j] -eq 0x00) {
                $nullTerminatorIndex = $j
                break
            }
        }
        
        if ($nullTerminatorIndex -ne -1) {
            for ($k = $i + 2; $k -le $nullTerminatorIndex; $k++) {
                $bytes[$k - 2] = $bytes[$k]
            }
            
            $bytes[$nullTerminatorIndex - 1] = 0x00
            $bytes[$nullTerminatorIndex] = 0x00 
            
            $modified = $true
            $i = $nullTerminatorIndex
        } else {
             # No null terminator found, stop searching prevent an overflow
             break
        }
    }
}

if ($modified) {
    [System.IO.File]::WriteAllBytes($MdbPath, $bytes)
    Write-Host "Successfully removed './' prefixes from $MdbPath"
} else {
    Write-Host "No './' prefixes found in $MdbPath"
}
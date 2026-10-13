# 현장 서버 공유 폴더(\\172.30.1.32\TASK, \\172.30.1.32\SIM)에 업로드 꾸러미를 올린다.
#   기존 파일은 "이름_YYYYMMDD_NNN.확장자" 로 이름을 바꿔 남기고(구동 중이어도 이름 바꾸기는 된다),
#   새 파일을 원래 이름으로 넣는다. Track 폴더는 폴더째 Track_YYYYMMDD_NNN 으로 남긴다.
#
#   .\deploy_site.ps1                 : 실제로 올린다
#   .\deploy_site.ps1 -WhatIf         : 무엇을 어디에 올릴지 보여만 준다
#   공유 접속(net use)은 먼저 해 두어야 한다.
param([switch]$WhatIf, [string]$Up = 'D:\인수인계\한국단자\ReNewal\업로드_20261013',
      [string]$TaskShare = '\\172.30.1.32\TASK', [string]$SimShare = '\\172.30.1.32\SIM')
$ErrorActionPreference = 'Stop'
$stamp = Get-Date -Format 'yyyyMMdd'

function Find-Existing([string]$share, [string]$name) {
  # 공유 안에서 같은 이름의 파일을 찾는다 (깊이 3까지). 있으면 그 자리에 올린다.
  $hit = Get-ChildItem $share -Recurse -Depth 3 -File -Filter $name -ErrorAction SilentlyContinue | Select-Object -First 1
  if ($hit) { return $hit.FullName }
  return (Join-Path $share $name)
}
function Next-BackupName([string]$path) {
  $dir = Split-Path $path; $base = [IO.Path]::GetFileNameWithoutExtension($path); $ext = [IO.Path]::GetExtension($path)
  for ($i = 1; $i -lt 1000; $i++) {
    $cand = Join-Path $dir ("{0}_{1}_{2:000}{3}" -f $base, $stamp, $i, $ext)
    if (-not (Test-Path $cand)) { return $cand }
  }
  throw "백업 이름을 못 정했다 : $path"
}
function Put-File([string]$src, [string]$share) {
  $name = Split-Path $src -Leaf
  $dst = Find-Existing $share $name
  if (Test-Path $dst) {
    $bak = Next-BackupName $dst
    "  백업  {0}  ->  {1}" -f $dst, (Split-Path $bak -Leaf)
    if (-not $WhatIf) { Rename-Item -Path $dst -NewName (Split-Path $bak -Leaf) }
  } else { "  (새 파일) $dst" }
  "  복사  {0}  ->  {1}" -f $name, $dst
  if (-not $WhatIf) { Copy-Item $src $dst -Force }
}
function Put-Dir([string]$srcDir, [string]$share, [string]$name) {
  $hit = Get-ChildItem $share -Recurse -Depth 2 -Directory -Filter $name -ErrorAction SilentlyContinue | Select-Object -First 1
  $dst = if ($hit) { $hit.FullName } else { Join-Path $share $name }
  if (Test-Path $dst) {
    $bak = Next-BackupName $dst
    "  백업  {0}  ->  {1}" -f $dst, (Split-Path $bak -Leaf)
    if (-not $WhatIf) { Copy-Item $dst $bak -Recurse -Force }
  }
  "  복사  {0}\*  ->  {1}" -f $srcDir, $dst
  if (-not $WhatIf) { New-Item -ItemType Directory -Force -Path $dst | Out-Null; Copy-Item "$srcDir\*" $dst -Recurse -Force }
}

"=== 1_WCS_TASK -> $TaskShare"
Get-ChildItem "$Up\1_WCS_TASK" -File | ForEach-Object { Put-File $_.FullName $TaskShare }

"=== 2_CLIENT_SIM -> $SimShare"
Get-ChildItem "$Up\2_CLIENT_SIM" -File | ForEach-Object { Put-File $_.FullName $SimShare }
Put-Dir "$Up\2_CLIENT_SIM\Track" $SimShare 'TRACK'

"=== 3_DB (SQL 은 복사만, 실행은 따로)"
$dbDst = Join-Path $TaskShare 'DB_20261013'
if (-not $WhatIf) { New-Item -ItemType Directory -Force -Path $dbDst | Out-Null; Copy-Item "$Up\3_DB\*.sql" $dbDst -Force }
"  복사  3_DB\*.sql  ->  $dbDst"
"끝."

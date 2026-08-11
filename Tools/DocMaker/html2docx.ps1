# HTML -> .docx via Word COM (no pandoc available).
#   html2docx.ps1 <in.html> <out.docx>
# NOTE: keep this file ASCII-only. PowerShell 5.1 reads BOM-less .ps1 as ANSI.
param([string]$Src, [string]$Dst)

$ErrorActionPreference = 'Stop'

$wdFormatXMLDocument = 16
$wdOrientPortrait    = 0
$wdAlignParagraphCenter = 1
$wdFieldPage         = 33
$wdAutoFitWindow     = 2
$wdStatisticPages    = 2

$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0

try {
    $doc = $word.Documents.Open($Src, $false, $false)

    # Narrow margins - the tables are wide (1 cm = 28.35 pt).
    $ps = $doc.PageSetup
    $ps.Orientation  = $wdOrientPortrait
    $ps.TopMargin    = 45
    $ps.BottomMargin = 45
    $ps.LeftMargin   = 45
    $ps.RightMargin  = 45

    # Fit every table to the page width, repeat header rows, no row splitting.
    foreach ($t in $doc.Tables) {
        $t.AutoFitBehavior($wdAutoFitWindow)
        $t.Rows.AllowBreakAcrossPages = $false
        $t.Rows.Item(1).HeadingFormat = $true
    }

    # Centered page number in the footer.
    $footer = $doc.Sections.Item(1).Footers.Item(1)
    $footer.Range.Paragraphs.Alignment = $wdAlignParagraphCenter
    $footer.Range.Fields.Add($footer.Range, $wdFieldPage) | Out-Null

    if (Test-Path $Dst) { Remove-Item $Dst -Force }
    $doc.SaveAs2($Dst, $wdFormatXMLDocument)
    $pages  = $doc.ComputeStatistics($wdStatisticPages)
    $tables = $doc.Tables.Count
    $doc.Close($false)
    Write-Output ("  -> {0}   {1} pages, {2} tables" -f $Dst, $pages, $tables)
}
finally {
    $word.Quit()
    [void][Runtime.InteropServices.Marshal]::ReleaseComObject($word)
    [GC]::Collect()
}

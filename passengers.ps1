# Try to make it fit on my tiny screen.
Import-Csv .\tiny_rails.csv `
  |? { [int]$_.Quantity -ge 1 } `
  |? { [int]$_.Cargo -lt 10 } `
  | Select-Object *, `
    @{ Name = 'Name'; Expression = { $_.Name -replace "( Car)?$","" }}, `
    @{ Name = 'Qty'; Expression = { [int]$_.Quantity }}, `
    @{ Name = 'Inc'; Expression = { [bool]$_.Include }}, `
    @{ Name = 'Lvl'; Expression = { [int]$_.Level }}, `
    @{ Name = 'Spd'; Expression = { [int]$_.Speed }}, `
    @{ Name = 'Wgt'; Expression = { [int]$_.Weight }}, `
    @{ Name = 'Pas'; Expression = { [int]$_.Passengers }}, `
    @{ Name = 'Cgo'; Expression = { [int]$_.Cargo }}, `
    @{ Name = 'Foo'; Expression = { [int]$_.Food }}, `
    @{ Name = 'Com'; Expression = { [int]$_.Comfort }}, `
    @{ Name = 'Ent'; Expression = { [int]$_.Entertainment }}, `
    @{ Name = 'Fac'; Expression = { [int]$_.Facilities }}, `
    @{ Name = 'Score'; Expression = { [int]$_.Food + [int]$_.Comfort + [int]$_.Entertainment + [int]$_.Facilities }} `
    -ExcludeProperty Name `
  | Sort-Object -Property `
    @{ Expression={$_.Score}; Descending=$true}, `
    @{ Expression={$_.Name};  Descending=$false},`
    @{ Expression={$_.Level}; Descending=$false} `
  | Format-Table Name,Qty,Inc,Lvl,Spd,Wgt,Pas,Cgo,Foo,Com,Ent,Fac,Score,Notes

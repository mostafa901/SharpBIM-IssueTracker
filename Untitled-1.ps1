# Install QRCoder module if needed
if (-not (Get-Module -ListAvailable -Name QRCoder)) {
    Install-Package -Name QRCoder -Source nuget.org -Scope CurrentUser -Force
}

Add-Type -Path (Join-Path ((Get-Package -Name QRCoder).Source) 'lib/netstandard2.0/QRCoder.dll')

# Define payment info - CHANGE these to your values
$BIC = "REVOGB21"                     # Revolut BIC (dummy example, replace with actual if needed)
$Name = "John Doe"
$IBAN = "GB29NWBK60161331926819"     # Put your IBAN here
$Currency = "EUR"
$Amount = "123.45"
$Reference = "Invoice 123 payment"

# Build EPC QR code text (SEPA Credit Transfer - EPC069-12)
$epc = @"
BCD
001
1
SCT
$BIC
$Name
$IBAN
$Currency$Amount
$Reference
"@.Trim()

# Generate QR code image
Add-Type -AssemblyName System.Drawing

$qrcodeGenerator = New-Object QRCoder.QRCodeGenerator
$data = $qrcodeGenerator.CreateQrCode($epc, [QRCoder.QRCodeGenerator+ECCLevel]::Q)
$qrcode = New-Object QRCoder.QRCode $data
$bitmap = $qrcode.GetGraphic(20)

# Save image to file
$outputFile = "RevolutPaymentQR.png"
$bitmap.Save($outputFile, [System.Drawing.Imaging.ImageFormat]::Png)

Write-Output "QR code saved to $outputFile"
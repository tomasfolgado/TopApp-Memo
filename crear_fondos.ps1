Add-Type -AssemblyName System.Drawing
$ruta = 'C:\topapp-posit\PostitApp\Assets\Backgrounds'

function Create-Background($filename, $r, $g, $b) {
    $bmp = New-Object System.Drawing.Bitmap(300, 300)
    $graphics = [System.Drawing.Graphics]::FromImage($bmp)
    $color = [System.Drawing.Color]::FromArgb(255, $r, $g, $b)
    $graphics.Clear($color)
    $graphics.Dispose()
    $fullPath = Join-Path $ruta $filename
    $bmp.Save($fullPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
}

# Colores vivos y luminosos para posit
Create-Background "posit_yellow.png" 255 235 59   # Amarillo Canario
Create-Background "posit_blue.png"   100 181 246  # Azul Cielo
Create-Background "posit_green.png"  140 233 154  # Verde Menta
Create-Background "posit_red.png"    255 128 171  # Rosa Fucsia
Create-Background "posit_purple.png" 206 147 216  # Morado Lavanda
Create-Background "posit_orange.png" 255 167 38   # Naranja Mandarina
Create-Background "posit_cyan.png"   77  208 225  # Cyan Turquesa
Create-Background "posit_white.png"  255 255 255  # Blanco Puro

Write-Host "Fondos vivos actualizados correctamente."

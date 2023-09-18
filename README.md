# Pdfium.Rendering
This library is a wrapper around Pdfium, and uses [pdfium-binaries](https://github.com/bblanchon/pdfium-binaries).

Use this library in combination with the native pdfium binaries build. You can add the correct native pdfium nuget package to your project, or add the following snippet to your `.csproj` file:

```
  <PropertyGroup>
    <PdfiumVersion>119.0.6015</PdfiumVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="bblanchon.PDFium.Win32" Version="$(PdfiumVersion)"
                      Condition="'$(RuntimeIdentifier)'==''
                        Or '$(RuntimeIdentifier)'=='win-x64'
                        Or '$(RuntimeIdentifier)'=='win-x86'
                        Or '$(RuntimeIdentifier)'=='win-arm64'" />
    <PackageReference Include="bblanchon.PDFium.Linux" Version="$(PdfiumVersion)"
                      Condition="'$(RuntimeIdentifier)'==''
                        Or '$(RuntimeIdentifier)'=='linux-x64'
                        Or '$(RuntimeIdentifier)'=='linux-x86'
                        Or '$(RuntimeIdentifier)'=='linux-musl-x64'
                        Or '$(RuntimeIdentifier)'=='linux-musl-x86'
                        Or '$(RuntimeIdentifier)'=='linux-arm'
                        Or '$(RuntimeIdentifier)'=='linux-arm64'" />
    <PackageReference Include="bblanchon.PDFium.macOS" Version="$(PdfiumVersion)"
                      Condition="'$(RuntimeIdentifier)'==''
                        Or '$(RuntimeIdentifier)'=='osx-x64'
                        Or '$(RuntimeIdentifier)'=='osx-arm64'" />
    <PackageReference Include="bblanchon.PDFium.Android" Version="$(PdfiumVersion)"
                      Condition="'$(RuntimeIdentifier)'=='android-arm64'" />
    <PackageReference Include="bblanchon.PDFium.iOS" Version="$(PdfiumVersion)"
                      Condition="'$(RuntimeIdentifier)'=='ios-arm64'" />
    <PackageReference Include="bblanchon.PDFium.WebAssembly " Version="$(PdfiumVersion)"
                      Condition="'$(RuntimeIdentifier)'=='browser-wasm'" />
  </ItemGroup>
```

This snippet includes pdfium for windows, linux & osx by default, and adds Android, iOS or webassembly when that runtime identifier is specified.
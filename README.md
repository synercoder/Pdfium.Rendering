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

## Threading

**Pdfium is not thread-safe, and this library does not make it thread-safe. It makes it safe to *call* from multiple threads.**

Every call into pdfium is serialised behind a single process-wide lock. You can call this library from any number of threads without corrupting anything, but the pdfium work itself will not run in parallel — threads queue up and execute one at a time.

This is not a limitation of the wrapper that can be tuned away. Pdfium places the burden of serialisation on the embedder. From the note above `FPDF_InitLibrary` in pdfium's `public/fpdfview.h`:

> NOTE: None of the PDFium APIs are thread-safe. They expect to be called from a single thread. Barring that, embedders are required to ensure (via a mutex or similar) that only a single PDFium call can be made at a time.

Giving each thread its own `PdfDocument` does **not** avoid this. Rendering any page reaches through a global module into one process-wide font manager, which mutates shared caches and drives a single shared FreeType library handle. Two threads rendering completely unrelated documents still race on that state, and the result is heap corruption rather than a clean exception. The race is timing-dependent and mostly disappears once the font cache is warm, so code that removes the lock tends to pass its tests and then fail in production.

### Scaling up

To render in parallel, use **multiple processes** rather than multiple threads. Each process gets its own pdfium instance and its own lock, and they scale independently. This is the approach pdfium's maintainers recommend and what pdfium's own test tooling does.

Within a single process, two things are worth knowing:

- Threads are still useful for everything *around* pdfium. Only the native calls are serialised — image encoding, I/O and other work run concurrently as normal.
- If you pass a slow `Stream` (a network share, an HTTP response), be aware that pdfium reads document data lazily through a callback *during rendering*, and those reads happen while the lock is held — so one slow stream stalls every other thread. Buffering the PDF into a `MemoryStream` before constructing a `PdfDocument` keeps that I/O out of the critical section.

### Object lifetime

`PdfDocument` and `PdfPage` are safe to dispose from any thread, and disposing twice is harmless. However, a `PdfPage` does not keep its `PdfDocument` alive: disposing a document invalidates every page obtained from it, and using such a page afterwards is undefined. Dispose pages before the document they came from.
using System;

namespace Pdfium.Rendering.Internals;

internal class FPDFInitializer : IDisposable
{
    private static readonly object _syncRoot = new();
    private static FPDFInitializer? _library;

    public static void EnsureLoaded()
    {
        lock (_syncRoot)
        {
            if (_library == null)
                _library = new FPDFInitializer();
        }
    }

    private bool _disposed;

    private FPDFInitializer()
    {
        SecuredWrapper.InitLibrary();
    }

    ~FPDFInitializer()
    {
        _dispose();
    }

    public void Dispose()
    {
        _dispose();

        GC.SuppressFinalize(this);
    }

    private void _dispose()
    {
        if (!_disposed)
        {
            SecuredWrapper.DestroyLibrary();

            _disposed = true;
        }
    }
}

using System;
using System.Runtime.InteropServices;

namespace Pdfium.Rendering.Internals;

internal class Pinned : IDisposable
{
    private readonly GCHandle _handle;
    private bool _disposed;

    public Pinned(object? value)
    {
        Value = value;

        _handle = GCHandle.Alloc(value, GCHandleType.Pinned);
    }

    public static Pinned Pin(object? value)
        => new Pinned(value);

    public object? Value { get; }

    public IntPtr AddresOfPin
        => _handle.AddrOfPinnedObject();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _handle.Free();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

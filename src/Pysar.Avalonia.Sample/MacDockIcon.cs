using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Platform;

namespace Pysar.Avalonia.Sample;

/// <summary>
///     Sets the macOS Dock icon. <c>Window.Icon</c> only affects window chrome, and
///     <c>dotnet run</c> does not produce an .app bundle with an .icns.
/// </summary>
internal static class MacDockIcon
{
    private const string ObjC = "/usr/lib/libobjc.A.dylib";

    public static void Apply()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        ApplyCore();
    }

    [SupportedOSPlatform("macos")]
    private static void ApplyCore()
    {
        using var stream = AssetLoader.Open(new Uri("avares://Pysar.Avalonia.Sample/Assets/pysar.png"));
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        var png = memory.ToArray();

        var bytes = GCHandle.Alloc(png, GCHandleType.Pinned);
        try
        {
            var nsData = IntPtr_objc_msgSend_IntPtr_nint(
                objc_getClass("NSData"),
                sel_registerName("dataWithBytes:length:"),
                bytes.AddrOfPinnedObject(),
                png.Length);

            var nsImage = IntPtr_objc_msgSend_IntPtr(
                IntPtr_objc_msgSend(objc_getClass("NSImage"), sel_registerName("alloc")),
                sel_registerName("initWithData:"),
                nsData);

            IntPtr_objc_msgSend_IntPtr(
                IntPtr_objc_msgSend(objc_getClass("NSApplication"), sel_registerName("sharedApplication")),
                sel_registerName("setApplicationIconImage:"),
                nsImage);
        }
        finally
        {
            bytes.Free();
        }
    }

    [DllImport(ObjC)]
    private static extern IntPtr objc_getClass(string name);

    [DllImport(ObjC)]
    private static extern IntPtr sel_registerName(string name);

    [DllImport(ObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport(ObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr IntPtr_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport(ObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr IntPtr_objc_msgSend_IntPtr_nint(
        IntPtr receiver, IntPtr selector, IntPtr arg1, nint arg2);
}

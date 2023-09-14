using System.Runtime.InteropServices;

namespace NativeLibraryLoader;

public class NativeLibrary
{
    /// <summary>
    /// A list of all libraries loaded so far.
    /// </summary>
    public static List<NativeLibrary> LoadedLibraries = new List<NativeLibrary>();

    /// <summary>
    /// The filename of the library.
    /// </summary>
    public string LibraryName;
    /// <summary>
    /// The pointer to the library handle.
    /// </summary>
    public nint LibraryHandle;

    // Windows
    [DllImport("kernel32")]
    internal static extern nint LoadLibrary(string filename);

    [DllImport("kernel32")]
    internal static extern nint GetProcAddress(nint handle, string functionName);

    // Linux
    [DllImport("libdl.so.2", EntryPoint = "dlopen")]
    internal static extern nint l_dlOpen(string filename, int flags);

    [DllImport("libdl.so.2", EntryPoint = "dlsym")]
    internal static extern nint l_dlsym(nint handle, string functionName);

    // MacOS
    [DllImport("libdl.dylib", EntryPoint = "dlopen")]
    internal static extern nint m_dlopen(string filename, int flags);

    [DllImport("libdl.dylib", EntryPoint = "dlsym")]
    internal static extern nint m_dlsym(nint handle, string functionName);

    public static Platform? _platform;
    public static Platform Platform
    {
        get
        {
            if (_platform != null) return (Platform) _platform;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) _platform = Platform.Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) _platform = Platform.MacOS;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) _platform = Platform.Linux;
            else _platform = Platform.Other;
            return (Platform) _platform;
        }
    }

    /// <summary>
    /// Opens the specified library.
    /// </summary>
    /// <param name="Library">The filename to the library to load.</param>
    public static NativeLibrary Load(string Library)
    {
        NativeLibrary? lib = LoadedLibraries.Find(l => l.LibraryName == Library);
        if (lib is not null) return lib;
        return new NativeLibrary(Library);
    }

    /// <summary>
    /// Opens the specified library.
    /// </summary>
    /// <param name="library">The filename to the library to load.</param>
    /// <param name="preloadLibraries">A list of any other libraries that must be loaded before loading this library.</param>
    protected NativeLibrary(string library)
    {
        LibraryName = library;
        if (Platform == Platform.Windows) LibraryHandle = LoadLibrary(library);
        else if (Platform == Platform.Linux) LibraryHandle = l_dlOpen(library, 0x102);
        else if (Platform == Platform.MacOS) LibraryHandle = m_dlopen(library, 0x102);
        else throw new UnsupportedPlatformException();
        if (LibraryHandle == IntPtr.Zero) throw new LibraryLoadException(library);
        LoadedLibraries.Add(this);
    }

    /// <summary>
    /// Returns a delegate bound to the target method in this library.
    /// </summary>
    /// <typeparam name="TDelegate">The delegate to bind the target method with.</typeparam>
    /// <param name="FunctionName">The name of the target function.</param>
    /// <returns>A delegate bound to the target method.</returns>
    public TDelegate GetFunction<TDelegate>(string functionName)
    {
        IntPtr funcaddr = IntPtr.Zero;
        if (Platform == Platform.Windows) funcaddr = GetProcAddress(LibraryHandle, functionName);
        else if (Platform == Platform.Linux) funcaddr = l_dlsym(LibraryHandle, functionName);
        else if (Platform == Platform.MacOS) funcaddr = m_dlsym(LibraryHandle, functionName);
        else throw new UnsupportedPlatformException();
        if (funcaddr == IntPtr.Zero) throw new InvalidEntryPointException(LibraryName, functionName);
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(funcaddr);
    }

    /// <summary>
    /// Returns whether the given function exists in the library.
    /// </summary>
    /// <param name="functionName">The name of the target function.</param>
    /// <returns>Whether the method exists.</returns>
    public bool HasFunction(string functionName)
    {
        IntPtr funcaddr = IntPtr.Zero;
        if (Platform == Platform.Windows) funcaddr = GetProcAddress(LibraryHandle, functionName);
        else if (Platform == Platform.Linux) funcaddr = l_dlsym(LibraryHandle, functionName);
        else if (Platform == Platform.MacOS) throw new UnsupportedPlatformException();
        else throw new UnsupportedPlatformException();
        return funcaddr != IntPtr.Zero;
    }

    /// <summary>
    /// Represents an error with binding to a native function.
    /// </summary>
    public class InvalidEntryPointException : Exception
    {
        public InvalidEntryPointException(string library, string functionName) : base($"No entry point by the name of '{functionName}' could be found in '{library}'.") { }
    }

    /// <summary>
    /// Represents an error with the current platform not being supported.
    /// </summary>
    public class UnsupportedPlatformException : Exception
    {
        public UnsupportedPlatformException() : base("This platform is not supported.") { }
    }

    /// <summary>
    /// Represents an error with loading a library.
    /// </summary>
    public class LibraryLoadException : Exception
    {
        public LibraryLoadException(string library) : base($"Failed to load library '{library}'") { }
    }
}

public enum Platform
{
    Windows,
    Linux,
    MacOS,
    Other
}
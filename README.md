# NativeLibraryLoader

This is a small library that makes loading native libraries very easy across all platforms. It also provides the `PathInfo` and `PathPlatformInfo` classes which make it easy to configure different library paths depending on the platform being used.

A library can be loaded with `NativeLibrary.Load(string path, params string[] preload)`, where `path` is the path to the main library, and `preload` is a list of other paths to other libraries that need to be loaded before the first library to be able to be loaded. You could also do this in individual `NativeLibary.Load(...)` calls, but this just does that for you. It is by no means necessary or any different.

You can then retrieve the native functions by calling the `GetFunction<IDelegate>(string function_name)` method, with `function_name` being the function entry point in the library, and `IDelegate` being the delegate that represents the function in C#.

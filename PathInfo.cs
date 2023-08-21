namespace NativeLibraryLoader;

public class PathInfo
{
    private Dictionary<Platform, PathPlatformInfo> Paths = new Dictionary<Platform, PathPlatformInfo>();

    private PathInfo()
    {

    }

    private void AddPlatform(PathPlatformInfo Info)
    {
        if (Paths.ContainsKey(Info.Platform)) Paths.Remove(Info.Platform);
        Paths.Add(Info.Platform, Info);
    }

    public static PathInfo Create(params PathPlatformInfo[] Platforms)
    {
        PathInfo info = new PathInfo();
        for (int i = 0; i < Platforms.Length; i++)
        {
            info.AddPlatform(Platforms[i]);
        }
        return info;
    }

    public PathPlatformInfo GetPlatform(Platform Platform)
    {
        if (!Paths.ContainsKey(Platform)) throw new PlatformNotSupportedException();
        return Paths[Platform];
    }
}

public class PathPlatformInfo
{
    public Platform Platform;
    public Dictionary<string, string> Paths;

    public PathPlatformInfo(Platform Platform)
    {
        this.Platform = Platform;
        this.Paths = new Dictionary<string, string>();
    }

    public void AddPath(string Key, string Value)
    {
        if (Paths.ContainsKey(Key)) Paths.Remove(Key);
        this.Paths.Add(Key, Value);
    }

    public bool Has(string Key)
    {
        return Paths.ContainsKey(Key);
    }

    public string Get(string Key)
    {
        if (!Has(Key)) throw new MissingPathException(Key);
        return Paths[Key];
    }

    public class MissingPathException : Exception
    {
        public MissingPathException(string Path) : base($"No path entry for key '{Path}'") { }
    }

}
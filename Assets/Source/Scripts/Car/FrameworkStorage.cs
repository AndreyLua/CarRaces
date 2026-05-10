public class FrameworkStorage
{
    private static FrameworkStorage _instance;
    private GlobalData _globalData;
    public static bool Inited => _instance?._globalData != null;

    public static GlobalData GlobalData => _instance?._globalData;
    public FrameworkStorage()
    {
        _instance = this;
        _globalData = new GlobalData();
    }
}

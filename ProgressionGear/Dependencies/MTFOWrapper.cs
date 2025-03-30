using MTFO.API;

namespace ProgressionGear.Dependencies
{
    internal static class MTFOWrapper
    {
        public const string PLUGIN_GUID = "com.dak.MTFO";
        
        public static string GameDataPath => MTFOPathAPI.RundownPath;
        public static string CustomPath => MTFOPathAPI.CustomPath;
        public static bool HasCustomContent => MTFOPathAPI.HasRundownPath;
    }
}

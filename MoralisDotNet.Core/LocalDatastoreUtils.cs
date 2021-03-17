namespace MoralisDotNet.Core
{
    public static class LocalDatastoreUtils
    {
        public static string DEFAULT_PIN => "_default";
        public static string PIN_PREFIX => "parsePin_";
        public static string OBJECT_PREFIX => "Parse_LDS_";

        public static bool IsLocalDatastoreKey(string? key) =>
            (
                !string.IsNullOrWhiteSpace(key) &&
                (
                    key == DEFAULT_PIN ||
                    key.StartsWith(PIN_PREFIX) ||
                    key.StartsWith(OBJECT_PREFIX)));
    }
}

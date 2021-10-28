namespace HoneyComb
{
    public static class Constants
    {
        public const string Prefix = "honeycomb.io/";
        public static class LabelNames
        {
            public const string Name = Prefix + "name";
            // public const string DisplayName = Prefix + "display-name";
            public const string ApplicationId = Prefix + "app-id";
        }

        public static class AnnoNames
        {
            // public const string DisplayName = Prefix + "display-name";
        }

        public static class DingTalkParameterNames
        {
            public const string Url = "url";
            public const string AtMobiles = "at.atMobiles";
        }

        public static class CacheNames
        {
        }

        public static class HttpClientNames
        {
            public const string Default = Prefix + "default";
        }

    }
}

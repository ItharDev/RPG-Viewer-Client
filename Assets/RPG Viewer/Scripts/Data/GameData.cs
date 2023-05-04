namespace RPG
{
    public static class GameData
    {
        public static UserInfo User;
        public static ToolState ActiveTool;
        public static FogState ActiveFog;
        public static int FrameRate;
    }

    public struct UserInfo
    {
        public string id;
        public string name;

        public UserInfo(string _id, string _name)
        {
            id = _id;
            name = _name;
        }
    }
}
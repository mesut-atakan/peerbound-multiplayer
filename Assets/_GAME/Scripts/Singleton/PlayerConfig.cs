namespace Aventra.Game.Singleton
{
    public sealed class PlayerConfig
    {
        private static PlayerConfig _instance;

        public static PlayerConfig Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new PlayerConfig();
                }
                return _instance;
            }
        }
        public ulong SelectedCharacterId { get; set; }
        
    }
}
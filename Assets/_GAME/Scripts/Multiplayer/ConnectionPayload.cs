using System.Text;
using UnityEngine;

namespace Aventra.Game.Multiplayer
{
    // Oyuncunun Network'e aktarilacak bilgileri dusuk boyutta tutan sinif
    [System.Serializable]
    public struct ConnectionPayload
    {
        public string playerId;
        public string joinToken;
        public ulong selectedCharacterId;
        public string buildVersion;

        public byte[] ToBytes()
        {
            var json = JsonUtility.ToJson(this);
            return Encoding.UTF8.GetBytes(json);
        }

        public static bool TryFromBytes(byte[] data, out ConnectionPayload payload)
        {
            payload = default;
            
            try
            {
                var json = Encoding.UTF8.GetString(data);
                payload = JsonUtility.FromJson<ConnectionPayload>(json);
                return !string.IsNullOrWhiteSpace(payload.playerId);
            }
            catch
            {
                return false;
            }
        }
    }
}
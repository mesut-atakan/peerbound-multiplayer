using Unity.Netcode;
using UnityEngine;

namespace Aventra.Game
{
    public class Player : NetworkBehaviour
    {
        public CharacterConfig CharacterConfig { get; private set; }

        public void SetupCharacter(CharacterConfig characterConfig)
        {
            CharacterConfig = characterConfig;
            GameObject obj = Instantiate(CharacterConfig.Prefab, transform.position, Quaternion.identity, transform);
        }
    }
}
using System.Collections.Generic;
using Aventra.Game.Multiplayer;
using Unity.Netcode;
using UnityEngine;

namespace Aventra.Game
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private CharacterCatalog characterCatalog;
        [SerializeField] private Player playerPrefab;

        private readonly Dictionary<ulong, ulong> _clientCharacterMap = new Dictionary<ulong, ulong>();
        void OnEnable()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }

        void OnDisable()
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }

        // void Start()
        // {
        //     if (NetworkManager.Singleton.IsConnectedClient)
        //     {
        //         OnClientConnected(NetworkManager.Singleton.LocalClientId);
        //     }
        // }

        private void OnClientConnected(ulong clientId)
        {
            if (!NetworkManager.Singleton.IsServer)
                return;
            
            if (!_clientCharacterMap.TryGetValue(clientId, out var characterId))
            {
                
                characterId = characterCatalog.GetFirstCharacterConfig().Id; // default Id
                Debug.LogWarning($"No character ID found for client {clientId}, using default ID {characterId}");
            }


            Vector3 spawnPos = new Vector3(0, 1, 0);
            Player player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            player.SetupCharacter(characterCatalog.GetCharacterConfig(characterId));
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, 
            NetworkManager.ConnectionApprovalResponse response)
        {
            if (!ConnectionPayload.TryFromBytes(request.Payload, out var payload))
            {
                response.Approved = false;
                response.Reason = "Invalid connection payload.";
                return;
            }

            if (payload.buildVersion != Application.version)
            {
                response.Approved = false;
                response.Reason = $"Build version mismatch. Server: {Application.version}, Client: {payload.buildVersion}";
                return;
            }

            if (!ValidateJoinToken(payload.playerId, payload.joinToken))
            {
                response.Approved = false;
                response.Reason = "Invalid join token.";
                return;
            }

            if (!characterCatalog.TryGetCharacterConfig(payload.selectedCharacterId, out _))
            {
                response.Approved = false;
                response.Reason = $"Character ID {payload.selectedCharacterId} not found in catalog.";
                return;
            }

            _clientCharacterMap[request.ClientNetworkId] = payload.selectedCharacterId;
            response.Approved = true;
            response.CreatePlayerObject = false; // Kendimiz Spawn edecegiz!
        }

        private bool ValidateJoinToken(string playerId, string joinToken)
        {
            return !string.IsNullOrWhiteSpace(playerId) && !string.IsNullOrWhiteSpace(joinToken);
        }
    }
}
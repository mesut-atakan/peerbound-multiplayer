using System.Threading.Tasks;
using Aventra.Game.Singleton;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Aventra.Game.Multiplayer
{
    public sealed class P2PSessionService
    {
        [SerializeField] private UnityTransport transport;

        public async Task<string> StartHostAsync()
        {
            await EnsureServiceAsync();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
            
            var payload = new ConnectionPayload
            {
                playerId = AuthenticationService.Instance.PlayerId,
                joinToken = await CreateJoinTokenAsync(),
                selectedCharacterId = PlayerConfig.Instance.SelectedCharacterId,
                buildVersion = Application.version
            };

            // Sunucuyu kurmadan hemen once sunucuya ufak bir data gonderildi
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payload.ToBytes();

            NetworkManager.Singleton.StartHost();
            return joinCode;
        }

        public async Task StartClientAsync(string joinCode)
        {
            await EnsureServiceAsync();

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

            var payload = new ConnectionPayload
            {
                playerId = AuthenticationService.Instance.PlayerId,
                joinToken = await CreateJoinTokenAsync(),
                selectedCharacterId = PlayerConfig.Instance.SelectedCharacterId,
                buildVersion = Application.version
            };
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payload.ToBytes();

            NetworkManager.Singleton.StartClient();
        }

        private Task<string> CreateJoinTokenAsync()
        {
            return Task.FromResult(System.Guid.NewGuid().ToString("N"));
        }

        private async Task EnsureServiceAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();
            
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
}
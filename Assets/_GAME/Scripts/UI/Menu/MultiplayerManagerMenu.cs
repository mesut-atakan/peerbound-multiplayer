using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Aventra.Game
{
    public sealed class MultiplayerManagerMenu : MonoBehaviour
    {
        [SerializeField] private Button btnHost;
        [SerializeField] private Button btnServer;
        [SerializeField] private Button btnClient;

        void OnEnable()
        {
            btnHost.onClick.AddListener(OnHost);
            btnServer.onClick.AddListener(OnServer);
            btnClient.onClick.AddListener(OnClient);
        }

        void OnDisable()
        {
            btnHost.onClick.RemoveListener(OnHost);
            btnServer.onClick.RemoveListener(OnServer);
            btnClient.onClick.RemoveListener(OnClient);
        }

        private void OnHost()
        {
            NetworkManager.Singleton.StartHost();
        }

        private void OnServer()
        {
            NetworkManager.Singleton.StartServer();
        }

        private void OnClient()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
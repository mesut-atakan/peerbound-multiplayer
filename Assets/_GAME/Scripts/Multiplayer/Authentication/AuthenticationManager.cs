using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

namespace Aventra.Game
{
    public static class AuthenticationManager
    {
        private static readonly object InitLock = new();
        private static Task initializationTask;

        private static Task EnsureInitializedAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                return Task.CompletedTask;
            }

            lock (InitLock)
            {
                if (initializationTask == null || initializationTask.IsFaulted || initializationTask.IsCanceled)
                {
                    var options = new InitializationOptions().SetEnvironmentName("diamond");
                    initializationTask = UnityServices.InitializeAsync(options);
                }
                
                return initializationTask;
            }
        }

        public static async Task Login(string username, string password)
        {
            try
            {
                await EnsureInitializedAsync();
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                MultiplayerServiceEvents.RaiseLoginSuccess();
            }
            catch (AuthenticationException ex)
            {
                throw new System.Exception($"Failed to login with username: {username}\nReason: {ex.Message}");
            }
        }

        public static async Task Register(string username, string password)
        {
            try
            {
                await EnsureInitializedAsync();
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
                MultiplayerServiceEvents.RaiseLoginSuccess();
            }
            catch (AuthenticationException ex)
            {
                throw new System.Exception($"Failed to register with username: {username}\nReason: {ex.Message}");
            }
        }

        public static async Task Logout()
        {
            try
            {
                await EnsureInitializedAsync();
                AuthenticationService.Instance.SignOut();
                MultiplayerServiceEvents.RaiseLogoutSuccess();
            }
            catch (AuthenticationException ex)
            {
                throw new System.Exception($"Failed to logout\nReason: {ex.Message}");
            }
        }
    }
}
using System;

namespace Aventra.Game
{
    public static class MultiplayerServiceEvents
    {
        public static event Action OnLoginSuccess;
        public static event Action OnLogoutSuccess;
        public static event Action OnCompleteLoadStore;

        public static void RaiseLoginSuccess()
        {
            OnLoginSuccess?.Invoke();
        }

        public static void RaiseLogoutSuccess()
        {
            OnLogoutSuccess?.Invoke();
        }

        public static void RaiseCompleteLoadStore()
        {
            OnCompleteLoadStore?.Invoke();
        }
    }
}
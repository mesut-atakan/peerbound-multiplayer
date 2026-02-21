using System.Linq;

namespace Aventra.Game.Utility
{
    public static class PasswordValidation
    {
        private static readonly char[] SpecialCharacters = "!@#$%^&*()_+-=[]{}|;':\",.<>?/`~".ToCharArray();
        private const int MinimumLength = 8;
        public static bool Validate(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            if (password.Length < MinimumLength)
            {
                return false;
            }

            if (!HasUpperCase(password) || !HasLowerCase(password) || !HasDigit(password))
            {
                return false;
            }

            if (!HasSpecialCharacter(password))
            {
                return false;
            }

            return true;
        }

        public static bool HasUpperCase(string str) => str.Any(char.IsUpper);
        public static bool HasLowerCase(string str) => str.Any(char.IsLower);
        public static bool HasDigit(string str) => str.Any(char.IsDigit);
        public static bool HasSpecialCharacter(string str) => str.Any(c => SpecialCharacters.Contains(c));
    }
}
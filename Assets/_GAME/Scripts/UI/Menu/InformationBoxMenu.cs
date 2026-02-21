using Aventra.Nugget.Common.UI;
using TMPro;
using UnityEngine;

namespace Aventra.Game
{
    public sealed class InformationBoxMenu : BaseMenu
    {
        [SerializeField] private Transform box;
        [SerializeField] private TMP_Text message;

        public void ShowMessage(string text, Vector2 position)
        {
            box.position = position;
            message.text = text;
            OpenMenu();
        }
    }
}
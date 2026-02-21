using System;
using Aventra.Game.Utility;
using Aventra.Nugget.Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Aventra.Game
{
    public sealed class AuthenticationMenu : BaseMenu
    {
        [SerializeField] private InformationBoxMenu informationBoxMenu;
        [SerializeField] private TMP_InputField[] inputUserName;
        [SerializeField] private TMP_InputField[] inputPassword;
        [SerializeField] private Button btnLogin;
        [SerializeField] private Button btnRegister;
        [SerializeField] private Button[] btnExit;
        [SerializeField] private CanvasGroup[] menus;
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private Button[] btnSignButtons;

        protected override void Awake()
        {
            base.Awake();
            ConfigurePasswordFields();
        }

        private void ConfigurePasswordFields()
        {
            if (inputPassword == null)
            {
                return;
            }

            foreach (var passwordField in inputPassword)
            {
                if (passwordField == null)
                {
                    continue;
                }

                passwordField.contentType = TMP_InputField.ContentType.Password;
                passwordField.ForceLabelUpdate();
            }
        }

        void OnEnable()
        {
            btnLogin.onClick.AddListener(OnLoginClicked);
            btnRegister.onClick.AddListener(OnRegisterClicked);

            inputPassword[1].onValueChanged.AddListener(OnCreatePasswordValidate);

            btnSignButtons[0].onClick.AddListener(() => OpenSignMenu(0));
            btnSignButtons[1].onClick.AddListener(() => OpenSignMenu(1));

            foreach(var btn in btnExit)
            {
                btn.onClick.AddListener(OnExitClicked);
            }
        }

        void OnDisable()
        {
            btnLogin.onClick.RemoveListener(OnLoginClicked);
            btnRegister.onClick.RemoveListener(OnRegisterClicked);

            btnSignButtons[0].onClick.RemoveListener(() => OpenSignMenu(0));
            btnSignButtons[1].onClick.RemoveListener(() => OpenSignMenu(1));
            
            inputPassword[1].onValueChanged.RemoveListener(OnCreatePasswordValidate);
            
            foreach(var btn in btnExit)
            {
                btn.onClick.RemoveListener(OnExitClicked);
            }
        }

        private void OpenSignMenu(int v)
        {
            for (int i = 0; i < menus.Length; i++)
            {
                if (i == v)
                {
                    menus[i].alpha = 1;
                    menus[i].interactable = true;
                    menus[i].blocksRaycasts = true;
                }
                else
                {
                    menus[i].alpha = 0;
                    menus[i].interactable = false;
                    menus[i].blocksRaycasts = false;
                }
            }
        }

        private void OnCreatePasswordValidate(string arg0)
        {
            if (PasswordValidation.Validate(arg0))
            {
                informationBoxMenu.CloseMenu();
            }

            Vector3 messagePosition = GetLastPasswordCharacterPosition(inputPassword[1]);
            messagePosition.y -= 90; // Adjust the Y position to be below the input field
            messagePosition.x -= 50;
        
            if (!PasswordValidation.HasUpperCase(arg0))
            {
                informationBoxMenu.ShowMessage("Password must contain at least one uppercase letter.", messagePosition);
            }
            else if (!PasswordValidation.HasLowerCase(arg0))
            {
                informationBoxMenu.ShowMessage("Password must contain at least one lowercase letter.", messagePosition);
            }
            else if (!PasswordValidation.HasDigit(arg0))
            {
                informationBoxMenu.ShowMessage("Password must contain at least one digit.", messagePosition);
            }
            else if (!PasswordValidation.HasSpecialCharacter(arg0))
            {
                informationBoxMenu.ShowMessage("Password must contain at least one special character.", messagePosition);
            }
            else if (arg0.Length < 8)
            {
                informationBoxMenu.ShowMessage("Password must be at least 8 characters long.", messagePosition);
            }
        }

        private Vector3 GetLastPasswordCharacterPosition(TMP_InputField passwordField)
        {
            if (passwordField == null)
            {
                return transform.position;
            }

            if (string.IsNullOrEmpty(passwordField.text))
            {
                return passwordField.transform.position;
            }

            var textComponent = passwordField.textComponent;
            if (textComponent == null)
            {
                return passwordField.transform.position;
            }

            Canvas.ForceUpdateCanvases();
            textComponent.ForceMeshUpdate();

            int characterCount = textComponent.textInfo.characterCount;
            if (characterCount <= 0)
            {
                return passwordField.transform.position;
            }

            int lastIndex = Mathf.Clamp(passwordField.caretPosition - 1, 0, characterCount - 1);
            TMP_CharacterInfo characterInfo = textComponent.textInfo.characterInfo[lastIndex];
            Vector3 localPosition = (characterInfo.topRight + characterInfo.bottomRight) * 0.5f;

            return textComponent.rectTransform.TransformPoint(localPosition);
        }

        private async void OnRegisterClicked()
        {
            if (!RegisterValidation())
            {
                Debug.LogError("Invalid registration input");
                return;
            }

            try
            {
                await AuthenticationManager.Register(inputUserName[1].text, inputPassword[1].text);
                Debug.Log("Registration successful");
                CloseMenu();

                if (mainMenu != null)
                {
                    mainMenu.OpenMenu();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Registration failed: {ex.GetBaseException().Message}");
            }
        }

        private async void OnLoginClicked()
        {
            if (!LoginValidation())
            {
                Debug.LogError("Invalid login input");
                return;
            }

            try
            {
                await AuthenticationManager.Login(inputUserName[0].text, inputPassword[0].text);
                Debug.Log("Login successful");
                CloseMenu();

                if (mainMenu != null)
                {
                    mainMenu.OpenMenu();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Login failed: {ex.GetBaseException().Message}");
            }
        }

        private void OnExitClicked()
        {
            Application.Quit();
        }

        private bool LoginValidation()
        {
            if (string.IsNullOrEmpty(inputUserName[0].text))
            {
                return false;
            }

            if (string.IsNullOrEmpty(inputPassword[0].text))
            {
                return false;
            }

            return true;
        }

        private bool RegisterValidation()
        {
            if (string.IsNullOrEmpty(inputUserName[1].text))
            {
                return false;
            }

            if (string.IsNullOrEmpty(inputPassword[1].text) || string.IsNullOrEmpty(inputPassword[2].text))
            {
                return false;
            }

            if (inputPassword[1].text != inputPassword[2].text)
            {
                return false;
            }

            return ValidatePassword(inputPassword[1].text);
        }

        private bool ValidatePassword(string password)
        {
            return Utility.PasswordValidation.Validate(password);
        }
    }
}
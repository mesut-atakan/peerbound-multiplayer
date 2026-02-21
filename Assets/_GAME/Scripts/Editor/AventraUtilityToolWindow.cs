using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Aventra.Game.Editor
{
    public sealed class AventraUtilityToolWindow : EditorWindow
    {
        private const int MIN_KEYSTORE_LENGTH = 8;
        private const int MAX_KEYSTORE_LENGTH = 512;
        private const string PRINTABLE_ASCII = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        private const string ALPHA_NUMERIC = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly List<string> generatedItemIds = new();
        private int lastCopiedItemIndex = -1;

        private GUIStyle titleStyle;
        private GUIStyle cardStyle;
        private GUIStyle outputStyle;

        private int tabIndex;

        private ItemIdMode itemIdMode = ItemIdMode.PrefixedString;
        private string itemPrefix = "ITEM";
        private int itemCount = 5;
        private int customBodyLength = 10;
        private Vector2 itemScroll;

        private int keystoreLength = 32;
        private bool useSymbols = true;
        private string generatedKeystoreKey = string.Empty;
        private static readonly Color COPIED_BUTTON_TINT = new(0.72f, 0.90f, 0.72f, 1f);

        private enum ItemIdMode
        {
            ULong64 = 0,
            PrefixedString = 1
        }

        [MenuItem("Aventra/Tools/Utility Hub")]
        public static void Open()
        {
            var window = GetWindow<AventraUtilityToolWindow>("Aventra Utility Hub");
            window.minSize = new Vector2(640f, 460f);
            window.Show();
        }

        private void OnEnable()
        {
            CreateStyles();
        }

        private void OnGUI()
        {
            if (titleStyle == null)
            {
                CreateStyles();
            }

            DrawHeader();

            tabIndex = GUILayout.Toolbar(tabIndex, new[] { "Item ID Generator", "Android Keystore Key" }, GUILayout.Height(30));
            GUILayout.Space(8);

            switch (tabIndex)
            {
                case 0:
                    DrawItemIdTab();
                    break;
                case 1:
                    DrawKeystoreTab();
                    break;
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("Aventra Utility Hub", titleStyle);
            EditorGUILayout.LabelField("Item ID ve Android keystore key üretimini tek pencereden yönet.", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawItemIdTab()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("Item ID Generator", EditorStyles.boldLabel);

            itemIdMode = (ItemIdMode)EditorGUILayout.EnumPopup("ID Mode", itemIdMode);
            itemPrefix = EditorGUILayout.TextField("Prefix", itemPrefix);
            itemCount = EditorGUILayout.IntSlider("Generate Count", itemCount, 1, 100);

            if (itemIdMode == ItemIdMode.PrefixedString)
            {
                customBodyLength = EditorGUILayout.IntSlider("Body Length", customBodyLength, 4, 24);
            }

            GUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate IDs", GUILayout.Height(28)))
            {
                GenerateItemIds();
            }

            if (GUILayout.Button("Copy All", GUILayout.Height(28)))
            {
                CopyAllItemIds();
            }

            if (GUILayout.Button("Clear", GUILayout.Height(28)))
            {
                generatedItemIds.Clear();
                lastCopiedItemIndex = -1;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.Space(8);
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label($"Generated IDs ({generatedItemIds.Count})", EditorStyles.boldLabel);

            itemScroll = EditorGUILayout.BeginScrollView(itemScroll, GUILayout.MinHeight(220));
            if (generatedItemIds.Count == 0)
            {
                EditorGUILayout.HelpBox("Henüz ID üretilmedi. Generate IDs butonuna bas.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < generatedItemIds.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.SelectableLabel(generatedItemIds[i], outputStyle, GUILayout.Height(18));

                    Color previousColor = GUI.backgroundColor;
                    if (i == lastCopiedItemIndex)
                    {
                        GUI.backgroundColor = COPIED_BUTTON_TINT;
                    }

                    if (GUILayout.Button("Copy", GUILayout.Width(56)))
                    {
                        EditorGUIUtility.systemCopyBuffer = generatedItemIds[i];
                        lastCopiedItemIndex = i;
                    }

                    GUI.backgroundColor = previousColor;

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawKeystoreTab()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("Android Keystore Key Generator", EditorStyles.boldLabel);
            keystoreLength = EditorGUILayout.IntSlider("Key Length", keystoreLength, MIN_KEYSTORE_LENGTH, MAX_KEYSTORE_LENGTH);
            useSymbols = EditorGUILayout.ToggleLeft("Use symbols (printable ASCII)", useSymbols);

            EditorGUILayout.HelpBox("Android tarafında güçlü bir key/passphrase için en az 16, tercihen 24+ uzunluk önerilir.", MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Key", GUILayout.Height(30)))
            {
                generatedKeystoreKey = GenerateKeystoreKey(keystoreLength, useSymbols);
            }

            GUI.enabled = !string.IsNullOrEmpty(generatedKeystoreKey);
            if (GUILayout.Button("Copy Key", GUILayout.Height(30)))
            {
                EditorGUIUtility.systemCopyBuffer = generatedKeystoreKey;
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8);
            EditorGUILayout.LabelField("Generated Key", EditorStyles.miniBoldLabel);
            EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(generatedKeystoreKey) ? "Henüz key üretilmedi." : generatedKeystoreKey, outputStyle, GUILayout.MinHeight(58));
            EditorGUILayout.EndVertical();
        }

        private void GenerateItemIds()
        {
            generatedItemIds.Clear();
            lastCopiedItemIndex = -1;

            for (int i = 0; i < itemCount; i++)
            {
                switch (itemIdMode)
                {
                    case ItemIdMode.ULong64:
                        generatedItemIds.Add(GenerateULongId().ToString());
                        break;
                    default:
                        generatedItemIds.Add(GeneratePrefixedId(itemPrefix, customBodyLength));
                        break;
                }
            }
        }

        private void CopyAllItemIds()
        {
            if (generatedItemIds.Count == 0)
            {
                return;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < generatedItemIds.Count; i++)
            {
                builder.AppendLine(generatedItemIds[i]);
            }

            EditorGUIUtility.systemCopyBuffer = builder.ToString();
        }

        private static ulong GenerateULongId()
        {
            Guid guid = Guid.NewGuid();
            return BitConverter.ToUInt64(guid.ToByteArray(), 0);
        }

        private static string GeneratePrefixedId(string prefix, int bodyLength)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "ITEM";
            }

            string body = GetRandomString(ALPHA_NUMERIC, bodyLength);
            return $"{prefix.Trim().ToUpperInvariant()}_{body}";
        }

        private static string GenerateKeystoreKey(int length, bool withSymbols)
        {
            int safeLength = Mathf.Clamp(length, MIN_KEYSTORE_LENGTH, MAX_KEYSTORE_LENGTH);
            string source = withSymbols ? PRINTABLE_ASCII : ALPHA_NUMERIC + "abcdefghijklmnopqrstuvwxyz";
            return GetRandomString(source, safeLength);
        }

        private static string GetRandomString(string source, int length)
        {
            int max = source.Length;
            byte[] bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = source[bytes[i] % max];
            }

            return new string(chars);
        }

        private void CreateStyles()
        {
            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleLeft
            };

            cardStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(6, 6, 6, 6)
            };

            outputStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = false,
                richText = false
            };
        }
    }
}
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Aventra.Nugget.Common.UI;

namespace Aventra.Game.Editor
{
    public sealed class BaseMenuSceneManagerWindow : EditorWindow
    {
        private static readonly MethodInfo APPLY_CANVAS_SETTINGS_METHOD = typeof(BaseMenu).GetMethod(
            "ApplyCanvasSettings",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo OPEN_IMMEDIATE_METHOD = typeof(BaseMenu).GetMethod(
            "Open",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo CLOSE_IMMEDIATE_METHOD = typeof(BaseMenu).GetMethod(
            "Close",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private BaseMenu[] sceneMenus = Array.Empty<BaseMenu>();
        private BaseMenu[] interactiveMenus = Array.Empty<BaseMenu>();

        private Vector2 sceneScroll;
        private Vector2 interactiveScroll;
        private Vector2 settingsScroll;

        private bool settingIsWorldSpaceCanvas;
        private bool settingIsInteracteable = true;
        private bool settingOpenOnAwake;
        private float settingOpenOnAwakeOpacity = 1f;
        private float settingOpenMenuDelay;
        private Vector2 settingReferenceResolution = new(2560f, 1440f);
        private CanvasScaler.ScreenMatchMode settingScreenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        private float settingMatch = 0.5f;
        private float settingMenuSetVisibilityTweenSpeed = 4f;
        private float settingOpacity = 1f;

        private GUIStyle titleStyle;
        private GUIStyle cardStyle;
        private GUIStyle sectionTitleStyle;
        private GUIStyle listRowStyle;

        [MenuItem("Aventra/Tools/Base Menu Manager")]
        public static void OpenWindow()
        {
            BaseMenuSceneManagerWindow window = GetWindow<BaseMenuSceneManagerWindow>("Base Menu Manager");
            window.minSize = new Vector2(920f, 560f);
            window.Show();
        }

        private void OnEnable()
        {
            CreateStyles();
            RefreshSceneMenus();
        }

        private void OnHierarchyChange()
        {
            RefreshSceneMenus();
            Repaint();
        }

        private void OnGUI()
        {
            if (titleStyle == null)
            {
                CreateStyles();
            }

            DrawHeader();
            DrawToolbar();
            DrawBatchActions();

            EditorGUILayout.BeginHorizontal();
            DrawSceneMenuList();
            DrawInteractiveMenuList();
            EditorGUILayout.EndHorizontal();

            DrawInteractiveMenuSettings();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("Base Menu Scene Manager", titleStyle);
            EditorGUILayout.LabelField(
                "Sahnedeki tüm BaseMenu sınıflarını listele, etkileşim dizisine taşı ve standart menu ayarlarını tek yerden yönet.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(cardStyle);

            if (GUILayout.Button("Sahneyi Yenile", GUILayout.Height(28)))
            {
                RefreshSceneMenus();
            }

            GUI.enabled = sceneMenus.Length > 0;
            if (GUILayout.Button("Hepsini Etkileşime Ekle", GUILayout.Height(28)))
            {
                interactiveMenus = sceneMenus.Where(menu => menu != null).Distinct().ToArray();
            }

            GUI.enabled = interactiveMenus.Length > 0;
            if (GUILayout.Button("Etkileşim Dizisini Temizle", GUILayout.Height(28)))
            {
                interactiveMenus = Array.Empty<BaseMenu>();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBatchActions()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("Toplu İşlemler", sectionTitleStyle);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = interactiveMenus.Length > 0;
            if (GUILayout.Button("Hepsini Aç", GUILayout.Height(26)))
            {
                ForEachInteractiveMenu(OpenMenuImmediate);
            }

            if (GUILayout.Button("Hepsini Kapat", GUILayout.Height(26)))
            {
                ForEachInteractiveMenu(CloseMenuImmediate);
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawSceneMenuList()
        {
            EditorGUILayout.BeginVertical(cardStyle, GUILayout.Width(position.width * 0.33f));
            GUILayout.Label($"Sahnedeki Base Menuler ({sceneMenus.Length})", sectionTitleStyle);

            sceneScroll = EditorGUILayout.BeginScrollView(sceneScroll, GUILayout.MinHeight(210));

            if (sceneMenus.Length == 0)
            {
                EditorGUILayout.HelpBox("Sahnede BaseMenu bulunamadı.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < sceneMenus.Length; i++)
                {
                    BaseMenu menu = sceneMenus[i];
                    EditorGUILayout.BeginHorizontal(listRowStyle);

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(menu, typeof(BaseMenu), true);
                    EditorGUI.EndDisabledGroup();

                    bool alreadyExists = interactiveMenus.Contains(menu);
                    GUI.enabled = !alreadyExists;
                    if (GUILayout.Button(alreadyExists ? "Eklendi" : "Ekle", GUILayout.Width(72)))
                    {
                        AddInteractiveMenu(menu);
                    }

                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawInteractiveMenuList()
        {
            EditorGUILayout.BeginVertical(cardStyle, GUILayout.Width(position.width * 0.33f));
            GUILayout.Label($"Etkileşime Girilecek Menuler ({interactiveMenus.Length})", sectionTitleStyle);

            interactiveScroll = EditorGUILayout.BeginScrollView(interactiveScroll, GUILayout.MinHeight(210));

            if (interactiveMenus.Length == 0)
            {
                EditorGUILayout.HelpBox("Etkileşim dizisi boş. Soldaki listeden menü ekleyebilirsin.", MessageType.None);
            }
            else
            {
                for (int i = 0; i < interactiveMenus.Length; i++)
                {
                    BaseMenu menu = interactiveMenus[i];
                    EditorGUILayout.BeginHorizontal(listRowStyle);

                    BaseMenu updatedMenu = (BaseMenu)EditorGUILayout.ObjectField(menu, typeof(BaseMenu), true);
                    if (updatedMenu != menu)
                    {
                        interactiveMenus[i] = updatedMenu;
                        menu = updatedMenu;
                    }

                    if (GUILayout.Button("Seç", GUILayout.Width(50)) && menu != null)
                    {
                        Selection.activeObject = menu.gameObject;
                        EditorGUIUtility.PingObject(menu.gameObject);
                    }

                    if (GUILayout.Button("Çıkar", GUILayout.Width(58)))
                    {
                        RemoveInteractiveMenuAt(i);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawInteractiveMenuSettings()
        {
            EditorGUILayout.BeginVertical(cardStyle);
            GUILayout.Label("Ayarlar (Etkileşim Dizisi)", sectionTitleStyle);

            settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll, GUILayout.MinHeight(220));

            if (interactiveMenus.Length == 0)
            {
                EditorGUILayout.HelpBox("Ayar yapmak için önce etkileşim dizisine en az bir BaseMenu ekle.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Ayarlar listedeki tüm menülere uygulanır. Toplam menu: {interactiveMenus.Length}", MessageType.None);

                EditorGUILayout.BeginHorizontal();
                BaseMenu firstMenu = GetFirstInteractiveMenu();
                GUI.enabled = firstMenu != null;
                if (GUILayout.Button("İlk Menüden Değerleri Al", GUILayout.Height(24)) && firstMenu != null)
                {
                    LoadSettingsFromMenu(firstMenu);
                }

                GUI.enabled = interactiveMenus.Length > 0;
                if (GUILayout.Button("Ayarları Hepsine Uygula", GUILayout.Height(24)))
                {
                    ApplySettingsToInteractiveMenus();
                }

                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(6f);
                DrawSingleSettingsPanel();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSingleSettingsPanel()
        {
            settingIsWorldSpaceCanvas = EditorGUILayout.Toggle("World Space Canvas", settingIsWorldSpaceCanvas);
            settingIsInteracteable = EditorGUILayout.Toggle("Interacteable", settingIsInteracteable);
            settingOpenOnAwake = EditorGUILayout.Toggle("Open On Awake", settingOpenOnAwake);
            settingOpenOnAwakeOpacity = EditorGUILayout.Slider("Open On Awake Opacity", settingOpenOnAwakeOpacity, 0.01f, 1.0f);
            settingOpenMenuDelay = EditorGUILayout.FloatField("Open Menu Delay", settingOpenMenuDelay);
            settingReferenceResolution = EditorGUILayout.Vector2Field("Reference Resolution", settingReferenceResolution);
            settingScreenMatchMode = (CanvasScaler.ScreenMatchMode)EditorGUILayout.EnumPopup("Screen Match Mode", settingScreenMatchMode);
            if (settingScreenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
            {
                settingMatch = EditorGUILayout.Slider("Match", settingMatch, 0f, 1f);
            }

            settingMenuSetVisibilityTweenSpeed = EditorGUILayout.Slider("Visibility Tween Speed", settingMenuSetVisibilityTweenSpeed, 0.1f, 7f);
            settingOpacity = EditorGUILayout.Slider("Anlık Opaklık", settingOpacity, 0f, 1f);

            EditorGUILayout.Space(8f);
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = interactiveMenus.Length > 0;

            if (GUILayout.Button("Hepsini Aç", GUILayout.Height(24)))
            {
                ForEachInteractiveMenu(OpenMenuImmediate);
            }

            if (GUILayout.Button("Hepsini Kapat", GUILayout.Height(24)))
            {
                ForEachInteractiveMenu(CloseMenuImmediate);
            }

            if (GUILayout.Button("Canvas Ayarlarını Uygula", GUILayout.Height(24)))
            {
                ForEachInteractiveMenu(InvokeApplyCanvasSettings);
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void RefreshSceneMenus()
        {
            sceneMenus = FindObjectsByType<BaseMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(menu => menu != null)
                .OrderBy(menu => menu.name)
                .ToArray();

            interactiveMenus = interactiveMenus
                .Where(menu => menu != null && sceneMenus.Contains(menu))
                .Distinct()
                .ToArray();
        }

        private void AddInteractiveMenu(BaseMenu menu)
        {
            if (menu == null || interactiveMenus.Contains(menu))
            {
                return;
            }

            ArrayUtility.Add(ref interactiveMenus, menu);
            LoadSettingsFromMenu(menu);
        }

        private void RemoveInteractiveMenuAt(int index)
        {
            if (index < 0 || index >= interactiveMenus.Length)
            {
                return;
            }

            ArrayUtility.RemoveAt(ref interactiveMenus, index);
        }

        private void ForEachInteractiveMenu(Action<BaseMenu> action)
        {
            BaseMenu[] menus = interactiveMenus.Where(menu => menu != null).ToArray();
            for (int i = 0; i < menus.Length; i++)
            {
                action(menus[i]);
            }
        }

        private BaseMenu GetFirstInteractiveMenu()
        {
            return interactiveMenus.FirstOrDefault(menu => menu != null);
        }

        private void LoadSettingsFromMenu(BaseMenu menu)
        {
            if (menu == null)
            {
                return;
            }

            SerializedObject serializedMenu = new SerializedObject(menu);
            serializedMenu.Update();

            settingIsWorldSpaceCanvas = serializedMenu.FindProperty("isWorldSpaceCanvas")?.boolValue ?? settingIsWorldSpaceCanvas;
            settingIsInteracteable = serializedMenu.FindProperty("isInteracteable")?.boolValue ?? settingIsInteracteable;
            settingOpenOnAwake = serializedMenu.FindProperty("openOnAwake")?.boolValue ?? settingOpenOnAwake;
            settingOpenOnAwakeOpacity = serializedMenu.FindProperty("openOnAwakeOpacity")?.floatValue ?? settingOpenOnAwakeOpacity;
            settingOpenMenuDelay = serializedMenu.FindProperty("openMenuDelay")?.floatValue ?? settingOpenMenuDelay;
            settingReferenceResolution = serializedMenu.FindProperty("referenceResolution")?.vector2Value ?? settingReferenceResolution;

            SerializedProperty screenMatchProperty = serializedMenu.FindProperty("screenMatchMode");
            if (screenMatchProperty != null)
            {
                settingScreenMatchMode = (CanvasScaler.ScreenMatchMode)screenMatchProperty.enumValueIndex;
            }

            settingMatch = serializedMenu.FindProperty("match")?.floatValue ?? settingMatch;
            settingMenuSetVisibilityTweenSpeed = serializedMenu.FindProperty("menuSetVisibiltyTweenSpeed")?.floatValue ?? settingMenuSetVisibilityTweenSpeed;
            settingOpacity = Mathf.Clamp01(menu.Opacity);
        }

        private void ApplySettingsToInteractiveMenus()
        {
            ForEachInteractiveMenu(menu =>
            {
                SerializedObject serializedMenu = new SerializedObject(menu);
                serializedMenu.Update();

                SetBoolProperty(serializedMenu, "isWorldSpaceCanvas", settingIsWorldSpaceCanvas);
                SetBoolProperty(serializedMenu, "isInteracteable", settingIsInteracteable);
                SetBoolProperty(serializedMenu, "openOnAwake", settingOpenOnAwake);
                SetFloatProperty(serializedMenu, "openOnAwakeOpacity", settingOpenOnAwakeOpacity);
                SetFloatProperty(serializedMenu, "openMenuDelay", settingOpenMenuDelay);
                SetVector2Property(serializedMenu, "referenceResolution", settingReferenceResolution);
                SetEnumProperty(serializedMenu, "screenMatchMode", (int)settingScreenMatchMode);
                SetFloatProperty(serializedMenu, "match", settingMatch);
                SetFloatProperty(serializedMenu, "menuSetVisibiltyTweenSpeed", settingMenuSetVisibilityTweenSpeed);

                Undo.RecordObject(menu, "Apply BaseMenu Settings");
                serializedMenu.ApplyModifiedProperties();
                EditorUtility.SetDirty(menu);

                SetMenuOpacityImmediate(menu, settingOpacity);
                InvokeApplyCanvasSettings(menu);
            });
        }

        private static void SetBoolProperty(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetFloatProperty(SerializedObject serializedObject, string propertyName, float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetVector2Property(SerializedObject serializedObject, string propertyName, Vector2 value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.vector2Value = value;
            }
        }

        private static void SetEnumProperty(SerializedObject serializedObject, string propertyName, int enumValue)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.enumValueIndex = enumValue;
            }
        }

        private static void OpenMenuImmediate(BaseMenu menu)
        {
            if (menu == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                menu.OpenMenu();
                return;
            }

            if (OPEN_IMMEDIATE_METHOD != null)
            {
                Undo.RecordObject(menu, "Open Base Menu");
                OPEN_IMMEDIATE_METHOD.Invoke(menu, null);
                EditorUtility.SetDirty(menu);
            }
            else
            {
                SetMenuOpacityImmediate(menu, 1f);
            }
        }

        private static void CloseMenuImmediate(BaseMenu menu)
        {
            if (menu == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                menu.CloseMenu();
                return;
            }

            if (CLOSE_IMMEDIATE_METHOD != null)
            {
                Undo.RecordObject(menu, "Close Base Menu");
                CLOSE_IMMEDIATE_METHOD.Invoke(menu, null);
                EditorUtility.SetDirty(menu);
            }
            else
            {
                SetMenuOpacityImmediate(menu, 0f);
            }
        }

        private static void SetMenuOpacityImmediate(BaseMenu menu, float opacity)
        {
            if (menu == null)
            {
                return;
            }

            CanvasGroup canvasGroup = menu.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                return;
            }

            float safeOpacity = Mathf.Clamp01(opacity);
            Undo.RecordObject(canvasGroup, "Set Base Menu Opacity");
            canvasGroup.alpha = safeOpacity;
            bool interaction = safeOpacity > 0.001f;
            canvasGroup.interactable = interaction;
            canvasGroup.blocksRaycasts = interaction;
            EditorUtility.SetDirty(canvasGroup);
        }

        private static void InvokeApplyCanvasSettings(BaseMenu menu)
        {
            if (menu == null || APPLY_CANVAS_SETTINGS_METHOD == null)
            {
                return;
            }

            Undo.RecordObject(menu, "Apply Base Menu Canvas Settings");
            APPLY_CANVAS_SETTINGS_METHOD.Invoke(menu, null);
            EditorUtility.SetDirty(menu);
        }

        private void CreateStyles()
        {
            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft
            };

            cardStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(6, 6, 5, 5)
            };

            sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };

            listRowStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(6, 6, 4, 4),
                margin = new RectOffset(0, 0, 2, 2)
            };
        }
    }
}

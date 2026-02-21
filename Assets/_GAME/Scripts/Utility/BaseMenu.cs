using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Aventra.Nugget.Common.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseMenu : MonoBehaviour
    {
        [SerializeField] private bool isWorldSpaceCanvas;
        [SerializeField] private bool isInteracteable = true;
        [SerializeField] private bool openOnAwake = false;
        [SerializeField, Range(0.01f, 1.0f)] private float openOnAwakeOpacity = 1.0f;
        [SerializeField] private float openMenuDelay = 0.0f;
        [SerializeField] private Vector2 referenceResolution = new Vector2(2560, 1440);
        [SerializeField] private CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        [SerializeField] private float match = 0.5f;
        [SerializeField, Range(0.1f, 7f)] private float menuSetVisibiltyTweenSpeed = 4f;

        protected Canvas _canvas;
        protected CanvasScaler _canvasScaler;
        protected CanvasGroup _canvasGroup;

        private Coroutine _lastCoroutine;

        public bool IsOpen => _canvasGroup.alpha > 0.0f;
        public float Opacity => _canvasGroup.alpha;

        private const float OPEN_ALPHA = 1.0f;
        private const float CLOSE_ALPHA = 0.0f;
        protected virtual void Awake()
        {
            GetCanvasComponents();
            if (openOnAwake)
            {
                if (openOnAwakeOpacity < 1.0f)
                    SetOpacity(openOnAwakeOpacity, speedDelta:100, isInteraction: true);
                else
                    OpenMenu(0, 0);
            }
            else
            {
                CloseMenu(0, 0);
            }
        }
        private void OnEnable()
        {
            if (isWorldSpaceCanvas)
            {
                _canvas.worldCamera = Camera.main;
            }
        }
        private void Reset()
        {
            ApplyCanvasSettings();
        }
        public virtual void OpenMenu(float openMenuDelay = 0.0f, float openMenuIncreaseDelta = -1, Action OnStart = null, Action OnComplete = null)
        {
            if (_lastCoroutine != null)
            {
                StopCoroutine(_lastCoroutine);
            }
            _lastCoroutine = StartCoroutine(OpenMenuCoroutine(openMenuDelay, openMenuIncreaseDelta, OnStart, OnComplete));
        }
        public virtual void CloseMenu(float openMenuDelay = 0.0f, float openMenuIncreaseDelta = -1, Action OnStart = null, Action OnComplete = null)
        {
            if (_lastCoroutine != null)
            {
                StopCoroutine(_lastCoroutine);
            }
            _lastCoroutine = StartCoroutine(CloseMenuCoroutine(openMenuDelay, openMenuIncreaseDelta, OnStart, OnComplete));
        }
        public void SetOpacity(float opacityValue, float speedDelta = -1, bool isInteraction = false, Action OnComplete = null)
        {
            if (_lastCoroutine != null)
            {
                StopCoroutine(_lastCoroutine);
            }
            _lastCoroutine = StartCoroutine(SetOpacityCoroutine(opacityValue, speedDelta, isInteraction, OnComplete));
        }
        private IEnumerator SetOpacityCoroutine(float opacityValue, float speedDelta = -1, bool isInteraction = false, Action OnComplete = null)
        {
            float opacity = Mathf.Clamp(opacityValue, 0f, 1f);
            float delta = speedDelta == -1 ? menuSetVisibiltyTweenSpeed : speedDelta;
            delta = Mathf.Abs(delta); // Mutlaka pozitif olmal�

            bool isIncrease = opacity > _canvasGroup.alpha;

            while (Mathf.Abs(_canvasGroup.alpha - opacity) > 0.001f)
            {
                _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, opacity, delta * Time.unscaledDeltaTime);
                yield return null;
            }

            _canvasGroup.alpha = opacity;
            _canvasGroup.interactable = isInteraction;
            _canvasGroup.blocksRaycasts = isInteraction;

            OnComplete?.Invoke();
        }
        private IEnumerator OpenMenuCoroutine(float openMenuDelay = 0.0f, float openMenuIncreaseDelta = -1, Action OnStart = null, Action OnComplete = null)
        {
            float delay = openMenuDelay == 0.0f ? this.openMenuDelay : openMenuDelay;
            float delta = openMenuIncreaseDelta == -1 ? menuSetVisibiltyTweenSpeed : openMenuIncreaseDelta;
            SetInteractable(true);
            if (delay > 0.0f)
            {
                yield return new WaitForSecondsRealtime(delay);
            }
            OnStart?.Invoke();
            if (delta > 0.0f)
            {
                while (_canvasGroup.alpha < 1.0f)
                {
                    _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 1.0f, delta * Time.unscaledDeltaTime);
                    yield return null;
                }
                _canvasGroup.alpha = OPEN_ALPHA;
            }
            else
            {
                _canvasGroup.alpha = OPEN_ALPHA;
            }
            OnComplete?.Invoke();
        }
        private IEnumerator CloseMenuCoroutine(float openMenuDelay = 0.0f, float openMenuIncreaseDelta = -1, Action OnStart = null, Action OnComplete = null)
        {
            float delay = openMenuDelay == 0.0f ? this.openMenuDelay : openMenuDelay;
            float delta = openMenuIncreaseDelta == -1 ? menuSetVisibiltyTweenSpeed : openMenuIncreaseDelta;
            if (delay > 0.0f)
            {
                yield return new WaitForSecondsRealtime(delay);
            }
            OnStart?.Invoke();
            if (delta > 0.0f)
            {
                while (_canvasGroup.alpha > 0.0f)
                {
                    _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 0.0f, delta * Time.unscaledDeltaTime);
                    yield return null;
                }
                _canvasGroup.alpha = CLOSE_ALPHA;
            }
            else
            {
                _canvasGroup.alpha = CLOSE_ALPHA;
            }
            SetInteractable(false);
            OnComplete?.Invoke();
        }
        [ContextMenu(nameof(ApplyCanvasSettings))]
        private void ApplyCanvasSettings()
        {
            GetCanvasComponents();
            if (!isWorldSpaceCanvas)
            {
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                _canvasScaler.referenceResolution = referenceResolution;
                _canvasScaler.screenMatchMode = screenMatchMode;
                _canvasGroup.interactable = isInteracteable;
                if (screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
                {
                    _canvasScaler.matchWidthOrHeight = match;
                }
            }
            else
            {
                _canvas.renderMode = RenderMode.WorldSpace;
                _canvas.worldCamera = Camera.main;
            }
#if UNITY_EDITOR
            if (openOnAwake)
            {
                Open();
            }
            else
            {
                Close();
            }
#endif // UNITY_EDITOR
        }
        private void GetCanvasComponents()
        {
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
            }
            if (_canvasScaler == null)
            {
                _canvasScaler = GetComponent<CanvasScaler>();
            }
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }
        private void SetInteractable(bool value)
        {
            _canvasGroup.interactable = isInteracteable ? value : false;
            _canvasGroup.blocksRaycasts = isInteracteable ? value : false;
        }
#if UNITY_EDITOR
        [ContextMenu(nameof(Open))]
        [ExecuteInEditMode]
        private void Open()
        {
            GetCanvasComponents();
            _canvasGroup.alpha = OPEN_ALPHA;
            SetInteractable(true);
        }
        [ContextMenu(nameof(Close))]
        [ExecuteInEditMode]
        private void Close()
        {
            GetCanvasComponents();
            _canvasGroup.alpha = CLOSE_ALPHA;
            SetInteractable(false);
        }
#endif
    }
}
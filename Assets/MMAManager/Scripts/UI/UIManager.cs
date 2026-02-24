using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MMAManager.UI
{
    public abstract class UIScreen : MonoBehaviour
    {
        protected Canvas canvas;
        protected CanvasScaler scaler;
        protected Font uiFont;

        protected static readonly Color BgColor = new Color(0.08f, 0.08f, 0.12f);
        protected static readonly Color HeaderColor = new Color(0.12f, 0.12f, 0.18f, 0.95f);
        protected static readonly Color CardColor = new Color(0.15f, 0.15f, 0.22f);
        protected static readonly Color ButtonColor = new Color(0.2f, 0.5f, 0.9f);
        protected static readonly Color ButtonHoverColor = new Color(0.25f, 0.55f, 0.95f);
        protected static readonly Color GoldColor = new Color(1f, 0.85f, 0.3f);
        protected static readonly Color TextWhite = Color.white;
        protected static readonly Color TextGray = new Color(0.6f, 0.6f, 0.6f);

        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        protected virtual void OnShow() { }

        protected void InitCanvas(string screenName, int sortOrder = 10)
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();
        }

        protected abstract void BuildUI();

        #region UI Helpers

        protected GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
            return go;
        }

        protected GameObject CreateFullPanel(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }

        protected Text CreateText(Transform parent, string name, string content,
            int fontSize, FontStyle style, Color color, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.font = uiFont;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        protected Button CreateButton(Transform parent, string name, string label,
            Color bgColor, int fontSize, System.Action onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = colors;

            if (onClick != null)
                btn.onClick.AddListener(() => onClick());

            // Label
            CreateText(go.transform, "Label", label, fontSize, FontStyle.Bold,
                Color.white, TextAnchor.MiddleCenter);

            return btn;
        }

        protected Color GetStatColor(int value)
        {
            if (value >= 80) return new Color(0.2f, 0.8f, 0.3f);
            if (value >= 60) return new Color(0.9f, 0.8f, 0.2f);
            if (value >= 40) return new Color(0.9f, 0.5f, 0.2f);
            return new Color(0.9f, 0.25f, 0.2f);
        }

        #endregion
    }

    public class UIManager : MonoBehaviour
    {
        private static UIManager instance;
        public static UIManager Instance => instance;

        private Dictionary<string, UIScreen> screens = new Dictionary<string, UIScreen>();
        private Stack<string> screenStack = new Stack<string>();
        private string currentScreen;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                EnsureEventSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esObj = new GameObject("EventSystem");
                esObj.transform.SetParent(transform);
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }
        }

        public void RegisterScreen(string name, UIScreen screen)
        {
            screens[name] = screen;
            screen.Hide();
        }

        public void ShowScreen(string name)
        {
            if (!screens.ContainsKey(name))
            {
                Debug.LogError($"[UIManager] Screen '{name}' not found");
                return;
            }

            // Hide current
            if (!string.IsNullOrEmpty(currentScreen) && screens.ContainsKey(currentScreen))
            {
                screens[currentScreen].Hide();
                screenStack.Push(currentScreen);
            }

            currentScreen = name;
            screens[name].Show();
        }

        public void GoBack()
        {
            if (screenStack.Count == 0) return;

            if (!string.IsNullOrEmpty(currentScreen) && screens.ContainsKey(currentScreen))
                screens[currentScreen].Hide();

            currentScreen = screenStack.Pop();
            if (screens.ContainsKey(currentScreen))
                screens[currentScreen].Show();
        }

        public void GoHome()
        {
            if (!string.IsNullOrEmpty(currentScreen) && screens.ContainsKey(currentScreen))
                screens[currentScreen].Hide();

            screenStack.Clear();
            currentScreen = "MainMenu";
            if (screens.ContainsKey(currentScreen))
                screens[currentScreen].Show();
        }

        public T GetScreen<T>(string name) where T : UIScreen
        {
            if (screens.TryGetValue(name, out var screen))
                return screen as T;
            return null;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MMAManager.Combat;

namespace MMAManager.UI
{
    public class FightHUD : MonoBehaviour
    {
        private static FightHUD instance;
        public static FightHUD Instance => instance;

        // References
        private FighterAgent fighter1;
        private FighterAgent fighter2;
        private RoundManager roundManager;

        // UI Elements
        private Canvas canvas;
        private Text roundText;
        private Text timerText;
        private Image hp1Bar;
        private Image hp1BarBg;
        private Image hp2Bar;
        private Image hp2BarBg;
        private Text name1Text;
        private Text name2Text;
        private Text hp1Text;
        private Text hp2Text;
        private Text centerText;
        private Text subText;
        private CanvasGroup centerGroup;

        // Corner strategy UI
        private Button[] strategyButtons;
        private Image[] strategyBtnImages;
        private int selectedStrategyIndex = 5; // Balanced

        private static readonly CornerStrategy[] strategies = {
            CornerStrategy.Aggressive, CornerStrategy.Defensive, CornerStrategy.BodyWork,
            CornerStrategy.Takedown, CornerStrategy.Finish, CornerStrategy.Balanced
        };
        private static readonly string[] strategyLabels = {
            "공격적", "방어적", "바디", "테이크다운", "피니시", "균형"
        };

        // HP bar lerp
        private float hp1Display = 1f;
        private float hp2Display = 1f;
        private float hp1Target = 1f;
        private float hp2Target = 1f;

        // Announcement
        private Coroutine announcementCoroutine;

        private Font uiFont;

        void Awake()
        {
            instance = this;
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildUI();
        }

        void Update()
        {
            UpdateTimer();
            UpdateHPBars();
        }

        #region UI Construction

        private void BuildUI()
        {
            // Canvas
            GameObject canvasObj = new GameObject("FightHUD_Canvas");
            canvasObj.transform.SetParent(transform);
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Top bar background
            CreateTopBar(canvasObj.transform);

            // HP section
            CreateHPSection(canvasObj.transform);

            // Center announcement
            CreateCenterAnnouncement(canvasObj.transform);

            // Corner strategy buttons (bottom)
            CreateCornerUI(canvasObj.transform);
        }

        private void CreateTopBar(Transform parent)
        {
            // Background - full width, 40px tall
            var bg = CreatePanel(parent, "TopBar",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), new Vector2(0, 0),
                new Vector2(0, 40));
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            // Round text (center-left)
            roundText = CreateText(bg.transform, "RoundText", "ROUND 1",
                24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            var roundRect = roundText.GetComponent<RectTransform>();
            roundRect.anchorMin = new Vector2(0.3f, 0);
            roundRect.anchorMax = new Vector2(0.5f, 1);
            roundRect.offsetMin = Vector2.zero;
            roundRect.offsetMax = Vector2.zero;

            // Timer text (center-right)
            timerText = CreateText(bg.transform, "TimerText", "5:00",
                24, FontStyle.Bold, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter);
            var timerRect = timerText.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.5f, 0);
            timerRect.anchorMax = new Vector2(0.7f, 1);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;
        }

        private void CreateHPSection(Transform parent)
        {
            // Container below top bar - 70px tall
            var container = CreatePanel(parent, "HPSection",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), new Vector2(0, -40),
                new Vector2(0, 70));

            var containerImg = container.AddComponent<Image>();
            containerImg.color = new Color(0f, 0f, 0f, 0.6f);

            // --- Fighter 1 (Left side) ---
            // Name (top-left)
            name1Text = CreateText(container.transform, "Name1", "FIGHTER 1",
                18, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            var n1Rect = name1Text.GetComponent<RectTransform>();
            n1Rect.anchorMin = new Vector2(0, 0.55f);
            n1Rect.anchorMax = new Vector2(0.48f, 1f);
            n1Rect.offsetMin = new Vector2(15, 0);
            n1Rect.offsetMax = Vector2.zero;

            // HP bar background 1
            hp1BarBg = CreateHPBarBg(container.transform, "HP1Bg", true);
            hp1Bar = CreateHPBarFill(hp1BarBg.transform, "HP1Fill", true);

            // HP text 1 (below bar, left)
            hp1Text = CreateText(container.transform, "HP1Text", "100/100",
                14, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleLeft);
            var ht1Rect = hp1Text.GetComponent<RectTransform>();
            ht1Rect.anchorMin = new Vector2(0, 0f);
            ht1Rect.anchorMax = new Vector2(0.48f, 0.25f);
            ht1Rect.offsetMin = new Vector2(15, 0);
            ht1Rect.offsetMax = Vector2.zero;

            // --- Fighter 2 (Right side) ---
            // Name (top-right)
            name2Text = CreateText(container.transform, "Name2", "FIGHTER 2",
                18, FontStyle.Bold, Color.white, TextAnchor.MiddleRight);
            var n2Rect = name2Text.GetComponent<RectTransform>();
            n2Rect.anchorMin = new Vector2(0.52f, 0.55f);
            n2Rect.anchorMax = new Vector2(1f, 1f);
            n2Rect.offsetMin = Vector2.zero;
            n2Rect.offsetMax = new Vector2(-15, 0);

            // HP bar background 2
            hp2BarBg = CreateHPBarBg(container.transform, "HP2Bg", false);
            hp2Bar = CreateHPBarFill(hp2BarBg.transform, "HP2Fill", false);

            // HP text 2 (below bar, right)
            hp2Text = CreateText(container.transform, "HP2Text", "100/100",
                14, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleRight);
            var ht2Rect = hp2Text.GetComponent<RectTransform>();
            ht2Rect.anchorMin = new Vector2(0.52f, 0f);
            ht2Rect.anchorMax = new Vector2(1f, 0.25f);
            ht2Rect.offsetMin = Vector2.zero;
            ht2Rect.offsetMax = new Vector2(-15, 0);
        }

        private Image CreateHPBarBg(Transform parent, string name, bool isLeft)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();

            if (isLeft)
            {
                rect.anchorMin = new Vector2(0.01f, 0.28f);
                rect.anchorMax = new Vector2(0.48f, 0.55f);
            }
            else
            {
                rect.anchorMin = new Vector2(0.52f, 0.28f);
                rect.anchorMax = new Vector2(0.99f, 0.55f);
            }
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            return img;
        }

        private Image CreateHPBarFill(Transform parent, string name, bool isLeft)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            // Left bar fills left-to-right, right bar fills right-to-left
            img.fillOrigin = isLeft ? 0 : 1;
            img.fillAmount = 1f;
            img.color = Color.green;
            return img;
        }

        private void CreateCenterAnnouncement(Transform parent)
        {
            var container = new GameObject("CenterAnnouncement");
            container.transform.SetParent(parent, false);
            var cRect = container.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.2f, 0.3f);
            cRect.anchorMax = new Vector2(0.8f, 0.7f);
            cRect.offsetMin = Vector2.zero;
            cRect.offsetMax = Vector2.zero;

            centerGroup = container.AddComponent<CanvasGroup>();
            centerGroup.alpha = 0f;

            // Main text (big)
            centerText = CreateText(container.transform, "CenterText", "",
                72, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            var ctRect = centerText.GetComponent<RectTransform>();
            ctRect.anchorMin = new Vector2(0, 0.4f);
            ctRect.anchorMax = new Vector2(1, 1f);
            ctRect.offsetMin = Vector2.zero;
            ctRect.offsetMax = Vector2.zero;

            // Add outline for readability
            var outline = centerText.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(3, -3);

            // Sub text (smaller)
            subText = CreateText(container.transform, "SubText", "",
                36, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter);
            var stRect = subText.GetComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0, 0f);
            stRect.anchorMax = new Vector2(1, 0.4f);
            stRect.offsetMin = Vector2.zero;
            stRect.offsetMax = Vector2.zero;

            var subOutline = subText.gameObject.AddComponent<Outline>();
            subOutline.effectColor = Color.black;
            subOutline.effectDistance = new Vector2(2, -2);
        }

        private void CreateCornerUI(Transform parent)
        {
            // Container at bottom - 180px tall
            var container = CreatePanel(parent, "CornerUI",
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0.5f, 0), new Vector2(0, 0),
                new Vector2(0, 180));

            var bgImg = container.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.8f);

            // Title
            var title = CreateText(container.transform, "CornerTitle", "세컨 지시",
                20, FontStyle.Bold, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.82f);
            titleRect.anchorMax = new Vector2(1, 1f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            strategyButtons = new Button[6];
            strategyBtnImages = new Image[6];

            // 2 rows x 3 columns
            for (int i = 0; i < 6; i++)
            {
                int col = i % 3;
                int row = i / 3;

                float x0 = 0.02f + col * 0.33f;
                float x1 = x0 + 0.30f;
                float y1 = 0.78f - row * 0.40f;
                float y0 = y1 - 0.36f;

                var btnObj = new GameObject($"StrategyBtn_{i}");
                btnObj.transform.SetParent(container.transform, false);
                var btnRect = btnObj.AddComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(x0, y0);
                btnRect.anchorMax = new Vector2(x1, y1);
                btnRect.offsetMin = Vector2.zero;
                btnRect.offsetMax = Vector2.zero;

                var btnImg = btnObj.AddComponent<Image>();
                btnImg.color = new Color(0.2f, 0.2f, 0.3f, 1f);
                strategyBtnImages[i] = btnImg;

                var btn = btnObj.AddComponent<Button>();
                btn.targetGraphic = btnImg;
                int idx = i;
                btn.onClick.AddListener(() => OnStrategyButtonClicked(idx));
                strategyButtons[i] = btn;

                // Label
                CreateText(btnObj.transform, "Label", strategyLabels[i],
                    22, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            }

            // Highlight default (Balanced = index 5)
            UpdateStrategyHighlight();
        }

        private void OnStrategyButtonClicked(int index)
        {
            selectedStrategyIndex = index;
            if (fighter1 != null)
                fighter1.SetStrategy(strategies[index]);
            UpdateStrategyHighlight();
        }

        private void UpdateStrategyHighlight()
        {
            Color normalColor = new Color(0.2f, 0.2f, 0.3f, 1f);
            Color selectedColor = new Color(0.8f, 0.7f, 0.1f, 1f);

            for (int i = 0; i < strategyBtnImages.Length; i++)
            {
                if (strategyBtnImages[i] != null)
                    strategyBtnImages[i].color = (i == selectedStrategyIndex) ? selectedColor : normalColor;
            }
        }

        #endregion

        #region Helpers

        private GameObject CreatePanel(Transform parent, string name,
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

        private Text CreateText(Transform parent, string name, string content,
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

        #endregion

        #region Initialization

        public void Initialize(FighterAgent f1, FighterAgent f2, RoundManager rm)
        {
            fighter1 = f1;
            fighter2 = f2;
            roundManager = rm;

            // Set names
            name1Text.text = fighter1.fighterData.FullName.ToUpper();
            name2Text.text = fighter2.fighterData.FullName.ToUpper();

            // Set initial HP
            hp1Target = 1f;
            hp1Display = 1f;
            hp2Target = 1f;
            hp2Display = 1f;
            UpdateHPDisplay(hp1Bar, hp1Text, fighter1, 1f);
            UpdateHPDisplay(hp2Bar, hp2Text, fighter2, 1f);

            // Subscribe to events
            fighter1.OnHit += (dmg) => OnFighterHit(1);
            fighter2.OnHit += (dmg) => OnFighterHit(2);
            fighter1.OnKnockout += () => ShowKO(fighter1, fighter2);
            fighter2.OnKnockout += () => ShowKO(fighter2, fighter1);

            roundManager.OnRoundStart += OnRoundStart;
            roundManager.OnRoundEnd += OnRoundEnd;
            roundManager.OnFightEnd += OnFightEnd;

            // Show initial round announcement
            ShowAnnouncement("ROUND 1", "FIGHT!", Color.white, 2f);
        }

        #endregion

        #region Updates

        private void UpdateTimer()
        {
            if (roundManager == null) return;

            if (roundManager.IsResting)
            {
                float restRemaining = 60f - roundManager.RestTimer;
                int sec = Mathf.CeilToInt(restRemaining);
                timerText.text = $"REST {sec}s";
                roundText.text = $"ROUND {roundManager.CurrentRound}";
            }
            else if (roundManager.IsRoundActive)
            {
                float remaining = 300f - roundManager.RoundTimer;
                int min = Mathf.FloorToInt(remaining / 60f);
                int sec = Mathf.FloorToInt(remaining % 60f);
                timerText.text = $"{min:0}:{sec:00}";
                roundText.text = $"ROUND {roundManager.CurrentRound}";
            }
        }

        private void UpdateHPBars()
        {
            if (fighter1 == null || fighter2 == null) return;

            // Lerp toward target
            hp1Display = Mathf.Lerp(hp1Display, hp1Target, Time.unscaledDeltaTime * 5f);
            hp2Display = Mathf.Lerp(hp2Display, hp2Target, Time.unscaledDeltaTime * 5f);

            UpdateHPDisplay(hp1Bar, hp1Text, fighter1, hp1Display);
            UpdateHPDisplay(hp2Bar, hp2Text, fighter2, hp2Display);
        }

        private void UpdateHPDisplay(Image bar, Text hpText, FighterAgent fighter, float displayPercent)
        {
            bar.fillAmount = displayPercent;
            bar.color = GetHealthColor(displayPercent);
            hpText.text = $"{fighter.currentHP}/{fighter.maxHP}";
        }

        private Color GetHealthColor(float percent)
        {
            if (percent > 0.6f)
                return Color.Lerp(Color.yellow, Color.green, (percent - 0.6f) / 0.4f);
            if (percent > 0.3f)
                return Color.Lerp(Color.red, Color.yellow, (percent - 0.3f) / 0.3f);
            return Color.red;
        }

        #endregion

        #region Event Handlers

        private void OnFighterHit(int fighterIndex)
        {
            if (fighterIndex == 1 && fighter1 != null)
                hp1Target = (float)fighter1.currentHP / fighter1.maxHP;
            else if (fighterIndex == 2 && fighter2 != null)
                hp2Target = (float)fighter2.currentHP / fighter2.maxHP;
        }

        private void OnRoundStart(int round)
        {
            ShowAnnouncement($"ROUND {round}", "FIGHT!", Color.white, 2f);
        }

        private void OnRoundEnd(int round)
        {
            ShowAnnouncement($"ROUND {round}", "END", Color.yellow, 1.5f);
        }

        private void OnFightEnd()
        {
            if (fighter1 == null || fighter2 == null) return;

            // Only show decision if not KO (KO shows its own announcement)
            if (fighter1.currentState != FighterState.KO && fighter2.currentState != FighterState.KO)
            {
                string winner;
                if (fighter1.currentHP > fighter2.currentHP)
                    winner = fighter1.fighterData.FullName;
                else if (fighter2.currentHP > fighter1.currentHP)
                    winner = fighter2.fighterData.FullName;
                else
                    winner = "DRAW";

                if (winner == "DRAW")
                    ShowAnnouncement("DECISION", "DRAW", Color.yellow, 5f);
                else
                    ShowAnnouncement("DECISION", $"Winner: {winner}", Color.yellow, 5f);
            }
        }

        private void ShowKO(FighterAgent loser, FighterAgent winner)
        {
            string winnerName = winner.fighterData.FullName;
            ShowAnnouncement("KO!", $"Winner: {winnerName}", new Color(1f, 0.2f, 0.2f), 5f);
        }

        #endregion

        #region Announcements

        private void ShowAnnouncement(string main, string sub, Color color, float duration)
        {
            if (announcementCoroutine != null)
                StopCoroutine(announcementCoroutine);
            announcementCoroutine = StartCoroutine(AnnouncementSequence(main, sub, color, duration));
        }

        private IEnumerator AnnouncementSequence(string main, string sub, Color color, float duration)
        {
            centerText.text = main;
            centerText.color = color;
            subText.text = sub;

            // Fade in + scale up
            float fadeIn = 0.3f;
            float t = 0f;
            centerGroup.alpha = 0f;
            Vector3 startScale = Vector3.one * 1.5f;
            Vector3 endScale = Vector3.one;

            while (t < fadeIn)
            {
                t += Time.unscaledDeltaTime;
                float p = t / fadeIn;
                centerGroup.alpha = p;
                centerGroup.transform.localScale = Vector3.Lerp(startScale, endScale, p);
                yield return null;
            }

            centerGroup.alpha = 1f;
            centerGroup.transform.localScale = Vector3.one;

            // Hold
            yield return new WaitForSecondsRealtime(duration);

            // Fade out
            float fadeOut = 0.5f;
            t = 0f;
            while (t < fadeOut)
            {
                t += Time.unscaledDeltaTime;
                centerGroup.alpha = 1f - (t / fadeOut);
                yield return null;
            }

            centerGroup.alpha = 0f;
            announcementCoroutine = null;
        }

        #endregion
    }
}

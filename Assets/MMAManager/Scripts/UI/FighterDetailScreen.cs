using UnityEngine;
using UnityEngine.UI;
using MMAManager.Models;
using MMAManager.Systems;

namespace MMAManager.UI
{
    public class FighterDetailScreen : UIScreen
    {
        private Fighter currentFighter;

        // Dynamic UI references - Info
        private Text nameText;
        private Text infoText;
        private Text recordText;
        private Text recordDetailText;
        private Text styleText;
        private Text ovrText;
        private Image ovrBadgeBg;
        private Image portraitImage;

        // Stat bars
        private Image[] statBars = new Image[8];
        private Text[] statValueTexts = new Text[8];
        private static readonly string[] statNames = { "STR", "TEC", "SPD", "STA", "DEF", "WRS", "BJJ", "POT" };

        // Condition section
        private Image conditionBar;
        private Image fatigueBar;
        private Image healthBar;
        private Text conditionValueText;
        private Text fatigueValueText;
        private Text healthValueText;

        // Contract section
        private Text contractText;
        private Text winRateText;

        void Awake()
        {
            InitCanvas("FighterDetail", 12);
            BuildUI();
        }

        public void SetFighter(Fighter fighter)
        {
            currentFighter = fighter;
        }

        protected override void OnShow()
        {
            if (currentFighter != null)
                PopulateData();
        }

        protected override void BuildUI()
        {
            // Full background
            var bg = CreateFullPanel(transform, "Background");
            bg.AddComponent<Image>().color = BgColor;

            // Header
            CreateHeader(transform);

            // Scrollable content area
            CreateScrollContent(transform);

            // Action buttons (fixed bottom)
            CreateActionButtons(transform);
        }

        private void CreateHeader(Transform parent)
        {
            var header = CreatePanel(parent, "Header",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 60));
            header.AddComponent<Image>().color = HeaderColor;

            // Back button
            var backObj = new GameObject("BackBtn");
            backObj.transform.SetParent(header.transform, false);
            var backRect = backObj.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0);
            backRect.anchorMax = new Vector2(0.15f, 1);
            backRect.offsetMin = Vector2.zero;
            backRect.offsetMax = Vector2.zero;

            backObj.AddComponent<Image>().color = new Color(1, 1, 1, 0);
            var backBtn = backObj.AddComponent<Button>();
            backBtn.onClick.AddListener(() => UIManager.Instance?.GoBack());
            CreateText(backObj.transform, "Label", "< 뒤로",
                24, FontStyle.Normal, GoldColor, TextAnchor.MiddleCenter);

            // Title
            var title = CreateText(header.transform, "Title", "선수 상세",
                30, FontStyle.Bold, TextWhite, TextAnchor.MiddleCenter);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.15f, 0);
            titleRect.anchorMax = new Vector2(0.85f, 1);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
        }

        private void CreateScrollContent(Transform parent)
        {
            // Scroll area (between header and action bar)
            var scrollObj = CreatePanel(parent, "ScrollArea",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var scrollObjRect = scrollObj.GetComponent<RectTransform>();
            scrollObjRect.offsetMin = new Vector2(0, 80);   // action bar
            scrollObjRect.offsetMax = new Vector2(0, -60);   // header

            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            // Viewport
            var viewport = CreateFullPanel(scrollObj.transform, "Viewport");
            viewport.AddComponent<RectMask2D>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            // Content
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = Vector2.zero;

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(15, 15, 10, 20);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;

            // === Build sections ===
            BuildProfileCard(content.transform);
            BuildRecordCard(content.transform);
            BuildStatBars(content.transform);
            BuildConditionCard(content.transform);
            BuildContractCard(content.transform);
        }

        private void BuildProfileCard(Transform parent)
        {
            var card = CreateLayoutCard(parent, "ProfileCard", 200);

            // Portrait placeholder (left side)
            var portraitObj = new GameObject("Portrait");
            portraitObj.transform.SetParent(card.transform, false);
            var portraitRect = portraitObj.AddComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.03f, 0.08f);
            portraitRect.anchorMax = new Vector2(0.30f, 0.92f);
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;

            portraitImage = portraitObj.AddComponent<Image>();
            portraitImage.color = new Color(0.25f, 0.25f, 0.35f);

            // Portrait label
            CreateText(portraitObj.transform, "PortraitLabel", "PHOTO",
                18, FontStyle.Normal, TextGray, TextAnchor.MiddleCenter);

            // Name (right of portrait)
            nameText = CreateText(card.transform, "Name", "FIGHTER NAME",
                32, FontStyle.Bold, TextWhite, TextAnchor.MiddleLeft);
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.33f, 0.72f);
            nameRect.anchorMax = new Vector2(0.97f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Age + Weight class
            infoText = CreateText(card.transform, "Info", "28세 | 미들급",
                22, FontStyle.Normal, TextGray, TextAnchor.MiddleLeft);
            var infoRect = infoText.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.33f, 0.50f);
            infoRect.anchorMax = new Vector2(0.97f, 0.72f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;

            // Style
            styleText = CreateText(card.transform, "Style", "스타일: 타격형",
                22, FontStyle.Normal, GoldColor, TextAnchor.MiddleLeft);
            var styleRect = styleText.GetComponent<RectTransform>();
            styleRect.anchorMin = new Vector2(0.33f, 0.28f);
            styleRect.anchorMax = new Vector2(0.97f, 0.50f);
            styleRect.offsetMin = Vector2.zero;
            styleRect.offsetMax = Vector2.zero;

            // Win rate
            winRateText = CreateText(card.transform, "WinRate", "승률: 0%",
                22, FontStyle.Normal, TextWhite, TextAnchor.MiddleLeft);
            var wrRect = winRateText.GetComponent<RectTransform>();
            wrRect.anchorMin = new Vector2(0.33f, 0.05f);
            wrRect.anchorMax = new Vector2(0.97f, 0.28f);
            wrRect.offsetMin = Vector2.zero;
            wrRect.offsetMax = Vector2.zero;

            // OVR badge (top right)
            var ovrBadge = new GameObject("OVRBadge");
            ovrBadge.transform.SetParent(card.transform, false);
            var ovrBadgeRect = ovrBadge.AddComponent<RectTransform>();
            ovrBadgeRect.anchorMin = new Vector2(0.80f, 0.55f);
            ovrBadgeRect.anchorMax = new Vector2(0.97f, 0.95f);
            ovrBadgeRect.offsetMin = Vector2.zero;
            ovrBadgeRect.offsetMax = Vector2.zero;

            ovrBadgeBg = ovrBadge.AddComponent<Image>();
            ovrBadgeBg.color = new Color(0.3f, 0.3f, 0.4f);

            var ovrLabel = CreateText(ovrBadge.transform, "OVRLabel", "OVR",
                14, FontStyle.Normal, TextGray, TextAnchor.UpperCenter);
            var ovrLabelRect = ovrLabel.GetComponent<RectTransform>();
            ovrLabelRect.anchorMin = new Vector2(0, 0.55f);
            ovrLabelRect.anchorMax = new Vector2(1, 1);
            ovrLabelRect.offsetMin = Vector2.zero;
            ovrLabelRect.offsetMax = Vector2.zero;

            ovrText = CreateText(ovrBadge.transform, "OVRValue", "0",
                32, FontStyle.Bold, TextWhite, TextAnchor.MiddleCenter);
            var ovrValueRect = ovrText.GetComponent<RectTransform>();
            ovrValueRect.anchorMin = new Vector2(0, 0);
            ovrValueRect.anchorMax = new Vector2(1, 0.6f);
            ovrValueRect.offsetMin = Vector2.zero;
            ovrValueRect.offsetMax = Vector2.zero;
        }

        private void BuildRecordCard(Transform parent)
        {
            var card = CreateLayoutCard(parent, "RecordCard", 100);

            // Section title
            CreateSectionLabel(card.transform, "전적", 0.03f, 0.97f, 0.70f, 1f);

            // Record main
            recordText = CreateText(card.transform, "Record", "0전 0승 0패 0무",
                24, FontStyle.Bold, TextWhite, TextAnchor.MiddleLeft);
            var recRect = recordText.GetComponent<RectTransform>();
            recRect.anchorMin = new Vector2(0.03f, 0.35f);
            recRect.anchorMax = new Vector2(0.50f, 0.70f);
            recRect.offsetMin = Vector2.zero;
            recRect.offsetMax = Vector2.zero;

            // Record detail (KO/Sub/Dec)
            recordDetailText = CreateText(card.transform, "RecordDetail", "KO 0 | 서브 0 | 판정 0",
                20, FontStyle.Normal, TextGray, TextAnchor.MiddleLeft);
            var detRect = recordDetailText.GetComponent<RectTransform>();
            detRect.anchorMin = new Vector2(0.03f, 0.05f);
            detRect.anchorMax = new Vector2(0.97f, 0.35f);
            detRect.offsetMin = Vector2.zero;
            detRect.offsetMax = Vector2.zero;
        }

        private void BuildStatBars(Transform parent)
        {
            var card = CreateLayoutCard(parent, "StatsCard", 420);

            CreateSectionLabel(card.transform, "능력치", 0.03f, 0.97f, 0.93f, 1f);

            float startY = 0.88f;
            float barH = 0.095f;
            float gap = 0.01f;

            for (int i = 0; i < 8; i++)
            {
                float top = startY - i * (barH + gap);
                float bot = top - barH;

                // Stat label
                var label = CreateText(card.transform, $"Label_{i}", statNames[i],
                    20, FontStyle.Bold, GoldColor, TextAnchor.MiddleLeft);
                var labelRect = label.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0.03f, bot);
                labelRect.anchorMax = new Vector2(0.12f, top);
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                // Bar background
                var barBg = new GameObject($"BarBg_{i}");
                barBg.transform.SetParent(card.transform, false);
                var barBgRect = barBg.AddComponent<RectTransform>();
                barBgRect.anchorMin = new Vector2(0.14f, bot + 0.015f);
                barBgRect.anchorMax = new Vector2(0.85f, top - 0.015f);
                barBgRect.offsetMin = Vector2.zero;
                barBgRect.offsetMax = Vector2.zero;
                barBg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);

                // Bar fill (앵커 기반 - anchorMax.x로 크기 조절)
                var barFill = new GameObject($"BarFill_{i}");
                barFill.transform.SetParent(barBg.transform, false);
                var fillRect = barFill.AddComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = new Vector2(0.5f, 1f); // 기본 50%
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;

                var fillImg = barFill.AddComponent<Image>();
                fillImg.color = Color.green;
                statBars[i] = fillImg;

                // Value text
                statValueTexts[i] = CreateText(card.transform, $"Value_{i}", "50",
                    22, FontStyle.Bold, TextWhite, TextAnchor.MiddleRight);
                var valRect = statValueTexts[i].GetComponent<RectTransform>();
                valRect.anchorMin = new Vector2(0.87f, bot);
                valRect.anchorMax = new Vector2(0.97f, top);
                valRect.offsetMin = Vector2.zero;
                valRect.offsetMax = Vector2.zero;
            }
        }

        private void BuildConditionCard(Transform parent)
        {
            var card = CreateLayoutCard(parent, "ConditionCard", 170);

            CreateSectionLabel(card.transform, "컨디션", 0.03f, 0.97f, 0.82f, 1f);

            // Health bar
            CreateMiniBar(card.transform, "건강", 0.55f, 0.80f, out healthBar, out healthValueText,
                new Color(0.2f, 0.8f, 0.3f));

            // Fatigue bar
            CreateMiniBar(card.transform, "피로도", 0.28f, 0.53f, out fatigueBar, out fatigueValueText,
                new Color(0.9f, 0.6f, 0.2f));

            // Condition bar
            CreateMiniBar(card.transform, "컨디션", 0.02f, 0.27f, out conditionBar, out conditionValueText,
                new Color(0.3f, 0.7f, 0.9f));
        }

        private void CreateMiniBar(Transform parent, string label, float yMin, float yMax,
            out Image bar, out Text valueText, Color barColor)
        {
            // Label
            CreateText(parent, $"{label}Label", label,
                20, FontStyle.Normal, TextGray, TextAnchor.MiddleLeft)
                .GetComponent<RectTransform>().SetAnchors(0.03f, yMin, 0.20f, yMax);

            // Bar bg
            var barBg = new GameObject($"{label}BarBg");
            barBg.transform.SetParent(parent, false);
            var bgRect = barBg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.22f, yMin + 0.04f);
            bgRect.anchorMax = new Vector2(0.85f, yMax - 0.04f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            barBg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f);

            // Bar fill (앵커 기반)
            var fill = new GameObject($"{label}Fill");
            fill.transform.SetParent(barBg.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            bar = fill.AddComponent<Image>();
            bar.color = barColor;

            // Value
            valueText = CreateText(parent, $"{label}Value", "100",
                22, FontStyle.Bold, TextWhite, TextAnchor.MiddleRight);
            valueText.GetComponent<RectTransform>().SetAnchors(0.87f, yMin, 0.97f, yMax);
        }

        private void BuildContractCard(Transform parent)
        {
            var card = CreateLayoutCard(parent, "ContractCard", 90);

            CreateSectionLabel(card.transform, "계약 정보", 0.03f, 0.97f, 0.60f, 1f);

            contractText = CreateText(card.transform, "Contract",
                "월급: $2,000 | 승리보너스: $1,000 | 인기도: 20",
                20, FontStyle.Normal, TextGray, TextAnchor.MiddleLeft);
            var cRect = contractText.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.03f, 0.0f);
            cRect.anchorMax = new Vector2(0.97f, 0.58f);
            cRect.offsetMin = Vector2.zero;
            cRect.offsetMax = Vector2.zero;
        }

        private void CreateActionButtons(Transform parent)
        {
            var bar = CreatePanel(parent, "ActionBar",
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 80));
            bar.AddComponent<Image>().color = HeaderColor;

            // Train button
            var trainObj = new GameObject("TrainBtn");
            trainObj.transform.SetParent(bar.transform, false);
            var trainRect = trainObj.AddComponent<RectTransform>();
            trainRect.anchorMin = new Vector2(0.05f, 0.12f);
            trainRect.anchorMax = new Vector2(0.48f, 0.88f);
            trainRect.offsetMin = Vector2.zero;
            trainRect.offsetMax = Vector2.zero;

            trainObj.AddComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f);
            var trainBtn = trainObj.AddComponent<Button>();
            trainBtn.onClick.AddListener(() => Debug.Log("[FighterDetail] 훈련 - 준비 중"));
            CreateText(trainObj.transform, "Label", "훈련",
                26, FontStyle.Bold, TextWhite, TextAnchor.MiddleCenter);

            // Match button
            var matchObj = new GameObject("MatchBtn");
            matchObj.transform.SetParent(bar.transform, false);
            var matchRect = matchObj.AddComponent<RectTransform>();
            matchRect.anchorMin = new Vector2(0.52f, 0.12f);
            matchRect.anchorMax = new Vector2(0.95f, 0.88f);
            matchRect.offsetMin = Vector2.zero;
            matchRect.offsetMax = Vector2.zero;

            matchObj.AddComponent<Image>().color = new Color(0.9f, 0.3f, 0.2f);
            var matchBtn = matchObj.AddComponent<Button>();
            matchBtn.onClick.AddListener(() => Debug.Log("[FighterDetail] 대전 매칭 - 준비 중"));
            CreateText(matchObj.transform, "Label", "대전 매칭",
                26, FontStyle.Bold, TextWhite, TextAnchor.MiddleCenter);
        }

        #region Data Population

        private void PopulateData()
        {
            if (currentFighter == null) return;

            var f = currentFighter;

            // Profile
            nameText.text = f.FullName.ToUpper();
            infoText.text = $"{f.Age}세 | {GetWeightClassName(f.WeightClass)}";
            styleText.text = $"스타일: {GetFightingStyleName(f.PreferredStyle)}";
            winRateText.text = $"승률: {f.GetWinRate()}% ({f.TotalFights}전)";

            // OVR
            int ovr = f.Overall;
            ovrText.text = ovr.ToString();
            ovrText.color = GetStatColor(ovr);
            ovrBadgeBg.color = GetStatColor(ovr) * 0.4f;

            // Record
            recordText.text = $"{f.TotalFights}전 {f.Wins}승 {f.Losses}패 {f.Draws}무";
            recordDetailText.text = $"KO승 {f.KnockoutWins} | 서브미션승 {f.SubmissionWins} | 판정승 {f.DecisionWins}";

            // Stat bars
            int[] stats = {
                f.STR, f.TEC, f.SPD, f.STA, f.DEF, f.WREST, f.BJJ, f.POT
            };
            for (int i = 0; i < 8; i++)
            {
                SetBarFill(statBars[i], stats[i] / 100f);
                statBars[i].color = GetStatColor(stats[i]);
                statValueTexts[i].text = stats[i].ToString();
                statValueTexts[i].color = GetStatColor(stats[i]);
            }

            // Condition
            SetBarFill(healthBar, f.Health / 100f);
            healthValueText.text = f.Health.ToString();
            SetBarFill(fatigueBar, f.Fatigue / 100f);
            fatigueValueText.text = f.Fatigue.ToString();
            SetBarFill(conditionBar, f.Condition / 100f);
            conditionValueText.text = f.Condition.ToString();

            // Contract
            contractText.text = $"월급: ${f.MonthlySalary:N0} | 승리보너스: ${f.WinBonus:N0} | 인기도: {f.Popularity}";

            // Portrait (UMA)
            LoadPortrait(f);
        }

        private void LoadPortrait(Fighter fighter)
        {
            var renderer = FighterPortraitRenderer.Instance;
            if (renderer == null) return;

            // Check cached portrait first
            var cached = renderer.GetPortrait(fighter);
            if (cached != null)
            {
                ApplyPortraitTexture(cached);
                return;
            }

            // Request async render
            renderer.RenderPortraitAsync(fighter, (tex) =>
            {
                if (tex != null && currentFighter != null && currentFighter.FighterId == fighter.FighterId)
                    ApplyPortraitTexture(tex);
            });
        }

        private void ApplyPortraitTexture(Texture2D tex)
        {
            if (portraitImage == null || tex == null) return;

            var sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
            portraitImage.sprite = sprite;
            portraitImage.color = Color.white;

            // Remove placeholder text
            var placeholderText = portraitImage.GetComponentInChildren<Text>();
            if (placeholderText != null)
                placeholderText.text = "";
        }

        #endregion

        #region Helpers

        private GameObject CreateLayoutCard(Transform parent, string name, float height)
        {
            var card = new GameObject(name);
            card.transform.SetParent(parent, false);
            card.AddComponent<RectTransform>();
            card.AddComponent<Image>().color = CardColor;
            card.AddComponent<LayoutElement>().preferredHeight = height;
            return card;
        }

        private void SetBarFill(Image bar, float ratio)
        {
            var rect = bar.GetComponent<RectTransform>();
            rect.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
        }

        private void CreateSectionLabel(Transform parent, string text,
            float xMin, float xMax, float yMin, float yMax)
        {
            var label = CreateText(parent, $"Section_{text}", text,
                20, FontStyle.Bold, GoldColor, TextAnchor.MiddleLeft);
            var rect = label.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private string GetWeightClassName(WeightClass wc)
        {
            return wc switch
            {
                WeightClass.Flyweight => "플라이급",
                WeightClass.Bantamweight => "밴텀급",
                WeightClass.Featherweight => "페더급",
                WeightClass.Lightweight => "라이트급",
                WeightClass.Welterweight => "웰터급",
                WeightClass.Middleweight => "미들급",
                WeightClass.LightHeavyweight => "라이트헤비급",
                WeightClass.Heavyweight => "헤비급",
                _ => "?"
            };
        }

        private string GetFightingStyleName(FightingStyle style)
        {
            return style switch
            {
                FightingStyle.Striker => "타격형",
                FightingStyle.Grappler => "그래플링",
                FightingStyle.Wrestler => "레슬링",
                FightingStyle.Balanced => "밸런스형",
                FightingStyle.CounterFighter => "카운터형",
                FightingStyle.PressureFighter => "프레셔형",
                _ => "?"
            };
        }

        #endregion
    }

    // RectTransform extension for cleaner anchor setting
    public static class RectTransformExtensions
    {
        public static void SetAnchors(this RectTransform rect, float xMin, float yMin, float xMax, float yMax)
        {
            rect.anchorMin = new Vector2(xMin, yMin);
            rect.anchorMax = new Vector2(xMax, yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MMAManager.Models;
using MMAManager.Systems;

namespace MMAManager.UI
{
    public class FighterListScreen : UIScreen
    {
        private Transform contentParent;
        private ScrollRect listScrollRect;
        private List<GameObject> fighterCards = new List<GameObject>();
        private int selectedWeightFilter = -1; // -1 = all
        private Image[] filterBtnImages;

        void Awake()
        {
            InitCanvas("FighterList", 11);
            BuildUI();
        }

        protected override void OnShow()
        {
            RefreshList();
        }

        protected override void BuildUI()
        {
            // Full background
            var bg = CreateFullPanel(transform, "Background");
            bg.AddComponent<Image>().color = BgColor;

            // Header (top 60px)
            CreateHeader(transform);

            // Filter tabs (below header, 50px)
            CreateFilterTabs(transform);

            // Scrollable fighter list
            CreateScrollArea(transform);

            // Bottom add button (70px)
            CreateBottomBar(transform);
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
            var title = CreateText(header.transform, "Title", "선수 관리",
                30, FontStyle.Bold, TextWhite, TextAnchor.MiddleCenter);
            var tRect = title.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.15f, 0);
            tRect.anchorMax = new Vector2(0.85f, 1);
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
        }

        private void CreateFilterTabs(Transform parent)
        {
            // Container
            var container = CreatePanel(parent, "FilterContainer",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(0, 50));
            container.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Horizontal scroll
            var scrollObj = CreateFullPanel(container.transform, "FilterScroll");
            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = true;
            scrollRect.vertical = false;

            // Viewport with RectMask2D (more reliable than Mask for scrolling)
            var viewport = CreateFullPanel(scrollObj.transform, "Viewport");
            viewport.AddComponent<RectMask2D>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            // Content
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(0, 1);
            contentRect.pivot = new Vector2(0, 0.5f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var hlg = content.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var csf = content.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;

            // Filter buttons: "전체" + each weight class
            string[] labels = { "전체", "플라이", "밴텀", "페더",
                "라이트", "웰터", "미들", "L헤비", "헤비" };

            filterBtnImages = new Image[labels.Length];

            for (int i = 0; i < labels.Length; i++)
            {
                var btnObj = new GameObject($"Filter_{i}");
                btnObj.transform.SetParent(content.transform, false);
                btnObj.AddComponent<RectTransform>();

                var btnImg = btnObj.AddComponent<Image>();
                btnImg.color = (i == 0) ? GoldColor : CardColor;
                filterBtnImages[i] = btnImg;

                var le = btnObj.AddComponent<LayoutElement>();
                le.preferredWidth = 100;
                le.minWidth = 80;

                var btn = btnObj.AddComponent<Button>();
                btn.targetGraphic = btnImg;
                int idx = i;
                btn.onClick.AddListener(() => OnFilterClicked(idx));

                Color textColor = (i == 0) ? Color.black : TextWhite;
                CreateText(btnObj.transform, "Label", labels[i],
                    20, FontStyle.Bold, textColor, TextAnchor.MiddleCenter);
            }
        }

        private void OnFilterClicked(int index)
        {
            selectedWeightFilter = (index == 0) ? -1 : index - 1;

            for (int i = 0; i < filterBtnImages.Length; i++)
            {
                bool selected = (i == index);
                filterBtnImages[i].color = selected ? GoldColor : CardColor;

                var label = filterBtnImages[i].GetComponentInChildren<Text>();
                if (label != null)
                    label.color = selected ? Color.black : TextWhite;
            }

            RefreshList();
        }

        private void CreateScrollArea(Transform parent)
        {
            // Scroll area between filter and bottom bar
            var scrollObj = CreatePanel(parent, "ListScroll",
                new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var scrollObjRect = scrollObj.GetComponent<RectTransform>();
            scrollObjRect.offsetMin = new Vector2(0, 70);   // bottom bar
            scrollObjRect.offsetMax = new Vector2(0, -110);  // header + filter

            listScrollRect = scrollObj.AddComponent<ScrollRect>();
            listScrollRect.horizontal = false;
            listScrollRect.vertical = true;
            listScrollRect.movementType = ScrollRect.MovementType.Clamped;

            // Viewport with RectMask2D
            var viewport = CreateFullPanel(scrollObj.transform, "Viewport");
            viewport.AddComponent<RectMask2D>();
            listScrollRect.viewport = viewport.GetComponent<RectTransform>();

            // Content
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = Vector2.zero;

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8;
            vlg.padding = new RectOffset(15, 15, 10, 10);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            listScrollRect.content = contentRect;
            contentParent = content.transform;
        }

        private void CreateBottomBar(Transform parent)
        {
            var bar = CreatePanel(parent, "BottomBar",
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 70));
            bar.AddComponent<Image>().color = HeaderColor;

            // Add fighter button
            var btnObj = new GameObject("AddFighterBtn");
            btnObj.transform.SetParent(bar.transform, false);
            var btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.15f, 0.12f);
            btnRect.anchorMax = new Vector2(0.85f, 0.88f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            btnObj.AddComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f);
            var btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(OnAddRandomFighter);

            CreateText(btnObj.transform, "Label", "+ 랜덤 선수 생성",
                26, FontStyle.Bold, TextWhite, TextAnchor.MiddleCenter);
        }

        #region Data

        public void RefreshList()
        {
            // Clear existing cards
            foreach (var card in fighterCards)
            {
                if (card != null) Destroy(card);
            }
            fighterCards.Clear();

            // Get fighters
            var db = FighterDatabase.Instance;
            if (db == null)
            {
                Debug.LogWarning("[FighterListScreen] FighterDatabase not found");
                return;
            }

            List<Fighter> fighters;
            if (selectedWeightFilter < 0)
                fighters = db.GetAllFighters();
            else
                fighters = db.GetFightersByWeightClass((WeightClass)selectedWeightFilter);

            // Sort by overall
            fighters.Sort((a, b) => b.Overall.CompareTo(a.Overall));

            Debug.Log($"[FighterList] RefreshList: {fighters.Count} fighters");

            // Create cards
            foreach (var fighter in fighters)
            {
                CreateFighterCard(fighter);
            }

            // Empty state
            if (fighters.Count == 0)
            {
                var emptyObj = new GameObject("EmptyText");
                emptyObj.transform.SetParent(contentParent, false);
                emptyObj.AddComponent<RectTransform>();
                emptyObj.AddComponent<Image>().color = Color.clear;
                emptyObj.AddComponent<LayoutElement>().preferredHeight = 200;
                CreateText(emptyObj.transform, "Label",
                    "등록된 선수가 없습니다\n아래 버튼으로 선수를 생성하세요",
                    24, FontStyle.Normal, TextGray, TextAnchor.MiddleCenter);
                fighterCards.Add(emptyObj);
            }

            // Force layout rebuild
            Canvas.ForceUpdateCanvases();
            var cRect = contentParent.GetComponent<RectTransform>();
            if (cRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(cRect);

            // Reset scroll to top
            if (listScrollRect != null)
                listScrollRect.normalizedPosition = new Vector2(0, 1);
        }

        private void CreateFighterCard(Fighter fighter)
        {
            var card = new GameObject($"Card_{fighter.FirstName}");
            card.transform.SetParent(contentParent, false);
            var cardRect = card.AddComponent<RectTransform>();

            var cardImg = card.AddComponent<Image>();
            cardImg.color = CardColor;

            card.AddComponent<LayoutElement>().preferredHeight = 90;

            // Make entire card clickable
            var btn = card.AddComponent<Button>();
            btn.targetGraphic = cardImg;
            var fRef = fighter;
            btn.onClick.AddListener(() => OnFighterCardClicked(fRef));

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            btn.colors = colors;

            // Row 1: OVR badge + Name + Age
            // OVR badge (left)
            var ovrObj = new GameObject("OVR");
            ovrObj.transform.SetParent(card.transform, false);
            var ovrRect = ovrObj.AddComponent<RectTransform>();
            ovrRect.anchorMin = new Vector2(0.02f, 0.5f);
            ovrRect.anchorMax = new Vector2(0.15f, 0.95f);
            ovrRect.offsetMin = Vector2.zero;
            ovrRect.offsetMax = Vector2.zero;

            ovrObj.AddComponent<Image>().color = GetStatColor(fighter.Overall);
            CreateText(ovrObj.transform, "OVRText", fighter.Overall.ToString(),
                26, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);

            // Name
            var nameText = CreateText(card.transform, "Name",
                $"{fighter.FirstName[0]}.{fighter.LastName}",
                26, FontStyle.Bold, TextWhite, TextAnchor.MiddleLeft);
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.17f, 0.5f);
            nameRect.anchorMax = new Vector2(0.7f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Age
            var ageText = CreateText(card.transform, "Age", $"{fighter.Age}세",
                22, FontStyle.Normal, TextGray, TextAnchor.MiddleRight);
            var ageRect = ageText.GetComponent<RectTransform>();
            ageRect.anchorMin = new Vector2(0.75f, 0.5f);
            ageRect.anchorMax = new Vector2(0.98f, 0.95f);
            ageRect.offsetMin = Vector2.zero;
            ageRect.offsetMax = Vector2.zero;

            // Row 2: Weight class + Record + Top stat
            string wcName = GetWeightClassName(fighter.WeightClass);
            var infoText = CreateText(card.transform, "Info",
                $"{wcName}  {fighter.Wins}W-{fighter.Losses}L-{fighter.Draws}D",
                20, FontStyle.Normal, TextGray, TextAnchor.MiddleLeft);
            var infoRect = infoText.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0.04f, 0.05f);
            infoRect.anchorMax = new Vector2(0.65f, 0.48f);
            infoRect.offsetMin = Vector2.zero;
            infoRect.offsetMax = Vector2.zero;

            // Style tag
            string styleName = GetStyleName(fighter);
            var styleText = CreateText(card.transform, "Style", styleName,
                18, FontStyle.Normal, GoldColor, TextAnchor.MiddleRight);
            var styleRect = styleText.GetComponent<RectTransform>();
            styleRect.anchorMin = new Vector2(0.65f, 0.05f);
            styleRect.anchorMax = new Vector2(0.98f, 0.48f);
            styleRect.offsetMin = Vector2.zero;
            styleRect.offsetMax = Vector2.zero;

            fighterCards.Add(card);
        }

        private void OnFighterCardClicked(Fighter fighter)
        {
            var detail = UIManager.Instance?.GetScreen<FighterDetailScreen>("FighterDetail");
            if (detail != null)
            {
                detail.SetFighter(fighter);
                UIManager.Instance.ShowScreen("FighterDetail");
            }
        }

        private void OnAddRandomFighter()
        {
            var db = FighterDatabase.Instance;
            if (db == null) return;

            // Random weight class
            var wc = (WeightClass)Random.Range(0, System.Enum.GetValues(typeof(WeightClass)).Length);
            var fighter = db.GenerateRandomFighter(wc);
            fighter.RandomizeStats();
            db.AddFighter(fighter);

            RefreshList();
            Debug.Log($"[FighterList] 새 선수 추가: {fighter.FullName} ({wc})");
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
                WeightClass.LightHeavyweight => "L헤비급",
                WeightClass.Heavyweight => "헤비급",
                _ => "?"
            };
        }

        private string GetStyleName(Fighter f)
        {
            // Determine dominant style from stats
            if (f.STR >= f.TEC && f.STR >= f.WREST && f.STR >= f.BJJ) return "타격형";
            if (f.TEC >= f.STR && f.TEC >= f.WREST) return "기술형";
            if (f.WREST >= f.BJJ) return "레슬링";
            return "그래플링";
        }

        #endregion
    }
}

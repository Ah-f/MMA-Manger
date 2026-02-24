using UnityEngine;
using UnityEngine.UI;

namespace MMAManager.UI
{
    public class MainMenuScreen : UIScreen
    {
        void Awake()
        {
            InitCanvas("MainMenu", 10);
            BuildUI();
        }

        protected override void BuildUI()
        {
            // Full screen background
            var bg = CreateFullPanel(transform, "Background");
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = BgColor;

            // Title area (top 35%)
            var titleArea = CreatePanel(transform, "TitleArea",
                new Vector2(0, 0.55f), new Vector2(1, 1),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            // Main title
            var title = CreateText(titleArea.transform, "Title", "MMA MANAGER",
                72, FontStyle.Bold, TextWhite, TextAnchor.MiddleCenter);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.35f);
            titleRect.anchorMax = new Vector2(0.9f, 0.65f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            var titleOutline = title.gameObject.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0, 0, 0, 0.8f);
            titleOutline.effectDistance = new Vector2(3, -3);

            // Subtitle
            var subtitle = CreateText(titleArea.transform, "Subtitle", "격투기 매니저",
                32, FontStyle.Normal, GoldColor, TextAnchor.MiddleCenter);
            var subRect = subtitle.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.2f, 0.2f);
            subRect.anchorMax = new Vector2(0.8f, 0.35f);
            subRect.offsetMin = Vector2.zero;
            subRect.offsetMax = Vector2.zero;

            // Menu buttons area (middle)
            var menuArea = CreatePanel(transform, "MenuArea",
                new Vector2(0.12f, 0.15f), new Vector2(0.88f, 0.52f),
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            CreateMenuButton(menuArea.transform, "CareerBtn", "커리어 모드",
                new Vector2(0, 0.76f), new Vector2(1, 0.98f),
                () => Debug.Log("[MainMenu] 커리어 모드 - 준비 중"));

            CreateMenuButton(menuArea.transform, "QuickMatchBtn", "빠른 대전",
                new Vector2(0, 0.51f), new Vector2(1, 0.73f),
                () => Debug.Log("[MainMenu] 빠른 대전 - 준비 중"));

            CreateMenuButton(menuArea.transform, "FighterMgmtBtn", "선수 관리",
                new Vector2(0, 0.26f), new Vector2(1, 0.48f),
                () => UIManager.Instance?.ShowScreen("FighterList"));

            CreateMenuButton(menuArea.transform, "SettingsBtn", "설정",
                new Vector2(0, 0.01f), new Vector2(1, 0.23f),
                () => Debug.Log("[MainMenu] 설정 - 준비 중"));

            // Version text (bottom)
            var version = CreateText(transform, "Version", "v0.1 Alpha",
                20, FontStyle.Normal, TextGray, TextAnchor.MiddleCenter);
            var verRect = version.GetComponent<RectTransform>();
            verRect.anchorMin = new Vector2(0, 0.02f);
            verRect.anchorMax = new Vector2(1, 0.06f);
            verRect.offsetMin = Vector2.zero;
            verRect.offsetMax = Vector2.zero;
        }

        private void CreateMenuButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, System.Action onClick)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = btnObj.AddComponent<Image>();
            img.color = ButtonColor;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick());

            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
            btn.colors = colors;

            CreateText(btnObj.transform, "Label", label,
                36, FontStyle.Bold, TextWhite, TextAnchor.MiddleCenter);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace mod_menu
{
    public partial class ModMenuBehaviour
    {
        // ── Tab state ─────────────────────────────────────────────────────
        private int _activeTab = 0;
        private GameObject[] _tabPages;
        private Image[] _tabButtonImages;

        private static readonly Color ColAccent = new(0.85f, 0.15f, 0.15f, 1f);
        private static readonly Color ColDark = new(0.07f, 0.07f, 0.07f, 0.97f);
        private static readonly Color ColPanel = new(0.11f, 0.11f, 0.11f, 1f);
        private static readonly Color ColBtn = new(0.17f, 0.17f, 0.17f, 1f);
        private static readonly Color ColBtnOn = new(0.50f, 0.07f, 0.07f, 1f);
        private static readonly Color ColTab = new(0.13f, 0.13f, 0.13f, 1f);
        private static readonly Color ColTabOn = new(0.85f, 0.15f, 0.15f, 1f);
        private static readonly Color ColText = new(0.92f, 0.92f, 0.92f, 1f);
        private static readonly Color ColSub = new(0.50f, 0.50f, 0.50f, 1f);

        private const float MenuW = 320f;
        private const float MenuH = 480f;
        private const float HeaderH = 72f;
        private const float TabH = 34f;

        // ─────────────────────────────────────────────────────────────────

        private void BuildUI()
        {
            _menuRoot = new GameObject("ModMenu_Root");
            Object.DontDestroyOnLoad(_menuRoot);

            var canvas = _menuRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            _menuRoot.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _menuRoot.AddComponent<GraphicRaycaster>();
            
            // Ensure EventSystem exists for UI input to work
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Object.DontDestroyOnLoad(eventSystem);
            }

            // ── Root panel ────────────────────────────────────────────────
            var root = CreatePanel(_menuRoot, "Panel",
                new Vector2(MenuW, MenuH), new Vector2(20, -20), ColDark);

            // ── Red top bar ───────────────────────────────────────────────
            CreateSolidRect(root, "TopBar", new Vector2(MenuW, 3f),
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), ColAccent);

            // ── Title ─────────────────────────────────────────────────────
            CreateSimpleLabel(root, "Title", "Blood Menu V1.0",
                new Vector2(0f, -16f), 14, ColText, FontStyle.Bold, TextAnchor.MiddleCenter);
            CreateSimpleLabel(root, "Game", "Created by Wambo420Rambo",
                new Vector2(0f, -34f), 9, ColAccent, FontStyle.Normal, TextAnchor.MiddleCenter);

            // ── Tab bar ───────────────────────────────────────────────────
            string[] tabs = { "AIMBOT", "PLAYER", "ECONOMY", "MISC" };
            _tabPages = new GameObject[tabs.Length];
            _tabButtonImages = new Image[tabs.Length];

            float tabW = MenuW / tabs.Length;
            for (int i = 0; i < tabs.Length; i++)
            {
                int idx = i;

                var t = new GameObject($"Tab{i}");
                t.transform.SetParent(root.transform, false);
                var tr = t.AddComponent<RectTransform>();
                tr.anchorMin = new Vector2(0f, 1f);
                tr.anchorMax = new Vector2(0f, 1f);
                tr.pivot = new Vector2(0f, 1f);
                tr.sizeDelta = new Vector2(tabW - 1f, TabH);
                tr.anchoredPosition = new Vector2(tabW * i, -HeaderH);

                var ti = t.AddComponent<Image>();
                ti.color = i == 0 ? ColTabOn : ColTab;
                _tabButtonImages[i] = ti;

                var tb = t.AddComponent<Button>();
                tb.targetGraphic = ti;
                tb.onClick.AddListener(new System.Action(() => SwitchTab(idx)));

                var tl = new GameObject("L");
                tl.transform.SetParent(t.transform, false);
                var tlr = tl.AddComponent<RectTransform>();
                tlr.anchorMin = Vector2.zero;
                tlr.anchorMax = Vector2.one;
                tlr.sizeDelta = Vector2.zero;
                var tlt = tl.AddComponent<Text>();
                tlt.text = tabs[i];
                tlt.fontSize = 9;
                tlt.fontStyle = FontStyle.Bold;
                tlt.color = Color.white;
                tlt.alignment = TextAnchor.MiddleCenter;
                tlt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            // ── Separator ─────────────────────────────────────────────────
            CreateSolidRect(root, "Sep", new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -(HeaderH + TabH)),
                new Color(0.22f, 0.04f, 0.04f, 1f));

            // ── Pages ─────────────────────────────────────────────────────
            float pageY = -(HeaderH + TabH + 2f);
            float pageH = MenuH - HeaderH - TabH - 4f;

            _tabPages[0] = BuildAimbotPage(root, pageY, pageH);
            _tabPages[1] = BuildPlayerPage(root, pageY, pageH);
            _tabPages[2] = BuildEconomyPage(root, pageY, pageH);
            _tabPages[3] = BuildMiscPage(root, pageY, pageH);

            for (int i = 1; i < _tabPages.Length; i++)
                _tabPages[i].SetActive(false);

            _menuRoot.SetActive(false);
        }

        // ── Tab switching ─────────────────────────────────────────────────

        private void SwitchTab(int idx)
        {
            for (int i = 0; i < _tabPages.Length; i++)
            {
                _tabPages[i].SetActive(i == idx);
                _tabButtonImages[i].color = i == idx ? ColTabOn : ColTab;
            }
            _activeTab = idx;
        }

        // ── Page builders ─────────────────────────────────────────────────

        private GameObject BuildAimbotPage(GameObject root, float pageY, float pageH)
        {
            var page = MakePage(root, "PageAimbot", pageY, pageH);
            float y = -10f;

            SectionLabel(page, "TARGETING", ref y);
            ToggleRow(page, "BtnAimbot", "Aimbot", ref y, state =>
            {
                _aimbotEnabled = state;
                _smoothingSectionGo.SetActive(state);
                _smoothingSlider.SetActive(state);
                _smoothingLabelGo.SetActive(state);
                _radiusSectionGo.SetActive(state);
                _radiusSlider.SetActive(state);
                _radiusLabelGo.SetActive(state);
            });

            y -= 4f;
            _smoothingSectionGo = SectionLabel(page, "AIM SMOOTHING", ref y);
            _smoothingSectionGo.SetActive(false);

            _smoothingLabelGo = CreateSimpleLabel(page, "SmoothVal",
                $"{_aimSmoothing:F0}", new Vector2(0f, y), 11,
                ColAccent, FontStyle.Bold, TextAnchor.MiddleCenter).gameObject;
            y -= 20f;

            _smoothingSlider = CreateSlider(page, "SliderSmooth",
                new Vector2(0f, y - 10), 270f, SmoothingMin, SmoothingMax, _aimSmoothing, val =>
                {
                    _aimSmoothing = val;
                    var l = _menuRoot.transform
                        .Find("Panel/PageAimbot/SmoothVal")?.GetComponent<Text>();
                    if (l != null) l.text = $"{val:F0}";
                });
            y -= 38f;
            _smoothingSlider.SetActive(false);
            _smoothingLabelGo.SetActive(false);

            y -= 6f;
            _radiusSectionGo = SectionLabel(page, "AIM CIRCLE RADIUS", ref y);
            _radiusSectionGo.SetActive(false);

            var radiusLabel = CreateSimpleLabel(page, "RadiusVal",
                $"{_circleRadius:F0}px", new Vector2(0f, y), 11,
                ColAccent, FontStyle.Bold, TextAnchor.MiddleCenter);
            _radiusLabelGo = radiusLabel.gameObject;
            y -= 20f;

            _radiusSlider = CreateSlider(page, "SliderRadius",
                new Vector2(0f, y - 10), 270f, 50f, 400f, _circleRadius, val =>
                {
                    _circleRadius = val;
                    radiusLabel.text = $"{val:F0}px";
                });
            y -= 38f;
            _radiusSlider.SetActive(false);
            _radiusLabelGo.SetActive(false);

            return page;
        }

        private GameObject BuildPlayerPage(GameObject root, float pageY, float pageH)
        {
            var page = MakePage(root, "PagePlayer", pageY, pageH);
            float y = -10f;

            SectionLabel(page, "MOVEMENT", ref y);
            ToggleRow(page, "BtnBhop", "Bunny Hop", ref y, SetBunnyhop);

            y -= 6f;
            SectionLabel(page, "HEALTH", ref y);
            ToggleRow(page, "BtnImmortal", "Immortality", ref y, state => _immortalEnabled = state);
            ActionRow(page, "BtnSetHP", "Set Health to 100", ref y, () => SetHealth(100f));

            return page;
        }


        private GameObject BuildEconomyPage(GameObject root, float pageY, float pageH)
        {
            var page = MakePage(root, "PageEconomy", pageY, pageH);
            float y = -10f;

            SectionLabel(page, "CURRENCY", ref y);
            ActionRow(page, "BtnRevives", "+ 99 Revives", ref y, () => AddRevives(99));
            ActionRow(page, "BtnRerolls", "+ 99 Re-Rolls", ref y, () => AddReRolls(99));
            ActionRow(page, "BtnAways", "+ 99 Aways", ref y, () => AddAways(99));
            ActionRow(page, "BtnHolds", "+ 99 Holds", ref y, () => AddHolds(99));
            ActionRow(page, "BtnTickets", "+ 99 Tickets", ref y, () => AddTickets(99));
            ActionRow(page, "BtnSuper", "+ 99 Super Tickets", ref y, () => AddSuperTickets(99));
            ActionRow(page, "BtnCoins", "+ 100,000 Coins", ref y, () => AddCoins(100000f));

            y -= 4f;
            // Highlighted "add all" button
            ActionRow(page, "BtnAll", "▶  ADD EVERYTHING", ref y, () => AddAllCurrencies(),
                new Color(0.45f, 0.06f, 0.06f, 1f));

            return page;
        }

        private GameObject BuildMiscPage(GameObject root, float pageY, float pageH)
        {
            var page = MakePage(root, "PageMisc", pageY, pageH);
            float y = -10f;

            SectionLabel(page, "UNLOCKS", ref y);
            ActionRow(page, "BtnUnlockAll", "Unlock All", ref y, () => UnlockAll());
            ActionRow(page, "BtnAchievments", "Set All Achievements", ref y, () => SetAllAchievement());

            y -= 6f;
            SectionLabel(page, "SAVE", ref y);
            ActionRow(page, "BtnSave", "Force Save", ref y, () => ForceSave(),
                new Color(0.06f, 0.25f, 0.06f, 1f));

            return page;
        }

        // ── Row widgets ───────────────────────────────────────────────────

        // Change SectionLabel to return GameObject
        private GameObject SectionLabel(GameObject page, string text, ref float y)
        {
            var container = new GameObject($"Sec_{text}");
            container.transform.SetParent(page.transform, false);
            var cr = container.AddComponent<RectTransform>();
            cr.anchorMin = new Vector2(0f, 1f);
            cr.anchorMax = new Vector2(1f, 1f);
            cr.pivot = new Vector2(0.5f, 1f);
            cr.sizeDelta = new Vector2(0f, 27f);
            cr.anchoredPosition = new Vector2(0f, y);

            // Label
            var lgo = new GameObject("Text");
            lgo.transform.SetParent(container.transform, false);
            var lr = lgo.AddComponent<RectTransform>();
            lr.anchorMin = new Vector2(0f, 1f);
            lr.anchorMax = new Vector2(1f, 1f);
            lr.pivot = new Vector2(0.5f, 1f);
            lr.sizeDelta = new Vector2(-20f, 18f);
            lr.anchoredPosition = new Vector2(0f, 0f);
            var lt = lgo.AddComponent<Text>();
            lt.text = text;
            lt.fontSize = 9;
            lt.fontStyle = FontStyle.Bold;
            lt.color = ColAccent;
            lt.alignment = TextAnchor.MiddleLeft;
            lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Underline
            var line = new GameObject("Line");
            line.transform.SetParent(container.transform, false);
            var llr = line.AddComponent<RectTransform>();
            llr.anchorMin = new Vector2(0f, 1f);
            llr.anchorMax = new Vector2(1f, 1f);
            llr.pivot = new Vector2(0.5f, 1f);
            llr.sizeDelta = new Vector2(-20f, 1f);
            llr.anchoredPosition = new Vector2(0f, -20f);
            line.AddComponent<Image>().color = new Color(0.3f, 0.05f, 0.05f, 1f);

            y -= 27f;
            return container;
        }

        private void ToggleRow(GameObject page, string name, string label,
            ref float y, System.Action<bool> onToggle)
        {
            bool state = false;
            var go = new GameObject(name);
            go.transform.SetParent(page.transform, false);

            var r = go.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.sizeDelta = new Vector2(-20f, 32f);
            r.anchoredPosition = new Vector2(0f, y);

            var bg = go.AddComponent<Image>();
            bg.color = ColBtn;

            // Left red pip
            var pip = new GameObject("Pip");
            pip.transform.SetParent(go.transform, false);
            var pr = pip.AddComponent<RectTransform>();
            pr.anchorMin = new Vector2(0f, 0f);
            pr.anchorMax = new Vector2(0f, 1f);
            pr.pivot = new Vector2(0f, 0.5f);
            pr.sizeDelta = new Vector2(3f, 0f);
            pr.anchoredPosition = Vector2.zero;
            pip.AddComponent<Image>().color = ColAccent;

            // Label
            var lgo = new GameObject("Label");
            lgo.transform.SetParent(go.transform, false);
            var lr = lgo.AddComponent<RectTransform>();
            lr.anchorMin = new Vector2(0f, 0f);
            lr.anchorMax = new Vector2(0.68f, 1f);
            lr.pivot = new Vector2(0f, 0.5f);
            lr.offsetMin = new Vector2(10f, 0f);
            lr.offsetMax = new Vector2(0f, 0f);
            var lt = lgo.AddComponent<Text>();
            lt.text = label;
            lt.fontSize = 12;
            lt.color = ColText;
            lt.alignment = TextAnchor.MiddleLeft;
            lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Badge
            var bgo = new GameObject("Badge");
            bgo.transform.SetParent(go.transform, false);
            var br = bgo.AddComponent<RectTransform>();
            br.anchorMin = new Vector2(1f, 0.5f);
            br.anchorMax = new Vector2(1f, 0.5f);
            br.pivot = new Vector2(1f, 0.5f);
            br.sizeDelta = new Vector2(40f, 16f);
            br.anchoredPosition = new Vector2(-8f, 0f);
            var bi = bgo.AddComponent<Image>();
            bi.color = new Color(0.18f, 0.18f, 0.18f, 1f);

            var btgo = new GameObject("T");
            btgo.transform.SetParent(bgo.transform, false);
            var btr = btgo.AddComponent<RectTransform>();
            btr.anchorMin = Vector2.zero;
            btr.anchorMax = Vector2.one;
            btr.sizeDelta = Vector2.zero;
            var btt = btgo.AddComponent<Text>();
            btt.text = "OFF";
            btt.fontSize = 9;
            btt.fontStyle = FontStyle.Bold;
            btt.color = ColSub;
            btt.alignment = TextAnchor.MiddleCenter;
            btt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(new System.Action(() =>
            {
                state = !state;
                bg.color = state ? ColBtnOn : ColBtn;
                btt.text = state ? "ON" : "OFF";
                btt.color = state ? ColAccent : ColSub;
                bi.color = state
                    ? new Color(0.35f, 0.04f, 0.04f, 1f)
                    : new Color(0.18f, 0.18f, 0.18f, 1f);
                onToggle(state);
            }));

            y -= 36f;
        }

        private void ActionRow(GameObject page, string name, string label,
            ref float y, System.Action onClick, Color? bgOverride = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(page.transform, false);

            var r = go.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.sizeDelta = new Vector2(-20f, 30f);
            r.anchoredPosition = new Vector2(0f, y);

            var bg = go.AddComponent<Image>();
            bg.color = bgOverride ?? ColBtn;

            // Arrow
            var ago = new GameObject("Arrow");
            ago.transform.SetParent(go.transform, false);
            var ar = ago.AddComponent<RectTransform>();
            ar.anchorMin = new Vector2(1f, 0.5f);
            ar.anchorMax = new Vector2(1f, 0.5f);
            ar.pivot = new Vector2(1f, 0.5f);
            ar.sizeDelta = new Vector2(20f, 20f);
            ar.anchoredPosition = new Vector2(-6f, 0f);
            var at = ago.AddComponent<Text>();
            at.text = "›";
            at.fontSize = 16;
            at.color = bgOverride.HasValue ? Color.white : ColAccent;
            at.alignment = TextAnchor.MiddleCenter;
            at.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Label
            var lgo = new GameObject("Label");
            lgo.transform.SetParent(go.transform, false);
            var lr = lgo.AddComponent<RectTransform>();
            lr.anchorMin = new Vector2(0f, 0f);
            lr.anchorMax = new Vector2(1f, 1f);
            lr.pivot = new Vector2(0.5f, 0.5f);
            lr.offsetMin = new Vector2(10f, 0f);
            lr.offsetMax = new Vector2(-28f, 0f);
            var lt = lgo.AddComponent<Text>();
            lt.text = label;
            lt.fontSize = 11;
            lt.color = bgOverride.HasValue ? Color.white : ColText;
            lt.alignment = TextAnchor.MiddleLeft;
            lt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(new System.Action(onClick));

            y -= 34f;
        }

        // ── Low-level ─────────────────────────────────────────────────────

        private GameObject MakePage(GameObject root, string name, float pageY, float pageH)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 1f);
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            r.sizeDelta = new Vector2(0f, pageH);
            r.anchoredPosition = new Vector2(0f, pageY);
            go.AddComponent<Image>().color = ColPanel;
            return go;
        }

        private static void CreateSolidRect(GameObject parent, string name,
            Vector2 sizeDelta, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.pivot = new Vector2(0.5f, 1f);
            r.sizeDelta = sizeDelta;
            r.anchoredPosition = anchoredPos;
            go.AddComponent<Image>().color = color;
        }

        private void SetButtonLabel(string path, string text)
        {
            var lbl = _menuRoot.transform.Find($"{path}/Label")?.GetComponent<Text>();
            if (lbl == null) { Plugin.Log.LogWarning($"SetButtonLabel: '{path}/Label' not found."); return; }
            lbl.text = text;
        }
    }
}
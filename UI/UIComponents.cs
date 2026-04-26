using UnityEngine;
using UnityEngine.UI;

namespace mod_menu
{
    public partial class ModMenuBehaviour
    {
        // ── Texture factories ─────────────────────────────────────────────

        private void InitTextures()
        {
            _circleTex = CreateCircleTexture(200, Color.red, 3f);
        }

        private static Texture2D CreateCircleTexture(int size, Color color, float thickness)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float center = size / 2f;
            float outer = center;
            float inner = center - thickness;

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    pixels[y * size + x] = dist <= outer && dist >= inner ? color : Color.clear;
                }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        // ── Panel ─────────────────────────────────────────────────────────

        private GameObject CreatePanel(GameObject parent, string name,
            Vector2 size, Vector2 anchoredPos, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;

            go.AddComponent<Image>().color = color;
            return go;
        }

        // ── Label ─────────────────────────────────────────────────────────

        /// <summary>Full-option label. Returns the Text component.</summary>
        private Text CreateSimpleLabel(GameObject parent, string name, string text,
            Vector2 anchoredPos, int fontSize, Color color,
            FontStyle fontStyle, TextAnchor alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 20f);
            rect.anchoredPosition = anchoredPos;

            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.fontStyle = fontStyle;
            txt.color = color;
            txt.alignment = alignment;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return txt;
        }

        // ── Slider ────────────────────────────────────────────────────────

        private GameObject CreateSlider(GameObject parent, string name, Vector2 anchoredPos,
            float width, float minVal, float maxVal, float initialVal,
            System.Action<float> onValueChanged)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, 18f);
            rect.anchoredPosition = anchoredPos;

            // Background
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0.13f, 0.13f, 0.13f, 1f);

            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var far = fillArea.AddComponent<RectTransform>();
            far.anchorMin = new Vector2(0f, 0.25f);
            far.anchorMax = new Vector2(1f, 0.75f);
            far.offsetMin = new Vector2(5f, 0f);
            far.offsetMax = new Vector2(-15f, 0f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fr = fill.AddComponent<RectTransform>();
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = Vector2.one;
            fr.sizeDelta = new Vector2(10f, 0f);
            fill.AddComponent<Image>().color = new Color(0.75f, 0.15f, 0.15f, 1f);

            // Handle Slide Area
            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(go.transform, false);
            var har = handleArea.AddComponent<RectTransform>();
            har.anchorMin = Vector2.zero;
            har.anchorMax = Vector2.one;
            har.offsetMin = new Vector2(10f, 0f);
            har.offsetMax = new Vector2(-10f, 0f);

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var hr = handle.AddComponent<RectTransform>();
            hr.anchorMin = new Vector2(0f, 0f);
            hr.anchorMax = new Vector2(0f, 1f);
            hr.sizeDelta = new Vector2(18f, 0f);
            var hi = handle.AddComponent<Image>();
            hi.color = new Color(0.88f, 0.88f, 0.88f, 1f);

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fr;
            slider.handleRect = hr;
            slider.targetGraphic = hi;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = minVal;
            slider.maxValue = maxVal;
            slider.wholeNumbers = true;
            slider.value = initialVal;
            slider.onValueChanged.AddListener(new System.Action<float>(onValueChanged));

            return go;
        }
    }
}
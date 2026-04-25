using com8com1.SCFPS;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace mod_menu
{
    public class ModMenuBehaviour(System.IntPtr ptr) : MonoBehaviour(ptr)
    {
        // ── Constants ────────────────────────────────────────────────────
        private const KeyCode ToggleKey = KeyCode.M;
        private const float ScanInterval = 0.15f;  // enemy list refresh rate (seconds)
        private const float FovStep = 10f;
        private const float FovMin = 40f;
        private const float FovMax = 150f;

        // ── Menu ────────────────────────────────────────────────────────
        private GameObject _menuRoot;
        private bool _visible;

        // ── Aimbot ──────────────────────────────────────────────────────
        private bool _aimbotEnabled;
        private float _circleRadius = 150f;
        private float _aimSmoothing = 10f;
        private float _aimTargetOffset = 1.5f;
        private GameObject _currentTarget;
        private Texture2D _circleTex;

        // ── Cached references ────────────────────────────────────────────
        private GameObject _player;
        private Camera _cam;
        private Q3PlayerController _controller;

        // ── Enemy scan ───────────────────────────────────────────────────
        private List<GameObject> _cachedEnemies = new();
        private float _scanTimer;

        // ── UI live labels ───────────────────────────────────────────────
        private Text _enemyCountLabel;
        private Text _fovLabel;

        // ────────────────────────────────────────────────────────────────

        void Start()
        {
            CachePlayerComponents();
            _circleTex = CreateCircleTexture(200, Color.red, 3f);
            BuildUI();
        }

        void Update()
        {
            if (!EnsureReferences()) return;

            if (Input.GetKeyDown(ToggleKey))
                SetMenuVisible(!_visible);

            // Throttled enemy scan (avoids FindObjectsOfType every frame)
            _scanTimer -= Time.deltaTime;
            if (_scanTimer <= 0f)
            {
                _scanTimer = ScanInterval;
                _cachedEnemies = CollectAliveEnemies();

                if (_enemyCountLabel != null)
                    _enemyCountLabel.text = $"Enemies alive: {_cachedEnemies.Count}";
            }

            // Aimbot
            if (_aimbotEnabled)
            {
                _currentTarget = GetClosestEnemyInCircle(_cachedEnemies);
                if (_currentTarget != null)
                    AimAtTarget(_currentTarget.transform);
            }
            else
            {
                _currentTarget = null;
            }
        }

        void OnGUI()
        {
            if (!_aimbotEnabled) return;

            if (_circleTex == null)
                _circleTex = CreateCircleTexture(200, Color.red, 3f);

            float diameter = _circleRadius * 2f;
            Vector2 center = new(Screen.width / 2f, Screen.height / 2f);

            GUI.color = _currentTarget != null
                ? new Color(1f, 0.2f, 0.2f, 0.9f)
                : new Color(1f, 1f, 1f, 0.45f);

            GUI.DrawTexture(
                new Rect(center.x - _circleRadius, center.y - _circleRadius, diameter, diameter),
                _circleTex);

            GUI.color = Color.white;
        }

        // ── Reference management ─────────────────────────────────────────

        private void CachePlayerComponents()
        {
            _player = GameObject.Find("PLAYER");
            if (_player == null)
            {
                Plugin.Log.LogWarning("PLAYER not found!");
                return;
            }

            _controller = _player.GetComponentInChildren<Q3PlayerController>();
            if (_controller == null)
                Plugin.Log.LogWarning("Q3PlayerController not found under PLAYER!");

            foreach (var cam in _player.GetComponentsInChildren<Camera>())
            {
                if (cam.name != "Main Camera") continue;
                _cam = cam;
                Plugin.Log.LogInfo($"Camera found: {cam.name}");
                break;
            }

            if (_cam == null)
                Plugin.Log.LogWarning("Main Camera not found under PLAYER!");
        }

        /// <summary>Returns false when a critical reference is missing (caller skips this frame).</summary>
        private bool EnsureReferences()
        {
            if (_player != null && _cam != null && _controller != null) return true;
            CachePlayerComponents();
            return false;
        }

        // ── Aimbot ──────────────────────────────────────────────────────

        private GameObject GetClosestEnemyInCircle(List<GameObject> enemies)
        {
            Vector2 screenCenter = new(Screen.width / 2f, Screen.height / 2f);
            Vector3 myPos = _cam.transform.position;
            GameObject closest = null;
            float closestWorld = float.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                Vector3 screenPos = _cam.WorldToScreenPoint(enemy.transform.position);

                // Behind camera or off-screen
                if (screenPos.z <= 0f) continue;
                if (screenPos.x < 0f || screenPos.x > Screen.width ||
                    screenPos.y < 0f || screenPos.y > Screen.height) continue;

                // Outside aim circle
                if (Vector2.Distance(screenCenter, new Vector2(screenPos.x, screenPos.y)) > _circleRadius)
                    continue;

                // Prefer nearest enemy by world distance
                float worldDist = Vector3.Distance(myPos, enemy.transform.position);
                if (worldDist >= closestWorld) continue;

                closestWorld = worldDist;
                closest = enemy;
            }

            return closest;
        }

        private void AimAtTarget(Transform target)
        {
            Vector3 aimPoint = target.position + Vector3.up * _aimTargetOffset;
            Quaternion targetRot = Quaternion.LookRotation(aimPoint - _cam.transform.position);

            float yaw = targetRot.eulerAngles.y;
            float pitch = targetRot.eulerAngles.x;
            if (pitch > 180f) pitch -= 360f;

            float t = Time.deltaTime * _aimSmoothing;

            _controller.mouseLook.m_CharacterTargetRot = Quaternion.Slerp(
                _controller.mouseLook.m_CharacterTargetRot,
                Quaternion.Euler(0f, yaw, 0f), t);

            _controller.mouseLook.m_CameraTargetRot = Quaternion.Slerp(
                _controller.mouseLook.m_CameraTargetRot,
                Quaternion.Euler(pitch, 0f, 0f), t);
        }

        // ── Enemy detection ──────────────────────────────────────────────

        private static List<GameObject> CollectAliveEnemies()
        {
            var result = new List<GameObject>();
            int enemyLayer = LayerMask.NameToLayer("Enemy");

            foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
            {
                if (obj.layer == enemyLayer && obj.CompareTag("Enemy") && obj.activeInHierarchy)
                    result.Add(obj);
            }

            return result;
        }

        // ── Cheats ──────────────────────────────────────────────────────

        private void SetHealth(float health)
        {
            var hp = _player?.GetComponent<Health>();
            if (hp == null) { Plugin.Log.LogWarning("Health component not found."); return; }
            hp.SetHealth(health, _player);
            Plugin.Log.LogInfo($"Health set to {health}");
        }

        private void SetBunnyhop(bool enabled)
        {
            if (_controller == null) { Plugin.Log.LogWarning("Q3PlayerController not found."); return; }
            _controller.autoBunnyHop = enabled;
            Plugin.Log.LogInfo($"Bunnyhop: {enabled}");
        }

        private void AdjustFov(float delta)
        {
            if (_cam == null) return;
            _cam.fieldOfView = Mathf.Clamp(_cam.fieldOfView + delta, FovMin, FovMax);
            if (_fovLabel != null)
                _fovLabel.text = $"FOV: {_cam.fieldOfView:F0}";
        }

        // ── Menu visibility ──────────────────────────────────────────────

        private void SetMenuVisible(bool visible)
        {
            _visible = visible;
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = visible;
            _menuRoot.SetActive(visible);
        }

        // ── UI builder ───────────────────────────────────────────────────

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

            var panel = CreatePanel(_menuRoot, "Panel",
                new Vector2(300, 560),
                new Vector2(20, -20),
                new Color(0.08f, 0.08f, 0.08f, 0.93f));

            // Title
            CreateLabel(panel, "Title", "MOD MENU", new Vector2(0, -20), 20, Color.white);

            // Live labels
            _enemyCountLabel = CreateLabel(panel, "EnemyCount", "Enemies alive: 0",
                new Vector2(0, -55), 13, new Color(0.7f, 0.7f, 0.7f));

            _fovLabel = CreateLabel(panel, "FovLabel",
                $"FOV: {(_cam != null ? _cam.fieldOfView : 90f):F0}",
                new Vector2(0, -85), 13, new Color(0.7f, 0.7f, 0.7f));

            // FOV controls (side-by-side, narrow buttons)
            CreateButton(panel, "BtnFovDown", "FOV  –", new Vector2(-62, -128), 96,
                () => AdjustFov(-FovStep));

            CreateButton(panel, "BtnFovUp", "FOV  +", new Vector2(62, -128), 96,
                () => AdjustFov(+FovStep));

            // Aimbot toggle
            bool aimbotState = false;
            CreateButton(panel, "BtnAimbot", "Aimbot: OFF", new Vector2(0, -183), 220, () =>
            {
                aimbotState = !aimbotState;
                _aimbotEnabled = aimbotState;
                SetButtonLabel("Panel/BtnAimbot", aimbotState ? "Aimbot: ON" : "Aimbot: OFF");
                Plugin.Log.LogInfo($"Aimbot: {_aimbotEnabled}");
            });

            // Bunnyhop toggle
            bool bhopState = false;
            CreateButton(panel, "BtnBhop", "Bunnyhop: OFF", new Vector2(0, -233), 220, () =>
            {
                bhopState = !bhopState;
                SetBunnyhop(bhopState);
                SetButtonLabel("Panel/BtnBhop", bhopState ? "Bunnyhop: ON" : "Bunnyhop: OFF");
            });

            // Max health
            CreateButton(panel, "BtnMaxHP", "Set Max Health", new Vector2(0, -283), 220,
                () => SetHealth(100f));

            _menuRoot.SetActive(false);
        }

        // ── UI helpers ───────────────────────────────────────────────────

        private void SetButtonLabel(string path, string text)
        {
            var lbl = _menuRoot.transform.Find($"{path}/Label")?.GetComponent<Text>();
            if (lbl != null) lbl.text = text;
        }

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

        // Returns the Text component so callers can store a reference for live updates.
        private Text CreateLabel(GameObject parent, string name, string text,
            Vector2 anchoredPos, int fontSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(260f, 30f);
            rect.anchoredPosition = anchoredPos;

            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return txt;
        }

        private void CreateButton(GameObject parent, string name, string label,
            Vector2 anchoredPos, float width, System.Action onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(width, 36f);
            rect.anchoredPosition = anchoredPos;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.22f, 0.22f, 0.22f, 1f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(new System.Action(onClick));

            CreateLabel(go, "Label", label, Vector2.zero, 14, Color.white);
        }

        // ── Circle texture ───────────────────────────────────────────────

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
    }
}
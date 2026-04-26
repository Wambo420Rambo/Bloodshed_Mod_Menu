using com8com1.SCFPS;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace mod_menu
{
    public partial class ModMenuBehaviour(System.IntPtr ptr) : MonoBehaviour(ptr)
    {
        // ── Constants ─────────────────────────────────────────────────────
        private const KeyCode ToggleKey = KeyCode.M;
        private const float ScanInterval = 0.15f;
        private static readonly string[] NonGameplayScenes =
        {
            "00_Startup",
            "00_MainMenu",
            "00_LoadingScreenDefault",
            "MetaGame"
        };

        // ── Menu ──────────────────────────────────────────────────────────
        private GameObject _menuRoot;
        private GameObject _smoothingSlider;
        private GameObject _smoothingLabelGo;
        private GameObject _radiusSlider;
        private GameObject _radiusLabelGo;
        private GameObject _smoothingSectionGo;
        private GameObject _radiusSectionGo;
        private bool _visible;

        // ── Cached references ─────────────────────────────────────────────
        private GameObject _player;
        private Camera _cam;
        private Q3PlayerController _controller;

        // ── Enemy scan ────────────────────────────────────────────────────
        private List<GameObject> _cachedEnemies = new();
        private float _scanTimer;

        // ── UI live labels ────────────────────────────────────────────────
        private Text _enemyCountLabel;

        // ── Immortal ──────────────────────────────────────────────────────
        private bool _immortalEnabled;
        private const float maxHP = 100f;

        // ──────────────────────────────────────────────────────────────────



        void Start()
        {
            BuildUI();
        }

        void Update()
        {

            if (Input.GetKeyDown(ToggleKey))
                SetMenuVisible(!_visible);

            if (!IsIngame()) return;
            if (!EnsureReferences()) return;

            _scanTimer -= Time.deltaTime;
            if (_scanTimer <= 0f)
            {
                _scanTimer = ScanInterval;
                _cachedEnemies = CollectAliveEnemies();

                if (_enemyCountLabel != null)
                    _enemyCountLabel.text = $"Enemies alive: {_cachedEnemies.Count}";
            }

            UpdateAimbot();
            UpdateImmortal();
        }

        void OnGUI()
        {
            if (_aimbotEnabled) DrawAimbotCircle();
        }

        // ── Menu visibility ───────────────────────────────────────────────

        private void SetMenuVisible(bool visible)
        {
            _visible = visible;
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = visible;
            _menuRoot.SetActive(visible);
        }

        // ── Reference management ──────────────────────────────────────────

        private static bool IsIngame() =>
            System.Array.IndexOf(NonGameplayScenes, SceneManager.GetActiveScene().name) == -1;

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
        private bool EnsureReferences()
        {
            if (_player != null && _cam != null && _controller != null) return true;
            CachePlayerComponents();
            return _player != null && _cam != null && _controller != null;
        }
    }
}

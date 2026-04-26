using com8com1.SCFPS;

namespace mod_menu
{
    public partial class ModMenuBehaviour
    {
        private void SetHealth(float health)
        {
            var hp = _player?.GetComponent<Health>();
            if (hp == null) { Plugin.Log.LogWarning("Health component not found."); return; }
            hp.SetHealth(health, _player);
            Plugin.Log.LogInfo($"Health set to {health}");
        }

        private void UpdateImmortal()
        {
            if (!_immortalEnabled) return;

            var hp = _player.GetComponent<Health>();
            if (hp == null) return;

            if (hp.currentHealth < maxHP)
                hp.SetHealth(maxHP, _player);
        }
    }
}

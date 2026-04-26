namespace mod_menu
{
    public partial class ModMenuBehaviour
    {
        private bool _bunnyhopEnabled;

        private void SetBunnyhop(bool enabled)
        {
            if (_controller == null) { Plugin.Log.LogWarning("Q3PlayerController not found."); return; }
            _controller.autoBunnyHop = enabled;
            _bunnyhopEnabled = enabled;
            Plugin.Log.LogInfo($"Bunnyhop: {enabled}");
        }
    }
}

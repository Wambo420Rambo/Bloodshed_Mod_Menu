using com8com1.SCFPS;

namespace mod_menu
{
    public partial class ModMenuBehaviour
    {
        private PersistentData GetPersistentData()
        {
            var pd = FindObjectOfType<PersistentData>();
            if (pd == null) Plugin.Log.LogWarning("PersistentData not found.");
            return pd;
        }

        private void AddHolds(int amount)
        {
            var pd = GetPersistentData();
            if (pd == null) return;
            pd.currentHolds += amount;
            Plugin.Log.LogInfo($"Holds: {pd.currentHolds}");
            ForceSave();
        }

        private void AddAways(int amount)
        {
            var pd = GetPersistentData();
            if (pd == null) return;
            pd.currentAways += amount;
            Plugin.Log.LogInfo($"Aways: {pd.currentAways}");
            ForceSave();
        }

        private void AddCoins(float amount)
        {
            var pd = GetPersistentData();
            if (pd == null) return;
            pd.currentMoney += amount;
            Plugin.Log.LogInfo($"Coins: {pd.currentMoney}");
            ForceSave();
        }

        private void AddReRolls(int amount)
        {
            var pd = GetPersistentData();
            if (pd == null) return;
            pd.currentReRolls += amount;
            Plugin.Log.LogInfo($"ReRolls: {pd.currentReRolls}");
            ForceSave();
        }

        private void AddRevives(int amount)
        {
            var pd = GetPersistentData();
            if (pd == null) return;
            pd.currentRevives += amount;
            Plugin.Log.LogInfo($"Revives: {pd.currentRevives}");
            ForceSave();
        }

        private void AddSuperTickets(int amount)
        {
            var pd = GetPersistentData();
            if (pd == null) return;
            pd.currentSuperTickets += amount;
            Plugin.Log.LogInfo($"SuperTickets: {pd.currentSuperTickets}");
            ForceSave();
        }

        private void AddTickets(int amount)
        {
            var pd = GetPersistentData();
            if (pd == null) return;
            pd.currentTickets += amount;
            Plugin.Log.LogInfo($"Tickets: {pd.currentTickets}");
            ForceSave();
        }

        // Convenience — adds a chunk of everything at once
        private void AddAllCurrencies(float coinAmount = 9999f, int otherAmount = 99)
        {
            var pd = GetPersistentData();
            if (pd == null) return;
            pd.currentHolds += otherAmount;
            pd.currentAways += otherAmount;
            pd.currentMoney += coinAmount;
            pd.currentReRolls += otherAmount;
            pd.currentRevives += otherAmount;
            pd.currentSuperTickets += otherAmount;
            pd.currentTickets += otherAmount;
            Plugin.Log.LogInfo($"All currencies added.");
            ForceSave();
        }
    }
}
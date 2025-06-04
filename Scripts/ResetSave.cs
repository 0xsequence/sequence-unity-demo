namespace Game.Scripts
{
    public class ResetSave : CheatCode
    {
        protected override bool HandleCheatCode()
        {
            if (_cheatCode == "reset")
            {
                ResetWalletSave();
                return true;
            }

            return false;
        }

        public void ResetWalletSave()
        {
            SequenceConnector.Instance.Inventory.ResetSave();
        }
    }
}
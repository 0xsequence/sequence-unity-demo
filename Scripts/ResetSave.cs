namespace Game.Scripts
{
    public class ResetSave : CheatCode
    {
        protected override bool HandleCheatCode()
        {
            if (_cheatCode == "reset")
            {
                SequenceConnector.Instance.Inventory.ResetSave();
                return true;
            }

            return false;
        }
    }
}
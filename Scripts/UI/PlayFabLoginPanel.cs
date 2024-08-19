using Sequence.Demo;
using Sequence.Utils;

namespace Game.Scripts
{
    public class PlayFabLoginPanel : UIPanel
    {
        public override void Open(params object[] args)
        {
            bool federateAuth = args.GetObjectOfTypeIfExists<bool>();
            args.AppendObject(federateAuth);
            base.Open(args);
        }
    }
}
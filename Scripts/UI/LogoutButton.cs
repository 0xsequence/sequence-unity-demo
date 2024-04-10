using UnityEngine;

namespace Game.Scripts
{
    public class LogoutButton : MonoBehaviour
    {
        public void Logout()
        {
            SequenceConnector.Instance.Logout();
        }
    }
}
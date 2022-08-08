using UnityEngine;

namespace TEngine
{
    public partial class GameEntry:MonoBehaviour
    {
        void Start()
        {
            TEngineEntry.Active();
            RegisterSystem();
            OnStartGame();
        }

        void OnStartGame()
        {
            TLogger.LogError("OnStartGame");
        }
    }
}

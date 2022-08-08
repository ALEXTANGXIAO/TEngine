using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEngine;
using UnityEngine;

namespace Assets.TEngine.Runtime.Core
{
    public class GameEntry:MonoBehaviour
    {
        void Start()
        {
            TEngineEntry.Instance.OnStartGame += OnStartGame;
        }

        void OnStartGame()
        {
            TLogger.LogError("OnStartGame");
        }
    }
}

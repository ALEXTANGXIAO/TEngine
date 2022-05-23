using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TEngineHotUpdate
{
    public class GameLogicMain
    {
        public static void Init()
        {
            Debug.Log("Init");
        }

        public static void Start()
        {
            Debug.Log("Start");
        }

        public static void Update()
        {
            Debug.Log("Update");
        }

        public static void LateUpdate()
        {
            Debug.Log("LateUpdate");
        }

        public static void Destroy()
        {
            
        }

        public static void OnApplicationPause(bool isPause)
        {
            
        }
    }
}

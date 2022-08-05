using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TEngine
{
    public class Launcher:MonoBehaviour
    {

        void Start()
        {
            TLogger.Instance.Active();
            LoadMgr.Instance.InitParam("http://1.12.241.46:8081/TXYXGame/",isTestUpdate:true);
            LoadMgr.Instance.StartLoadInit(LaunchSuccess);
        }

        private void LaunchSuccess()
        {

        }
    }
}

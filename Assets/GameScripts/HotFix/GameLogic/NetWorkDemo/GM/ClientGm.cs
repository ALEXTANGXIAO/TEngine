using System;
using System.Collections.Generic;
using GameBase;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public delegate void HandleGM(List<string> @params);

    [Update]
    public class GMBehaviourSystem : BehaviourSingleton<GMBehaviourSystem>
    {
        public override void Active()
        {
            ClientGm.Instance.Init();
            base.Active();
        }

        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                GameModule.UI.ShowUIAsync<GMPanel>();
            }

            ;
        }
    }

    class ClientGm : Singleton<ClientGm>
    {
        private readonly Dictionary<string, HandleGM> _dictCmd = new Dictionary<string, HandleGM>();

        //缓存已输入过的命令
        private readonly List<string> _commendList = new List<string>();

        private GmCmdHandle _cmdHandle;

        public bool debugLowFps = false;
        public bool debugNotchScreen = false;

        public void Init()
        {
            _cmdHandle = new GmCmdHandle();
            _cmdHandle.Init();
        }

        public void HandleClientGm(string gmText)
        {
            gmText = gmText.Trim();

            Log.Debug("type: {0}", gmText.GetType().FullName);

            string[] list = gmText.Split(new[] { " " }, Int32.MaxValue, StringSplitOptions.RemoveEmptyEntries);

            Log.Debug("split list count: {0}", list.Length);

            List<string> gmParams = new List<string>();

            string cmd = list[0];
            for (int i = 1; i < list.Length; i++)
            {
                string param1 = list[i].Trim();
                if (string.IsNullOrEmpty(param1))
                {
                    continue;
                }

                gmParams.Add(param1);
            }

            Log.Warning("ClientGM:{0} param count: {1}", gmText, gmParams.Count);
            if (_dictCmd.TryGetValue(cmd, out var value))
            {
                value(gmParams);
            }
        }

        public void RegGmCmd(string cmd, HandleGM handle)
        {
            if (_dictCmd.ContainsKey(cmd))
            {
                Log.Error("repeat cmd: {0}", cmd);
                return;
            }

            _dictCmd.Add(cmd, handle);
        }

        public bool GetCommendByIndex(int index, out string commend)
        {
            var inRange = false;
            commend = string.Empty;

            if (index >= 0 && index < _commendList.Count)
            {
                inRange = true;
                commend = _commendList[index];
            }

            if (index >= _commendList.Count && _commendList.Count > 0)
            {
                commend = _commendList[0];
                inRange = false;
            }

            return inRange;
        }

        public void AddCommend(string commend)
        {
            _commendList.Insert(0, commend);
        }
    }
}
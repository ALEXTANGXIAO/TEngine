using System;
using System.Collections.Generic;
using System.Text;
using GameBase;
using GameLogic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    #region 红点添加步骤

    /// 1，增加RedNoteNotify定义，加入新的红点枚举。
    /// 2，添加红点关联，关联定义在InitRelation，查看当前有的关联关系，确定是增加还是重新创建
    /// 3，把RedNoteBehaviour挂在红点图片上，红点图片一般放置在需要显示红点的按钮/页签上，设置脚本上的枚举类型
    /// 4，如果是带参数的红点类型，在红点所在的UI声明红点对象，对参数进行设置，参数统一为uint，一般用一个可以唯一区分的ID。
    ///    有多个参数时，每后一个参数节点都是前一个参数的子节点。 无参数为该层级的根节点。
    /// 5，红点激活/隐藏
    ///    在对应模块数据管理类中，检测达到红点激活条件，或红点消失条件，调用SetNotifyValue激活/隐藏红点
    #endregion

    public enum RedNoteNotify
    {
        None = 0,
        CharacterMain,
        ShopMain,
        BagMain,
        BagUseType,
        ExploreMain,
        HomeUI,
    }

    /// <summary>
    /// 红点指引
    /// </summary>
    public class RedNoteMgr : Singleton<RedNoteMgr>
    {
        //红点状态记录
        private Dictionary<string, RedNoteValueItem> _notifyMap;

        //红点关联
        private readonly Dictionary<string, RedNoteCheckMgr> _checkDic = new Dictionary<string, RedNoteCheckMgr>();

        /// <summary>
        /// child to parent list
        /// </summary>
        private readonly Dictionary<string, List<string>> _checkOwnDic = new Dictionary<string, List<string>>();

        private readonly Dictionary<string, RedNoteKeyStruct> _keyConvertDic = new Dictionary<string, RedNoteKeyStruct>();
        private readonly Dictionary<string, RedNoteStructDic> _keyDic = new Dictionary<string, RedNoteStructDic>();

        /// <summary>
        /// 红点映射
        /// key => 红点名称
        /// val => 对应的红点类型
        /// </summary>
        private Dictionary<string, RedNoteNotify> _dicRedNoteMap;

        private Dictionary<int, string> _notifyStringMap;

        public void Init()
        {
            InitState();
            InitRedNoteConfig();
            InitRelation();
            InitRedNoteTween();
        }

        /// <summary>
        /// 全局缓动缩放
        /// </summary>
        public Vector3 GlobalTwScale { get; protected set; }

        /// <summary>
        /// 初始化红点缓动
        /// </summary>
        private void InitRedNoteTween()
        {
            // LeanTween.value(LeanTween.tweenEmpty, OnRedNoteTween, 1f, 0.75f, 0.5f).setLoopPingPong();
        }

        /// <summary>
        /// 缓动
        /// </summary>
        /// <param name="value"></param>
        private void OnRedNoteTween(float value)
        {
            GlobalTwScale = value * Vector3.one;
        }

        //注册红点通知
        public void RegisterNotify(RedNoteNotify notify, RedNoteBehaviour redNote)
        {
            RedNoteValueItem redNoteValueItem = GetOrNewNotifyValueItem(notify, redNote.IdParamList);
            redNoteValueItem.AddRedNote(redNote);
        }

        //销毁红点通知
        public void UnRegisterNotify(RedNoteNotify notify, RedNoteBehaviour redNote)
        {
            RedNoteValueItem redNoteValueItem = GetOrNewNotifyValueItem(notify, redNote.IdParamList);
            if (redNoteValueItem == null)
            {
                return;
            }

            redNoteValueItem.RemoveRedNote(redNote);
        }

        private readonly List<ulong> _tmpRedNoteParams = new List<ulong>();

        /// <summary>
        /// 设置红点状态。
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="value"></param>
        public void SetNotifyValue(RedNoteNotify notify, bool value)
        {
            _tmpRedNoteParams.Clear();
            SetNotifyValue(notify, value, _tmpRedNoteParams);
        }

        #region 参数重载 bool

        public void SetNotifyValue(RedNoteNotify notify, bool value, ulong param1)
        {
            _tmpRedNoteParams.Clear();
            _tmpRedNoteParams.Add(param1);
            SetNotifyValue(notify, value, _tmpRedNoteParams);
        }

        public void SetNotifyValue(RedNoteNotify notify, bool value, ulong param1, ulong param2)
        {
            _tmpRedNoteParams.Clear();
            _tmpRedNoteParams.Add(param1);
            _tmpRedNoteParams.Add(param2);
            SetNotifyValue(notify, value, _tmpRedNoteParams);
        }

        public void SetNotifyValue(RedNoteNotify notify, bool value, params ulong[] param)
        {
            _tmpRedNoteParams.Clear();
            for (var i = 0; i < param.Length; i++)
            {
                _tmpRedNoteParams.Add(param[i]);
            }

            SetNotifyValue(notify, value, _tmpRedNoteParams);
        }

        #endregion

        public void SetNotifyValue(RedNoteNotify notify, bool value, List<ulong> redNoteParamList)
        {
            var key = BuildKey(notify, redNoteParamList);
            if (!value && !_notifyMap.TryGetValue(key, out var redNoteValueItem))
            {
                return;
            }

            GetOrNewNotifyValueItem(notify, redNoteParamList);
            MarkNotifyKeyValueDirty(key, value);
        }

        public void MarkNotifyKeyValueDirty(string key, bool value)
        {
            if (!_notifyMap.TryGetValue(key, out var redNoteValueItem))
            {
                return;
            }
            redNoteValueItem.SetStateDirty(value);
        }

        /// <summary>
        /// 设置红点状态。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetNotifyKeyValue(string key, bool value)
        {
            if (!_notifyMap.TryGetValue(key, out var redNoteValueItem))
            {
                return;
            }

            //设置红点状态
            if (redNoteValueItem.SetRedNoteState(value))
            {
                //设置红点关联状态
                CalcRedNoteRelation(key);
            }
        }

        //设置红点状态数量
        public void SetNotifyKeyPointNum(string key, int pointNum)
        {
            if (!_notifyMap.TryGetValue(key, out var redNoteValueItem))
            {
                return;
            }

            if (redNoteValueItem.SetRedNotePoint(pointNum))
            {
                //设置红点关联状态
                CalcRedNoteRelation(key);
            }
        }

        public bool GetNotifyValue(RedNoteNotify notify, ulong param1)
        {
            _tmpRedNoteParams.Clear();
            _tmpRedNoteParams.Add(param1);
            return GetNotifyValue(notify, _tmpRedNoteParams);
        }

        public bool GetNotifyValue(RedNoteNotify notify, ulong param1, ulong param2)
        {
            _tmpRedNoteParams.Clear();
            _tmpRedNoteParams.Add(param1);
            _tmpRedNoteParams.Add(param2);
            return GetNotifyValue(notify, _tmpRedNoteParams);
        }

        /// <summary>
        /// 获取红点状态。
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool GetNotifyValue(RedNoteNotify notify, List<ulong> param = null)
        {
            if (notify == (uint)RedNoteNotify.None)
                return false;

            RedNoteValueItem item = GetOrNewNotifyValueItem(notify, param);
            return item.GetRedNoteState();
        }

        /// <summary>
        /// 获取红点数量。
        /// </summary>
        /// <param name="notify"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public int GetNotifyPointNum(RedNoteNotify notify, List<ulong> param)
        {
            if (notify == (uint)RedNoteNotify.None)
                return 0;

            RedNoteValueItem item = GetOrNewNotifyValueItem(notify, param);
            return item.GetRedNotePointNum();
        }

        public bool GetNotifyValue(string paramKey)
        {
            if (_notifyMap.TryGetValue(paramKey, out var redNoteValueItem))
            {
                return redNoteValueItem.GetRedNoteState();
            }

            return false;
        }

        /// <summary>
        /// 清理红点状态.
        /// </summary>
        /// <param name="notify"></param>
        public void ClearNotifyValue(RedNoteNotify notify)
        {
            var notifyStr = NotifyTypeToString(notify);
            RecursiveClearNotifyKeyValue(notifyStr);

            RedNoteValueItem redNoteValueItem = GetNotifyValueItem(notifyStr);
            redNoteValueItem.ClearRedNoteState(false);
            CalcRedNoteRelation(notifyStr);
        }

        public void RecursiveClearNotifyKeyValue(string key)
        {
            if (!_checkDic.TryGetValue(key, out var checkMgr))
            {
                return;
            }

            var childList = checkMgr.m_childList;
            foreach (var childKey in childList)
            {
                RedNoteValueItem redNoteValueItem = GetNotifyValueItem(childKey);
                redNoteValueItem.ClearRedNoteState(false);

                RecursiveClearNotifyKeyValue(childKey);
            }
        }

        /// <summary>
        /// 清理数据。
        /// </summary>
        public void OnRoleLogout()
        {
            var enumerator = _notifyMap.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.ClearRedNoteState(true);
            }

            enumerator.Dispose();
        }

        public string NotifyTypeToString(RedNoteNotify notify)
        {
            _notifyStringMap.TryGetValue((int)notify, out var str);
            return str;
        }

        public string BuildKey(RedNoteNotify notifyType, List<ulong> paramList)
        {
            var notifyStr = NotifyTypeToString(notifyType);
            if (notifyStr == null)
            {
                Log.Error("RedNoteNotifyId :{0} Not Exit! Please Check", notifyType.ToString());
                return string.Empty;
            }

            if (!_keyDic.TryGetValue(notifyStr, out var dicData))
            {
                dicData = new RedNoteStructDic();
                _keyDic[notifyStr] = dicData;
            }

            var key = dicData.TryGetKey(notifyStr, paramList);
            return key;
        }

        public static string GetKeyString(string notify, List<ulong> paramList)
        {
            if (paramList == null || paramList.Count == 0)
            {
                return notify;
            }

            string key;
            if (paramList.Count <= 1)
            {
                key = $"{notify}-{paramList[0]}";
            }
            else if (paramList.Count <= 2)
            {
                key = $"{notify}-{paramList[0]}-{paramList[1]}";
            }
            else if (paramList.Count <= 3)
            {
                key = $"{notify}-{paramList[0]}-{paramList[1]}-{paramList[2]}";
            }
            else
            {
                StringBuilder s = new StringBuilder();
                s.Append(notify + "-");
                for (var i = 0; i < paramList.Count; i++)
                {
                    s.Append(paramList[i]);
                    if (i != paramList.Count - 1)
                        s.Append("-");
                }

                key = s.ToString();
            }

            return key;
        }

        public void SetKeyConvertDic(string key, RedNoteKeyStruct keyStruct)
        {
            _keyConvertDic[key] = keyStruct;
        }

        private readonly List<ulong> _tmpParamList = new List<ulong>();

        /// <summary>
        /// 计算红点关联.
        /// </summary>
        /// <param name="notifyKey"></param>
        private void CalcRedNoteRelation(string notifyKey)
        {
            var key = notifyKey;
            if (_checkOwnDic.TryGetValue(key, out var ownerList))
            {
                foreach (var owner in ownerList)
                {
                    if (_checkDic.TryGetValue(owner, out var checker))
                    {
                        checker.CheckChildRedNote();
                    }
                }
            }
        }

        /// <summary>
        /// 初始化红点状态.
        /// </summary>
        private void InitState()
        {
            var array = (RedNoteNotify[])Enum.GetValues(typeof(RedNoteNotify));

            var redNoteCnt = array.Length;
            _notifyMap = new Dictionary<string, RedNoteValueItem>();
            _dicRedNoteMap = new Dictionary<string, RedNoteNotify>(redNoteCnt);
            _notifyStringMap = new Dictionary<int, string>(redNoteCnt);

            foreach (var redNoteNotify in array)
            {
                var redNoteStr = redNoteNotify.ToString();
                _dicRedNoteMap.Add(redNoteStr, redNoteNotify);
                _notifyStringMap.Add((int)redNoteNotify, redNoteStr);

                var key = BuildKey(redNoteNotify, _tmpParamList);
                var redNoteValueItem = new RedNoteValueItem();
                bool isNumType = IsNumType(redNoteNotify, null);
                redNoteValueItem.Init(key, isNumType ? RedNoteType.WithNum : RedNoteType.Simple);
                _notifyMap.Add(key, redNoteValueItem);
            }
        }

        public RedNoteValueItem GetNotifyValueItem(string key)
        {
            _notifyMap.TryGetValue(key, out var redNoteValueItem);
            return redNoteValueItem;
        }

        private RedNoteValueItem GetOrNewNotifyValueItem(RedNoteNotify notify, List<ulong> paramList)
        {
            var key = BuildKey(notify, paramList);
            var redNoteValueItem = GetNotifyValueItem(key);

            if (redNoteValueItem == null)
            {
                List<ulong> tmpParamList = new List<ulong>(paramList);

                //从后往前创建item，如(A, 1, 2)会创建A-1-2，A-1，A。
                string lastChildKey = string.Empty;
                int paramIndex = paramList.Count;
                while (paramIndex >= 0)
                {
                    var keyStr = BuildKey(notify, tmpParamList);

                    if (!_notifyMap.ContainsKey(keyStr))
                    {
                        RedNoteValueItem noteValueItem = new RedNoteValueItem();
                        bool isNumType = IsNumType(notify, paramList);
                        noteValueItem.Init(keyStr, isNumType ? RedNoteType.WithNum : RedNoteType.Simple);
                        _notifyMap.Add(keyStr, noteValueItem);
                    }

                    //叶子节点跳过（因为他没有子节点）
                    if (tmpParamList.Count < paramList.Count)
                    {
                        bool addedChild;
                        if (!_checkDic.TryGetValue(keyStr, out var checkMgr))
                        {
                            checkMgr = new RedNoteCheckMgr(keyStr);
                            addedChild = checkMgr.AddChild(lastChildKey);
                        }
                        else
                        {
                            addedChild = checkMgr.AddChild(lastChildKey);
                        }

                        if (addedChild)
                        {
                            AddDic(checkMgr); //重新生成父子关系
                        }
                    }

                    lastChildKey = keyStr;
                    paramIndex--;
                    if (paramIndex < tmpParamList.Count && tmpParamList.Count > 0)
                    {
                        tmpParamList.RemoveAt(paramIndex);
                    }
                }

                redNoteValueItem = _notifyMap[key];
            }

            return redNoteValueItem;
        }

        public void AddDic(RedNoteNotify owner, List<RedNoteNotify> childList)
        {
            AddDic(new RedNoteCheckMgr(owner, childList));
        }

        private void AddDic(RedNoteCheckMgr checker)
        {
            var owner = checker.m_ownerStr;
            _checkDic[owner] = checker;

            var childList = checker.m_childList;
            int count = childList.Count;
            for (int i = 0; i < count; i++)
            {
                var child = childList[i];
                if (!_checkOwnDic.TryGetValue(child, out var ownerList))
                {
                    ownerList = new List<string>();
                    _checkOwnDic[child] = ownerList;
                }

                if (!ownerList.Contains(owner))
                {
                    ownerList.Add(owner);
                }
            }
        }

        public bool IsNumType(RedNoteNotify noteNotify, List<ulong> paramList)
        {
            if (paramList is { Count: > 0 })
            {
                return true;
            }

            return false;
        }

        // 通过名字获取红点类型
        public static RedNoteNotify GetRedNoteByName(string redNoteName)
        {
            Instance._dicRedNoteMap.TryGetValue(redNoteName, out var redNote);

            return redNote;
        }

        public void OnUpdate()
        {
            foreach (var redNoteValueItem in _notifyMap)
            {
                redNoteValueItem.Value.CheckDirty();
            }
        }

        #region 初始化红点

        /// <summary>
        /// 初始化红点配置表
        /// </summary>
        private void InitRedNoteConfig()
        {
            // ResDictionaryList<uint, RedNoteConfig> cfgs = new ResDictionaryList<uint, RedNoteConfig>();
            // cfgs.Init(val => val.RedNoteParentID);
            // List<RedNoteNotify> list = new List<RedNoteNotify>();
            // foreach (var kv in cfgs.Data)
            // {
            //     list.Clear();
            //     foreach (var cfg in kv.Value)
            //     {
            //         list.Add((RedNoteNotify)cfg.RedNoteID);
            //     }
            //
            //     AddDic((RedNoteNotify)kv.Key, list);
            // }
        }

        //初始化红点关联
        private void InitRelation()
        {
        }
        #endregion
    }
}

public class RedNoteStructDic
{
    private readonly List<RedNoteKeyStruct> _keyStructList = new List<RedNoteKeyStruct>();

    public string TryGetKey(string notify, List<ulong> paramList)
    {
        string key = string.Empty;
        List<RedNoteKeyStruct> list = _keyStructList;
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                var keyStruct = list[i];
                if (keyStruct.IsSame(notify, paramList))
                {
                    key = keyStruct.Key;
                    break;
                }
            }
        }


        if (string.IsNullOrEmpty(key))
        {
            var keyStruct = new RedNoteKeyStruct(notify, paramList);
            key = keyStruct.Key;
            RedNoteMgr.Instance.SetKeyConvertDic(key, keyStruct);
            list.Add(keyStruct);
        }
        return key;
    }
}

public class RedNoteKeyStruct
{
    public string Notify;
    public List<ulong> ParamList;

    public RedNoteKeyStruct(string notify, List<ulong> paramList)
    {
        Notify = notify;
        if (paramList != null)
        {
            ParamList = new List<ulong>(paramList);
        }
        else
        {
            ParamList = new List<ulong>();
        }
    }

    private string _key;

    public string Key
    {
        get
        {
            if (string.IsNullOrEmpty(_key))
            {
                _key = RedNoteMgr.GetKeyString(Notify, ParamList);
            }
            return _key;
        }
    }

    public bool IsSame(string notify, List<ulong> paramList)
    {
        if (notify != Notify)
        {
            return false;
        }

        var list1 = paramList;
        var list2 = ParamList;

        int cnt1 = list1?.Count ?? 0;
        int cnt2 = list2?.Count ?? 0;

        if (cnt1 != cnt2)
        {
            return false;
        }

        if (cnt1 == 0)
        {
            return true;
        }

        for (int i = 0; i < cnt1; i++)
        {
            // ReSharper disable PossibleNullReferenceException
            if (list1[i] != list2[i])
            {
                return false;
            }
        }
        return true;
    }
}
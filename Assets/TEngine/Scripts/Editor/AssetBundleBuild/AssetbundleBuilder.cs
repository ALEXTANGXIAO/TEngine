using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TEngine.Runtime;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TEngineCore.Editor
{
    public class AssetbundleBuilder
    {
        internal struct AssetBundleMeta
        {
            public string BundleName;
            public Hash128 Hash;
            public string[] Assets;
            public string[] Dependencies;
        }

        /// <summary>
        /// ab环的名称过滤器
        /// </summary>
        private HashSet<string> _filterAbCircle;

        /// <summary>
        /// AB环信息
        /// </summary>
        private List<List<string>> _abCircles;

        /// <summary>
        /// 环检测最大数量上限
        /// </summary>
        private const int MAXCircleNum = 800;

        /// <summary>
        /// ab到子资源的映射
        /// </summary>
        Dictionary<string, List<AbBuildInfo>> _abName2AssetsInfos;

        /// <summary>
        /// 路径到其对应的Build信息的映射
        /// </summary>
        private Dictionary<string, AbBuildInfo> _assetBuildInfos;

        /// <summary>
        /// AB包对应的AB依赖关系
        /// </summary>
        private Dictionary<string, List<string>> _abDeps;

        /// <summary>
        /// 资源环Index
        /// </summary>
        private int _assetCircleIndex = 0;

        /// <summary>
        /// 资源环的容器(判断用)
        /// </summary>
        private List<string> _assetCircleFilter;

        /// <summary>
        /// 环名到info的映射
        /// </summary>
        private Dictionary<string, List<AbBuildInfo>> _assetCircle2Infos;

        /// <summary>
        /// AB包连接的分隔符
        /// </summary>
        char abSep = 'S';

        /// <summary>
        /// 文件过滤
        /// </summary>
        private string[] fileExcludeFilter = new[] { ".cs", ".meta", ".dll", ".DS_Store", ".unity" };

        /// <summary>
        /// 资源环信息初始化
        /// </summary>
        void InitCircleInfo()
        {
            _assetCircle2Infos = new Dictionary<string, List<AbBuildInfo>>();
            _assetCircleFilter = new List<string>();
            _assetCircleIndex = 0;
        }

        /// <summary>
        /// 设置资源所对应的BuildInfo信息
        /// </summary>
        /// <param name="abBuildInfoDic"></param>
        /// <param name="path"></param>
        /// <param name="abName"></param>
        /// <param name="parent"></param>
        /// <param name="parents"></param>
        void SetAssetAbBuildInfo(string path, string abName, AbBuildInfo parent, List<AbBuildInfo> parents = null, bool isTop = false)
        {
            var info = GetOrCreateAbBuildInfo(path);
            if (isTop)
                info.SetAsTopRes();

            info.SetABName(abName, path);
            info.AddMyParent(parent);
            info.AddMyParent(parents);
        }

        AbBuildInfo GetOrCreateAbBuildInfo(string path)
        {
            if (!_assetBuildInfos.TryGetValue(path, out AbBuildInfo info))
            {
                bool isTopRes = path.LastIndexOf(FileSystem.GameResourcePath, StringComparison.Ordinal) == 0;
                info = new AbBuildInfo(isTopRes, path);
                if (path.EndsWith(".unity"))
                {
                    info.SetAsScene();
                }

                _assetBuildInfos.Add(path, info);
            }

            return info;
        }

        /// <summary>
        /// 设置子项资源的依赖情况，剔除了scriptobject
        /// </summary>
        /// <param name="abBuildInfoDic"></param>
        /// <param name="path"></param>
        void SetDependenciesExcludeSo(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (IsSOCutOff(path))
                return;

            var info = GetOrCreateAbBuildInfo(path);
            var parent = info.GetChildParent();

            var childs = info.GetChilds();
            if (childs != null)
            {
                foreach (var depend in childs)
                {
                    Check(depend.Path);
                }
            }
            else
            {
                //一层依赖
                string[] depends = null;
                //mat特殊处理，获取全依赖
                if (path.EndsWith(".mat"))
                {
                    var mat = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    var deps = EditorUtility.CollectDependencies(new[] { mat });
                    depends = new string[deps.Length - 1];
                    int count = 0;
                    foreach (var dep in deps)
                    {
                        string matDPath = AssetDatabase.GetAssetPath(dep);
                        ;
                        if (matDPath.Equals(path))
                            continue;
                        depends[count] = matDPath;
                        count++;
                    }
                }
                else
                    depends = AssetDatabase.GetDependencies(path, false);

                foreach (string depend in depends)
                {
                    Check(depend);
                }
            }

            void Check(string targetPath)
            {
                if (targetPath.EndsWith(".cs"))
                    return;

                CheckIfHandle(targetPath);

                info.AddChild(GetOrCreateAbBuildInfo(targetPath));

                //判断是否在环中
                int start = _assetCircleFilter.IndexOf(targetPath);
                if (start > -1)
                {
                    //设置当前环信息
                    SetAssetCircleAbName(_assetCircleFilter.GetRange(start, _assetCircleFilter.Count - start));
                    //跳过
                    return;
                }

                SetAssetAbBuildInfo(targetPath, null, parent.Item1, parent.Item2);


                if (IsSOCutOff(targetPath))
                    return;


                _assetCircleFilter.Add(targetPath);
                SetDependenciesExcludeSo(targetPath);
                _assetCircleFilter.Remove(targetPath);
            }
        }

        internal Dictionary<string, List<AbBuildInfo>> GetAbName2AssetsInfos()
        {
            return _abName2AssetsInfos;
        }

        internal Dictionary<string, List<string>> GetAbDepence()
        {
            return _abDeps;
        }

        /// <summary>
        /// 设置额外的ab信息
        /// </summary>
        /// <param name="path2AbNames"></param>
        internal void InsertAdditionalTopRes(Dictionary<string, string> path2AbNames, bool forceAdd = true)
        {

            if (path2AbNames != null && path2AbNames.Count > 0)
            {
                foreach (var p2a in path2AbNames)
                {
                    string curPath = p2a.Key.Replace("\\", "/");
                    _assetBuildInfos.TryGetValue(curPath, out var info);
                    //如果当前资源不在收集表中，则剔除
                    if (!forceAdd)
                    {
                        if (info == null)
                            continue;
                    }

                    if (info != null)
                        info.SetOnlyRecord(false);

                    SetDependenciesAdditional(curPath, p2a
                        .Value);
                }
            }
        }


        /// <summary>
        /// 设置规则外的AB信息
        /// </summary>
        /// <param name="path">对应资源路径</param>
        public void SetDependenciesAdditional(string path, string abName)
        {
            if (_assetBuildInfos == null)
            {
                Debug.LogError("在错误的时机调用了SetDependenciesAdditional函数！");
                return;
            }

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(abName))
                return;

            abName = abName.ToLower();
            SetAssetAbBuildInfo(path, abName, null, null, true);

            _assetCircleFilter.Add(path);
            SetDependenciesExcludeSo(path);
            _assetCircleFilter.Remove(path);
        }


        /// <summary>
        /// 是否属于被切断的SO
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool IsSOCutOff(string path)
        {
            return false;
        }

        /// <summary>
        /// 设置资源环的AB名
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="dic"></param>
        void SetAssetCircleAbName(List<string> paths)
        {
            //环名容器，要求唯一
            List<string> circleNames = new List<string>();
            foreach (var path in paths)
            {
                if (_assetBuildInfos.TryGetValue(path, out var info))
                {
                    if (!string.IsNullOrEmpty(info.AbName) && info.IsInCircle)
                    {
                        if (!circleNames.Contains(info.AbName))
                            circleNames.Add(info.AbName);
                    }
                }
                else
                {
                    Debug.LogError("不存在的环节点！path：" + path);
                }
            }

            string abName = string.Empty;
            if (circleNames.Count == 0)
            {
                //如果没有数据，说明是新的环，获取名字
                abName = GetCurCircleName();
            }
            else
            {
                //否则取第一个环的名字
                abName = circleNames[0];
            }

            //更新环字典
            //1.将多余的部分添加进第一个名字的部分，去除多余的名字
            //2.将新路径传入其中

            if (_assetCircle2Infos.ContainsKey(abName))
            {
                //将circleNames中index从1开始的全部设为0的环名
                for (int i = 1; i < circleNames.Count; ++i)
                {
                    if (_assetCircle2Infos.TryGetValue(circleNames[i], out var abs))
                    {
                        for (int j = 0; j < abs.Count; ++j)
                        {
                            //将多余的部分添加进第一个名字的部分
                            AddInCircleDic(abName, abs[j]);
                        }
                    }

                    //去除多余的环名字
                    _assetCircle2Infos.Remove(circleNames[i]);
                }
            }

            //设置传入数据的ab情况
            foreach (var path in paths)
            {
                if (_assetBuildInfos.TryGetValue(path, out var info))
                {
                    AddInCircleDic(abName, info);
                }
            }
        }

        /// <summary>
        /// 将成环的资源信息设入资源环容器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        void AddInCircleDic(string key, AbBuildInfo info)
        {
            if (!_assetCircle2Infos.ContainsKey(key))
            {
                _assetCircle2Infos.Add(key, new List<AbBuildInfo>());
            }

            if (!_assetCircle2Infos[key].Contains(info))
            {
                info.SetCircleInfo(key);
                _assetCircle2Infos[key].Add(info);
            }
        }


        /// <summary>
        /// 收集所有的Atlas，将其下资源命名统一AB包名
        /// </summary>
        protected void CollectAllAtlasSprite()
        {
            if (_assetBuildInfos == null)
            {
                Debug.LogError("在错误的时机调用了收集Atlas！");
                return;
            }
            string[] atlas = AssetDatabase.FindAssets("t:SpriteAtlas");
            for (int i = 0; i < atlas.Length; i++)
            {
                string atlasPath = AssetDatabase.GUIDToAssetPath(atlas[i]);
                EditorUtility.DisplayProgressBar("InitSpriteAtlasRef...", atlasPath, (float)(i + 1) / atlas.Length);

                string[] str = AssetDatabase.GetDependencies(atlasPath, false);
                //如果其中资源存在被引用情况，那么整个Atlas资源都需要设置统一包名
                bool setSameAB = false;
                for (int j = 0; j < str.Length; ++j)
                {
                    //查看有无记录
                    if (_assetBuildInfos.TryGetValue(str[j], out var info))
                    {
                        if (info.OnlyRecord)
                            continue;
                        setSameAB = true;
                        break;
                    }
                }

                //需要统一设置
                if (setSameAB)
                {
                    string abName = $"atlas_{BuilderUtility.GetStringHash(atlasPath)}";
                    for (int j = 0; j < str.Length; ++j)
                    {
                        AbBuildInfo info = GetOrCreateAbBuildInfo(str[j]);
                        info.SetOnlyRecord(false);
                        info.SetSpriteAtlasInfo(abName);
                    }

                    //设置图集本身AB名
                    GetOrCreateAbBuildInfo(atlasPath).SetOnlyRecord(false);
                    GetOrCreateAbBuildInfo(atlasPath).SetSpriteAtlasInfo(abName);
                }
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 合并资源环
        /// </summary>
        void CombineCircle()
        {
            StringBuilder stringbuilder = new StringBuilder();

            //初始环信息记录
            stringbuilder.Append("路径名").Append(",").Append("所在环包").Append(",").Append("环包总数：" + _assetCircle2Infos.Count)
                .Append("\n");
            foreach (var name2Path in _assetCircle2Infos)
            {
                foreach (var info in name2Path.Value)
                {
                    stringbuilder.Append(info.Path).Append(",").Append(name2Path.Key).Append("\n");
                }
            }

            //合并环
            /*
             * 1.父ab一致，设为父
             * 2.无父或父不一致，不处理，合并
             */
            List<string> _todel = new List<string>();
            foreach (var info in _assetCircle2Infos)
            {
                List<string> parentsAB = new List<string>();

                //如果当前资源是场景，那么这个环外，
                //  其他数据，如果包含顶层资源，除去场景，合并
                //          ，如果全非顶层资源，删除记录，auto
                bool hasScene = false;
                bool hasTopRes = false;
                List<AbBuildInfo> scenes = new List<AbBuildInfo>();
                foreach (var c in info.Value)
                {
                    if (c.IsScene || c.Path.EndsWith(".unity"))
                    {
                        hasScene = true;
                        scenes.Add(c);
                        continue;
                    }

                    if (c.IsInTopRes)
                    {
                        hasTopRes = true;
                    }
                }

                //从环中删除场景
                foreach (var scene in scenes)
                {
                    scene.SetAsScene();
                    scene.SetCircleInfo(GetSceneName(scene.Path));
                    info.Value.Remove(scene);
                }

                if (hasScene)
                {
                    if (!hasTopRes)
                    {
                        _todel.Add(info.Key);
                        foreach (var c in info.Value)
                        {
                            _assetBuildInfos.Remove(c.Path);
                        }

                        continue;
                    }
                }

                //遍历当前的资源环数据
                foreach (var c in info.Value)
                {
                    foreach (var parent in c.Parents)
                    {
                        //父物体为环，无视，环最后会统一融入，此时融入无意义
                        if (parent.AbName.IndexOf("circleab_", StringComparison.Ordinal) == 0)
                            continue;
                        //父物体无AB名
                        if (string.IsNullOrEmpty(parent.AbName))
                        {
                            Debug.LogError("parent.AbName = null" + parent.Path);
                        }

                        //正常的父物体
                        if (!string.IsNullOrEmpty(parent.AbName) && !parentsAB.Contains(parent.AbName))
                            parentsAB.Add(parent.AbName);
                    }
                }

                //这些环的父AB一致且只有一个，设为父AB
                if (parentsAB.Count == 1)
                {
                    foreach (var c in info.Value)
                    {
                        c.SetCircleInfo(parentsAB[0]);
                    }

                    if (!_todel.Contains(info.Key))
                        _todel.Add(info.Key);
                }
            }

            //删除被合并的的环
            foreach (var del in _todel)
            {
                Debug.Log("删除环：" + del);
                _assetCircle2Infos.Remove(del);
            }

            //合并环，使用第一个环的环名
            string abName = string.Empty;
            foreach (var info in _assetCircle2Infos)
            {
                abName = string.IsNullOrEmpty(abName) ? info.Key : abName;
                if (info.Key.Equals(abName))
                    continue;
                foreach (var abinfo in info.Value)
                {
                    abinfo.SetCircleInfo(abName);
                }

                _assetCircle2Infos[abName].AddRange(info.Value);
            }

            // 将环信息结构存盘
            try
            {
                if (_assetCircle2Infos.Count > 0)
                {
                    stringbuilder.Append("\n").Append("\n").Append("\n").Append("\n").Append("\n");
                    //环信息输出
                    stringbuilder.Append("路径名").Append(",").Append("所在环包").Append(",").Append("环包总数：" + _assetCircle2Infos.Count)
                        .Append("\n");
                    foreach (var name2Path in _assetCircle2Infos)
                    {
                        if (!name2Path.Key.Equals(abName))
                            continue;
                        foreach (var info in name2Path.Value)
                        {
                            stringbuilder.Append(info.Path).Append(",").Append(name2Path.Key).Append("\n");
                        }
                    }

                    stringbuilder.Append("\n").Append("\n").Append("\n").Append("\n").Append("\n");
                    stringbuilder.Append("环包名").Append(",").Append("环信息").Append(",").Append("环包总数：" + _assetCircle2Infos.Count)
                        .Append("\n");
                    foreach (var name2Path in _assetCircle2Infos)
                    {
                        if (!name2Path.Key.Equals(abName))
                            continue;
                        foreach (var info in name2Path.Value)
                        {
                            string parentsAB = string.Empty;
                            foreach (var parent in info.Parents)
                            {
                                parentsAB += parent.AbName + "&&&&&&";
                            }

                            stringbuilder.Append(name2Path.Key).Append(",").Append(parentsAB).Append("\n");
                        }
                    }

                    BuilderUtility.ExportCsv("AssetCircleInfo.csv", stringbuilder);
                    Debug.Log("<color=green>AssetCircleInfo 为资源环信息</color>");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("<color=red>数据导出 失败</color>");
                Debug.Log(e);
            }
        }

        /// <summary>
        /// 获取AB名到资源的映射
        /// </summary>
        /// <param name="name2Paths"></param>
        /// <returns></returns>
        AssetBundleBuild[] GetAbName2PathsDic(out Dictionary<string, List<string>> name2Paths)
        {
            name2Paths = new Dictionary<string, List<string>>();
            foreach (var info in _abName2AssetsInfos)
            {
                if (info.Value == null)
                {
                    Debug.LogError(info.Key + " has not any abInfos");
                    continue;
                }

                string abName = info.Key;

                if (name2Paths.ContainsKey(abName))
                {
                    Debug.LogError("重复的AB名出现了！" + abName);
                }
                else
                {
                    name2Paths.Add(abName, new List<string>());
                    foreach (AbBuildInfo abBuildInfo in info.Value)
                    {
                        name2Paths[abName].Add(abBuildInfo.Path);
                    }
                }
                name2Paths[abName].Sort();
            }

            AssetBundleBuild[] buildMap = new AssetBundleBuild[name2Paths.Count];

            int curIndex = 0;
            foreach (var info in name2Paths)
            {
                buildMap[curIndex].assetNames = info.Value.ToArray();
                buildMap[curIndex].assetBundleName = info.Key;
                curIndex++;
            }

            return buildMap;
        }

        /// <summary>
        /// 获取单包混合的新AB包名
        /// </summary>
        /// <param name="names"></param>
        /// <param name="nameDic"></param>
        /// <returns></returns>
        private string GetMixAbNameInfo(List<string> names)
        {
            names.Sort();
            StringBuilder sb = new StringBuilder();
            foreach (var name in names)
            {
                sb.Append(name).Append(abSep);
            }

            return sb.ToString();
        }


        /// <summary>
        /// 获取资源环的ab名
        /// </summary>
        /// <returns></returns>
        string GetCurCircleName()
        {
            return ("circleab_strange_circleab_" + (_assetCircleIndex++)).ToLower();
        }

        /// <summary>
        /// 是否属于操作范围内的资源，gameresources与artraw下
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool CheckIfHandle(string path)
        {
            if (path.LastIndexOf(FileSystem.GameResourcePath, StringComparison.Ordinal) == 0)
                return true;

            if (Builder.Instance.CurBuilderData.bundleConfig == null || Builder.Instance.CurBuilderData.bundleConfig.filters == null)
            {
                if (path.LastIndexOf(FileSystem.ArtResourcePath, StringComparison.Ordinal) == 0)
                    return true;

                GetOrCreateAbBuildInfo(path).SetOnlyRecord();
                return false;
            }

            foreach (var filter in Builder.Instance.CurBuilderData.bundleConfig.filters)
            {
                if (path.LastIndexOf(filter.filterPath, StringComparison.Ordinal) == 0)
                    return true;
            }

            GetOrCreateAbBuildInfo(path).SetOnlyRecord();
            return false;
        }


        /// <summary>
        /// 按规则生成AssetBundle Name
        /// </summary>
        internal bool CollectAssetBundles(BuilderUtility.BuilderBundlePolicy bundlePolicy,
            BundlePolicyConfig config = null)
        {
            InitCircleInfo();

            _assetBuildInfos = new Dictionary<string, AbBuildInfo>();

            bool result = true;
            switch (bundlePolicy)
            {
                case BuilderUtility.BuilderBundlePolicy.SingleFile:
                    result = SimplePolicy();
                    break;
                case BuilderUtility.BuilderBundlePolicy.Directory:
                    result = DirectoryBasedPolicy();
                    break;
                case BuilderUtility.BuilderBundlePolicy.Configuration:
                    result = ConfigurationBasedPolicy(config);
                    break;
            }


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();

            return result;
        }

        /// <summary>
        /// 按GameResources下一个资源一个AB的策略
        /// </summary>
        protected bool SimplePolicy()
        {
            if (!CollectAssetBundle(FileSystem.GameResourcePath, BuilderUtility.BundlePolicy.SingleFile))
                return false;

            if (!CollectScenes())
                return false;
            // SetAndCheckAssetDependencies(_assetBuildInfos);

            return true;
        }

        /// <summary>
        /// 按GameResources下子目录一个AB的策略
        /// </summary>
        protected bool DirectoryBasedPolicy()
        {
            string[] subDirs = Directory.GetDirectories(FileSystem.GameResourcePath);

            for (int i = 0; i < subDirs.Length; ++i)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Collecting AssetBundle in directories", subDirs[i],
                    (float)(i + 1) / subDirs.Length))
                {
                    EditorUtility.ClearProgressBar();
                    return false;
                }

                if (!CollectAssetBundle(subDirs[i], BuilderUtility.BundlePolicy.Directory))
                    return false;
            }

            if (!CollectScenes())
                return false;

            // SetAndCheckAssetDependencies(_assetBuildInfos);

            return true;
        }

        /// <summary>
        /// 按配置策略打AB
        /// </summary>
        protected bool ConfigurationBasedPolicy(BundlePolicyConfig config)
        {
            if (config)
            {
                Builder.Instance.CurBuilderData.bundleConfig = config;
                foreach (var dir in Builder.Instance.CurBuilderData.bundleConfig.directoryBuild)
                {
                    string temp = AssetDatabase.GetAssetPath(dir.mObject);
                    BuilderUtility.BundlePolicy policy = (BuilderUtility.BundlePolicy)(dir.buildType);

                    if (policy == BuilderUtility.BundlePolicy.ChildDirectory)
                    {
                        ChildDirectoryPolicy(temp);
                        continue;
                    }

                    if (!CollectAssetBundle(temp, policy))
                        return false;
                }

                if (!CollectScenes())
                    return false;

                // SetAndCheckAssetDependencies(_assetBuildInfos);
            }
            else
                UnityEngine.Debug.LogError($"Missing AssetBundle Collection Configuration");

            return true;
        }

        /// <summary>
        /// 按GameResources下子目录一个AB的策略
        /// </summary>
        bool ChildDirectoryPolicy(string path)
        {
            string[] subDirs = Directory.GetDirectories(path);
            string[] subFiles = Directory.GetFiles(path);


            for (int i = 0; i < subDirs.Length; ++i)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Collecting AssetBundle in directories", subDirs[i],
                    (float)(i + 1) / subDirs.Length))
                {
                    return false;
                }
                bool result = CollectAssetBundle(subDirs[i], BuilderUtility.BundlePolicy.ChildDirectory);
                if (!result)
                    return false;
            }

            for (int i = 0; i < subFiles.Length; ++i)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Collecting AssetBundle in directories", subFiles[i],
                    (float)(i + 1) / subFiles.Length))
                {
                    return false;
                }
                bool isBreak = false;
                for (int j = 0; j < fileExcludeFilter.Length; ++j)
                {
                    if (subFiles[i].EndsWith(fileExcludeFilter[j]))
                    {
                        isBreak = true;
                        break;
                    }
                }
                if (isBreak)
                    continue;
                bool result = CollectAssetBundle(subFiles[i], BuilderUtility.BundlePolicy.ChildDirectory, true);
                if (!result)
                    return false;
            }


            EditorUtility.ClearProgressBar();
            return true;
        }

        bool CollectAssetBundle(string path, BuilderUtility.BundlePolicy policy, bool isSingleFile = false)
        {
            if (isSingleFile)
            {
                string suffix = GetPathSuffix(path);
                SetAssetBuildInfo(policy, path.Replace("\\", "/"), "", suffix);
                return true;
            }

            using (FileTree fileTree = FileTree.CreateWithExcludeFilter(path, fileExcludeFilter))
            {
                //前缀
                string suffix = GetPathSuffix(path);


                List<FileInfo> files = fileTree.GetAllFiles();
                for (int i = 0; i < files.Count; ++i)
                {
                    string assetPath = files[i].GetAssetPath();

                    if (EditorUtility.DisplayCancelableProgressBar("Collecting AssetBundle", assetPath,
                        (float)(i + 1) / files.Count))
                    {
                        EditorUtility.ClearProgressBar();
                        return false;
                    }

                    SetAssetBuildInfo(policy, assetPath, fileTree.Name, suffix);
                }
            }

            return true;
        }

        string GetPathSuffix(string path)
        {
            if (path[path.Length - 1] == '/')
            {
                path = path.Substring(0, path.Length - 1);
            }

            path = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string[] name = path?.Split('/');
            string suffix;
            if (name?.Length > 1)
                suffix = name[name.Length - 1] + "_";
            else
            {
                Debug.LogError("路径无法正确读取文件夹名称" + path);
                suffix = "ErrorDirection" + "_";
            }

            return suffix;
        }

        void SetAssetBuildInfo(BuilderUtility.BundlePolicy policy, string assetPath, string directoryName, string suffix)
        {
            string abName = string.Empty;
            switch (policy)
            {
                case BuilderUtility.BundlePolicy.SingleFile:
                    abName = GetAssetBundleName(assetPath, BuilderUtility.AssetSourceType.GameSource);
                    break;
                case BuilderUtility.BundlePolicy.Directory:
                    abName = directoryName;
                    break;
                case BuilderUtility.BundlePolicy.ChildDirectory:

                    //如果是文件夹，父文件夹+子文件夹名||||如果是文件，父文件夹+single
                    if (!string.IsNullOrEmpty(directoryName) && string.IsNullOrEmpty(Path.GetExtension(directoryName)))
                        abName = suffix + directoryName;
                    else
                        abName = suffix + "single";
                    break;
                case BuilderUtility.BundlePolicy.CustomAbName:
                    abName = AssetImporter.GetAtPath(assetPath).assetBundleName;
                    if (string.IsNullOrEmpty(abName))
                    {
                        Debug.LogError("发现Custom策略下未命名AB资源，单包处理:" + assetPath);
                        abName = GetAssetBundleName(assetPath, BuilderUtility.AssetSourceType.GameSource);
                    }

                    break;
            }

            //设置自身信息
            SetAssetAbBuildInfo(assetPath, abName, null);

            _assetCircleFilter.Clear();
            _assetCircleFilter.Add(assetPath);
            SetDependenciesExcludeSo(assetPath);
            _assetCircleFilter.Remove(assetPath);
        }


        bool CollectScenes()
        {
            EditorBuildSettingsScene[] buildSettingsScenes = EditorBuildSettings.scenes;

            // 默认第一个为内嵌场景
            for (int i = 1; i < buildSettingsScenes.Length; ++i)
            {
                if (buildSettingsScenes[i] == null || string.IsNullOrEmpty(buildSettingsScenes[i].path) ||
                    !buildSettingsScenes[i].enabled)
                    continue;

                if (EditorUtility.DisplayCancelableProgressBar("Collecting Scenes", buildSettingsScenes[i].path,
                    (float)(i + 1) / buildSettingsScenes.Length))
                {
                    EditorUtility.ClearProgressBar();
                    return false;
                }


                SetAssetAbBuildInfo(buildSettingsScenes[i].path,
                    $"unityscene_{GetSceneName(buildSettingsScenes[i].path)}", null);

                GetOrCreateAbBuildInfo(buildSettingsScenes[i].path).SetAsScene();


                _assetCircleFilter.Clear();
                _assetCircleFilter.Add(buildSettingsScenes[i].path);
                SetDependenciesExcludeSo(buildSettingsScenes[i].path);
                _assetCircleFilter.Remove(buildSettingsScenes[i].path);
            }

            return true;
        }

        string GetSceneName(string scenePath)
        {
            scenePath = scenePath.Substring(0, scenePath.LastIndexOf('.'));
            return scenePath.Substring(scenePath.LastIndexOf('/') + 1);
        }

        /// <summary>
        /// 最后设置ab包名，应放在所有基础ab信息设置完成之后
        /// </summary>
        /// <param name="doSingleCombine">是否进行单包合并</param>
        internal void SetAndCheckAssetDependencies(bool doSingleCombine = true)
        {
            if (_assetBuildInfos == null)
            {
                Debug.LogError("在错误的时机调用了SetDependenciesAdditional函数！");
                return;
            }

            //合并资源环
            CombineCircle();
            //合并所有SpriteAtlas图集信息
            CollectAllAtlasSprite();



            //设置AB名
            List<string> excludeKeys = new List<string>();
            foreach (var info in _assetBuildInfos)
            {
                //剔除文件夹
                if (Directory.Exists(info.Key))
                {
                    excludeKeys.Add(info.Key);
                    continue;
                }
                //剔除RawBytes
                if (info.Key.Contains(AssetConfig.AssetRootPath + "/" + "RawBytes"))
                {
                    if (Builder.Instance.CurBuilderData.builderBundlePolicy == BuilderUtility.BuilderBundlePolicy.Configuration)
                    {
                        Debug.LogError($"存在RawBytes下的资源被依赖：{info.Key}");
                    }
                    excludeKeys.Add(info.Key);
                    continue;
                }
                //剔除单纯做记录的数据
                if (info.Value.OnlyRecord)
                {
                    excludeKeys.Add(info.Key);
                    continue;
                }

                //如果一个AB会成为单包，先不设置AB名，标志标记为为单包
                info.Value.FixAbName(info.Key);
            }

            //剔除无意义的ab信息
            foreach (var key in excludeKeys)
            {
                _assetBuildInfos.Remove(key);
            }
            #region 单包合并
            //记录"单包合并名"所对应单包信息
            Dictionary<string, List<AbBuildInfo>> abName2AssetDic = new Dictionary<string, List<AbBuildInfo>>();
            int cc = 0;
            foreach (var abInfo in _assetBuildInfos)
            {
                if (!abInfo.Value.IsSingle)
                    continue;
                string abName;
                if (abInfo.Value.Path.EndsWith(".unity"))
                {
                    abInfo.Value.SetAsScene();
                    abName = abInfo.Value.Path;
                }
                else
                {
                    AbBuildInfo info = abInfo.Value;
                    List<string> parents = new List<string>();
                    foreach (var abBuildInfo in info.Parents)
                    {
                        if (!parents.Contains(abBuildInfo.AbName))
                            parents.Add(abBuildInfo.AbName);
                    }

                    abName = GetMixAbNameInfo(parents);
                }

                cc++;


                if (abName2AssetDic.ContainsKey(abName))
                {
                    abName2AssetDic[abName].Add(abInfo.Value);
                }
                else
                {
                    abName2AssetDic.Add(abName, new List<AbBuildInfo>() { abInfo.Value });
                }
            }

            Debug.Log("非顶层资源单包数量：" + cc);

            //默认进行单包合并操作

            //对于只有一个资源的单包合并名，恢复成单包名
            StringBuilder linker = new StringBuilder();
            foreach (var list in abName2AssetDic)
            {
                if (list.Value.Count == 1)
                {
                    list.Value[0].SetABName(GetAssetBundleName(list.Value[0].Path,
                        list.Value[0].IsInTopRes
                            ? BuilderUtility.AssetSourceType.GameSource
                            : BuilderUtility.AssetSourceType.ArtSource));
                }
                else
                {
                    if (doSingleCombine)
                    {
                        List<string> abs = new List<string>(list.Key.Split(abSep));
                        abs.Sort();
                        linker.Clear();
                        foreach (string ab in abs)
                        {
                            linker.Append(ab);
                        }

                        string abName = "artcombine_" + BuilderUtility.GetStringHash(linker.ToString());
                        foreach (var info in list.Value)
                        {
                            info.SetABName(abName);
                        }
                    }
                    else
                    {

                        foreach (var info in list.Value)
                        {
                            info.SetABName(GetAssetBundleName(
                                info.Path, info.IsInTopRes ? BuilderUtility.AssetSourceType.GameSource : BuilderUtility.AssetSourceType.ArtSource));
                        }
                    }
                }
            }
            #endregion


            //ab到子资源的映射
            _abName2AssetsInfos = new Dictionary<string, List<AbBuildInfo>>();
            List<string> combinelist = new List<string>();
            foreach (var info in _assetBuildInfos)
            {
                if (string.IsNullOrEmpty(info.Value.AbName))
                {
                    Debug.LogError("cannot use auto with emptyname:::path:" + info.Value.Path);
                    continue;
                }

                if (info.Value.IsSingle && !combinelist.Contains(info.Value.AbName))
                    combinelist.Add(info.Value.AbName);
                string abName = info.Value.AbName.ToLower();

                if (_abName2AssetsInfos.ContainsKey(abName))
                {
                    _abName2AssetsInfos[abName].Add(info.Value);
                }
                else
                {
                    _abName2AssetsInfos.Add(abName, new List<AbBuildInfo>() { info.Value });
                }
            }
            Debug.Log("合并后数量：" + combinelist.Count);
            combinelist.Clear();


            /*
             * 主动记录AB包依赖
             */
            _abDeps = new Dictionary<string, List<string>>();
            foreach (var info in _abName2AssetsInfos)
            {
                if (!_abDeps.ContainsKey(info.Key))
                    _abDeps.Add(info.Key, new List<string>());
                foreach (var value in info.Value)
                {
                    foreach (var child in value.DirectChilds)
                    {
                        if (child.OnlyRecord)
                            continue;
                        if (!_abDeps[info.Key].Contains(child.AbName) && !info.Key.Equals(child.AbName))
                            _abDeps[info.Key].Add(child.AbName);
                    }
                }
                _abDeps[info.Key].Sort();
            }


        }


        internal void BuildAssetBundlesAfterCollect(bool bDevelopment, bool bCleanCache = false)
        {
            HandleOriginAssetBundleOutput(bDevelopment, bCleanCache);
            string outputPath = GetAssetBundleOutputPath();

            //构造buildMap
            var buildMap = GetAbName2PathsDic(out Dictionary<string, List<string>> name2Paths);

            // 将AB信息结构存盘
            try
            {
                StringBuilder stringbuilder = new StringBuilder();
                stringbuilder.Append("资源路径").Append(",").Append("所在AB包").Append(",").Append("AB包总数：" + name2Paths.Count)
                    .Append("\n");
                foreach (var name2Path in name2Paths)
                {
                    foreach (var path in name2Path.Value)
                    {
                        stringbuilder.Append(path).Append(",").Append(name2Path.Key).Append("\n");
                    }
                }

                BuilderUtility.ExportCsv("AssetBundleInfo.csv", stringbuilder);
                Debug.Log("<color=green>AssetBundleInfo 可以查看资源所对应的AB包</color>");
            }
            catch (System.Exception e)
            {
                Debug.Log("<color=red>数据导出 失败</color>");
                Debug.Log(e);
            }

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(outputPath, buildMap,
                BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);
            if (manifest != null)
            {

                uint offset = 0;
                if (Builder.Instance.CurBuilderData.bEnableEncrypt)
                {
                    try
                    {
                        string offVal = Builder.Instance.CurBuilderData.abEncryptOffset.Trim();
                        offset = Convert.ToUInt32(offVal);
                    }
                    catch (Exception e)
                    {
                        offset = 0;
                        Debug.LogWarning("AB加密不执行：" + e.ToString());
                    }
                }

                WriteAssetBundleManifest(outputPath, manifest, name2Paths, offset);

                if (Builder.Instance.CurBuilderData.bEnableEncrypt && offset > 0)
                {
                    var start = DateTime.Now;
                    string buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();
                    AssetbundleEncryption.EncryptAssetBundlesByDirectory($"{FileSystem.AssetBundleBuildPath}/{buildTarget}", offset);
                    Debug.Log("AB加密总耗时：" + (DateTime.Now - start).TotalSeconds.ToString("F2") + " 秒");
                }

                CopyAssetBundles();
                AssetDatabase.Refresh();
            }
        }

        void WriteAssetBundleManifest(string path, AssetBundleManifest manifest,
            Dictionary<string, List<string>> buildMap, uint abOffset)
        {
            List<AssetBundleMeta> assetBundleMetas = new List<AssetBundleMeta>();
            // 名字到bundleMeta的表，做环检查的时候会用到
            Dictionary<string, AssetBundleMeta> nameToBundleMetas =
                new Dictionary<string, AssetBundleMeta>();
            foreach (var bundleName in manifest.GetAllAssetBundles())
            {
                AssetBundleMeta assetBundleMeta = new AssetBundleMeta
                {
                    BundleName = bundleName,
                    Hash = manifest.GetAssetBundleHash(bundleName),
                    Dependencies = _abDeps[bundleName].ToArray()//manifest.GetDirectDependencies(bundleName)
                };

#if NEONABYSS_SUPPORT
                if (assetBundleMeta.BundleName == "scriptobject")
                {
                    assetBundleMeta.Dependencies = new string[0];
                }
#endif

                if (!buildMap.ContainsKey(bundleName))
                {
                    Debug.LogError("不存在的bundleName：" + bundleName);
                    continue;
                }

                string[] assetPaths = buildMap[bundleName].ToArray();
                List<string> toRecordAssets = new List<string>();
                for (int j = 0; j < assetPaths.Length; ++j)
                {
                    if (_assetBuildInfos.TryGetValue(assetPaths[j], out AbBuildInfo info))
                    {
                        if (info.IsScene)
                        {
                            toRecordAssets.Add(assetPaths[j]);
                        }
                        else if (info.IsInTopRes)
                        {
                            toRecordAssets.Add(assetPaths[j]);
                        }
                    }
                }

                assetBundleMeta.Assets = toRecordAssets.ToArray();

                assetBundleMetas.Add(assetBundleMeta);
                // 保存名字到bundleMeta的表， 环检查要用到
                nameToBundleMetas[bundleName] = assetBundleMeta;
            }

            FileStream stream = new FileStream(path + "/" + AssetConfig.AssetBundleMeta, FileMode.OpenOrCreate);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((uint)1);
            writer.Write(assetBundleMetas.Count);
            writer.Write(abOffset);
            foreach (var meta in assetBundleMetas)
            {
                writer.Write(meta.BundleName);
                writer.Write(meta.Assets.Length);
                foreach (var assetPath in meta.Assets)
                {
                    if (_assetBuildInfos.TryGetValue(assetPath, out AbBuildInfo info))
                    {
                        if (info.IsScene)
                        {
                            // 场景只用名字索引
                            writer.Write(GetSceneName(assetPath));
                        }
                        else if (info.IsInTopRes)
                        {
                            int index = assetPath.IndexOf(FileSystem.GameResourcePath, StringComparison.Ordinal);
                            if (index >= 0)
                                writer.Write(assetPath.Remove(index, FileSystem.GameResourcePath.Length + 1));
                            else
                                writer.Write(assetPath);
                        }
                        else
                            Debug.LogError($"非顶层的AB信息无法写入：assetPaths[j]:" + assetPath);
                    }
                    else
                        Debug.LogError($"不存在的AB信息无法写入：assetPaths[j]:" + assetPath);
                }

                writer.Write(meta.Dependencies.Length);
                foreach (var dep in meta.Dependencies)
                    writer.Write(dep);
            }

            writer.Close();

            // 环检查
            _filterAbCircle = new HashSet<string>();

            List<string> checkedBundleMetas = new List<string>();
            _abCircles = new List<List<string>>();
            for (int i = 0; i < assetBundleMetas.Count; i++)
            {
                if (_filterAbCircle.Contains(assetBundleMetas[i].BundleName))
                    continue;

                List<string> filter = new List<string>();
                if (CheckDependentRing(assetBundleMetas[i], nameToBundleMetas, checkedBundleMetas, filter))
                {
                    Debug.LogError($"环数量已超过{MAXCircleNum}，退出循环");
                    break;
                }

                foreach (var ab in filter)
                {
                    if (!_filterAbCircle.Contains(ab))
                        _filterAbCircle.Add(ab);
                }

                _filterAbCircle.Add(assetBundleMetas[i].BundleName);
            }

            //输出AB环信息
            ExportAbBlackList();
        }


        /// <summary>
        /// 深度优先检查依赖环
        /// </summary>
        /// <param name="assetBundleMeta">待检查bundle</param>
        /// <param name="allBundleMetas">所有bundle</param>
        /// <param name="checkedBundleMetas">检查过的bundle</param>
        /// <returns></returns>
        bool CheckDependentRing(AssetBundleMeta assetBundleMeta,
            Dictionary<string, AssetBundleMeta> allBundleMetas,
            List<string> checkedBundleMetas, List<string> filter, List<string> nameFilter = null)
        {
            nameFilter = nameFilter ?? new List<string>();
            int start = checkedBundleMetas.IndexOf(assetBundleMeta.BundleName);
            if (start > -1)
            {
                //发现环
                List<string> range = checkedBundleMetas.GetRange(start, checkedBundleMetas.Count - start);
                if (AddAbCircleRnage(range, nameFilter))
                    return true;
                //_abCircles.Add(range);
                return false;
            }

            //如果一个节点的子节点已经参与过环检查，那么这个子节点一定不会诞生新的环，跳过
            if (_filterAbCircle.Contains(assetBundleMeta.BundleName))
                return false;

            checkedBundleMetas.Add(assetBundleMeta.BundleName);
            if (!filter.Contains(assetBundleMeta.BundleName))
                filter.Add(assetBundleMeta.BundleName);
            for (int i = 0; i < assetBundleMeta.Dependencies.Length; i++)
            {
                if (CheckDependentRing(allBundleMetas[assetBundleMeta.Dependencies[i]], allBundleMetas,
                    checkedBundleMetas,
                    filter, nameFilter))
                    return true;
            }

            checkedBundleMetas.Remove(assetBundleMeta.BundleName);
            return false;
        }


        /// <summary>
        /// 添加AB环信息
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="namefilter"></param>
        /// <returns>是否退出</returns>
        bool AddAbCircleRnage(List<string> circle, List<string> namefilter)
        {
            string sep = "&";
            StringBuilder sb = new StringBuilder();
            foreach (var str in circle)
            {
                sb.Append(str).Append(sep);
            }

            if (namefilter.Contains(sb.ToString()))
            {
                return false;
            }
            else
            {
                for (int off = 0; off < circle.Count; off++)
                {
                    sb.Clear();
                    for (int i = off; i < circle.Count; ++i)
                    {
                        sb.Append(circle[i]).Append(sep);
                    }

                    for (int i = 0; i < off; ++i)
                    {
                        sb.Append(circle[i]).Append(sep);
                    }

                    if (namefilter.Contains(sb.ToString()))
                    {
                        Debug.LogError("Namefilter 已存在" + sb.ToString());
                    }
                    else
                    {
                        namefilter.Add(sb.ToString());
                    }
                }
            }

            _abCircles.Add(circle);
            return _abCircles.Count > MAXCircleNum;
        }


        /// <summary>
        /// 拷贝AB数据
        /// </summary>
        internal void CopyAssetBundles()
        {
            if (Directory.Exists(FileSystem.StreamAssetBundlePath))
                BuilderUtility.ClearAllByPath(FileSystem.StreamAssetBundlePath);
            AssetDatabase.Refresh();
            Directory.CreateDirectory(FileSystem.StreamAssetBundlePath);

            string buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString();

            string srcPath = $"{FileSystem.AssetBundleBuildPath}/{buildTarget}";
            if (!Directory.Exists(srcPath))
            {
                Debug.LogWarning("不存在AB包，无法复制");
                return;
            }

            using (FileTree fileTree =
                FileTree.CreateWithExcludeFilter(srcPath,
                    new[] { ".manifest" }))
            {
                foreach (FileInfo file in fileTree.GetAllFiles())
                {
                    if (file.Name != buildTarget)
                    {
                        file.CopyTo($"{FileSystem.StreamAssetBundlePath}/{file.Name}");
                    }
                }
            }

            AssetDatabase.Refresh();

            if (Builder.Instance.CurBuilderData != null && Builder.Instance.CurBuilderData.buildType == BuilderUtility.BuildType.Development)
            {
                string target = $"{FileSystem.ResourceRoot}/AssetBundles";
                if (Directory.Exists(target))
                    Directory.Delete(target, true);
                LoaderUtilities.CopyDirectory(FileSystem.StreamAssetBundlePath, target);
            }
        }


        void ExportAbBlackList()
        {
            string separation = "->";
            if (_abCircles == null || _abCircles.Count == 0)
            {
                return;
            }

            StringBuilder abCircleList = new StringBuilder();
            abCircleList.Append("AB环信息,具体数据,依赖关系\n");
            Dictionary<string, List<string>> infoItem = new Dictionary<string, List<string>>();
            for (int i = 0; i < _abCircles.Count; ++i)
            {
                //当前的AB环
                var curList = _abCircles[i];
                string name = "";
                List<string> nameOrder = new List<string>();
                for (int j = 0, l = curList.Count; j < l; ++j)
                {
                    //当前处理的节点
                    string cur = curList[j];
                    string next;
                    next = (j == curList.Count - 1) ? curList[0] : curList[j + 1];

                    //当前next中资源在cur的路径关系

                    //cur name:A->B 
                    string curName = cur + separation + next;
                    nameOrder.Add(curName);
                    name += cur + separation;
                    if (infoItem.ContainsKey(curName))
                    {
                        continue;
                    }

                    infoItem.Add(curName, new List<string>());

                    foreach (var asset in _abName2AssetsInfos[cur])
                    {
                        var depends = asset.GetChilds();

                        if (depends != null && depends.Count > 0)
                        {
                            foreach (var dep in depends)
                            {
                                if (dep.OnlyRecord)
                                    continue;

                                if (dep.AbName.Equals(next))
                                {
                                    //出现了当前的依赖项，依赖到了目标AB
                                    infoItem[curName].Add(asset.Path + "==依赖==>" + dep.Path);
                                }
                            }
                        }
                    }

                    if (infoItem[curName].Count == 0)
                    {
                        Debug.LogWarning("无法找到对应AB依赖关系，可能为mat资源残留的错误引用：" + curName);
                    }
                }

                name += curList[0];
                abCircleList.Append(name + "\n");
                foreach (var p2p in nameOrder)
                {
                    foreach (var path in infoItem[p2p])
                    {
                        abCircleList.Append("  ,").Append(path).Append(",").Append(p2p).Append("\n");
                    }
                }
            }

            BuilderUtility.ExportCsv("AssetbundleCircleList.csv", abCircleList);
            abCircleList.Clear();
            _abCircles.Clear();
        }

        protected virtual string GetAssetBundleName(string assetPath, BuilderUtility.AssetSourceType sourceType)
        {
            string assetBundleName = null;

            if (sourceType == BuilderUtility.AssetSourceType.GameSource)
            {
                assetBundleName = assetPath.Remove(0, AssetConfig.AssetRootPath.Length + 1);
                int dotIndex = assetBundleName.LastIndexOf('.');
                if (dotIndex > 0)
                    return assetBundleName.Substring(0, dotIndex).Replace('/', '_').Replace('.', '_');
                else
                    return assetBundleName.Replace('/', '_').Replace('.', '_');
            }
            else if (sourceType == BuilderUtility.AssetSourceType.ArtSource)
            {
                if (Builder.Instance.CurBuilderData.bundleConfig != null)
                {
                    foreach (var item in Builder.Instance.CurBuilderData.bundleConfig.filters)
                    {
                        if (assetPath.IndexOf(item.filterPath) != -1)
                        {
                            assetBundleName = assetPath.Remove(0, item.filterPath.Length + 1);
                        }
                    }
                }
                else
                {
                    assetBundleName = assetPath.Remove(0, FileSystem.ArtResourcePath.Length + 1);
                }

                int dotIndex = assetBundleName.LastIndexOf('.');
                if (dotIndex > 0)
                    return $"_{assetBundleName.Substring(0, dotIndex).Replace('/', '_').Replace('.', '_')}";
                else
                    return $"_{assetBundleName.Replace('/', '_').Replace('.', '_')}";
            }
            else
            {
                UnityEngine.Debug.LogError("Not support AssetSourceType");
                return null;
            }
        }

        protected virtual void HandleOriginAssetBundleOutput(bool bDevelopment, bool bCleanCache = false)
        {
            string outputPath = GetAssetBundleOutputPath();

            if (bCleanCache && bDevelopment)
            {
                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);
            }
            else
            {
                if (Directory.Exists(outputPath))
                    Directory.Delete(outputPath, true);
                Directory.CreateDirectory(outputPath);
            }
        }
        protected virtual string GetAssetBundleOutputPath()
        {
            return $"{FileSystem.AssetBundleBuildPath}/{EditorUserBuildSettings.activeBuildTarget.ToString()}"; ;
        }

        public class AbBuildInfo
        {
            /// <summary>
            /// ab包名
            /// </summary>
            public string AbName { get; private set; }


            /// <summary>
            /// 资源路径
            /// </summary>
            public string Path { get; private set; }

            /// <summary>
            /// 父物体
            /// </summary>
            public List<AbBuildInfo> Parents;

            /// <summary>
            /// 直接子对象
            /// </summary>
            public List<AbBuildInfo> DirectChilds;

            /// <summary>
            /// 是否是auto进包资源
            /// </summary>
            public bool IsAuto { get; private set; }

            /// <summary>
            /// 是否是单包
            /// </summary>
            public bool IsSingle { get; private set; }

            /// <summary>
            /// 是否是顶层资源
            /// </summary>
            public bool IsInTopRes { get; private set; }

            /// <summary>
            /// 是否是场景资源
            /// </summary>
            public bool IsScene { get; private set; }

            /// <summary>
            /// 是否属于图集子资源
            /// </summary>
            public bool IsInSpriteAtlas { get; private set; }

            /// <summary>
            /// 是否属于环
            /// </summary>
            public bool IsInCircle { get; private set; }

            /// <summary>
            /// 只作为记录
            /// </summary>
            public bool OnlyRecord = false;

            public AbBuildInfo(bool isInTopRes, string assetPath)
            {
                AbName = string.Empty;
                Parents = new List<AbBuildInfo>();
                DirectChilds = new List<AbBuildInfo>();
                IsInTopRes = isInTopRes;
                Path = assetPath;
                IsAuto = false;
                OnlyRecord = false;
            }

            public void AddChild(AbBuildInfo info)
            {
                if (!DirectChilds.Contains(info))
                    DirectChilds.Add(info);
            }

            public List<AbBuildInfo> GetChilds()
            {
                if (DirectChilds.Count > 0)
                    return DirectChilds;
                return null;
            }

            public void SetAsTopRes()
            {
                IsInTopRes = true;
            }

            public void SetCircleInfo(string name)
            {
                AbName = name.ToLower();
                IsInCircle = true;
            }
            public void SetSpriteAtlasInfo(string name)
            {
                AbName = name.ToLower();
                IsInSpriteAtlas = true;
            }

            public void SetAsScene()
            {
                IsScene = true;
            }

            public void SetOnlyRecord(bool enable = true)
            {
                OnlyRecord = enable;
            }

            public void SetABName(string name, string path = null)
            {
                if (IsInCircle)
                    return;
                name = name?.ToLower();

                if (!string.IsNullOrEmpty(AbName))
                {
                    if (!string.IsNullOrEmpty(name) && !AbName.Equals(name))
                    {
                        //Debug.LogWarning($"重复设置AB包名!原：{AbName}，尝试设为:{name}！！！" + path);
                        AbName = name;
                    }

                    return;
                }

                AbName = name;
            }


            /// <summary>
            /// 获得该对象的子对象所对应的父对象
            /// </summary>
            /// <returns></returns>
            public (AbBuildInfo, List<AbBuildInfo>) GetChildParent()
            {
                //如果本身是GameRes 或 是一个场景资源下，自己就是子的Parent
                if (IsInTopRes || IsScene)
                    return (this, null);
                //非GameRes下，直接给自己的父
                return (null, Parents);
            }

            public void AddMyParent(AbBuildInfo parent)
            {
                if (parent == null)
                    return;
                if (!Parents.Contains(parent))
                    Parents.Add(parent);
            }

            public void AddMyParent(List<AbBuildInfo> parents)
            {
                if (parents == null || parents.Count < 1)
                    return;
                foreach (var abBuildInfo in parents)
                {
                    AddMyParent(abBuildInfo);
                }
            }

            /// <summary>
            /// 最终设置AB名，主要针对非顶层资源
            /// </summary>
            /// <param name="assetPath"></param>
            public void FixAbName(string assetPath)
            {
                /*特殊：环，
                    如果在GameRes或场景下，必已有AB名，否则有问题
                    否则在ArtRaw下
                    在ArtRaw下，
                        如果没有父物体，怎么进来的？
                        如果父物体有多个，父物体AB名字一样？设置包名
                                        ab名字不一样？单包
                        如果只有一个父物体，且父物体无父，auto
                                        ，父物体有父，且父物体ab名字不一样，设为父物体的ab名
                                                                一样，auto
                */
                if (IsScene)
                    AbName = ("scene" + Path.Substring(0, Path.LastIndexOf('.')).Substring(Path.LastIndexOf('/') + 1)).ToLower();
                if (IsInTopRes || IsScene || IsInCircle || IsInSpriteAtlas)
                {
                    if (AbName == null)
                    {
                        if (System.IO.Path.GetExtension(assetPath).Equals(""))
                            Debug.LogWarning("不正常的AB名：检测到文件夹：" + assetPath);
                        else
                            Debug.LogError("不正常的AB名：Gamres下资源无AB名：" + assetPath);
                    }

                    return;
                }

                if (Parents == null || Parents.Count == 0)
                {
                    Debug.LogError("不正常的Parent：ArtRaw下资源无父物体：" + assetPath);
                    AbName = string.Empty;
                    return;
                }

                //多个父物体
                if (Parents.Count > 1)
                {
                    string abName = Parents[0].AbName;
                    if (abName == null)
                    {
                        Debug.LogError("不正常的AB名：其路径" + Parents[0].Path);
                        return;
                    }

                    for (int i = 1; i < Parents.Count; ++i)
                    {
                        if (!abName.Equals(Parents[i].AbName))
                        {
                            //单包
                            IsSingle = true;
                            return;
                        }
                    }

                    if (Parents[0].IsScene)
                        AbName = ("_scenedeps_" + abName).ToLower();
                    else
                        AbName = abName;
                }
                else
                {
                    if (Parents[0].Parents == null || Parents[0].Parents.Count == 0)
                    {
                        //auto
                        if (Parents[0].IsScene)
                            AbName = ("_scenedeps_" + Parents[0].AbName).ToLower();
                        else
                            AbName = Parents[0].AbName;
                    }
                    else
                    {
                        //todo 判断父的父，如果ab都一样，auto
                        if (Parents[0].IsScene)
                            AbName = ("_scenedeps_" + Parents[0].AbName).ToLower();
                        else
                            AbName = Parents[0].AbName;
                    }
                }
            }
        }
    }

    public static class BuildHelper
    {
        [MenuItem("Assets/Get Asset Path", priority = 3)]
        static void GetAssetPath()
        {
            UnityEngine.Object selObj = Selection.activeObject;

            if (selObj != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(selObj);
                int sindex = assetPath.IndexOf(AssetConfig.AssetRootPath);
                int eindex = assetPath.LastIndexOf('.');
                if (sindex >= 0 && eindex >= 0)
                {
                    string resPath = assetPath.Substring(sindex + AssetConfig.AssetRootPath.Length + 1);
                    if (selObj is Sprite)
                        resPath += $"#{selObj.name}";
                    Debug.Log($"Asset path is {resPath}");
                    EditorGUIUtility.systemCopyBuffer = resPath;
                }
                else if (assetPath.StartsWith("Packages/"))
                {
                    Debug.Log($"Packages Asset path is {assetPath}");
                    EditorGUIUtility.systemCopyBuffer = assetPath;
                }
                else
                {
                    Debug.Log("This is not a Asset file!");
                }
            }
        }
    }
}
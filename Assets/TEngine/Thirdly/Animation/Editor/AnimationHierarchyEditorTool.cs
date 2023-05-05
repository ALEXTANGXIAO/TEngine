/// INFORMATION
/// 
/// Project: Chloroplast Games Framework
/// Game: Chloroplast Games Framework
/// Date: 02/05/2017
/// Author: Chloroplast Games
/// Web: http://www.chloroplastgames.com
/// Programmers:  David Cuenca Diez
/// Description: Tool that allows change the path of hierarchy from a animation from an animation clip.
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Reflection;
using UnityEngine.Profiling;

// Local Namespace
namespace CGF.Editor.Tools
{
    public class PathData
    {
        public string ClipName { get; }
        public string CurrentPath { get; set; }
        public string LastPath { get; set; }
        public AnimationClip Clip { get; set; }

        public EditorCurveBinding EditorCurve;

        public PathData(string clipName, string currentPath, string lastPath, EditorCurveBinding editorCurve, AnimationClip clip)
        {
            ClipName = clipName;
            CurrentPath = currentPath;
            LastPath = lastPath;
            EditorCurve = editorCurve;
            Clip = clip;
        }
    }


    /// \english
    /// <summary>
    /// Tool that allows change the path of hierarchy from a animation from an animation clip.
    /// </summary>
    /// \endenglish
    /// \spanish
    /// <summary>
    /// Herramienta que permite canviar la ruta de la jerarqu韆 de las animaciones de un clip de animaci髇.
    /// </summary>
    /// \endspanish
    public class CGFAnimationHierarchyEditorTool : EditorWindow
    {
        #region Public Variables

        #endregion


        #region Private Variables

        private enum TypeGameObject
        {
            None,

            GameObject,

            /*AnimationClip,

            RuntimeAnimatorController*/
        };

        private TypeGameObject _currentSelectionGameObject;

        private int index;

        private List<GameObject> _gameObjects;

        private Animator _animatorObject;

        private Animator _animatorObject2;

        private List<PathData> _pathDataList;

        private RuntimeAnimatorController _myruntime;

        private List<AnimationClip> _currentAnimationClips;

        private List<AnimationClip> _myanimationClips;

        private List<string> _clipNames = new List<string>();

        private string[] _modes = new string[] { "Path", "GameObject" };

        private int _selectedMode = 0;

        private string _replacePath = "";

        private GameObject _replaceGameObject;

        private string _replacementPath = "";

        private GameObject _replacementGameObject;

        private Vector2 scrollPosContent;

        #endregion


        #region Main Methods

        [MenuItem("Tools/Animation Hierarchy Editor Tool")]
        private static void ShowWindow()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(CGFAnimationHierarchyEditorTool), false, "Animation Hierarchy Editor Tool", true);

            window.minSize = new Vector2(1024, 250);
        }

        public CGFAnimationHierarchyEditorTool()
        {
            _currentSelectionGameObject = new TypeGameObject();

            _currentSelectionGameObject = TypeGameObject.None;

            _myanimationClips = new List<AnimationClip>();

            _gameObjects = new List<GameObject>();
        }

        private void OnEnable()
        {
            ResetSelection();
        }

        private void OnSelectionChange()
        {
            ResetSelection();

            this.Repaint();
        }

        private void ResetSelection()
        {
            _myanimationClips.Clear();

            _clipNames.Clear();

            _animatorObject = null;

            _myruntime = null;

            _animatorObject2 = null;

            index = 0;

            if (Selection.activeGameObject is GameObject)
            {
                _currentSelectionGameObject = TypeGameObject.GameObject;
            }
            else
            {
                _currentSelectionGameObject = TypeGameObject.None;
            }

            DrawSeletedAnimator();
        }

        private void DrawSeletedAnimator()
        {
            if (_currentSelectionGameObject != TypeGameObject.GameObject)
            {
                return;
            }

            _animatorObject2 = Selection.activeGameObject.GetComponent<Animator>();

            if (_animatorObject2 == null)
            {
                _currentSelectionGameObject = TypeGameObject.None;
                return;
            }

            _animatorObject = _animatorObject2;

            if (_animatorObject2.runtimeAnimatorController == null)
            {
                _currentSelectionGameObject = TypeGameObject.None;
                return;
            }

            _myruntime = _animatorObject2.runtimeAnimatorController;

            if (_myruntime.animationClips.Length == 0)
            {
                _currentSelectionGameObject = TypeGameObject.None;
                return;
            }

            foreach (AnimationClip i in _myruntime.animationClips)
            {
                _myanimationClips.Add(i);
            }

            _clipNames.Add("All Clips");

            foreach (AnimationClip e in _myanimationClips)
            {
                _clipNames.Add(e.name);
            }

            if (_myanimationClips.Count > 0)
            {
                _currentAnimationClips = _myanimationClips;
            }
            else
            {
                _currentAnimationClips = null;
            }

            FillModel();
        }

        private void DrawGui()
        {
            bool animations = true;

            EditorGUILayout.Space();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Selected Animator", GUILayout.Width(170));

            GUI.enabled = false;

            _animatorObject = ((Animator)EditorGUILayout.ObjectField(_animatorObject, typeof(Animator), true, GUILayout.Width(300)));

            GUILayout.FlexibleSpace();

            GUI.enabled = _currentSelectionGameObject == TypeGameObject.GameObject;

            GUILayout.Label("Bulk Replace Mode");

            _selectedMode = EditorGUILayout.Popup(_selectedMode, _modes);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = false;

            GUILayout.Label("Selected Animator Controller", GUILayout.Width(170));

            _myruntime = ((RuntimeAnimatorController)EditorGUILayout.ObjectField(_myruntime, typeof(RuntimeAnimatorController), true, GUILayout.Width(300)));

            GUILayout.FlexibleSpace();

            GUI.enabled = _currentSelectionGameObject == TypeGameObject.GameObject;

            switch (_selectedMode)
            {
                case 0:

                    GUILayout.Label("Path");

                    _replacePath = EditorGUILayout.TextField(_replacePath);

                    break;

                case 1:

                    GUILayout.Label("GameObject");

                    _replaceGameObject = (GameObject)EditorGUILayout.ObjectField(_replaceGameObject, typeof(GameObject), true);

                    break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            GUILayout.Label("Selected Animation Clip", GUILayout.Width(170));

            switch (_currentSelectionGameObject)
            {
                case TypeGameObject.GameObject:

                    GUI.enabled = true;

                    index = EditorGUILayout.Popup(index, _clipNames.ToArray(), GUILayout.Width(300));

                    if (index == 0)
                    {
                        _currentAnimationClips = _myanimationClips;
                    }
                    else
                    {
                        _currentAnimationClips = new List<AnimationClip>() { _myanimationClips[index - 1] };
                    }

                    break;
                case TypeGameObject.None:

                    GUI.enabled = false;

                    EditorGUILayout.Popup(index, new string[] { "" }, GUILayout.Width(300));

                    animations = false;

                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                FillModel();
            }

            GUI.enabled = _currentSelectionGameObject == TypeGameObject.GameObject;

            GUILayout.FlexibleSpace();

            GUILayout.Label("Replacement");

            switch (_selectedMode)
            {
                case 0:

                    _replacementPath = EditorGUILayout.TextField(_replacementPath);

                    break;

                case 1:

                    _replacementGameObject = (GameObject)EditorGUILayout.ObjectField(_replacementGameObject, typeof(GameObject), true);

                    break;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Replace", GUILayout.Width(205)))
            {
                Replace();
            }

            _gameObjects.Clear();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal("Toolbar");

            GUILayout.Label("Animation Clip", GUILayout.Width(188));

            GUILayout.Label("Property");

            GUILayout.Label("Path");

            GUILayout.Label("Object", GUILayout.Width(150));

            if (GUILayout.Button("Apply All", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                foreach (var pathData in _pathDataList)
                {
                    UpdatePath(pathData);
                }

                EditorGUI.FocusTextInControl(null);
            }

            if (GUILayout.Button("Revert All", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                foreach (var pathData in _pathDataList)
                {
                    Revert(pathData);
                }

                EditorGUI.FocusTextInControl(null);
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (_currentSelectionGameObject != TypeGameObject.None)
            {
                if (_pathDataList != null)
                {
                    scrollPosContent = EditorGUILayout.BeginScrollView(scrollPosContent);

                    foreach (var pathData in _pathDataList)
                    {
                        GUICreatePathItem(pathData);
                    }

                    EditorGUILayout.EndScrollView();
                }
            }

            if (!animations)
            {
                EditorGUILayout.BeginHorizontal();

                GUI.enabled = false;

                EditorGUILayout.LabelField(new GUIContent(EditorGUIUtility.ObjectContent(null, typeof(Animator)).image), GUILayout.Width(16));

                EditorGUILayout.LabelField("Property");

                EditorGUILayout.TextField("SomeNewObject");

                EditorGUILayout.ObjectField(null, typeof(GameObject), true);

                GUILayout.Button("Apply", GUILayout.Width(60));

                GUILayout.Button("Revert", GUILayout.Width(60));

                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();

                GUI.enabled = true;

                GUILayout.FlexibleSpace();

                EditorGUILayout.HelpBox("Please select a GameObject or Prefab with an Animator, Animation Clip or an Animator Controller.", MessageType.Info);

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
        }

        private void Revert(PathData pathData)
        {
            pathData.CurrentPath = pathData.LastPath;

            UpdatePath(pathData);
        }

        private void Replace()
        {
            switch (_selectedMode)
            {
                case 0:

                    if (string.IsNullOrEmpty(_replacePath))
                    {
                        return;
                    }

                    foreach (var pathData in _pathDataList)
                    {
                        pathData.CurrentPath = pathData.CurrentPath.Replace(_replacePath, _replacementPath);
                    }

                    break;

                case 1:

                    if (_replaceGameObject == null || _replacementGameObject == null)
                    {
                        return;
                    }

                    foreach (var pathData in _pathDataList)
                    {
                        if (pathData.CurrentPath.Equals(ChildPath(_replaceGameObject)))
                        {
                            pathData.CurrentPath = ChildPath(_replacementGameObject);
                        }
                    }

                    break;
            }
        }

        private void OnGUI()
        {
            DrawGui();
        }

        private void GUICreatePathItem(PathData pathData)
        {
            GameObject newObj;

            GameObject obj;

            obj = FindObjectInRoot(pathData.CurrentPath);

            EditorGUILayout.BeginHorizontal();

            GUIStyle propertyNameStyle = new GUIStyle(EditorStyles.label);
            propertyNameStyle.richText = true;

            GUIStyle pathNameStyle = new GUIStyle(EditorStyles.textField);

            if (obj == null || obj.GetComponent(pathData.EditorCurve.type) == null)
            {
                propertyNameStyle.normal.textColor = Color.yellow;

                pathNameStyle.normal.textColor = Color.yellow;
            }

            EditorGUILayout.LabelField(pathData.ClipName, GUILayout.Width(280));

            GUILayout.Space(-90);

            var lastPath = pathData.CurrentPath.Split('/').Last();
            var gameObjectName = string.IsNullOrEmpty(lastPath) ? obj?.name : lastPath;
            EditorGUILayout.LabelField(new GUIContent(EditorGUIUtility.ObjectContent(null, pathData.EditorCurve.type).image), GUILayout.Width(16));
            EditorGUILayout.LabelField(
                $"<i>{gameObjectName}</i> - " +
                $"{string.Join(" ", Regex.Split(pathData.EditorCurve.type.Name, @"(?<!^)(?=[A-Z])"))} - " +
                $"<b>{ObjectNames.NicifyVariableName(pathData.EditorCurve.propertyName)}</b>",
                propertyNameStyle
            );

            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton);


            if (!pathData.CurrentPath.Equals(pathData.EditorCurve.path))
            {
                buttonStyle.fontStyle = FontStyle.Bold;

                pathNameStyle.fontStyle = FontStyle.Bold;
            }

            pathData.CurrentPath = EditorGUILayout.TextField(pathData.CurrentPath, pathNameStyle);

            Color standardColor = GUI.color;

            if (obj != null)
            {
                GUI.color = Color.white;
            }

            if (obj == null || obj.GetComponent(pathData.EditorCurve.type) == null)
            {
                GUI.color = Color.yellow;
            }

            newObj = (GameObject)EditorGUILayout.ObjectField(obj, typeof(GameObject), true, GUILayout.Width(150));

            if (obj != null)
            {
                _gameObjects.Add(obj);
            }

            GUI.color = standardColor;

            GUI.enabled = true;

            buttonStyle.fontSize = 11;

            buttonStyle.fixedHeight = 18;

            buttonStyle.fixedWidth = 68;

            if (GUILayout.Button("Apply", buttonStyle) && !pathData.CurrentPath.Equals(pathData.EditorCurve.path))
            {
                UpdatePath(pathData);

                EditorGUI.FocusTextInControl(null);
            }

            buttonStyle.fontStyle = !pathData.LastPath.Equals(pathData.EditorCurve.path) ? FontStyle.Bold : FontStyle.Normal;

            if (GUILayout.Button("Revert", buttonStyle) && pathData.CurrentPath.Equals(pathData.EditorCurve.path))
            {
                Revert(pathData);
            }

            EditorGUILayout.EndHorizontal();

            try
            {
                if (obj != newObj)
                {
                    pathData.CurrentPath = ChildPath(newObj);
                }
            }
            catch (UnityException ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        private void OnInspectorUpdate()
        {
            this.Repaint();
        }

        private void FillModel()
        {
            try
            {
                _pathDataList = new List<PathData>();

                foreach (var currentAnimationClip in _currentAnimationClips)
                {
                    var pathDataListByClip = FillModelWithCurves(currentAnimationClip.name, AnimationUtility.GetCurveBindings(currentAnimationClip), currentAnimationClip);

                    var pathDataListByObjectReference = FillModelWithCurves(currentAnimationClip.name, AnimationUtility.GetObjectReferenceCurveBindings(currentAnimationClip),
                        currentAnimationClip);

                    _pathDataList.AddRange(pathDataListByClip);

                    _pathDataList.AddRange(pathDataListByObjectReference);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        private List<PathData> FillModelWithCurves(string clipName, EditorCurveBinding[] curves, AnimationClip clip)
        {
            var pathDataList = new List<PathData>();

            foreach (var curve in curves)
            {
                var pathData = _pathDataList?.Find(pd => pd.EditorCurve.Equals(curve) && pd.ClipName == clipName);

                if (pathData != null)
                {
                    pathData.EditorCurve = curve;
                    pathDataList.Add(pathData);
                }
                else
                {
                    pathDataList.Add(new PathData(clipName, curve.path, curve.path, curve, clip));
                }
            }

            return pathDataList;
        }

        private void UpdatePath(PathData pathData)
        {
            if (pathData.CurrentPath.Equals(pathData.EditorCurve.path))
            {
                return;
            }

            pathData.LastPath = pathData.EditorCurve.path;

            AssetDatabase.StartAssetEditing();

            AnimationClip animationClip = pathData.Clip;

            Undo.RecordObject(animationClip, "Animation Hierarchy Change");

            AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, pathData.EditorCurve);

            ObjectReferenceKeyframe[] objectReferenceCurve = AnimationUtility.GetObjectReferenceCurve(animationClip, pathData.EditorCurve);

            if (curve != null)
            {
                AnimationUtility.SetEditorCurve(animationClip, pathData.EditorCurve, null);
            }
            else
            {
                AnimationUtility.SetObjectReferenceCurve(animationClip, pathData.EditorCurve, null);
            }

            pathData.EditorCurve.path = pathData.CurrentPath;

            if (curve != null)
            {
                AnimationUtility.SetEditorCurve(animationClip, pathData.EditorCurve, curve);
            }
            else
            {
                AnimationUtility.SetObjectReferenceCurve(animationClip, pathData.EditorCurve, objectReferenceCurve);
            }

            AssetDatabase.StopAssetEditing();

            EditorUtility.ClearProgressBar();

            FillModel();

            this.Repaint();
        }

        private GameObject FindObjectInRoot(string path)
        {
            if (_animatorObject == null)
            {
                return null;
            }

            Transform child = _animatorObject.transform.Find(path);

            if (child != null)
            {
                return child.gameObject;
            }

            else
            {
                return null;
            }
        }

        private string ChildPath(GameObject obj, bool sep = false)
        {
            if (_animatorObject == null)
            {
                throw new UnityException("Please assign Referenced Animator (Root) first!");
            }

            if (obj == _animatorObject.gameObject)
            {
                return "";
            }

            else
            {
                if (obj.transform.parent == null)
                {
                    throw new UnityException("Object must belong to " + _animatorObject.ToString() + "!");
                }
                else
                {
                    return ChildPath(obj.transform.parent.gameObject, true) + obj.name + (sep ? "/" : "");
                }
            }
        }

        #endregion


        #region Utility Methods

        #endregion


        #region Utility Events

        #endregion
    }
}


namespace TEngine.Editor
{
    class AnimationOpt
    {
        static Dictionary<uint, string> _FLOAT_FORMAT;
        static MethodInfo getAnimationClipStats;
        static FieldInfo sizeInfo;
        static object[] _param = new object[1];

        static AnimationOpt()
        {
            _FLOAT_FORMAT = new Dictionary<uint, string>();
            for (uint i = 1; i < 6; i++)
            {
                _FLOAT_FORMAT.Add(i, "f" + i.ToString());
            }

            Assembly asm = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            getAnimationClipStats = typeof(AnimationUtility).GetMethod(
                "GetAnimationClipStats",
                BindingFlags.Static | BindingFlags.NonPublic
            );
            Type aniclipstats = asm.GetType("UnityEditor.AnimationClipStats");
            sizeInfo = aniclipstats.GetField("size", BindingFlags.Public | BindingFlags.Instance);
        }

        AnimationClip _clip;
        string _path;

        public string path
        {
            get { return _path; }
        }

        public long originFileSize { get; private set; }

        public int originMemorySize { get; private set; }

        public int originInspectorSize { get; private set; }

        public long optFileSize { get; private set; }

        public int optMemorySize { get; private set; }

        public int optInspectorSize { get; private set; }

        public AnimationOpt(string path, AnimationClip clip)
        {
            _path = path;
            _clip = clip;
            _GetOriginSize();
        }

        void _GetOriginSize()
        {
            originFileSize = _GetFileZie();
            originMemorySize = _GetMemSize();
            originInspectorSize = _GetInspectorSize();
        }

        void _GetOptSize()
        {
            optFileSize = _GetFileZie();
            optMemorySize = _GetMemSize();
            optInspectorSize = _GetInspectorSize();
        }

        long _GetFileZie()
        {
            FileInfo fi = new FileInfo(_path);
            return fi.Length;
        }

        int _GetMemSize()
        {
            return (int)Profiler.GetRuntimeMemorySizeLong(_clip);
        }

        int _GetInspectorSize()
        {
            _param[0] = _clip;
            var stats = getAnimationClipStats.Invoke(null, _param);
            return (int)sizeInfo.GetValue(stats);
        }

        void _OptmizeAnimationScaleCurve()
        {
            if (_clip != null)
            {
                //去除scale曲线
                foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(_clip))
                {
                    string name = theCurveBinding.propertyName.ToLower();
                    if (name.Contains("scale"))
                    {
                        AnimationUtility.SetEditorCurve(_clip, theCurveBinding, null);
                        Debug.LogFormat("关闭{0}的scale curve", _clip.name);
                    }
                }
            }
        }

        void _OptmizeAnimationFloat_X(uint x)
        {
            if (_clip != null && x > 0)
            {
                //浮点数精度压缩到f3
                AnimationClipCurveData[] curves = null;
#pragma warning disable CS0618
                curves = AnimationUtility.GetAllCurves(_clip);
#pragma warning restore CS0618
                Keyframe key;
                Keyframe[] keyFrames;
                string floatFormat;
                if (_FLOAT_FORMAT.TryGetValue(x, out floatFormat))
                {
                    if (curves != null && curves.Length > 0)
                    {
                        for (int ii = 0; ii < curves.Length; ++ii)
                        {
                            AnimationClipCurveData curveDate = curves[ii];
                            if (curveDate.curve == null || curveDate.curve.keys == null)
                            {
                                //Debug.LogWarning(string.Format("AnimationClipCurveData {0} don't have curve; Animation name {1} ", curveDate, animationPath));
                                continue;
                            }

                            keyFrames = curveDate.curve.keys;
                            for (int i = 0; i < keyFrames.Length; i++)
                            {
                                key = keyFrames[i];
                                key.value = float.Parse(key.value.ToString(floatFormat));
                                key.inTangent = float.Parse(key.inTangent.ToString(floatFormat));
                                key.outTangent = float.Parse(key.outTangent.ToString(floatFormat));
                                key.inWeight = float.Parse(key.inWeight.ToString(floatFormat));
                                key.outWeight = float.Parse(key.outWeight.ToString(floatFormat));
                                keyFrames[i] = key;
                            }

                            curveDate.curve.keys = keyFrames;
                            _clip.SetCurve(
                                curveDate.path,
                                curveDate.type,
                                curveDate.propertyName,
                                curveDate.curve
                            );
                        }
                    }
                }
                else
                {
                    Debug.LogErrorFormat("目前不支持{0}位浮点", x);
                }
            }
        }

        public void Optimize(bool scaleOpt, uint floatSize)
        {
            if (scaleOpt)
            {
                _OptmizeAnimationScaleCurve();
            }

            _OptmizeAnimationFloat_X(floatSize);
            _GetOptSize();
        }

        public void Optimize_Scale_Float3()
        {
            Optimize(false, 3);
        }

        public void LogOrigin()
        {
            _logSize(originFileSize, originMemorySize, originInspectorSize);
        }

        public void LogOpt()
        {
            _logSize(optFileSize, optMemorySize, optInspectorSize);
        }

        public void LogDelta()
        {
        }

        void _logSize(long fileSize, int memSize, int inspectorSize)
        {
            Debug.LogFormat(
                "{0} \nSize=[ {1} ]",
                _path,
                string.Format(
                    "FSize={0} ; Mem->{1} ; inspector->{2}",
                    EditorUtility.FormatBytes(fileSize),
                    EditorUtility.FormatBytes(memSize),
                    EditorUtility.FormatBytes(inspectorSize)
                )
            );
        }
    }

    public class OptimizeAnimationClipTool
    {
        static List<AnimationOpt> _AnimOptList = new List<AnimationOpt>();
        static List<string> _Errors = new List<string>();
        static int _Index = 0;

        [MenuItem("Assets/Animation/裁剪浮点数去除Scale")]
        public static void Optimize()
        {
            _AnimOptList = FindAnims();
            if (_AnimOptList.Count > 0)
            {
                _Index = 0;
                _Errors.Clear();
                EditorApplication.update = ScanAnimationClip;
            }
        }

        private static void ScanAnimationClip()
        {
            AnimationOpt _AnimOpt = _AnimOptList[_Index];
            bool isCancel = EditorUtility.DisplayCancelableProgressBar(
                "优化AnimationClip",
                _AnimOpt.path,
                (float)_Index / (float)_AnimOptList.Count
            );
            _AnimOpt.Optimize_Scale_Float3();
            _Index++;
            if (isCancel || _Index >= _AnimOptList.Count)
            {
                EditorUtility.ClearProgressBar();
                Debug.Log(
                    string.Format(
                        "--优化完成--    错误数量: {0}    总数量: {1}/{2}    错误信息↓:\n{3}\n----------输出完毕----------",
                        _Errors.Count,
                        _Index,
                        _AnimOptList.Count,
                        string.Join(string.Empty, _Errors.ToArray())
                    )
                );
                Resources.UnloadUnusedAssets();
                GC.Collect();
                AssetDatabase.SaveAssets();
                EditorApplication.update = null;
                _AnimOptList.Clear();
                _cachedOpts.Clear();
                _Index = 0;
            }
        }

        static Dictionary<string, AnimationOpt> _cachedOpts = new Dictionary<string, AnimationOpt>();

        static AnimationOpt _GetNewAOpt(string path)
        {
            AnimationOpt opt = null;
            if (!_cachedOpts.ContainsKey(path))
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip != null)
                {
                    opt = new AnimationOpt(path, clip);
                    _cachedOpts[path] = opt;
                }
            }

            return opt;
        }

        static List<AnimationOpt> FindAnims()
        {
            string[] guids = null;
            List<string> path = new List<string>();
            List<AnimationOpt> assets = new List<AnimationOpt>();
            UnityEngine.Object[] objs = Selection.GetFiltered(typeof(object), SelectionMode.Assets);
            if (objs.Length > 0)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i].GetType() == typeof(AnimationClip))
                    {
                        string p = AssetDatabase.GetAssetPath(objs[i]);
                        AnimationOpt animopt = _GetNewAOpt(p);
                        if (animopt != null) assets.Add(animopt);
                    }
                    else
                        path.Add(AssetDatabase.GetAssetPath(objs[i]));
                }

                if (path.Count > 0)
                    guids = AssetDatabase.FindAssets(
                        string.Format("t:{0}", typeof(AnimationClip).ToString().Replace("UnityEngine.", "")),
                        path.ToArray()
                    );
                else
                    guids = new string[] { };
            }

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                AnimationOpt animopt = _GetNewAOpt(assetPath);
                if (animopt != null) assets.Add(animopt);
            }

            return assets;
        }
    }
}
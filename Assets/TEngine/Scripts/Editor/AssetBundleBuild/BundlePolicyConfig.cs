using System.Collections.Generic;
using System.IO;
using TEngine.Runtime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Object = UnityEngine.Object;

namespace TEngineCore.Editor
{
    [CreateAssetMenu]
    [MovedFrom(true, null, null, "MakeBuildConfig")]
    public class BundlePolicyConfig : ScriptableObject
    {
        [SerializeField]
        public List<BundleConfigItem> directoryBuild;
        [SerializeField]
        public List<BuildFilter> filters;
        private string _path = TEngine.Constant.Setting.AssetRootPath;
        public void OnEnable()
        {
            if (filters == null || filters.Count == 0)
            {
                List<BuildFilter> list_filter = new List<BuildFilter>();
                BuildFilter filter = new BuildFilter { filterPath = TEngine.Constant.Setting.AssetFilterPath };
                list_filter.Add(filter);
                filters = list_filter;
            }

            if (directoryBuild == null || directoryBuild.Count == 0)
            {
                var dir = new DirectoryInfo(_path);
                var directorise = dir.GetDirectories();
                List<BundleConfigItem> temp = new List<BundleConfigItem>();
                for (int i = 0; i < directorise.Length; i++)
                {
                    if (directorise[i].Name.EndsWith(".meta"))
                    {
                        continue;
                    }
                    BundleConfigItem item = new BundleConfigItem();
                    item.mObject = AssetDatabase.LoadAssetAtPath<Object>(_path + directorise[i].Name);
                    item.buildType = 1;
                    temp.Add(item);
                }
                this.directoryBuild = temp;
            }
        }
    }


    [System.Serializable]
    public class BundleConfigItem
    {
        [SerializeField]
        public Object mObject;

        [SerializeField]
        public int buildType;
    }

    [System.Serializable]
    public class BuildFilter
    {
        [SerializeField]
        public string filterPath;
    }

    [CustomEditor(typeof(BundlePolicyConfig)), CanEditMultipleObjects]
    public class BundlePolicyConfigInspector : UnityEditor.Editor
    {
        private ReorderableList _reorderableList;
        private SerializedProperty prop;

        private ReorderableList _filterableList;
        private SerializedProperty filter;

        private BundlePolicyConfig Configuration { get { return target as BundlePolicyConfig; } }
        private void OnEnable()
        {
            prop = serializedObject.FindProperty("directoryBuild");
            _reorderableList = new ReorderableList(serializedObject, prop);
            _reorderableList.elementHeight = 30;
            _reorderableList.drawElementCallback =
                (rect, index, isActive, isFocused) =>
                {
                    var element = prop.GetArrayElementAtIndex(index);
                    rect.height -= 4;
                    rect.y += 3;
                    EditorGUI.PropertyField(rect, element);
                };

            var defaultColor = GUI.backgroundColor;
            _reorderableList.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
            {

            };
            _reorderableList.drawHeaderCallback = (rect) =>
                EditorGUI.LabelField(rect, prop.displayName);

            //过滤规则
            filter = serializedObject.FindProperty("filters");
            _filterableList = new ReorderableList(serializedObject, filter);
            _filterableList.elementHeight = 30;
            _filterableList.drawElementCallback =
                (rect, index, isActive, isFocused) =>
                {
                    var element = filter.GetArrayElementAtIndex(index);
                    rect.height -= 4;
                    rect.y += 3;
                    EditorGUI.PropertyField(rect, element);
                };

            _filterableList.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
            {

            };
            _filterableList.drawHeaderCallback = (rect) =>
                EditorGUI.LabelField(rect, filter.displayName);
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUILayout.Label("打包配置");
            _reorderableList.DoLayoutList();

            GUILayout.Label("美术资源检测路径");
            _filterableList.DoLayoutList();

            //剔除错误目录
            if (_reorderableList.serializedProperty?.serializedObject != null)
            {
                var s = _reorderableList.serializedProperty.serializedObject.targetObject;
                if (s != null && s is BundlePolicyConfig bundleConfig)
                {
                    List<BundleConfigItem> tDNodes = new List<BundleConfigItem>();
                    foreach (var cur in bundleConfig.directoryBuild)
                    {
                        var path = AssetDatabase.GetAssetPath(cur.mObject);
                        if (!path.StartsWith(FileSystem.GameResourcePath))
                        {
                            Debug.LogError("剔除！不合法的路径:" + path + "，当前只支持GameResources下路径");
                            tDNodes.Add(cur);
                        }
                        else if (path.StartsWith(FileSystem.ArtResourcePath))
                        {
                            Debug.LogWarning(path + "，该路径不会加入Bundle配置");
                            tDNodes.Add(cur);
                        }
                    }

                    foreach (var node in tDNodes)
                    {
                        bundleConfig.directoryBuild.Remove(node);
                    }

                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomPropertyDrawer(typeof(BundleConfigItem))]
    public class CharacterDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                EditorGUIUtility.labelWidth = 50;
                position.height = EditorGUIUtility.singleLineHeight;

                Rect iconRect = new Rect(position)
                {
                    width = 300,
                    height = 20
                };

                Rect nameRect = new Rect(position)
                {
                    width = position.width - 300,
                    x = position.x + 300,
                };

                SerializedProperty Object = property.FindPropertyRelative("mObject");
                SerializedProperty BulidType = property.FindPropertyRelative("buildType");

                Object.objectReferenceValue = EditorGUI.ObjectField(iconRect, Object.objectReferenceValue, typeof(Object), false);
                BulidType.intValue = (int)(BuilderUtility.BundlePolicy)EditorGUI.EnumPopup(nameRect, (BuilderUtility.BundlePolicy)BulidType.intValue);
            }
        }
    }

    [CustomPropertyDrawer(typeof(BuildFilter))]
    public class BuildFilterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                EditorGUIUtility.labelWidth = 50;
                position.height = EditorGUIUtility.singleLineHeight;

                Rect textRect = new Rect(position)
                {
                    width = 400,
                    height = 20
                };

                SerializedProperty filterPath = property.FindPropertyRelative("filterPath");

                filterPath.stringValue = EditorGUI.TextField(textRect, filterPath.stringValue);
            }
        }
    }
}
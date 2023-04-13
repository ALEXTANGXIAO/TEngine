using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YooAsset.Editor;

namespace TEngine.Editor.Inspector
{
    [CustomEditor(typeof(ResourceModule))]
    internal sealed class ResourceModuleInspector : GameFrameworkInspector
    {
        private static readonly string[] ResourceModeNames = new string[] { "EditorSimulateMode (编辑器下的模拟模式)", "OfflinePlayMode (单机模式)", "HostPlayMode (联机运行模式)" };
        private static readonly string[] VerifyLevelNames = new string[] { "Low (验证文件存在)", "Middle (验证文件大小)", "High (验证文件大小和CRC)" };

        private SerializedProperty m_PackageName = null;
        private SerializedProperty m_PlayMode = null;
        private SerializedProperty m_ReadWritePathType = null;
        private SerializedProperty m_VerifyLevel = null;
        private SerializedProperty m_Milliseconds = null;
        private SerializedProperty m_MinUnloadUnusedAssetsInterval = null;
        private SerializedProperty m_MaxUnloadUnusedAssetsInterval = null;
        private SerializedProperty m_DownloadingMaxNum = null;
        private SerializedProperty m_FailedTryAgain = null;

        private int m_ResourceModeIndex = 0;
        private int m_PackageIndex = 0;
        private int m_VerifyIndex = 0;

        private PopupField<string> _buildPackageField;
        private List<string> _buildPackageNames;

        private void OnEnable()
        {
            m_PackageName = serializedObject.FindProperty("packageName");
            m_PlayMode = serializedObject.FindProperty("playMode");
            m_VerifyLevel = serializedObject.FindProperty("verifyLevel");
            m_Milliseconds = serializedObject.FindProperty("milliseconds");

            m_ReadWritePathType = serializedObject.FindProperty("readWritePathType");
            m_MinUnloadUnusedAssetsInterval = serializedObject.FindProperty("minUnloadUnusedAssetsInterval");
            m_MaxUnloadUnusedAssetsInterval = serializedObject.FindProperty("maxUnloadUnusedAssetsInterval");
            m_DownloadingMaxNum = serializedObject.FindProperty("downloadingMaxNum");
            m_FailedTryAgain = serializedObject.FindProperty("failedTryAgain");

            RefreshModes();
            RefreshTypeNames();
        }

        private void RefreshModes()
        {
            m_ResourceModeIndex = m_PlayMode.enumValueIndex > 0 ? m_PlayMode.enumValueIndex : 0;
            m_VerifyIndex = m_VerifyLevel.enumValueIndex > 0 ? m_VerifyLevel.enumValueIndex : 0;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ResourceModule t = (ResourceModule)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
                {
                    EditorGUILayout.EnumPopup("Resource Mode", t.playMode);

                    EditorGUILayout.EnumPopup("VerifyLevel", t.verifyLevel);

                    _buildPackageNames = GetBuildPackageNames();
                    if (_buildPackageNames.Count > 0)
                    {
                        GUILayout.Label(_buildPackageNames[0]);
                    }
                }
                else
                {
                    // 资源模式
                    int selectedIndex = EditorGUILayout.Popup("Resource Mode", m_ResourceModeIndex, ResourceModeNames);
                    if (selectedIndex != m_ResourceModeIndex)
                    {
                        m_ResourceModeIndex = selectedIndex;
                        m_PlayMode.enumValueIndex = selectedIndex;
                    }

                    int selectedVerifyIndex = EditorGUILayout.Popup("VerifyLevel", m_VerifyIndex, VerifyLevelNames);
                    if (selectedVerifyIndex != m_VerifyIndex)
                    {
                        m_VerifyIndex = selectedVerifyIndex;
                        m_VerifyLevel.enumValueIndex = selectedVerifyIndex;
                    }

                    // 包裹名称列表
                    _buildPackageNames = GetBuildPackageNames();
                    if (_buildPackageNames.Count > 0)
                    {
                        int selectedPackageIndex = EditorGUILayout.Popup("Used Packages", m_PackageIndex, _buildPackageNames.ToArray());
                        if (selectedPackageIndex != m_PackageIndex)
                        {
                            m_PackageIndex = selectedPackageIndex;
                            m_PlayMode.enumValueIndex = selectedIndex + 1;
                        }

                        int defaultIndex = GetDefaultPackageIndex(AssetBundleBuilderSettingData.Setting.BuildPackage);
                        _buildPackageField = new PopupField<string>(_buildPackageNames, defaultIndex);
                        _buildPackageField.label = "Build Package";
                        _buildPackageField.style.width = 350;
                        _buildPackageField.RegisterValueChangedCallback(evt =>
                        {
                            AssetBundleBuilderSettingData.IsDirty = true;
                            AssetBundleBuilderSettingData.Setting.BuildPackage = _buildPackageField.value;
                        });
                    }
                    else
                    {
                        GUILayout.Label("Please Create Packages with YooAssets ...!");
                    }
                }

                m_ReadWritePathType.enumValueIndex = (int)(ReadWritePathType)EditorGUILayout.EnumPopup("Read-Write Path Type", t.ReadWritePathType);
            }
            EditorGUI.EndDisabledGroup();

            int milliseconds = EditorGUILayout.DelayedIntField("Milliseconds", m_Milliseconds.intValue);
            if (milliseconds != m_Milliseconds.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.milliseconds = milliseconds;
                }
                else
                {
                    m_Milliseconds.longValue = milliseconds;
                }
            }

            float minUnloadUnusedAssetsInterval = EditorGUILayout.Slider("Min Unload Unused Assets Interval", m_MinUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (Math.Abs(minUnloadUnusedAssetsInterval - m_MinUnloadUnusedAssetsInterval.floatValue) > 0.001f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.MinUnloadUnusedAssetsInterval = minUnloadUnusedAssetsInterval;
                }
                else
                {
                    m_MinUnloadUnusedAssetsInterval.floatValue = minUnloadUnusedAssetsInterval;
                }
            }

            float maxUnloadUnusedAssetsInterval = EditorGUILayout.Slider("Max Unload Unused Assets Interval", m_MaxUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (Math.Abs(maxUnloadUnusedAssetsInterval - m_MaxUnloadUnusedAssetsInterval.floatValue) > 0.001f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.MaxUnloadUnusedAssetsInterval = maxUnloadUnusedAssetsInterval;
                }
                else
                {
                    m_MaxUnloadUnusedAssetsInterval.floatValue = maxUnloadUnusedAssetsInterval;
                }
            }

            float downloadingMaxNum = EditorGUILayout.Slider("Max Downloading Num", m_DownloadingMaxNum.intValue, 1f, 48f);
            if (Math.Abs(downloadingMaxNum - m_DownloadingMaxNum.intValue) > 0.001f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.downloadingMaxNum = (int)downloadingMaxNum;
                }
                else
                {
                    m_DownloadingMaxNum.intValue = (int)downloadingMaxNum;
                }
            }

            float failedTryAgain = EditorGUILayout.Slider("Max FailedTryAgain Count", m_FailedTryAgain.intValue, 1f, 48f);
            if (Math.Abs(failedTryAgain - m_FailedTryAgain.intValue) > 0.001f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.failedTryAgain = (int)failedTryAgain;
                }
                else
                {
                    m_FailedTryAgain.intValue = (int)failedTryAgain;
                }
            }


            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Unload Unused Assets",
                    Utility.Text.Format("{0:F2} / {1:F2}", t.LastUnloadUnusedAssetsOperationElapseSeconds, t.MaxUnloadUnusedAssetsInterval));
                EditorGUILayout.LabelField("Read-Only Path", t.ReadOnlyPath.ToString());
                EditorGUILayout.LabelField("Read-Write Path", t.ReadWritePath.ToString());
                EditorGUILayout.LabelField("Applicable Game Version", t.ApplicableGameVersion ?? "<Unknwon>");
                EditorGUILayout.LabelField("Internal Resource Version", t.InternalResourceVersion.ToString());
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }

        // 构建包裹相关
        private int GetDefaultPackageIndex(string packageName)
        {
            for (int index = 0; index < _buildPackageNames.Count; index++)
            {
                if (_buildPackageNames[index] == packageName)
                {
                    return index;
                }
            }

            AssetBundleBuilderSettingData.IsDirty = true;
            AssetBundleBuilderSettingData.Setting.BuildPackage = _buildPackageNames[0];
            return 0;
        }

        private List<string> GetBuildPackageNames()
        {
            List<string> result = new List<string>();
            foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
            {
                result.Add(package.PackageName);
            }

            return result;
        }
    }
}
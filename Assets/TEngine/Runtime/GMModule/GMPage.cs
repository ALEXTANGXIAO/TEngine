#if ENABLE_GM || UNITY_EDITOR || _DEVELOPMENT_BUILD_
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TEngine
{
    public class GMPage : UnitySingleton<GMPage>
    {
        private static List<string> mGMCommandPages = new List<string>();
        
        private static List<GMDrawer.GMCommandDrawer> mGMCommands = new List<GMDrawer.GMCommandDrawer>();

        private string mCurPage;

        private bool mOpened = false;
        protected override void OnLoad()
        {
            
        }

        private EventSystem mDisabledEventSystem;
        private int mTouchCount = 0;
        private void Update()
        {
            int touchcount = Input.touchCount;
            if (Input.GetKeyDown(KeyCode.BackQuote) || (touchcount == 4 && mTouchCount != touchcount))
            {
                if (IsEditor())
                {
                    if (mOpened)
                    {
                        CLosePage();
                    }
                    else
                    {
                        OpenPage();
                    }
                }
                else
                {
                    OpenPage();
                }
                
            }

            mTouchCount = touchcount;
        }

        private void OpenPage()
        {
            GMDrawer.InitStyle();
            mWindowRect = GMDrawer.SelectWindowRect();
            
            mOpened = true;
            
            mDisabledEventSystem = EventSystem.current;
            EventSystem.current.enabled = false;
        }

        private void CLosePage()
        {
            mOpened = false;
            mDisabledEventSystem.enabled = true;
        }
        
        public static bool IsTypeWithAttributeType(Type inAttributeType, Type inCurType)
        {
            try
            {
                if ((inCurType.Attributes & (TypeAttributes.Interface | TypeAttributes.HasSecurity)) != 0)
                {
                    return false;
                }

                if (inCurType.IsEnum || inCurType.IsPrimitive || inCurType.IsGenericType)
                {
                    return false;
                }

                MethodInfo[] methods = inCurType.GetMethods(BindingFlags.Public | BindingFlags.Static);

                for (int j = 0; j < methods.Length; j++)
                {
                    MethodInfo method = methods[j];
                    Attribute attr = method.GetCustomAttribute(inAttributeType);

                    if (attr == null)
                    {
                        continue;
                    }

                    if (attr is CommonGMCommandAttribute execCommandAttribute)
                    {
                        if (execCommandAttribute.mIsEditorOnly && IsEditor())
                        {
                            continue;
                        }
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
            }

            return false;
        }

        public static bool IsEditor()
        {
            bool isEditor = false;
#if UNITY_EDITOR
            isEditor = true;
#endif
            return isEditor;
        }

        public static void CollectExecCommands()
        {
            Type[] types = null;

            TypeUtility.GetTypesImpl(typeof(CommonGMCommandAttribute),
                "", true, IsTypeWithAttributeType, out types);

            if (types == null)
            {
                return;
            }

            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                for (int j = 0; j < methods.Length; j++)
                {
                    MethodInfo method = methods[j];
                    CommonGMCommandAttribute attr = method.GetCustomAttribute<CommonGMCommandAttribute>();

                    if (attr == null)
                    {
                        continue;
                    }

                    if (attr.mIsEditorOnly && IsEditor())
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(attr.mThread))
                    {
                        attr.mThread = "MainThread";
                    }


                    attr.mThread = string.Intern(attr.mThread);
                    attr.mPage = string.Intern(attr.mPage);
                    attr.mName = string.Intern(attr.mName);
                    attr.mButtonName = string.Intern(attr.mButtonName);

                    if (attr.mIsDrawer)
                    {
                        attr.mCustomDrawCallback = () => method.Invoke(null, null);
                    }

                    if (!mGMCommandPages.Contains(attr.mPage))
                    {
                        mGMCommandPages.Add(attr.mPage);
                    }

                    GMDrawer.GMCommandDrawer drawer = GMDrawer.GetExecCommandDrawer(type, method, attr);
                    mGMCommands.Add(drawer);
                }
            }
        }
        
        static int InternalID = 0x00dbdbdb;
        private Vector2 mGUIScroll;
        public Rect mWindowRect;

        private void OnGUI()
        {
            if (!mOpened)
            {
                return;
            }
            
            int CONST_SIZE = 32;
            GUI.skin.verticalScrollbar.fixedWidth = Screen.width * 0.001f * CONST_SIZE;
            GUI.skin.verticalScrollbarThumb.fixedWidth = Screen.width * 0.001f * CONST_SIZE;
            GUI.skin.label.fontSize = CONST_SIZE;
            GUI.skin.textField.fontSize = CONST_SIZE;
            GUI.skin.button.fontSize = CONST_SIZE;
            GUI.skin.toggle.fontSize = 64;
            GUI.skin.toggle.fixedHeight = 50;
            GUI.skin.toggle.fixedWidth = 50;
            GUI.skin.textArea.fontSize = CONST_SIZE;
            GUI.skin.textArea.richText = true;
            GUILayout.Window(InternalID, mWindowRect, DrawExeCommandWindow, "GM Command");
        }

        void DrawExeCommandWindow(int InWindowID)
        {
            GUILayout.BeginVertical();
           
            string pageName = "Current Page: " + mCurPage;
            GUILayout.Label(pageName, GUILayout.ExpandWidth(true));
            
            GUILayout.EndVertical();
            
            
            mGUIScroll = GUILayout.BeginScrollView(mGUIScroll);
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("box", GUILayout.Width(mWindowRect.width * 0.1f), GUILayout.ExpandHeight(true));
            if (GUILayout.Button("关闭"))
            {
                CLosePage();
            }

            for (int i = mGMCommandPages.Count - 1; i >= 0; i--)
            {
                string str = mGMCommandPages[i];
                if (GUILayout.Button(str))
                {
                    mCurPage = str;
                }
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical();



                for (int i = 0; i < mGMCommands.Count; i++)
                {
                    GMDrawer.GMCommandDrawer command = mGMCommands[i];
                    if (command.mPage == mCurPage)
                    {
                        command.OnGUI();
                    }
                    else
                    {
                        command.Reset();
                    }
                    GUILayout.Space(1.0f);
                }


            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }
    }
}
#endif
#if ENABLE_GM || UNITY_EDITOR || _DEVELOPMENT_BUILD_
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TEngine
{
    public static class GMDrawer
    {
        public static int LABEL_WIDTH = 150;
        public static List<GMCommandDrawer> listExecCommandDrawer = new List<GMCommandDrawer>();
        public static Rect SelectWindowRect()
        {
            return new Rect(0, 0, Screen.width, Screen.height);
        }
        public static void InitStyle()
        {
            EnumParameterDrawer.Style.InitStyle();
        }
        public static GMCommandDrawer GetExecCommandDrawer(Type type, MethodInfo methodInfo, CommonGMCommandAttribute attribute)
        {
            GMCommandDrawer newGmCommandDrawer= new GMCommandDrawer(attribute.mThread, attribute.mPage, attribute.mName, attribute.mButtonName, type, methodInfo, attribute.mCustomDrawCallback);
            listExecCommandDrawer.Add(newGmCommandDrawer);
            return newGmCommandDrawer;
        }
        public class GMCommandDrawer
        {
            public string mThread;
            public string mPage;
            public string mName;
            private bool mHasDone = false;
            private object mExecReult = null;
            private string mButtonName;
            private Type mType;
            private MethodInfo mMethodInfo;
            private Action mCustomDrawCallback;
            private List<ParameterDrawer> mDrawers = new List<ParameterDrawer>();

            public GMCommandDrawer(string thread, string page, string name, string buttonName, Type type, MethodInfo method, Action customDrawCallback)
            {
                mThread = thread;
                mPage = page;
                mName = name;
                mButtonName = buttonName;
                mType = type;
                mMethodInfo = method;
                mCustomDrawCallback = customDrawCallback;
                mExecReult = null;
                if (mMethodInfo != null)
                {
                    ParameterInfo[] parameters = mMethodInfo.GetParameters();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        ParameterInfo param = parameters[i];
                        if (null == param)
                        {
                            continue;
                        }

                        ParameterDrawer drawer = GetDrawer(param.ParameterType, param.Name);

                        if (param.HasDefaultValue)
                        {
                            drawer.SetValue(param.DefaultValue);
                        }

                        if (drawer != null)
                        {
                            mDrawers.Add(drawer);
                        }
                    }
                }
            }

            public void OnGUI()
            {
                if (mCustomDrawCallback != null)
                {
                    mCustomDrawCallback();
                    return;
                }
                DrawDefault();
            }


            public void Reset()
            {
                mHasDone = false;
                mExecReult = null;
            }

            private void DrawDefault()
            {
                
                GUILayout.BeginVertical("box");
                
                string name = string.Format("<color=#FFFF00FF>{0}</color>", mName);
                GUILayout.Label(name);
                
                for (int i = 0; i < mDrawers.Count; i++)
                {
                    mDrawers[i].OnGUI(new Rect(200,200,500,50));
                }

                if (mHasDone)
                { 

                    if (mExecReult != null)
                    {
                        GUILayout.Label(" done result : " + mExecReult.ToString());
                    }
                    else
                    {
                        GUILayout.Label("done!");
                    }
                }

                if (GUILayout.Button(mButtonName ))
                {
                    if (mMethodInfo != null)
                    {
                        List<object> ps = new List<object>();
                        for (int i = 0; i < mDrawers.Count; i++)
                        {
                            ps.Add(mDrawers[i].GetValue());
                        }

                        if (mThread == "MainThread")
                        {
                            mExecReult = mMethodInfo.Invoke(null, ps.ToArray());
                        }
                        else if (mThread == "LogicThread")
                        {
                            TLogger.LogError("LogicThread GM not supported by now");
                        }
                        else if (mThread == "NetThread")
                        {
                            TLogger.LogError("NetThread GM not supported by now");
                        }
                    }

                    
                    mHasDone = true;
                }
                GUILayout.EndVertical();
            }
        }
        public class ParameterDrawer
        {
            public string mName;
            public ParameterDrawer(string name)
            {
                mName = name;
            }
            public virtual void OnGUI(Rect InBoundRect) { throw new NotImplementedException(); }
            public virtual object GetValue() { throw new NotImplementedException(); }
            public virtual void SetValue(object objValue) { }
        }
        public class IntParameterDrawer : ParameterDrawer
        {
            public int mObj;
            public IntParameterDrawer(string name) : base(name)
            {
            }
            public override void OnGUI(Rect InBoundRect)
            {
                int ret = mObj;
                GUILayout.BeginHorizontal();
                GUILayout.Label(mName, GUILayout.Width(LABEL_WIDTH), GUILayout.ExpandWidth(false));
                if (int.TryParse(GUILayout.TextField(ret.ToString()), out ret))
                {
                    mObj = ret;
                }
                GUILayout.EndHorizontal();
            }

            public override object GetValue()
            {
                return mObj;
            }

            public override void SetValue(object objValue)
            {
                mObj = (int)objValue;
            }
        }
        public class FloatParameterDrawer : ParameterDrawer
        {
            private float mObj = 0.0f;
            public FloatParameterDrawer(string name) : base(name)
            {
            }
            public override void OnGUI(Rect InBoundRect)
            {
                float ret = mObj;
                GUILayout.BeginHorizontal();
                GUILayout.Label(mName, GUILayout.Width(LABEL_WIDTH), GUILayout.ExpandWidth(false));
                if (float.TryParse(GUILayout.TextField(ret.ToString("#0.000")), out ret))
                {
                    mObj = ret;
                }
                GUILayout.EndHorizontal();
            }

            public override object GetValue()
            {
                return mObj;
            }

            public override void SetValue(object objValue)
            {
                mObj = (float)objValue;
            }
        }
        public class DoubleParameterDrawer : ParameterDrawer
        {
            private double mObj = 0.0;
            public DoubleParameterDrawer(string name) : base(name)
            {
            }
            public override void OnGUI(Rect InBoundRect)
            {
                double ret = mObj;
                GUILayout.BeginHorizontal();
                GUILayout.Label(mName, GUILayout.Width(LABEL_WIDTH), GUILayout.ExpandWidth(false));
                if (double.TryParse(GUILayout.TextField(ret.ToString("#0.000")), out ret))
                {
                    mObj = ret;
                }
                GUILayout.EndHorizontal();
            }

            public override object GetValue()
            {
                return mObj;
            }

            public override void SetValue(object objValue)
            {
                mObj = (double)objValue;
            }
        }
        public class LongParameterDrawer : ParameterDrawer
        {
            private long mObj;
            public LongParameterDrawer(string name) : base(name)
            {
            }
            public override void OnGUI(Rect InBoundRect)
            {
                long ret = mObj;
                GUILayout.BeginHorizontal();
                GUILayout.Label(mName, GUILayout.Width(LABEL_WIDTH), GUILayout.ExpandWidth(false));
                if (long.TryParse(GUILayout.TextField(ret.ToString()), out ret))
                {
                    mObj = ret;
                }
                GUILayout.EndHorizontal();
            }

            public override object GetValue()
            {
                return mObj;
            }

            public override void SetValue(object objValue)
            {
                mObj = (long)objValue;
            }
        }
        public class BooleanParameterDrawer : ParameterDrawer
        {
            private bool mObj;
            public BooleanParameterDrawer(string name) : base(name)
            {
            }
            public override void OnGUI(Rect InBoundRect)
            {
                GUILayout.BeginHorizontal(); 
                GUILayout.Label(mName, GUILayout.Width(LABEL_WIDTH), GUILayout.ExpandWidth(false));
                mObj = GUILayout.Toggle(mObj, "",GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
            }

            public override object GetValue()
            {
                return mObj;
            }

            public override void SetValue(object objValue)
            {
                mObj = (bool)objValue;
            }
        }
        public class StringParameterDrawer : ParameterDrawer
        {
            private string mObj = "";
            public StringParameterDrawer(string name) : base(name)
            {
            }
            public override void OnGUI(Rect InBoundRect)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(mName, GUILayout.Width(LABEL_WIDTH), GUILayout.ExpandWidth(false));
                mObj = GUILayout.TextField(mObj);
                GUILayout.EndHorizontal();
            }

            public override object GetValue()
            {
                return mObj;
            }

            public override void SetValue(object objValue)
            {
                mObj = objValue as string;
            }
        }
        public class EnumParameterDrawer : ParameterDrawer
        {
            public static class Style
            {
                public static float ItemHeight;
                public static GUIStyle Normal;
                public static GUIStyle Selected;

                public static void InitStyle()
                {
                    Normal = new GUIStyle();
                    Normal.normal.textColor = Color.white;
                    Texture2D texInit = new Texture2D(1, 1);
                    texInit.Apply();
                    texInit.SetPixel(0, 0, Color.white);
                    Texture2D backgroundInit = new Texture2D(1, 1);
                    backgroundInit.SetPixel(0, 0, new Color(0,0,0,0.5f));
                    backgroundInit.Apply();

                    Texture2D activeBgInit = new Texture2D(1, 1);
                    activeBgInit.SetPixel(0, 0, new Color(0, 0,1.0f));
                    activeBgInit.Apply();
                    Normal.active.background = activeBgInit;

                    Normal.hover.background = backgroundInit;
                    Normal.onHover.background = backgroundInit;
                    Normal.hover.textColor = Color.black;
                    Normal.onHover.textColor = Color.black;
                    Normal.padding = new RectOffset(4, 4, 4, 4);
                    Normal.fontSize = 30;

                    Selected = new GUIStyle();
                    Selected.normal.textColor = Color.red;
                    Selected.hover.background = texInit;
                    Selected.onHover.background = texInit;
                    Selected.hover.textColor = Color.red;
                    Selected.onHover.textColor = Color.red;
                    Selected.padding = new RectOffset(4, 4, 4, 4);
                    Selected.fontSize = 30;

                    ItemHeight = 30;
                }
            }

            private int mObj;

            private Type mType;
            private string[] mItems;

            private int mSelectedIndex;
            private Rect mListBoxRect;
            private bool mVisible;

            public delegate void SelectionEventHandler(int InIndex);
            private event SelectionEventHandler OnSelectedEvent;
            private event SelectionEventHandler OnSelectedChangedEvent;

            public EnumParameterDrawer(string name, Type type, SelectionEventHandler onSelected = null, SelectionEventHandler onChanged = null)
                : base(name)
            {
                mType = type;
                mItems = Enum.GetNames(type);
                mVisible = false;
                mSelectedIndex = 0;
                OnSelectedEvent = onSelected;
                OnSelectedChangedEvent = onChanged;
                if (mItems != null && mItems.Length > 0)
                {
                    mObj = (int)Enum.Parse(type, mItems[0]);
                }
            }

            public override void OnGUI(Rect InBoundRect)
            {
                if (mItems == null || mItems.Length == 0)
                {
                    return;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label(mName, GUILayout.Width(LABEL_WIDTH), GUILayout.ExpandWidth(false));
                string selectedString = mItems[mSelectedIndex];
                if (GUILayout.Button(selectedString))
                {
                    mVisible = !mVisible;
                }
                InBoundRect = GUILayoutUtility.GetLastRect();
                
                GUILayout.EndHorizontal();
                if (mVisible && mItems != null)
                {
                    mListBoxRect = new Rect(InBoundRect);
                    mListBoxRect.y = mListBoxRect.y + mListBoxRect.height;
                    mListBoxRect.height = mItems.Length * Style.ItemHeight;

                    GUI.Box(mListBoxRect, "");

                    for (int i = 0; i < mItems.Length; i++)
                    {
                        Rect ListButtonRect = new Rect(mListBoxRect);
                        ListButtonRect.y = ListButtonRect.y + Style.ItemHeight * i;
                        ListButtonRect.height = Style.ItemHeight;

                        GUIStyle StyleSelection = mSelectedIndex == i ? Style.Selected : Style.Normal;

                        if (GUI.Button(ListButtonRect,new GUIContent(mItems[i], mItems[i]),StyleSelection))
                        {
                            if (OnSelectedEvent != null)
                            {
                                OnSelectedEvent(i);
                            }
                            if (mSelectedIndex != i)
                            {
                                mSelectedIndex = i;
                                mObj = (int)Enum.Parse(mType, mItems[i]);
                                if (OnSelectedChangedEvent != null)
                                {
                                    OnSelectedChangedEvent(i);
                                }
                            }
                            mVisible = false;
                        }
                    }
                }
            }

            public override object GetValue()
            {
                return mObj;
            }

            public override void SetValue(object objValue)
            {
                mObj = (int)objValue;
            }
        }
        public class UnityVector3ParameterDrawer : ParameterDrawer
        {
            protected UnityEngine.Vector3 mObj = UnityEngine.Vector3.zero;
            public UnityVector3ParameterDrawer(string name) : base(name)
            {
            }
            public override void OnGUI(Rect InBoundRect)
            {
                Vector3 ret = mObj;
                GUILayout.BeginHorizontal();
                GUILayout.Label(mName, GUILayout.Width(LABEL_WIDTH), GUILayout.ExpandWidth(false));
                if (float.TryParse(GUILayout.TextField(ret.x.ToString("#0.000")), out ret.x))
                {
                    mObj.x = ret.x;
                }
                if (float.TryParse(GUILayout.TextField(ret.y.ToString("#0.000")), out ret.y))
                {
                    mObj.y = ret.y;
                }
                if (float.TryParse(GUILayout.TextField(ret.z.ToString("#0.000")), out ret.z))
                {
                    mObj.z = ret.z;
                }
                GUILayout.EndHorizontal();
            }

            public override object GetValue()
            {
                return mObj;
            }

            public override void SetValue(object objValue)
            {
                mObj = (Vector3)objValue;
            }
        }
        public class UnityVector2ParameterDrawer : ParameterDrawer
        {
            protected UnityEngine.Vector2 mObj = UnityEngine.Vector2.zero;
            public UnityVector2ParameterDrawer(string name) : base(name)
            {
            }
            public override void OnGUI(Rect InBoundRect)
            {
                Vector2 ret = mObj;
                GUILayout.BeginHorizontal();
                GUILayout.Label(mName, GUILayout.Width(LABEL_WIDTH), GUILayout.ExpandWidth(false));
                if (float.TryParse(GUILayout.TextField(ret.x.ToString("#0.000")), out ret.x))
                {
                    mObj.x = ret.x;
                }
                if (float.TryParse(GUILayout.TextField(ret.y.ToString("#0.000")), out ret.y))
                {
                    mObj.y = ret.y;
                }
                GUILayout.EndHorizontal();
            }

            public override object GetValue()
            {
                return mObj;
            }

            public override void SetValue(object objValue)
            {
                mObj = (Vector2)objValue;
            }
        }
        public class UnityQuaternionParameterDrawer : ParameterDrawer
        {
            protected UnityEngine.Vector3 mObj = UnityEngine.Vector3.zero;
            public UnityQuaternionParameterDrawer(string name) : base(name)
            {
            }
            public override void OnGUI(Rect InBoundRect)
            {
                Vector3 ret = mObj;
                GUILayout.BeginHorizontal();
                GUILayout.Label(mName, GUILayout.Width(LABEL_WIDTH), GUILayout.ExpandWidth(false));
                if (float.TryParse(GUILayout.TextField(ret.x.ToString("#0.000")), out ret.x))
                {
                    mObj.x = ret.x;
                }
                if (float.TryParse(GUILayout.TextField(ret.y.ToString("#0.000")), out ret.y))
                {
                    mObj.y = ret.y;
                }
                if (float.TryParse(GUILayout.TextField(ret.z.ToString("#0.000")), out ret.z))
                {
                    mObj.z = ret.z;
                }
                GUILayout.EndHorizontal();
            }

            public override object GetValue()
            {
                return UnityEngine.Quaternion.Euler(mObj);
            }

            public override void SetValue(object objValue)
            {
                mObj = (Vector3)objValue;
            }
        }
        

        public static ParameterDrawer GetDrawer(Type type, string name)
        {
            if (type == typeof(int))
            {
                return new IntParameterDrawer(name);
            }
            if (type == typeof(float))
            {
                return new FloatParameterDrawer(name);
            }
            if (type == typeof(double))
            {
                return new DoubleParameterDrawer(name);
            }
            if (type == typeof(long))
            {
                return new LongParameterDrawer(name);
            }
            if (type == typeof(string))
            {
                return new StringParameterDrawer(name);
            }
            if (type == typeof(bool))
            {
                return new BooleanParameterDrawer(name);
            }
            if (type == typeof(UnityEngine.Vector2))
            {
                return new UnityVector2ParameterDrawer(name);
            }
            if (type == typeof(UnityEngine.Vector3))
            {
                return new UnityVector3ParameterDrawer(name);
            }
            if (type == typeof(UnityEngine.Quaternion))
            {
                return new UnityQuaternionParameterDrawer(name);
            }
            if (type.IsEnum)
            {
                return new EnumParameterDrawer(name, type);
            }
            return null;
        }
    }
}
#endif

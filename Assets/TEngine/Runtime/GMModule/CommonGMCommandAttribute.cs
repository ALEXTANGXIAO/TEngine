using System;

namespace TEngine
{
    public class CommonGMCommandAttribute : Attribute
    {
        public string mThread;

        public string mPage;

        public string mName;

        public string mButtonName;

        public bool mIsDrawer;

        public Action mCustomDrawCallback;

        public bool mIsEditorOnly = false;

        public CommonGMCommandAttribute(string page, string name, bool isEditorOnly = false)
        {
            mThread = "";
            mPage = page;
            mName = name;
            mButtonName = "Execute";
            mCustomDrawCallback = null;
        }
        public CommonGMCommandAttribute(string page, string name, string buttonName, bool isEditorOnly = false)
        {
            mThread = "";
            mPage = page;
            mName = name;
            mButtonName = buttonName;
            mCustomDrawCallback = null;
        }
        public CommonGMCommandAttribute(string page, string name, string buttonName, string thread, bool isEditorOnly = false)
        {
            mThread = thread;
            mPage = page;
            mName = name;
            mButtonName = buttonName;
            mCustomDrawCallback = null;
        }
        public CommonGMCommandAttribute(string page, string name, string buttonName, string thread, Action customDrawCallback, bool isEditorOnly = false)
        {
            mThread = thread;
            mPage = page;
            mName = name;
            mButtonName = buttonName;
            mCustomDrawCallback = customDrawCallback;
        }
        public CommonGMCommandAttribute(string page, string name, Action customDrawCallback, bool isEditorOnly = false)
        {
            mThread = "";
            mPage = page;
            mName = name;
            mButtonName = "Execute";
            mCustomDrawCallback = customDrawCallback;
        }
        public CommonGMCommandAttribute(string page, string name, string thread, Action customDrawCallback, bool isEditorOnly = false)
        {
            mThread = thread;
            mPage = page;
            mName = name;
            mButtonName = "Execute";
            mCustomDrawCallback = customDrawCallback;
        }
    }
}
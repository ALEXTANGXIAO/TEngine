using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime.UIModule
{
    public enum WindowStackIndex
    {
        StackNormal = 0,
        StackTop = 1,
        StackMax
    };

    public class UIWindowStack
    {
        public WindowStackIndex m_stackIndex;
        public int m_baseOrder = 0;
        public List<uint> m_windowList = new List<uint>();
        public Transform m_parentTrans;

        public int FindIndex(uint windowId)
        {
            for (int i = 0; i < m_windowList.Count; i++)
            {
                if (m_windowList[i] == windowId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
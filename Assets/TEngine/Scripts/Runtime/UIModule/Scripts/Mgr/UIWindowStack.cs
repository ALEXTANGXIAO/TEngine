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
        public WindowStackIndex StackIndex;
        public int BaseOrder = 0;
        public readonly List<uint> WindowsList = new List<uint>();
        public Transform ParentTrans;

        public int FindIndex(uint windowId)
        {
            for (int i = 0; i < WindowsList.Count; i++)
            {
                if (WindowsList[i] == windowId)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
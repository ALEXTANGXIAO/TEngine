using System.Collections.Generic;

namespace GameLogic
{
    /// <summary> 红点关联 </summary>
    public class RedNoteCheckMgr
    {
        public string m_ownerStr;
        public List<string> m_childList { get; private set; }    

        public RedNoteCheckMgr(RedNoteNotify ower, List<RedNoteNotify> childList)
        {
            m_ownerStr = ower.ToString();
            m_childList = new List<string>();
            for (int i = 0; i < childList.Count; i++)
            {
                var value = childList[i];
                m_childList.Add(value.ToString());
            }
        }

        public RedNoteCheckMgr(string paramKey)
        {
            m_ownerStr = paramKey;
        }

        public bool AddChild(string childKey)
        {
            if (m_childList == null)
            {
                m_childList = new List<string>();
            }
            if (!m_childList.Contains(childKey))
            {
                m_childList.Add(childKey);
                return true;
            }

            return false;
        }

        public void CheckChildRedNote()
        {
            var valueItem = RedNoteMgr.Instance.GetNotifyValueItem(m_ownerStr);

            bool childHaveRed = false;
            int childNotePointNum = 0;

            int count = m_childList.Count;
            for (var index = 0; index < count; index++)
            {
                var child = m_childList[index];
                var childItem = RedNoteMgr.Instance.GetNotifyValueItem(child);
                if (childItem.GetRedNoteType() == RedNoteType.Simple)
                {
                    if (RedNoteMgr.Instance.GetNotifyValue(child))
                    {
                        childHaveRed = true;
                        childNotePointNum++;
                    }
                }
                else
                {
                    childNotePointNum += childItem.GetRedNotePointNum();
                }
            }

            if (valueItem.GetRedNoteType() == RedNoteType.Simple)
            {
                RedNoteMgr.Instance.SetNotifyKeyValue(m_ownerStr, childHaveRed);
            }
            else
            {
                RedNoteMgr.Instance.SetNotifyKeyPointNum(m_ownerStr, childNotePointNum);
            }
        }


        public bool CheckChild(string childKey)
        {
            bool red = RedNoteMgr.Instance.GetNotifyValue(childKey);
            return red;
        }
    }
}

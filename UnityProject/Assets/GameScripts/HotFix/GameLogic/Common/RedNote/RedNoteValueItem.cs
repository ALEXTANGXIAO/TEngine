using System.Collections.Generic;

namespace GameLogic
{
    public class RedNoteValueItem
    {
        public string m_key;
        //所有关联的红点UI
        private HashSet<RedNoteBehaviour> m_redNoteDic = new HashSet<RedNoteBehaviour>();
        private bool m_state;
        private int m_pointNum;
        private RedNoteType m_noteType;

        private bool m_dirty;
        private bool m_tmpState;

        public void Init(string keyStr, RedNoteType noteType)
        {
            m_key = keyStr;
            m_noteType = noteType;
        }

        //添加红点对象
        public void AddRedNote(RedNoteBehaviour redNote)
        {
            m_redNoteDic.Add(redNote);
        }

        //移除对象
        public void RemoveRedNote(RedNoteBehaviour redNote)
        {
            m_redNoteDic.Remove(redNote);
        }

        //获取具体对象的状态
        public bool GetRedNoteState()
        {
            if (m_dirty)
            {
                return m_tmpState;
            }

            return m_state;
        }

        //获取具体对象的红点数
        public int GetRedNotePointNum()
        {
            return m_pointNum;
        }

        public RedNoteType GetRedNoteType()
        {
            return m_noteType;
        }

        public void SetStateDirty(bool state)
        {
            m_dirty = m_state != state;
            m_tmpState = state;
        }

        //设置对象状态
        public bool SetRedNoteState(bool state)
        {
            bool chg = state != m_state;
            m_state = state;
            if (chg)
            {
                SetBehaviourState(state);
            }

            return chg;
        }

        public bool SetRedNotePoint(int num)
        {
            if (m_pointNum != num)
            {
                m_pointNum = num;
                SetBehaviourPoint(num);
                return true;
            }

            return false;
        }

        private void SetBehaviourState(bool state)
        {
            //检查是否注册过具体对象
            foreach (var redNote in m_redNoteDic)
            {
                if (redNote == null || redNote.gameObject == null)
                    continue;

                redNote.SetRedNoteState(state);
            }

            // 移除空的红点
            ClearTheNullRedNote();
        }

        private void SetBehaviourPoint(int pointNum)
        {
            foreach (var redNote in m_redNoteDic)
            {
                if (redNote == null || redNote.gameObject == null)
                    continue;

                redNote.SetRedNotePointNum(pointNum);
            }

            // 移除空的红点
            ClearTheNullRedNote();
        }

        // 移除空的红点
        private void ClearTheNullRedNote()
        {
            m_redNoteDic.RemoveWhere(redNote => redNote == null || redNote.gameObject == null);
        }

        //清理状态
        public void ClearRedNoteState(bool clearBehavior)
        {
            foreach (var redNote in m_redNoteDic)
            {
                if (redNote != null)
                {
                    redNote.SetRedNoteState(false);
                }
            }

            if (clearBehavior)
            {
                m_redNoteDic.Clear();
            }

            m_state = false;
            m_dirty = false;
            m_tmpState = false;
        }

        public void CheckDirty()
        {
            if (m_dirty)
            {
                m_dirty = false;
                RedNoteMgr.Instance.SetNotifyKeyValue(m_key, m_tmpState);
            }
        }
    }

    public enum RedNoteType
    {
        Simple,
        WithNum,
    }
}

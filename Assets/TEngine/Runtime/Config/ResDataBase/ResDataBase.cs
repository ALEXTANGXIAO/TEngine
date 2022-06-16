using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEngine
{
    public interface IResRawListInterface<T>
    {
        List<T> RawList { get; }
    }

    public class ResDataBase<T> : IResRawListInterface<T>
    {
        private IResRawListInterface<T> m_sourceRawList = null;

        private string m_fileName = null;
        private List<T> m_rawList = null;
        private bool m_loaded = false;

        public List<T> RawList
        {
            get
            {
                CheckLoad();
                return m_rawList;
            }
        }

        protected void InitBase(string fileName)
        {
            m_fileName = fileName;
        }

        protected void InitBase(IResRawListInterface<T> sourceList)
        {
            m_sourceRawList = sourceList;
        }

        protected void ClearBase()
        {
            m_loaded = false;
            m_rawList = null;
        }

        protected void CheckLoad()
        {
            if (m_loaded)
            {
                return;
            }

#if UNITY_EDITOR
            GameTickWatcher tickWatcher = new GameTickWatcher();
#endif

            m_loaded = true;
            if (m_sourceRawList != null)
            {
                m_rawList = LoadFromSourceList(m_sourceRawList);
            }
            else
            {
                m_rawList = LoadFromFile(m_fileName);
            }

            if (m_rawList == null)
            {
                m_rawList = new List<T>();
            }

#if UNITY_EDITOR
            TLogger.LogInfoSuccessd("Read Config {0} Used Time: {1}", typeof(T).ToString(), tickWatcher.ElapseTime());
#endif
        }


        #region 处理载入

        protected virtual List<T> LoadFromFile(string fileName)
        {
            return null;
        }

        protected virtual List<T> LoadFromSourceList(IResRawListInterface<T> sourceList)
        {
            return null;
        }


        #endregion
    }
}

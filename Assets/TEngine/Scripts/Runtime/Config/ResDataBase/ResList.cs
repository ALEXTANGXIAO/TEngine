using System;
using System.Collections.Generic;

namespace TEngine.Runtime
{
    public class ResList<T> : ResDataBase<T> where T : new()
    {
        private Comparison<T> m_comparer;

        public List<T> Data
        {
            get { return RawList; }
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="comparer">如果需要排序，那么传入排序函数</param>
        public void Init(Comparison<T> comparer = null)
        {
            InitBase(string.Empty);
            m_comparer = comparer;
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="fileName">指定配置文件名</param>
        /// <param name="comparer">如果需要排序，那么传入排序函数</param>
        public void Init(string fileName, Comparison<T> comparer = null)
        {
            InitBase(fileName);
            m_comparer = comparer;
        }

        public void Clear()
        {
            ClearBase();
        }

        protected override List<T> LoadFromFile(string fileName)
        {
            List<T> listData;
            if (string.IsNullOrEmpty(fileName))
            {
                listData = ResConfigUtil.ReadConfigListRes<T>();
            }
            else
            {
                listData = ResConfigUtil.ReadConfigListRes<T>(fileName);
            }

            if (m_comparer != null)
            {
                listData.Sort(m_comparer);
            }

            return listData;
        }

        protected override List<T> LoadFromSourceList(IResRawListInterface<T> sourceList)
        {
            if (m_comparer != null)
            {
                List<T> listSorted = new List<T>();

                var list = sourceList.RawList;
                for (int i = 0; i < list.Count; i++)
                {
                    listSorted.Add(list[i]);
                }

                listSorted.Sort(m_comparer);

                return listSorted;
            }

            return sourceList.RawList;
        }
    }
}
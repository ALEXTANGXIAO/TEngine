using System.Collections.Generic;

namespace TEngine
{
    class ResDictionaryList<K, T> : ResDataBase<T> where T : new()
    {
        private ConvertDictionaryKey<K, T> m_convKey = null;
        private Dictionary<K, List<T>> m_data = null;

        public Dictionary<K, List<T>> Data
        {
            get
            {
                CheckLoad();
                return m_data;
            }
        }

        public void Init(string fileName, ConvertDictionaryKey<K, T> convKey, FilterResBin<T> filter = null)
        {
            InitBase(fileName);
            m_convKey = convKey;
        }

        public void Init(ConvertDictionaryKey<K, T> convKey)
        {
            Init(string.Empty, convKey);
        }

        /// <summary>
        /// 构造list数据结构，依赖基础的数据源
        /// </summary>
        /// <param name="rawList"></param>
        /// <param name="convKey"></param>
        public void Init(IResRawListInterface<T> sourceList, ConvertDictionaryKey<K, T> convKey, FilterResBin<T> filter = null)
        {
            InitBase(sourceList);
            m_convKey = convKey;
        }

        public void Clear()
        {
            m_data = null;
            ClearBase();
        }

        protected override List<T> LoadFromSourceList(IResRawListInterface<T> sourceList)
        {
            m_data = new Dictionary<K, List<T>>();

            var rawList = sourceList.RawList;
            for (int i = 0; i < rawList.Count; i++)
            {
                var config = rawList[i];
                var key = m_convKey(config);
                List<T> listData;
                if (!m_data.TryGetValue(key, out listData))
                {
                    listData = new List<T>();
                    m_data.Add(key, listData);
                }

                listData.Add(config);
            }

            return rawList;
        }

        protected override List<T> LoadFromFile(string fileName)
        {
            m_data = new Dictionary<K, List<T>>();

            List<T> list;
            if (string.IsNullOrEmpty(fileName))
            {
                list = ResConfigUtil.ReadResBinDict(m_data, m_convKey);
            }
            else
            {
                list = ResConfigUtil.ReadResBinDict(m_data,  m_convKey,fileName);
            }

            return list;
        }
    }
}

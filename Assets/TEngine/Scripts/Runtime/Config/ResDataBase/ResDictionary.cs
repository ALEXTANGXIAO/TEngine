using System.Collections.Generic;

namespace TEngine.Runtime
{
    public class ResDictionary<K, T> : ResDataBase<T> where T : new()
    {
        private FilterResBin<T> m_filter = null;
        private ConvertDictionaryKey<K, T> m_convKey = null;
        private Dictionary<K, T> m_data = null;

        public Dictionary<K, T> Data
        {
            get
            {
                CheckLoad();
                return m_data;
            }
        }

        public void Init(string fileName, ConvertDictionaryKey<K, T> convKey)
        {
            InitBase(fileName);
            m_convKey = convKey;
            m_filter = null;
        }

        public void Init(ConvertDictionaryKey<K, T> convKey)
        {
            Init(string.Empty, convKey);
        }

        public void Init(IResRawListInterface<T> rawList, ConvertDictionaryKey<K, T> convKey, FilterResBin<T> filter = null)
        {
            InitBase(rawList);
            m_convKey = convKey;
            m_filter = filter;
        }

        public void Clear()
        {
            m_data = null;
            ClearBase();
        }

        protected override List<T> LoadFromSourceList(IResRawListInterface<T> sourceList)
        {
            m_data = new Dictionary<K, T>();
            var rawList = sourceList.RawList;
            for (int i = 0; i < rawList.Count; i++)
            {
                var config = rawList[i];
                if (m_filter != null && !m_filter(config))
                {
                    continue;
                }

                var key = m_convKey(config);
                if (m_data.ContainsKey(key))
                {
                    TLogger.LogError("Config {0} load error, repeat config: {0}", typeof(T).ToString(), key.ToString());
                    continue;
                }

                m_data.Add(key, config);
            }

            return rawList;
        }

        protected override List<T> LoadFromFile(string fileName)
        {
            m_data = new Dictionary<K, T>();

            //读取文件不支持filter,实际也没这个需求
            TLogger.LogAssert(m_filter == null);

            List<T> rawList;
            if (string.IsNullOrEmpty(fileName))
            {
                rawList = ResConfigUtil.ReadResBinDict(m_data, m_convKey);
            }
            else
            {
                rawList = ResConfigUtil.ReadResBinDict(m_data, m_convKey,fileName);
            }

            return rawList;
        }

        public bool TryGetValue(K key, out T itemData, bool showLog = false)
        {
            if (Data.TryGetValue(key, out itemData))
            {
                return true;
            }

            if (showLog)
            {
                TLogger.LogError("get config {0} failed, key: {1}!", typeof(T), key);
            }
            return false;
        }
    }
}

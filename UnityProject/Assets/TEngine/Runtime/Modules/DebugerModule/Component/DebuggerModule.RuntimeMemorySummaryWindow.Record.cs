namespace TEngine
{
    public sealed partial class DebuggerModule : Module
    {
        private sealed partial class RuntimeMemorySummaryWindow : ScrollableDebuggerWindowBase
        {
            private sealed class Record
            {
                private readonly string m_Name;
                private int m_Count;
                private long m_Size;

                public Record(string name)
                {
                    m_Name = name;
                    m_Count = 0;
                    m_Size = 0L;
                }

                public string Name
                {
                    get
                    {
                        return m_Name;
                    }
                }

                public int Count
                {
                    get
                    {
                        return m_Count;
                    }
                    set
                    {
                        m_Count = value;
                    }
                }

                public long Size
                {
                    get
                    {
                        return m_Size;
                    }
                    set
                    {
                        m_Size = value;
                    }
                }
            }
        }
    }
}

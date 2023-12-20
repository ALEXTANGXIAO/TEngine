using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 子界面之间共享的数据。
    /// </summary>
    public class ChildPageSharData
    {
        private object[] m_arrParams = new object[3];    // 共享参数列表

        public object Param1 { get { return m_arrParams[0]; } }
        public object Param2 { get { return m_arrParams[1]; } }
        public object Param3 { get { return m_arrParams[2]; } }

        /// <summary>
        /// 设置指定索引的参数。
        /// </summary>
        /// <param name="paramIdx"></param>
        /// <param name="param"></param>
        public void SetParam(int paramIdx, object param)
        {
            if (paramIdx >= m_arrParams.Length)
                return;

            m_arrParams[paramIdx] = param;
        }
    }

    public class ChildPageBase : UIWidget
    {

        protected ChildPageSharData m_shareObjData;     // 共享数据

        // 初始化数据
        public void InitData(ChildPageSharData shareObjData)
        {
            m_shareObjData = shareObjData;
        }

        public virtual void OnPageShowed(int oldShowType, int newShowType)
        {

        }

        // 无参数刷新当前子界面
        // 收到那些数据变动的消息的时候使用
        public virtual void RefreshPage()
        {

        }

        public object ShareData1
        {
            get => m_shareObjData.Param1;
            set => m_shareObjData.SetParam(0, value);
        }

        public object ShareData2
        {
            get => m_shareObjData.Param2;
            set => m_shareObjData.SetParam(1, value);
        }

        public object ShareData3
        {
            get => m_shareObjData.Param3;
            set => m_shareObjData.SetParam(2, value);
        }
    }
}
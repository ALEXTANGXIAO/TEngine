namespace GameBase
{
    public class BaseClsTemplate<T>
    {
        protected static T Imp;

        /// <summary>
        /// Unity工程，注册处理函数。
        /// </summary>
        /// <param name="imp">实现类。</param>
        public static void RegisterImp(T imp)
        {
            Imp = imp;
        }
    }
}
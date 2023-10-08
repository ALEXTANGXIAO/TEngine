/// <summary>
/// App阶段标识枚举。
/// </summary>
public enum AppStageEnum
{
    /// <summary>
    /// 测试版本。
    /// </summary>
    Debug = 1,

    /// <summary>
    /// 前期版本。
    /// </summary>
    Alpha = 2,

    /// <summary>
    /// 中期版本。
    /// </summary>
    Beta = 3,

    /// <summary>
    /// 后期版本。
    /// <remarks>与发布版本差别不大。</remarks>
    /// </summary>
    Rc = 4,

    /// <summary>
    /// 发布版本。
    /// </summary>
    Release = 5
}
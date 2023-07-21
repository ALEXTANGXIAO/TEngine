namespace TEngine.Helper;

/// <summary>
/// 可寻址帮助类。
/// </summary>
public static class AddressableSceneHelper
{
    public static long GetSceneEntityId()
    {
        var sceneEntityId = 0L;
        foreach (var sceneConfig in SceneConfigData.Instance.List)
        {
            if (sceneConfig.RouteId == 3072)
            {
                sceneEntityId = sceneConfig.EntityId;
                break;
            }
        }
        return sceneEntityId;
    }
    
    public static long GetSceneEntityIdByType(Logic.SceneType sceneType)
    {
        var sceneEntityId = 0L;
        foreach (var sceneConfig in SceneConfigData.Instance.List)
        {
            if (sceneConfig.SceneType.Parse<Logic.SceneType>() == sceneType)
            {
                sceneEntityId = sceneConfig.EntityId;
                break;
            }
        }
        return sceneEntityId;
    }
    
    public static long GetSceneEntityIdBySceneId(uint sceneId)
    {
        var sceneEntityId = 0L;
        var sceneConfig = SceneConfigData.Instance.Get(sceneId);
        sceneEntityId = sceneConfig.EntityId;
        return sceneEntityId;
    }
}
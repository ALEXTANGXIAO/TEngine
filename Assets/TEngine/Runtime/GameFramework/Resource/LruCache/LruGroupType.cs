namespace TEngine
{
    public enum LruGroupType : byte
    {
        [LruGroup(20)] 
        GameActor,
        [LruGroup(50)] 
        GameObject,
        [LruGroup(100)] 
        Sprite,
    }
}
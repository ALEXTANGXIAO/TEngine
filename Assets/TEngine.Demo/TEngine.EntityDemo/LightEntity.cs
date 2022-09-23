using TEngine.Runtime;
using TEngine.Runtime.Entity;
using UnityEngine;

public class LightEntity:EntityLogicEx
{
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);
        
        Log.Debug(userData.ToString());
    }
}
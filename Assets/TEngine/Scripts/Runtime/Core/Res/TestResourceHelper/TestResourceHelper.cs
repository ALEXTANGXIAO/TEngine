using System;
using TEngine.Runtime;
using UnityEngine;

public class TestResourceHelper : ResourceHelperBase
{
    public override GameObject Load(string path)
    {
        throw new NotImplementedException();
    }

    public override GameObject Load(string path, Transform parent)
    {
        throw new NotImplementedException();
    }

    public override T Load<T>(string path)
    {
        throw new NotImplementedException();
    }

    public override void LoadAsync(string path, Action<GameObject> callBack)
    {
        throw new NotImplementedException();
    }

    public override void LoadAsync<T>(string path, Action<T> callBack, bool withSubAsset = false)
    {
        throw new NotImplementedException();
    }
}

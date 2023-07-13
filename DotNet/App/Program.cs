using TEngine;
using TEngine.Core;
using TEngine.Logic;

try
{
    App.Init();

    AssemblySystem.Init();

    ConfigTableSystem.Bind();

    App.Start().Coroutine();

    Entry.Start().Coroutine();
    
    while(true)
    {
        Thread.Sleep(1);
        ThreadSynchronizationContext.Main.Update();
        SingletonSystem.Update();
    }
}
catch (Exception e)
{
    Log.Error(e);
}






using TEngine;
using TEngine.Core;
using TEngine.Logic;

public class ProgramInfo
{
    /// <summary>
    /// 启动说明。
    /// </summary>
    public void Temp()
    {
        try
        {
            // 框架启动需要在命令行后面添加参数才会正常使用分别是:
            // 例如demo的服务器参数: --Mode Develop --AppType Game --AppId 100
            // Mode有两种:
            //      Develop:开发模式 这个是所有在配置定义的服务器都会启动并且在同一个进程下、方便开发调试。
            //      当然如果游戏体量不大也可以用这个模式发布，后期改成Release模式也是没有问题的
            //      Release:发布模式只会启动启动参数传递的Server、也就是只会启动一个Server
            //      您可以做一个Server专门用于管理启动所有Server的工具或脚本、一般都是运维同学来做
            // AppType有两种:
            //      Game:游戏服务器
            //      Export:导出配置表工具
            //      例如我要启动导表工具参数就应该是--AppType Export就可以了Mode和AppId都可以不用设置
            // AppId:告诉框架应该启动哪个服务器、对应ServerConfig.xls的Id 如果Mode使用的Develop的话、这个Id不生效
            // 初始化框架
            App.Init();

            // 演示的框架创建了Model和Hotfix两个工程所以需要AssemblyManager.Load来加载这两个程序集
            // 这个看个人而定、你也可以不按照演示框架这样创建2个程序集、总之有几个程序集就AssemblyManager.Load一下加载到框架中
            // 因为App这个工程就不需要了、因为这里没有逻辑、具体看AssemblyLoadHelper的逻辑、自己写一下
            // 加载需要的程序集、这里因为每个人都框架规划都不一样、所以这块开放出自己定义
            AssemblySystem.Init();

            // 绑定框架需要的配置文件
            // 框架启动服务器需要配置文件才可以启动、比如需要启动什么服务器、服务器的监听地址是什么等等、所以要提前绑定一下
            ConfigTableSystem.Bind();

            // 启动框架
            // 启动框架会加载Demo下Config/Excel/Server里四个文件配置
            // 因为上面ConfigTableHelper.Bind已经绑定好了、所以框架可以直接读取这4个配置文件进行启动
            App.Start().Coroutine();

            // 框架启动后需要执行的逻辑、现在是我是写的启动服务器的逻辑、同上这里也开放出来自定义
            // 但这里一定是异步的、不然框架部分功能可能不会正常、因为阻塞到这里、需要Update需要下面的才可以驱动
            // 这个入口代码可以不用调用、这里只是演示下如果调用应该怎么处理
            Entry.Start().Coroutine();

            while (true)
            {
                Thread.Sleep(1);
                ThreadSynchronizationContext.Main.Update();
                SingletonSystem.Update();
            }
        }
        catch
            (Exception e)
        {
            Log.Error(e.ToString());
        }
    }
}
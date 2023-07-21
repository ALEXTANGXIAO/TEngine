#if TENGINE_NET
using System;
using System.Collections.Generic;
using TEngine.Core.Network;

namespace TEngine.Logic
{
    public class Unit : Entity
    {
        public long GateRouteId;
    }

    public static class AddressManage
    {
        public static readonly Dictionary<long, Unit> Units = new Dictionary<long, Unit>();

        public static Unit Add(Scene scene, long addressId, long gateRouteId)
        {
            var unit = Entity.Create<Unit>(scene, addressId);
            unit.GateRouteId = gateRouteId;
            Units.Add(unit.Id, unit);
            return unit;
        }

        public static Unit? Get(long addressId)
        {
            return Units.TryGetValue(addressId, out var unit) ? unit : null;
        }
    }

    public class I_G2M_LoginAddressRequestHandler : RouteRPC<Scene,I_G2M_LoginAddressRequest,I_M2G_LoginAddressResponse>
    {
        protected override async FTask Run(Scene scene, I_G2M_LoginAddressRequest request, I_M2G_LoginAddressResponse response, Action reply)
        {
            // 现在这里是MAP服务器了、玩家进入这里如果是首次进入会有玩家的所有信息
            // 一般这个信息是数据库里拿到或者其他服务器给传递过来了、这里主要演示怎么注册Address、所以这些步骤这里就不做了
            // 这里我就模拟一个假的Unit数据使用
            // 1、首先创建一个Unit
            var unit = AddressManage.Add(scene, request.AddressableId, request.GateRouteId);
            // 2、挂在AddressableMessageComponent组件、让这个Unit支持Address、并且会自动注册到网格中
            await unit.AddComponent<AddressableMessageComponent>().Register();
            response.AddressableId = unit.Id;
        }
    }
}
#endif

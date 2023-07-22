# kcp2k
C# KCP based on the original C [kcp](https://github.com/skywind3000/kcp).

Works with **netcore** and **Unity**. 

Developed for [Mirror Networking](https://github.com/MirrorNetworking/Mirror).

# Features
* Kcp.cs based on kcp.c v1.7, line-by-line translation to C#
* Heavy test coverage
* Fixed [WND_RCV bug](https://github.com/skywind3000/kcp/pull/291) from original kcp
* Optional high level C# code for client/server connection handling
* Optional high level Unreliable channel added

Pull requests for bug fixes & tests welcome.

# Unity
kcp2k works perfectly with Unity, see the Mirror repository's KcpTransport.

# Allocations
The client is allocation free.
The server's SendTo/ReceiveFrom still allocate.

Previously, [where-allocation](https://github.com/vis2k/where-allocation) for a 25x reduction in server allocations. However:
- It only worked with Unity's old Mono version.
- It didn't work in Unity's IL2CPP builds, which are still faster than Mono + NonAlloc
- It didn't work in regular C# projects.
- Overall, the extra complexity is not worth it. Use IL2CPP instead.
- Microsoft is considering to [remove the allocation](https://github.com/dotnet/runtime/issues/30797#issuecomment-1308599410).

# Remarks
- **Congestion Control** should be left disabled. It seems to be broken in KCP.

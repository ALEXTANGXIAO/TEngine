using UnityEngine;
using Unity.Mathematics;

namespace BattleCore.Runtime
{
    public static class MathematicsExt
    {
        public static int2 ToInt2(this Vector2Int vec) => new int2(vec.x, vec.y);
        public static int3 ToInt3(this Vector3Int vec) => new int3(vec.x, vec.y, vec.z);
        public static float2 ToFloat2(this Vector2 vec) => new float2(vec.x, vec.y);
        public static float3 ToFloat3(this Vector3 vec) => new float3(vec.x, vec.y, vec.z);

        public static bool IsEquals(this int2 a, int2 b) => math.all(a == b);
        public static bool IsEquals(this int3 a, int3 b) => math.all(a == b);


        public static Vector2Int ToVec2(this int2 vec) => new Vector2Int(vec.x, vec.y);
        public static Vector3Int ToVec3(this int2 vec) => new Vector3Int(vec.x, vec.y, 0);
        public static Vector3Int ToVec3(this int3 vec) => new Vector3Int(vec.x, vec.y, vec.z);
        public static Vector2 ToVec2(this float2 vec) => new Vector2(vec.x, vec.y);
        public static Vector3 ToVec3(this float3 vec) => new Vector3(vec.x, vec.y, vec.z);
        public static int ManhattanDist(this int2 vec) => vec.x + vec.y;
        public static int ManhattanDist(this int3 vec) => vec.x + vec.y + vec.z;
        public static float ManhattanDist(this float2 vec) => vec.x + vec.y;
        public static float ManhattanDist(this float3 vec) => vec.x + vec.y + vec.z;
    }
}
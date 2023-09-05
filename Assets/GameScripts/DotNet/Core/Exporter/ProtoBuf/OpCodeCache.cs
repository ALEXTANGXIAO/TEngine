#if TENGINE_NET
using TEngine.Helper;

namespace TEngine.Core;

/// <summary>
/// 网络协议操作码缓存。
/// </summary>
public class OpCodeCache
{
    private readonly List<uint> _opCodes = new List<uint>();
    private readonly SortedDictionary<string, uint> _opcodeCache;
    private readonly SortedDictionary<string, uint> _saveOpCodeCache = new();
    private readonly string _opcodeCachePath = $"{Define.ProtoBufDirectory}OpCode.Cache";

    /// <summary>
    /// 构造函数，用于初始化网络协议操作码缓存。
    /// </summary>
    public OpCodeCache(bool regenerate)
    {
        if (File.Exists(_opcodeCachePath) && !regenerate)
        {
            var readAllText = File.ReadAllText(_opcodeCachePath);
            _opcodeCache = readAllText.Deserialize<SortedDictionary<string, uint>>();
            _opCodes.AddRange(_opcodeCache.Values);
        }
        else
        {
            _opcodeCache = new SortedDictionary<string, uint>();
        }
    }

    /// <summary>
    /// 保存网络协议操作码。
    /// </summary>
    public void Save()
    {
        File.WriteAllText(_opcodeCachePath, _saveOpCodeCache.ToJson());
    }
    
    /// <summary>
    /// 根据className获得OpCode、如果是新增的会产生一个新的OpCode。
    /// </summary>
    /// <param name="className">协议名。</param>
    /// <param name="opcode">操作码。</param>
    /// <returns></returns>
    public uint GetOpcodeCache(string className, ref uint opcode)
    {
        if (!_opcodeCache.TryGetValue(className, out var opCode))
        {
            while (_opCodes.Contains(++opcode)) { }
            opCode = opcode;
            _opCodes.Add(opCode);
        }
        
        _saveOpCodeCache.Add(className, opCode);
        return opCode;
    }
}
#endif
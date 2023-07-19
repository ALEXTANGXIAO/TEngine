namespace TEngine.Core.Network
{
    public interface IMessage
    {
        uint OpCode();
    }
    
    public interface IRequest : IMessage
    {
        
    }
    
    public interface IResponse : IMessage
    {
        uint ErrorCode { get; set; }
    }
}
namespace TEngine.Core.Network
{
    public interface IBsonMessage : IMessage
    {
    
    }
    
    public interface IBsonRequest : IBsonMessage, IRequest
    {
        
    }

    public interface IBsonResponse : IBsonMessage, IResponse
    {
        
    }
}
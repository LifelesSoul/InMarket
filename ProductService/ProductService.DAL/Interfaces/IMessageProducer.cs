namespace ProductService.DAL.Interfaces;

public interface IMessageProducer
{
    void SendMessage<T>(T message);
}

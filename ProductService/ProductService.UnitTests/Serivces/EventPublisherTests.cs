using MassTransit;
using Moq;
using ProductService.BLL.Events;
using ProductService.BLL.Services;
using Xunit;

namespace ProductService.Tests.Services;

public class EventPublisherTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly EventPublisher _publisher;

    public EventPublisherTests()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _publisher = new EventPublisher(_publishEndpointMock.Object);
    }

    [Fact]
    public async Task PublishNotification_ShouldCallPublishOnEndpoint()
    {
        var notificationEvent = new CreateNotificationEvent
        {
            Title = "Test Title",
            Message = "Test Message",
            UserId = Guid.NewGuid(),
            ExternalId = "auth0|123"
        };

        await _publisher.PublishNotification(notificationEvent);

        _publishEndpointMock.Verify(x => x.Publish(
            notificationEvent,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}

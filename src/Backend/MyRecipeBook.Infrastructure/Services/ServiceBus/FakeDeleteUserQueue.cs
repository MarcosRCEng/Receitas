using System.Threading.Tasks;
using MyRecipeBook.Domain.Services.ServiceBus;

namespace MyRecipeBook.Infrastructure.Services.ServiceBus
{
    public class FakeDeleteUserQueue : IDeleteUserQueue
    {
        public Task SendMessage(Guid userIdentifier, Guid? requestId = null, string? messageId = null) => Task.CompletedTask;
    }
}

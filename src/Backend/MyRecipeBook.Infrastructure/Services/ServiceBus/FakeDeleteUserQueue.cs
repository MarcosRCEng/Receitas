using System.Threading.Tasks;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Services.ServiceBus;

namespace MyRecipeBook.Infrastructure.Services.ServiceBus
{
    public class FakeDeleteUserQueue : IDeleteUserQueue
    {
        public Task SendMessage(User user) => Task.CompletedTask;
    }
}

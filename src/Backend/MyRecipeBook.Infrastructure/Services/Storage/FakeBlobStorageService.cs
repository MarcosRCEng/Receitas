using System;
using System.IO;
using System.Threading.Tasks;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Services.Storage;

namespace MyRecipeBook.Infrastructure.Services.Storage
{
    public class FakeBlobStorageService : IBlobStorageService
    {
        public Task Delete(User user, string fileName) => Task.CompletedTask;

        public Task DeleteContainer(Guid userIdentifier) => Task.CompletedTask;

        public Task<string> GetFileUrl(User user, string fileName)
        {
            var id = user?.UserIdentifier.ToString() ?? "unknown";
            return Task.FromResult($"https://fake.storage/{id}/{fileName}");
        }

        public Task Upload(User user, Stream file, string fileName) => Task.CompletedTask;
    }
}

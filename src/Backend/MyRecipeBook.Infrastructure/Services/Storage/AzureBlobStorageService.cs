using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.Services.Storage;

namespace MyRecipeBook.Infrastructure.Services.Storage
{
    // Minimal Azure implementation to satisfy DI registration in Program.cs.
    // A full implementation should use Azure.Storage.Blobs SDK.
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("Azure:Storage:ConnectionString") ?? string.Empty;
        }

        public Task Delete(User user, string fileName) => Task.CompletedTask;

        public Task DeleteContainer(Guid userIdentifier) => Task.CompletedTask;

        public Task<string> GetFileUrl(User user, string fileName)
        {
            var id = user?.UserIdentifier.ToString() ?? "unknown";
            var account = !string.IsNullOrEmpty(_connectionString) ? "azure.storage" : "unknown.storage";
            return Task.FromResult($"https://{account}/{id}/{fileName}");
        }

        public Task Upload(User user, Stream file, string fileName) => Task.CompletedTask;
    }
}

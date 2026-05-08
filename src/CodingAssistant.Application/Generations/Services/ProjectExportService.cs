using CodingAssistant.Application.Abstractions;
using System.IO.Compression;
using System.Text;

namespace CodingAssistant.Application.Generations.Services
{
    public sealed class ProjectExportService : IProjectExportService
    {
        public readonly IProjectGenerationRepository _repository;

        public ProjectExportService(IProjectGenerationRepository repository)
        {
            _repository = repository;
        }

        public async Task<byte[]> ExportZipAsync(
            Guid generationId,
            CancellationToken cancellationToken = default)
        {
            var generation = await _repository.GetByIdAsync(generationId, cancellationToken);

            if (generation is null)
                throw new InvalidOperationException("Generation not found.");

            using var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in generation.Files.OrderBy(x => x.Order))
                {
                    var entry = archive.CreateEntry(file.RelativePath, CompressionLevel.Fastest);

                    await using var entryStream = entry.Open();
                    await using var writer = new StreamWriter(entryStream, Encoding.UTF8);

                    await writer.WriteAsync(file.Content);
                }
            }

            return memoryStream.ToArray();
        }
    }
}

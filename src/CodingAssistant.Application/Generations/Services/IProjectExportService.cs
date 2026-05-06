namespace CodingAssistant.Application.Generations.Services
{
    public interface IProjectExportService
    {
        Task<byte[]> ExportZipAsync(Guid generationId, CancellationToken cancellationToken = default);
    }
}

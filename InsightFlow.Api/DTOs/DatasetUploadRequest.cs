
using Microsoft.AspNetCore.Http;

namespace InsightFlow.Api.DTOs;

public sealed class DatasetUploadRequest
{
    public IFormFile? File { get; set; }

    public string? Name { get; set; }
}



using Microsoft.AspNetCore.Http;

namespace HrappModels
{
    public record FileModel
    {
        public IFormFile FileData { get; set; }
    }
}
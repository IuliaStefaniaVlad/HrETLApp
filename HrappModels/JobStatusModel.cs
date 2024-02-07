using System.ComponentModel.DataAnnotations;

namespace HrappModels
{
    public class JobStatusModel
    {
        [Key]
        public string MessageID { get; set; }
        public string TenantID { get; set; }
        public string TextMessage { get; set; }
        public bool Success { get; set; }
    }
}

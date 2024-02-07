using System.ComponentModel.DataAnnotations;

namespace HrappModels
{
    public class EmployeeDBModel
    {
        [Key]
        public int EmployeeID { get; set; }
        public string TenantID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public decimal AnnualIncome { get; set; }
    }
}

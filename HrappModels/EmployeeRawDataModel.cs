using CsvHelper.Configuration.Attributes;

namespace HrappModels
{
    public record EmployeeRawDataModel
    {
        [Index(0)]
        public int EmployeeID { get; set; }
        [Index(1)]
        public string FirstName { get; set; }
        [Index(2)]
        public string LastName { get; set; }
        [Index(3)]
        public DateTime DateOfBirth { get; set; }
        [Index(4)]
        public double GrossAnnualSalary { get; set; }
    }
}

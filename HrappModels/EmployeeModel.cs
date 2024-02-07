namespace HrappModels
{
    public record EmployeeModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public decimal AnnualIncome { get; set; }
    }
}

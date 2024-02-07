using HrappModels;
using HrappServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace HrappServices
{
    public class TransformDataService : ITransformDataService
    {
        private readonly ILogger<TransformDataService> _logger;
        public TransformDataService(ILogger<TransformDataService> logger) 
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public List<EmployeeDBModel> TransformData(IEnumerable<EmployeeRawDataModel> rawData, string tenantId)
        {
            try
            {
                var transformedData = rawData.Select(x => new EmployeeDBModel() 
                {
                    EmployeeID = x.EmployeeID,
                    LastName = x.LastName,
                    FirstName = x.FirstName,
                    BirthDate = x.DateOfBirth,
                    TenantID = tenantId,
                    AnnualIncome = CalculateNetSalary(x.GrossAnnualSalary)
                }).ToList();
                
                return transformedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        private static decimal CalculateNetSalary(double grossAnnualIncome)
        {
            double taxPaid = 0;
            var grossSalary = grossAnnualIncome;

            //Tax Band A
            grossSalary -= 5000;
            //Tax Band B
            if (grossSalary > 0)
            {
                taxPaid = Math.Min(grossSalary, 15000) * 0.2;
                grossSalary -= 15000;
            }
            if (grossSalary > 0)
            {
                //Tax Band C
                taxPaid += grossSalary * 0.4;
            }
            return (decimal)(grossAnnualIncome - taxPaid);
        }
    }
}

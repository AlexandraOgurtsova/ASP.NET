using System;
using System.Threading.Tasks;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain.Administration;

namespace Pcf.Administration.Core.Services
{
    public class EmployeePromoCodeService : IEmployeePromoCodeService
    {
        private readonly IRepository<Employee> _employeeRepository;

        public EmployeePromoCodeService(IRepository<Employee> employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task UpdateEmployeeAppliedPromocodesAsync(Guid employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee != null)
            {
                employee.AppliedPromocodesCount++;
                await _employeeRepository.UpdateAsync(employee);
            }
        }
    }
}
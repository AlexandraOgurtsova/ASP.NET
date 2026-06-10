using System;
using System.Threading.Tasks;

namespace Pcf.Administration.Core.Services
{
    public interface IEmployeePromoCodeService
    {
        Task UpdateEmployeeAppliedPromocodesAsync(Guid employeeId);
    }
}
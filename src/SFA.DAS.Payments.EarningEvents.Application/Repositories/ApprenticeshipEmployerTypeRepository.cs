using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.Application.Repositories
{
    public interface IApprenticeshipEmployerTypeRepository
    {
        Task<ApprenticeshipEmployerType?> GetEmployerType(long ukprn, long uln, long standardCode);
    }

    public class ApprenticeshipEmployerTypeRepository : IApprenticeshipEmployerTypeRepository
    {
        private readonly IPaymentsDataContext paymentsDataContext;

        public ApprenticeshipEmployerTypeRepository(IPaymentsDataContext paymentsDataContext)
        {
            this.paymentsDataContext = paymentsDataContext;
        }

        public async Task<ApprenticeshipEmployerType?> GetEmployerType(long ukprn, long uln, long standardCode)
        {
            return await paymentsDataContext.Apprenticeship
                .Where(x => x.Ukprn == ukprn && x.Uln == uln && x.StandardCode == standardCode)
                .OrderByDescending(x => x.Status == ApprenticeshipStatus.Active)
                .ThenByDescending(x => x.AgreedOnDate)
                .Select(x => (ApprenticeshipEmployerType?)x.ApprenticeshipEmployerType)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
    }
}

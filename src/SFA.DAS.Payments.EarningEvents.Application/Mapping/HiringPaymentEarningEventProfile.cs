using AutoMapper;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.Application.Mapping
{
    public class HiringPaymentEarningEventProfile : Profile
    {
        public HiringPaymentEarningEventProfile()
        {
            CreateMap<ApprenticeshipContractType1EarningEvent, GSLApprenticeshipEarningsEvent>()
                .ForMember(destinationMember => destinationMember.ContractType, opt => opt.MapFrom(source => ContractType.Act1))
                .ForMember(destinationMember => destinationMember.FundingPlatformType, opt => opt.MapFrom(source => FundingPlatformType.SubmitLearnerData))
                .Ignore(destinationMember => destinationMember.ExternalEarningsId);
        }
    }
}

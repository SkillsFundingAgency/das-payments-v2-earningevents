using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping
{
    public interface IGSLFunctionalSkillMapper: IGrowthAndSkillsMapper
    {
        void Map(CalculateGrowthAndSkillsPayments source, CollectionPeriodModel collectionPeriod, GSLFunctionalSkillEarningsEvent destination);
    }

    public class GSLFunctionalSkillMapper : GrowthAndSkillsMapper, IGSLFunctionalSkillMapper
    {
        public void Map(CalculateGrowthAndSkillsPayments source, CollectionPeriodModel collectionPeriod, GSLFunctionalSkillEarningsEvent destination)
        {
            destination.JobId = 0;
            destination.FundingPlatformType = FundingPlatformType.DigitalApprenticeshipService;
            destination.Ukprn = source.UKPRN;
            destination.CollectionYear = collectionPeriod.AcademicYear;
            destination.ContractType = ContractType.Act1;
            destination.PriceEpisodes = new List<Payments.Model.Core.PriceEpisode>();
            destination.CollectionPeriod = new Payments.Model.Core.CollectionPeriod { AcademicYear = collectionPeriod.AcademicYear, Period = collectionPeriod.Period };
            destination.StartDate = source.Training.StartDate;
            destination.AgeAtStartOfLearning= source.Training.AgeAtStartOfTraining;            
            destination.Learner = new Payments.Model.Core.Learner
            {
                Uln = source.Learner.ULN,
                ReferenceNumber = source.Learner.Reference                
            };
            destination.LearningAim = new Payments.Model.Core.LearningAim
            {
                Reference = source.Training.CourseReference,
                StandardCode = int.Parse(source.Training.CourseCode),
                ProgrammeType = 25,
                FrameworkCode = 0,
                PathwayCode = 0,
                StartDate = source.Training.StartDate,
                CourseCode = source.Training.CourseCode,
                //FundingLineType = source.Training.
                LearningType = LearningType.MathsAndEnglish,
            };
        }
    }
}
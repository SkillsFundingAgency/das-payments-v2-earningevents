using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using Common = SFA.DAS.Payments.Model.Core;
using EarningPeriod = SFA.DAS.Payments.EarningEvents.Messages.External.EarningPeriod;

// ReSharper disable InconsistentNaming

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping
{
    public class GrowthAndSkillsMapper : IGrowthAndSkillsMapper
    {
        public GrowthAndSkillsEarningModel MapToGrowthAndSkillsEarningModel(CalculateGrowthAndSkillsPayments source)
        {
            return new GrowthAndSkillsEarningModel
            {
                EarningsId = source.EarningsId,
                UKPRN = source.UKPRN,
                LearnerKey = source.Learner.LearnerKey,
                LearnerUln = source.Learner.ULN,
                LearnerReference = source.Learner.Reference,
                LearningType = (Model.LearningType)source.Training.LearningType,
                CourseCode = source.Training.CourseCode,
                CourseReference = source.Training.CourseReference,
                StartDate = source.Training.StartDate,
                AgeAtStartOfTraining = source.Training.AgeAtStartOfTraining,
                PlannedEndDate = source.Training.PlannedEndDate,
                ActualEndDate = source.Training.ActualEndDate,
                TrainingStatus = (Model.TrainingStatus)source.Training.TrainingStatus,
                EmployerContribution = source.EmployerContribution,
                CourseType = (Model.CourseType)source.Training.CourseType,
                LearningKey = source.Training.LearningKey,
                PricePeriods = MapToPricePeriodModels(source)
            };
        }

        public IEnumerable<CollectionPeriodModel> MapCollectionYearToCollectionPeriodModels(CollectionYear collectionYear)
        {
            var collectionPeriodModels = new List<CollectionPeriodModel>();

            foreach (var period in collectionYear.Periods)
            {
                collectionPeriodModels.Add(new CollectionPeriodModel
                {
                    AcademicYear = collectionYear.Year,
                    Period = period.Period,
                    Status = period.Status,
                    Id = period.Id
                });
            }
            return collectionPeriodModels;
        }

        public IEnumerable<DasEarningsReceivedEvent> MapToDasEarningsReceivedEvents(CalculateGrowthAndSkillsPayments source, IEnumerable<CollectionPeriodModel> openCollectionPeriods)
        {
            var earningsEvents = new List<DasEarningsReceivedEvent>();

            foreach (var collectionPeriod in openCollectionPeriods)
            {
                earningsEvents.Add(new DasEarningsReceivedEvent
                {
                    EarningsId = source.EarningsId,
                    CourseCode = source.Training.CourseCode,
                    CollectionPeriod = new Common.CollectionPeriod
                    {
                        AcademicYear = collectionPeriod.AcademicYear,
                        Period = collectionPeriod.Period
                    },
                    ULN = source.Learner.ULN,
                    UKPRN = source.UKPRN,
                    LearningAimReference = source.Training.CourseReference,
                });
            }

            return earningsEvents;
        }

        protected long? MapTransferSenderAccountId(EarningPeriod earningPeriod)
        {
            if (earningPeriod.Employer.AccountId != earningPeriod.Employer.FundingAccountId)
            {
                return earningPeriod.Employer.FundingAccountId;
            }

            return null;
        }

        private List<GrowthAndSkillsEarningPricePeriodModel> MapToPricePeriodModels(CalculateGrowthAndSkillsPayments source)
        {
            var output = new List<GrowthAndSkillsEarningPricePeriodModel>();

            foreach (var earning in source.Earnings)
            {
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    foreach (var earningPeriod in pricePeriod.Periods)
                    {
                        var shortCourseEarningPricePeriodRecord = new GrowthAndSkillsEarningPricePeriodModel
                        {
                            AcademicYear = earning.AcademicYear,
                            Price = pricePeriod.Price,
                            StartDate = pricePeriod.StartDate,
                            EndDate = pricePeriod.EndDate,
                            DeliveryPeriod = earningPeriod.DeliveryPeriod,
                            EarningType = (Model.EarningType)earningPeriod.EarningType,
                            Amount = earningPeriod.Amount,
                            EmployerAccountId = earningPeriod.Employer.AccountId,
                            EmployerType = (Model.EmployerType)earningPeriod.Employer.EmployerType,
                            FundingAccountId = earningPeriod.Employer.FundingAccountId,
                            GrowthAndSkillsEarningsId = source.EarningsId,
                            ApprenticeshipId = earningPeriod.LearningId
                        };

                        output.Add(shortCourseEarningPricePeriodRecord);
                    }

                }
            }
            return output;
        }

    }
}



using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using Common = SFA.DAS.Payments.Model.Core;
using EarningPeriod = SFA.DAS.Payments.EarningEvents.Messages.External.EarningPeriod;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;

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
        
        public CalculateGrowthAndSkillsPayments MapToCalculateGrowthAndSkillsPayments(GrowthAndSkillsEarningModel earning)
        {
            return new CalculateGrowthAndSkillsPayments
            {
                EarningsId = earning.EarningsId,
                UKPRN = earning.UKPRN,
                EmployerContribution = earning.EmployerContribution,
                Learner = new Messages.External.Learner
                {
                    LearnerKey = earning.LearnerKey,
                    ULN = earning.LearnerUln,
                    Reference = earning.LearnerReference
                },
                Training = new Training
                {
                    CourseType = (Messages.External.CourseType)earning.CourseType,
                    LearningType = (Messages.External.LearningType)earning.LearningType,
                    CourseCode = earning.CourseCode,
                    CourseReference = earning.CourseReference,
                    StartDate = earning.StartDate,
                    AgeAtStartOfTraining = earning.AgeAtStartOfTraining,
                    PlannedEndDate = earning.PlannedEndDate,
                    ActualEndDate = earning.ActualEndDate,
                    TrainingStatus = (Messages.External.TrainingStatus)earning.TrainingStatus,
                    LearningKey = earning.LearningKey ?? Guid.Empty
                },
                Earnings = earning.PricePeriods
                    .GroupBy(pricePeriod => pricePeriod.AcademicYear)
                    .Select(group => new Messages.External.Earnings
                    {
                        AcademicYear = group.Key,
                        PricePeriods = group.Select(pricePeriod => new Messages.External.PricePeriod
                        {
                            Price = pricePeriod.Price,
                            StartDate = pricePeriod.StartDate,
                            EndDate = pricePeriod.EndDate,
                            Periods = new List<EarningPeriod>
                            {
                                new EarningPeriod
                                {
                                    DeliveryPeriod = pricePeriod.DeliveryPeriod,
                                    EarningType = (Messages.External.EarningType)pricePeriod.EarningType,
                                    Amount = pricePeriod.Amount,
                                    Employer = new Messages.External.Employer
                                    {
                                        AccountId = pricePeriod.EmployerAccountId,
                                        EmployerType = (EmployerType)pricePeriod.EmployerType,
                                        FundingAccountId = pricePeriod.FundingAccountId
                                    },
                                    LearningId = pricePeriod.ApprenticeshipId ?? 0
                                }
                            }
                        }).ToList()
                    }).ToList()
            };
        }

        

    }
}



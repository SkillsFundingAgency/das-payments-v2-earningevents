using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;
using Common = SFA.DAS.Payments.Model.Core;
using CourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

// ReSharper disable InconsistentNaming

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
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
                PricePeriods = MapToPricePeriodModels(source)
            };
        }

        public IEnumerable<GSLShortCourseEarningsEvent> MapToShortCourseEarningEvents(CalculateGrowthAndSkillsPayments source, IEnumerable<CollectionPeriodModel> openCollectionPeriods)
        {
            var earningEvents = new Dictionary<short, GSLShortCourseEarningsEvent>();
            var collectionPeriods = openCollectionPeriods
                .GroupBy(x => x.AcademicYear)
                .ToDictionary(x => x.Key, x => x.First()); // shouldn't have duplicates

            foreach (var earning in source.Earnings.Where(e => collectionPeriods.ContainsKey(e.AcademicYear)))
            {
                if (!earningEvents.ContainsKey(earning.AcademicYear))
                {
                    earningEvents.Add(earning.AcademicYear, new GSLShortCourseEarningsEvent
                    {
                        JobId = 0,
                        EventTime = DateTimeOffset.UtcNow,
                        EventId = Guid.NewGuid(),
                        ExternalEarningsId = source.EarningsId,
                        Ukprn = source.UKPRN,
                        Learner = new Common.Learner
                        {
                            ReferenceNumber = source.Learner.Reference,
                            Uln = source.Learner.ULN
                        },
                        LearningAim = new Common.LearningAim
                        {
                            Reference = source.Training.CourseReference,
                            ProgrammeType = 0,
                            StandardCode = 0,
                            CourseCode = source.Training.CourseCode,
                            FrameworkCode = 0,
                            PathwayCode = 0,
                            FundingLineType = "",
                            SequenceNumber = 0,
                            StartDate = source.Training.StartDate,
                            LearningType = (Common.TrainingType)source.Training.LearningType,
                        },
                        CollectionPeriod = new Common.CollectionPeriod
                        {
                            AcademicYear = earning.AcademicYear,
                            Period = openCollectionPeriods.First(x => x.AcademicYear == earning.AcademicYear).Period
                        },
                        AgeAtStartOfLearning = source.Training.AgeAtStartOfTraining,
                        FundingPlatformType = FundingPlatformType.DigitalApprenticeshipService
                    });
                }
            }

            foreach (var collectionPeriod in openCollectionPeriods)
            {
                if (earningEvents.ContainsKey(collectionPeriod.AcademicYear))
                {
                    earningEvents[collectionPeriod.AcademicYear].Earnings = MapToEarnings(source, collectionPeriod.AcademicYear);
                    earningEvents[collectionPeriod.AcademicYear].PriceEpisodes = MapToEarningEventPriceEpisodes(source, collectionPeriod.AcademicYear);
                }
            }

            return earningEvents.Values.ToList();
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
                if (source.Earnings.Any(x => x.AcademicYear == collectionPeriod.AcademicYear))
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
            }

            return earningsEvents;
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

        private List<Common.PriceEpisode> MapToEarningEventPriceEpisodes(CalculateGrowthAndSkillsPayments source, short academicYear)
        {
            var priceEpisodes = new List<Common.PriceEpisode>();

            foreach (var earning in source.Earnings.Where(x => x.AcademicYear == academicYear))
            {
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    foreach (var earningPeriod in pricePeriod.Periods)
                    {
                        priceEpisodes.Add(new Common.PriceEpisode
                        {
                            Identifier = BuildPriceEpisodeIdentifier(source.Training, pricePeriod),
                            AgreedPrice = pricePeriod.Price,
                            CourseStartDate = source.Training.StartDate,
                            StartDate = source.Training.StartDate,
                            EffectiveTotalNegotiatedPriceStartDate = source.Training.StartDate,
                            PlannedEndDate = source.Training.PlannedEndDate,
                            ActualEndDate = source.Training.ActualEndDate,
                            NumberOfInstalments = pricePeriod.NumberOfInstalments,
                            InstalmentAmount = pricePeriod.InstalmentAmount,
                            CompletionAmount = pricePeriod.CompletionAmount,
                            Completed = (source.Training.TrainingStatus == TrainingStatus.Completed),
                            FundingLineType = BuildFundingLineType(earningPeriod.Employer.EmployerType),
                        });
                    }
                }
            }

            return priceEpisodes;
        }

        private string BuildFundingLineType(EmployerType employerType)
        {
            var employerTypeText = "Levy";
            if (employerType == EmployerType.NonLevy)
            {
                employerTypeText = "Non-Levy";
            }

            return $"GSO Short Courses (Apprenticeship Units) {employerTypeText}";
        }

        private string BuildPriceEpisodeIdentifier(Training training, PricePeriod pricePeriod)
        {
            return $"{training.CourseCode}-{pricePeriod.StartDate}";
        }

        private IEnumerable<ShortCourseEarning> MapToEarnings(CalculateGrowthAndSkillsPayments source, short academicYear)
        {
            var shortCourseEarnings = new List<ShortCourseEarning>();

            foreach (var earning in source.Earnings.Where(x => x.AcademicYear == academicYear))
            {
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    foreach (var period in pricePeriod.Periods)
                    {
                        shortCourseEarnings.Add(new ShortCourseEarning
                        {
                            Type = (ShortCourseEarningType)period.EarningType,
                            Periods = new List<Common.EarningPeriod>
                                {
                                    new Common.EarningPeriod
                                    {
                                        AccountId = period.Employer.AccountId,
                                        Amount = period.Amount,
                                        ApprenticeshipEmployerType = (ApprenticeshipEmployerType)period.Employer.EmployerType,
                                        Period = period.DeliveryPeriod,
                                        SfaContributionPercentage = MapSfaContributionPercentage(period.Employer.EmployerType),
                                        ApprenticeshipId = period.LearningId
                                    }
                                }
                        }

                        );
                    }
                }
            }

            return shortCourseEarnings;
        }

        private decimal? MapSfaContributionPercentage(EmployerType employerType)
        {
            if (employerType == EmployerType.NonLevy)
            {
                return 1m; // 100%
            }

            return 0.95m; // 95% for Levy employers
        }
    }
}


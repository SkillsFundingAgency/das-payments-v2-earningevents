using SFA.DAS.Payments.EarningEvents.Messages;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;
using Common = SFA.DAS.Payments.Model.Core;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

// ReSharper disable InconsistentNaming

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public class GSLEarningsMapper : IGSLEarningsMapper
    {
        public GrowthAndSkillsEarningModel MapToGrowthAndSkillsEarningModel(CalculateGrowthAndSkillsPayments source)
        {
            return new GrowthAndSkillsEarningModel
            {
                EarningsId = source.EarningsId,
                UKPRN = source.UKPRN,
                LearnerId = source.Learner.LearnerId,
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

        public GSLShortCourseEarningsEvent MapToShortCourseEarningEvent(CalculateGrowthAndSkillsPayments source, short academicYear, byte collectionPeriod)
        {
            var earningsEvent = new GSLShortCourseEarningsEvent
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
                    StartDate = source.Training.StartDate,
                    Reference = source.Training.CourseReference
                },
                CollectionPeriod = new Common.CollectionPeriod
                {
                    AcademicYear = academicYear,
                    Period = collectionPeriod
                },
                PriceEpisodes = MapToEarningEventPriceEpisodes(source),
                AgeAtStartOfLearning = source.Training.AgeAtStartOfTraining,
                Earnings = MapToEarnings(source),
                FundingPlatformType = FundingPlatformType.DigitalApprenticeshipService
            };

            return earningsEvent;
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

        private List<Common.PriceEpisode> MapToEarningEventPriceEpisodes(CalculateGrowthAndSkillsPayments source)
        {
            var priceEpisodes = new List<Common.PriceEpisode>();

            foreach (var earning in source.Earnings)
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

        private IEnumerable<ShortCourseEarning> MapToEarnings(CalculateGrowthAndSkillsPayments source)
        {
            var shortCourseEarnings = new List<ShortCourseEarning>();

            foreach (var earning in source.Earnings)
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

        public DasEarningsReceivedEvent MapToDasEarningsReceivedEvent(CalculateGrowthAndSkillsPayments source, short academicYear, byte collectionPeriod)
        {
            var earningsEvent = new DasEarningsReceivedEvent
            {
                EarningsId = source.EarningsId,
                CourseCode = source.Training.CourseCode,
                CollectionPeriod = new Common.CollectionPeriod
                {
                    AcademicYear = academicYear,
                    Period = collectionPeriod
                },
                AcademicYear = academicYear,
                ULN = source.Learner.ULN,
                UKPRN = source.UKPRN,
                LearningAimReference = source.Training.CourseReference,
            };

            return earningsEvent;
        }
    }
}


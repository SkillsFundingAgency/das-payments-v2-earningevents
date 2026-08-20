using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using UUIDNext;
using Common = SFA.DAS.Payments.Model.Core;
using LearningType = SFA.DAS.Payments.Model.Core.Entities.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping
{
    public class GSLApprenticeshipsMapper : GrowthAndSkillsMapper, IGSLApprenticeshipsMapper
    {
        private const int ApprenticeshipProgrammeType = 25;
        private const int ApprenticeshipFundingAge19 = 19;
        private const string FundingLineType16To18 = "16-18 Apprenticeship (Employer on App Service)";
        private const string FundingLineType19Plus = "19+ Apprenticeship (Employer on App Service)";
        private const int FundingRules2026AgeThreshold = 25;
        private const decimal DefaultSfaContribution = 0.95m;

        private static readonly HashSet<int> OnProgrammeEarningTypes = new()
        {
            (int)OnProgrammeEarningType.Learning,
            (int)OnProgrammeEarningType.Completion,
            (int)OnProgrammeEarningType.Balancing,
        };

        private static readonly HashSet<int> IncentiveEarningTypes = new()
        {
            (int)IncentiveEarningType.First16To18EmployerIncentive,
            (int)IncentiveEarningType.First16To18ProviderIncentive,
            (int)IncentiveEarningType.Second16To18EmployerIncentive,
            (int)IncentiveEarningType.Second16To18ProviderIncentive,
            (int)IncentiveEarningType.OnProgramme16To18FrameworkUplift,
            (int)IncentiveEarningType.Completion16To18FrameworkUplift,
            (int)IncentiveEarningType.Balancing16To18FrameworkUplift,
            (int)IncentiveEarningType.FirstDisadvantagePayment,
            (int)IncentiveEarningType.SecondDisadvantagePayment,
            (int)IncentiveEarningType.LearningSupport,
            (int)IncentiveEarningType.CareLeaverApprenticePayment
        };

        public IEnumerable<GSLApprenticeshipEarningsEvent> MapToApprenticeshipEarningEvents(CalculateGrowthAndSkillsPayments source, 
            IEnumerable<CollectionPeriodModel> openCollectionPeriods)
        {
            var earningEvents = new Dictionary<short, GSLApprenticeshipEarningsEvent>();
            var collectionPeriods = openCollectionPeriods
                .GroupBy(x => x.AcademicYear)
                .ToDictionary(x => x.Key, x => x.First());

            var earnings = source.Earnings.Where(e => collectionPeriods.ContainsKey(e.AcademicYear)).ToList();

            if (!earnings.Any())
            {
                foreach (var collectionPeriod in collectionPeriods)
                {
                    var earningEvent = GenerateApprenticeshipEarningEvent(source, collectionPeriod.Key, openCollectionPeriods);
                    earningEvents.Add(earningEvent.Key, earningEvent.Value);
                }
                return earningEvents.Values.ToList();
            }

            foreach (var earning in earnings)
            {
                if (!earningEvents.ContainsKey(earning.AcademicYear))
                {
                    var earningEvent = GenerateApprenticeshipEarningEvent(source, earning.AcademicYear, openCollectionPeriods);
                    earningEvents.Add(earningEvent.Key, earningEvent.Value);
                }
            }

            foreach (var collectionPeriod in openCollectionPeriods)
            {
                if (earningEvents.ContainsKey(collectionPeriod.AcademicYear))
                {
                    earningEvents[collectionPeriod.AcademicYear].OnProgrammeEarnings = MapToOnProgrammeEarnings(source, collectionPeriod.AcademicYear);
                    earningEvents[collectionPeriod.AcademicYear].IncentiveEarnings = MapToIncentiveEarnings(source, collectionPeriod.AcademicYear);
                    earningEvents[collectionPeriod.AcademicYear].PriceEpisodes = MapToEarningEventPriceEpisodes(source, collectionPeriod.AcademicYear);
                }
            }

            return earningEvents.Values.ToList();
        }

        private List<Common.PriceEpisode> MapToEarningEventPriceEpisodes(CalculateGrowthAndSkillsPayments source, short academicYear)
        {
            var priceEpisodes = new List<Common.PriceEpisode>();
            foreach (var earning in source.Earnings.Where(x => x.AcademicYear == academicYear))
                foreach (var pricePeriod in earning.PricePeriods)
                    priceEpisodes.Add(new Common.PriceEpisode
                    {
                        Identifier = BuildPriceEpisodeIdentifier(source.Training, pricePeriod.StartDate),
                        AgreedPrice = pricePeriod.Price,
                        CourseStartDate = source.Training.StartDate,
                        StartDate = pricePeriod.StartDate,
                        EffectiveTotalNegotiatedPriceStartDate = pricePeriod.StartDate,
                        PlannedEndDate = source.Training.PlannedEndDate,
                        ActualEndDate = source.Training.ActualEndDate,
                        NumberOfInstalments = pricePeriod.NumberOfInstalments,
                        InstalmentAmount = pricePeriod.InstalmentAmount,
                        CompletionAmount = pricePeriod.CompletionAmount,
                        Completed = (source.Training.TrainingStatus == TrainingStatus.Completed),
                        FundingLineType = MapFundingLineTypeForApprenticeship(source.Training.AgeAtStartOfTraining)
                    });
            return priceEpisodes;
        }

        private List<IncentiveEarning> MapToIncentiveEarnings(CalculateGrowthAndSkillsPayments source, short academicYear)
        {
            var incentiveEarnings = new List<IncentiveEarning>();

            foreach (var earning in source.Earnings.Where(x => x.AcademicYear == academicYear))
            {
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    foreach (var period in pricePeriod.Periods.Where(p => IncentiveEarningTypes.Contains((int)p.EarningType)))
                    {
                        incentiveEarnings.Add(new IncentiveEarning
                        {
                            Type = (IncentiveEarningType)period.EarningType,
                            Periods = new ReadOnlyCollection<Common.EarningPeriod>(new List<Common.EarningPeriod>
                            {
                                new Common.EarningPeriod
                                {
                                    PriceEpisodeIdentifier = BuildPriceEpisodeIdentifier(source.Training, pricePeriod.StartDate),
                                    Period = period.DeliveryPeriod,
                                    Amount = period.Amount,
                                    AccountId = period.Employer.AccountId,
                                    ApprenticeshipId = period.LearningId,
                                    ApprenticeshipEmployerType = (ApprenticeshipEmployerType)period.Employer.EmployerType,
                                    SfaContributionPercentage = CalculateSfaContributionPercentage(source.Training, (ApprenticeshipEmployerType)period.Employer.EmployerType),
                                    TransferSenderAccountId = MapTransferSenderAccountId(period)
                                }
                            })
                        });
                    }
                }
            }

            return incentiveEarnings;
        }

        private List<OnProgrammeEarning> MapToOnProgrammeEarnings(CalculateGrowthAndSkillsPayments source, short academicYear)
        {
            var onProgrammeEarnings = new List<OnProgrammeEarning>();

            foreach (var earning in source.Earnings.Where(x => x.AcademicYear == academicYear))
            {
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    foreach (var period in pricePeriod.Periods.Where(p => OnProgrammeEarningTypes.Contains((int)p.EarningType)))
                    {
                        onProgrammeEarnings.Add(new OnProgrammeEarning
                        {
                            Type = (OnProgrammeEarningType)period.EarningType,
                            Periods = new ReadOnlyCollection<Common.EarningPeriod>(new List<Common.EarningPeriod>
                            {
                                new Common.EarningPeriod
                                {
                                    PriceEpisodeIdentifier = BuildPriceEpisodeIdentifier(source.Training, pricePeriod.StartDate),
                                    Period = period.DeliveryPeriod,
                                    Amount = period.Amount,
                                    AccountId = period.Employer.AccountId,
                                    ApprenticeshipId = period.LearningId,
                                    ApprenticeshipEmployerType = (ApprenticeshipEmployerType)period.Employer.EmployerType,
                                    SfaContributionPercentage = CalculateSfaContributionPercentage(source.Training, (ApprenticeshipEmployerType)period.Employer.EmployerType),
                                    TransferSenderAccountId = MapTransferSenderAccountId(period)
                                }
                            })
                        });
                    }
                }
            }

            return onProgrammeEarnings;
        }

        private string BuildPriceEpisodeIdentifier(Training training, DateTime startDate) => $"{training.CourseCode}-{startDate}";

        private decimal CalculateSfaContributionPercentage(Training training, ApprenticeshipEmployerType employerType)
        {
            int ageAtStartOfLearning = training.AgeAtStartOfTraining;

            if (ageAtStartOfLearning < FundingRules2026AgeThreshold)
            {
                return 1m;
            }

            if (employerType == ApprenticeshipEmployerType.Levy)
            {
                return 0.75m;
            }

            return DefaultSfaContribution;
        }

        private static string MapFundingLineTypeForApprenticeship(int ageAtStartOfTraining)
        {
            return ageAtStartOfTraining < ApprenticeshipFundingAge19
                ? FundingLineType16To18
                : FundingLineType19Plus;
        }

        private KeyValuePair<short, GSLApprenticeshipEarningsEvent> GenerateApprenticeshipEarningEvent(
            CalculateGrowthAndSkillsPayments source, short earningYear, IEnumerable<CollectionPeriodModel> openCollectionPeriods)
        {
            return new KeyValuePair<short, GSLApprenticeshipEarningsEvent>
            (
                earningYear, new GSLApprenticeshipEarningsEvent
                {
                    JobId = 0,
                    EventTime = DateTimeOffset.UtcNow,
                    EventId = Uuid.NewDatabaseFriendly(Database.SqlServer),
                    ExternalEarningsId = source.EarningsId,
                    Ukprn = source.UKPRN,
                    ContractType = ContractType.Act1,
                    Learner = new Common.Learner { ReferenceNumber = source.Learner.Reference, Uln = source.Learner.ULN },
                    LearningAim = new Common.LearningAim
                    {
                        Reference = source.Training.CourseReference,
                        ProgrammeType = ApprenticeshipProgrammeType,
                        StandardCode = int.TryParse(source.Training.CourseCode, out var courseCode) ? courseCode : 0,
                        CourseCode = source.Training.CourseCode,
                        FrameworkCode = 0,
                        PathwayCode = 0,
                        FundingLineType = MapFundingLineTypeForApprenticeship(source.Training.AgeAtStartOfTraining),
                        SequenceNumber = 0,
                        StartDate = source.Training.StartDate,
                        LearningType = (LearningType)source.Training.LearningType
                    },
                    CollectionPeriod = new Common.CollectionPeriod
                    {
                        AcademicYear = earningYear,
                        Period = openCollectionPeriods.First(x => x.AcademicYear == earningYear).Period
                    },
                    AgeAtStartOfLearning = source.Training.AgeAtStartOfTraining,
                    FundingPlatformType = FundingPlatformType.DigitalApprenticeshipService,
                    IlrSubmissionDateTime = SqlDateTime.MinValue.Value,
                    StartDate = source.Training.StartDate,
                    OnProgrammeEarnings = new List<OnProgrammeEarning>(),
                    IncentiveEarnings = new List<IncentiveEarning>(),
                    PriceEpisodes = new List<Common.PriceEpisode>()
                });
        }
    }
}
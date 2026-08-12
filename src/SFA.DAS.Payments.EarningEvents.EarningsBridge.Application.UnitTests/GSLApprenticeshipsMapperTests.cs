using System.Data.SqlTypes;
using FluentAssertions;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using Common = SFA.DAS.Payments.Model.Core;
using CourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]
    public class GSLApprenticeshipsMapperTests
    {
        private CalculateGrowthAndSkillsPayments _message;
        private GSLApprenticeshipsMapper _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new GSLApprenticeshipsMapper();

            _message = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = Guid.NewGuid(),
                EmployerContribution = 1000m,
                UKPRN = 10002233,
                Training = new Training
                {
                    CourseCode = "123456",
                    CourseType = CourseType.Apprenticeship,
                    CourseReference = "ZPROG001",
                    LearningType = LearningType.Apprenticeship,
                    StartDate = new DateTime(2026, 1, 1),
                    TrainingStatus = TrainingStatus.Continuing,
                    AgeAtStartOfTraining = 25,
                    PlannedEndDate = new DateTime(2027, 1, 15),
                    ActualEndDate = null,
                    LearningKey = Guid.NewGuid()
                },
                Learner = new Learner
                {
                    ULN = 12345678,
                    Reference = "LEARNREF001",
                    LearnerKey = Guid.NewGuid()
                },
                Earnings = new List<Earnings>
                {
                    new Earnings
                    {
                        AcademicYear = 2526,
                        PricePeriods = new List<PricePeriod>
                        {
                            new PricePeriod
                            {
                                StartDate = new DateTime(2026, 1, 1),
                                Price = 15000m,
                                EndDate = new DateTime(2026, 7, 31),
                                CompletionAmount = 1000m,
                                InstalmentAmount = 700m,
                                NumberOfInstalments = 12,
                                Periods = new List<EarningPeriod>
                                {
                                    new EarningPeriod
                                    {
                                        Employer = new Employer
                                        {
                                            EmployerType = EmployerType.Levy,
                                            AccountId = 10000,
                                            FundingAccountId = 10000
                                        },
                                        Amount = 700m,
                                        DeliveryPeriod = 1,
                                        EarningType = EarningType.Learning,
                                        LearningId = 123456
                                    },
                                    new EarningPeriod
                                    {
                                        Employer = new Employer
                                        {
                                            EmployerType = EmployerType.Levy,
                                            AccountId = 10000,
                                            FundingAccountId = 10000
                                        },
                                        Amount = 500m,
                                        DeliveryPeriod = 1,
                                        EarningType = EarningType.Completion,
                                        LearningId = 123456
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [Test]
        public void Properties_are_mapped_from_inbound_message_to_apprenticeship_earning_events()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel
                {
                    AcademicYear = 2526,
                    Period = 1,
                    Status = CollectionPeriodStatus.Open
                }
            };

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            earningEvents.Count().Should().Be(1);
            var earningEvent = earningEvents.First();
            VerifyEarningEvent(earningEvent, collectionPeriods[0].Period, 2526);
        }

        [Test]
        public void Properties_are_mapped_from_inbound_message_to_apprenticeship_earning_events_over_multiple_academic_years()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel
                {
                    AcademicYear = 2526,
                    Period = 14,
                    Status = CollectionPeriodStatus.Open
                },
                new CollectionPeriodModel
                {
                    AcademicYear = 2627,
                    Period = 2,
                    Status = CollectionPeriodStatus.Open
                }
            };

            _message.Earnings = _message.Earnings.Concat(new List<Earnings>
            {
                new Earnings
                {
                    AcademicYear = 2627,
                    PricePeriods = new List<PricePeriod>
                    {
                        new PricePeriod
                        {
                            StartDate = new DateTime(2027, 1, 1),
                            Price = 16000m,
                            EndDate = new DateTime(2027, 7, 31),
                            CompletionAmount = 1000m,
                            InstalmentAmount = 800m,
                            NumberOfInstalments = 12,
                            Periods = new List<EarningPeriod>
                            {
                                new EarningPeriod
                                {
                                    Employer = new Employer
                                    {
                                        EmployerType = EmployerType.Levy,
                                        AccountId = 10000,
                                        FundingAccountId = 10000
                                    },
                                    Amount = 800m,
                                    DeliveryPeriod = 1,
                                    EarningType = EarningType.Learning,
                                    LearningId = 123456
                                }
                            }
                        }
                    }
                }
            }).ToList();

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods).ToList();

            earningEvents.Count.Should().Be(2);
            var firstEarningEvent = earningEvents.FirstOrDefault(x => x.CollectionPeriod.AcademicYear == 2526);
            var secondEarningEvent = earningEvents.FirstOrDefault(x => x.CollectionPeriod.AcademicYear == 2627);

            VerifyEarningEvent(firstEarningEvent, collectionPeriods[0].Period, 2526);
            VerifyEarningEvent(secondEarningEvent, collectionPeriods[1].Period, 2627);
        }

        [Test]
        public void ContractType_is_always_Act1()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            earningEvents.First().ContractType.Should().Be(ContractType.Act1);
        }

        [Test]
        public void FundingPlatformType_is_DigitalApprenticeshipService()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            earningEvents.First().FundingPlatformType.Should().Be(FundingPlatformType.DigitalApprenticeshipService);
        }

        [Test]
        public void TrainingStatus_is_mapped_correctly_for_completed_courses()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };
            _message.Training.TrainingStatus = TrainingStatus.Completed;

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            earningEvents.First().PriceEpisodes.First().Completed.Should().BeTrue();
        }

        [Test]
        public void Only_Learning_earning_type_periods_are_mapped_to_OnProgrammeEarnings()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            var earningEvent = earningEvents.First();
            earningEvent.OnProgrammeEarnings.Count.Should().Be(1);
            var onProgrammeEarning = earningEvent.OnProgrammeEarnings.Single();
            onProgrammeEarning.Type.Should().Be(OnProgrammeEarningType.Learning);
            onProgrammeEarning.Periods.Count.Should().Be(1);
            onProgrammeEarning.Periods.Single().Amount.Should().Be(700m);
            earningEvent.IncentiveEarnings.Should().BeEmpty();
        }

        [Test]
        public void PriceEpisode_StartDate_is_mapped_from_the_price_period_not_the_course()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };
            _message.Training.StartDate = new DateTime(2025, 9, 1);
            var pricePeriodStartDate = new DateTime(2026, 1, 1);
            _message.Earnings.First().PricePeriods.First().StartDate = pricePeriodStartDate;

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            var priceEpisode = earningEvents.First().PriceEpisodes.Single();
            priceEpisode.StartDate.Should().Be(pricePeriodStartDate);
            priceEpisode.CourseStartDate.Should().Be(_message.Training.StartDate);
        }

        [Test]
        public void SfaContributionPercentage_is_mapped_correctly_for_non_levy_employers()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };
            foreach (var earning in _message.Earnings)
                foreach (var pricePeriod in earning.PricePeriods)
                    foreach (var earningPeriod in pricePeriod.Periods)
                        earningPeriod.Employer.EmployerType = EmployerType.NonLevy;

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            var earningEvent = earningEvents.First();
            earningEvent.SfaContributionPercentage.Should().Be(1m);
            earningEvent.OnProgrammeEarnings.Single().Periods.Single().SfaContributionPercentage.Should().Be(1m);
        }

        [Test]
        public void TransferSenderAccountId_is_mapped_correctly_when_funding_account_id_is_different_to_employer_account_id()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };
            foreach (var earning in _message.Earnings)
                foreach (var pricePeriod in earning.PricePeriods)
                    foreach (var earningPeriod in pricePeriod.Periods)
                        earningPeriod.Employer.FundingAccountId = 1234567;

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            var learningPeriod = earningEvents.First().OnProgrammeEarnings.Single().Periods.Single();
            learningPeriod.TransferSenderAccountId.Should().Be(1234567);
        }

        [Test]
        public void Blank_earning_event_is_generated_for_open_collection_periods_with_no_matching_earnings()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2627, Period = 2, Status = CollectionPeriodStatus.Open }
            };

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods).ToList();

            earningEvents.Count.Should().Be(1);
            var earningEvent = earningEvents.Single();
            earningEvent.CollectionPeriod.AcademicYear.Should().Be(2627);
            earningEvent.PriceEpisodes.Should().BeEmpty();
            earningEvent.OnProgrammeEarnings.Should().BeEmpty();
            earningEvent.IncentiveEarnings.Should().BeEmpty();
            earningEvent.SfaContributionPercentage.Should().Be(1m);
        }

        private void VerifyEarningEvent(GSLApprenticeshipEarningsEvent? earningEvent, byte collectionPeriod, short academicYear)
        {
            earningEvent.JobId.Should().Be(0);
            earningEvent.EventTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
            earningEvent.EventId.Should().NotBe(Guid.Empty);
            earningEvent.ExternalEarningsId.Should().Be(_message.EarningsId);
            earningEvent.Ukprn.Should().Be(_message.UKPRN);
            earningEvent.ContractType.Should().Be(ContractType.Act1);
            earningEvent.Learner.ReferenceNumber.Should().Be(_message.Learner.Reference);
            earningEvent.Learner.Uln.Should().Be(_message.Learner.ULN);
            earningEvent.LearningAim.Reference.Should().Be(_message.Training.CourseReference);
            earningEvent.LearningAim.CourseCode.Should().Be(_message.Training.CourseCode);
            earningEvent.LearningAim.StartDate.Should().Be(_message.Training.StartDate);
            earningEvent.LearningAim.LearningType.Should().Be((Common.Entities.LearningType)_message.Training.LearningType);
            earningEvent.CollectionPeriod.AcademicYear.Should().Be(academicYear);
            earningEvent.CollectionPeriod.Period.Should().Be(collectionPeriod);
            earningEvent.AgeAtStartOfLearning.Should().Be(_message.Training.AgeAtStartOfTraining);
            earningEvent.FundingPlatformType.Should().Be(FundingPlatformType.DigitalApprenticeshipService);
            earningEvent.IlrSubmissionDateTime.Should().Be(SqlDateTime.MinValue.Value);
            earningEvent.StartDate.Should().Be(_message.Training.StartDate);

            var earning = _message.Earnings.Single(x => x.AcademicYear == academicYear);
            var pricePeriod = earning.PricePeriods.Single();

            var priceEpisode = earningEvent.PriceEpisodes.Single();
            priceEpisode.Identifier.Should().Be($"{_message.Training.CourseCode}-{pricePeriod.StartDate}");
            priceEpisode.AgreedPrice.Should().Be(pricePeriod.Price);
            priceEpisode.StartDate.Should().Be(pricePeriod.StartDate);
            priceEpisode.EffectiveTotalNegotiatedPriceStartDate.Should().Be(pricePeriod.StartDate);
            priceEpisode.CourseStartDate.Should().Be(_message.Training.StartDate);
            priceEpisode.PlannedEndDate.Should().Be(_message.Training.PlannedEndDate);
            priceEpisode.ActualEndDate.Should().Be(_message.Training.ActualEndDate);
            priceEpisode.NumberOfInstalments.Should().Be(pricePeriod.NumberOfInstalments);
            priceEpisode.InstalmentAmount.Should().Be(pricePeriod.InstalmentAmount);
            priceEpisode.CompletionAmount.Should().Be(pricePeriod.CompletionAmount);
            priceEpisode.FundingLineType.Should().Be("");

            var learningEarningPeriod = pricePeriod.Periods.Single(p => p.EarningType == EarningType.Learning);
            var onProgrammeEarning = earningEvent.OnProgrammeEarnings.Single();
            onProgrammeEarning.Type.Should().Be(OnProgrammeEarningType.Learning);
            var mappedPeriod = onProgrammeEarning.Periods.Single();
            mappedPeriod.PriceEpisodeIdentifier.Should().Be(priceEpisode.Identifier);
            mappedPeriod.Period.Should().Be(learningEarningPeriod.DeliveryPeriod);
            mappedPeriod.Amount.Should().Be(learningEarningPeriod.Amount);
            mappedPeriod.AccountId.Should().Be(learningEarningPeriod.Employer.AccountId);
            mappedPeriod.ApprenticeshipId.Should().Be(learningEarningPeriod.LearningId);
            ((int)mappedPeriod.ApprenticeshipEmployerType).Should().Be((int)learningEarningPeriod.Employer.EmployerType);
            mappedPeriod.SfaContributionPercentage.Should().Be(0.95m);
            mappedPeriod.TransferSenderAccountId.Should().BeNull();

            earningEvent.IncentiveEarnings.Should().BeEmpty();
        }
    }
}

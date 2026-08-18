using System.Data.SqlTypes;
using FluentAssertions;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
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
    public class GslApprenticeshipsMapperTests
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
        public void Properties_Are_Mapped_From_Inbound_Message_To_Apprenticeship_Earning_Events()
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
        public void Properties_Are_Mapped_From_Inbound_Message_To_Apprenticeship_Earning_Events_Over_Multiple_Academic_Years()
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
        public void ContractType_Is_Always_Act1()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            earningEvents.First().ContractType.Should().Be(ContractType.Act1);
        }

        [Test]
        public void FundingPlatformType_Is_DigitalApprenticeshipService()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            earningEvents.First().FundingPlatformType.Should().Be(FundingPlatformType.DigitalApprenticeshipService);
        }

        [Test]
        public void TrainingStatus_Is_Mapped_Correctly_For_Completed_Courses()
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
        public void Only_Learning_Earning_Type_Periods_Are_Mapped_To_OnProgrammeEarnings()
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
        public void PriceEpisode_StartDate_Is_Mapped_From_The_Price_Period_Not_The_Course()
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
        public void SfaContributionPercentage_Is_75_Percent_For_Levy_Employers_When_Apprentice_Is_25_Or_Over()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };

            _message.Training.AgeAtStartOfTraining = 25;

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            earningEvents.First()
                .OnProgrammeEarnings.Single()
                .Periods.Single()
                .SfaContributionPercentage
                .Should().Be(0.75m);
        }

        [Test]
        public void SfaContributionPercentage_Is_100_Percent_For_Apprentices_Under_25()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };

            _message.Training.StartDate = new DateTime(2026, 8, 1);
            _message.Training.AgeAtStartOfTraining = 24;

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            earningEvents.First()
                .OnProgrammeEarnings.Single()
                .Periods.Single()
                .SfaContributionPercentage
                .Should().Be(1m);
        }

        [Test]
        public void SfaContributionPercentage_Is_95_Percent_For_NonLevy_Employers_When_Apprentice_Is_25_Or_Over()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };

            _message.Training.StartDate = new DateTime(2026, 8, 1);
            _message.Training.AgeAtStartOfTraining = 25;
            _message.Earnings.First().PricePeriods.First().Periods.Single(p => p.EarningType == EarningType.Learning)
                .Employer.EmployerType = EmployerType.NonLevy;

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            earningEvents.First()
                .OnProgrammeEarnings.Single()
                .Periods.Single()
                .SfaContributionPercentage
                .Should().Be(0.95m);
        }

        [Test]
        public void SfaContributionPercentage_Is_Calculated_Per_Price_Period_When_The_Employer_Changes_During_Training()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };
            _message.Training.StartDate = new DateTime(2026, 8, 1);
            _message.Training.AgeAtStartOfTraining = 25;
            _message.Earnings.First().PricePeriods.First().Periods.Single(p => p.EarningType == EarningType.Learning).Employer.EmployerType = EmployerType.Levy;
            _message.Earnings.First().PricePeriods = _message.Earnings.First().PricePeriods.Concat(new List<PricePeriod>
            {
                new PricePeriod
                {
                    StartDate = new DateTime(2027, 1, 1),
                    Price = 15000m,
                    EndDate = null,
                    CompletionAmount = 1000m,
                    InstalmentAmount = 700m,
                    NumberOfInstalments = 12,
                    Periods = new List<EarningPeriod>
                    {
                        new EarningPeriod
                        {
                            Employer = new Employer
                            {
                                EmployerType = EmployerType.NonLevy,
                                AccountId = 20000,
                                FundingAccountId = 20000
                            },
                            Amount = 700m,
                            DeliveryPeriod = 2,
                            EarningType = EarningType.Learning,
                            LearningId = 123456
                        }
                    }
                }
            }).ToList();

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            var learningPeriods = earningEvents.First().OnProgrammeEarnings.Single().Periods.ToList();
            learningPeriods.Count.Should().Be(2);
            learningPeriods.Single(p => p.AccountId == 10000).SfaContributionPercentage.Should().Be(0.75m);
            learningPeriods.Single(p => p.AccountId == 20000).SfaContributionPercentage.Should().Be(0.95m);
        }

        [Test]
        public void TransferSenderAccountId_Is_Mapped_Correctly_When_Funding_Account_Id_Is_Different_To_Employer_Account_Id()
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
        public void Blank_Earning_Event_Is_Generated_For_Open_Collection_Periods_With_No_Matching_Earnings()
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

        [Test]
        public void FundingLineType_Is_16To18_When_Age_At_Start_Of_Training_Is_Under_19()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };

            _message.Training.AgeAtStartOfTraining = 18;

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            earningEvents.First()
                .PriceEpisodes.Single()
                .FundingLineType
                .Should().Be("16-18 Apprenticeship (Employer on App Service)");
        }

        [Test]
        public void FundingLineType_Is_19Plus_When_Age_At_Start_Of_Training_Is_19()
        {
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel { AcademicYear = 2526, Period = 1, Status = CollectionPeriodStatus.Open }
            };

            _message.Training.AgeAtStartOfTraining = 19;

            var earningEvents = _sut.MapToApprenticeshipEarningEvents(_message, collectionPeriods);

            earningEvents.First()
            .PriceEpisodes.Single()
            .FundingLineType
            .Should().Be("19+ Apprenticeship (Employer on App Service)");
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
            priceEpisode.FundingLineType.Should().Be("19+ Apprenticeship (Employer on App Service)");

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
            mappedPeriod.SfaContributionPercentage.Should().Be(0.75m);
            mappedPeriod.TransferSenderAccountId.Should().BeNull();

            earningEvent.IncentiveEarnings.Should().BeEmpty();
        }
    }
}

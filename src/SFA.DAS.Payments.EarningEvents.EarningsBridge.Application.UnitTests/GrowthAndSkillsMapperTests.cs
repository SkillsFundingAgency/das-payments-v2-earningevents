
using FluentAssertions;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;
using Common = SFA.DAS.Payments.Model.Core;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]

    // ReSharper disable once InconsistentNaming
    public class GrowthAndSkillsMapperTests
    {
        private CalculateGrowthAndSkillsPayments _message;
        private GrowthAndSkillsMapper _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new GrowthAndSkillsMapper();

            _message = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = Guid.NewGuid(),
                EmployerContribution = 1000m,
                UKPRN = 10002233,
                Training = new Training
                {
                    CourseCode = "123456",
                    CourseReference = "ZSC00123",
                    LearningType = LearningType.ApprenticeshipUnit,
                    StartDate = new DateTime(2026, 1, 1),
                    TrainingStatus = TrainingStatus.Continuing,
                    AgeAtStartOfTraining = 25,
                    PlannedEndDate = new DateTime(2026, 1, 15),
                    ActualEndDate = new DateTime(2026, 1, 31)
                },
                Learner = new Learner
                {
                    ULN = 12345678,
                    Reference = "LEARNREF001",
                    LearnerId = Guid.NewGuid()
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
                                Price = 5000m,
                                EndDate = new DateTime(2026, 1, 31),
                                CompletionAmount = 1000m,
                                InstalmentAmount = 2000m,
                                NumberOfInstalments = 2,
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
                                        Amount = 2000m,
                                        DeliveryPeriod = 1,
                                        EarningType = EarningType.Milestone1,
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
        public void Properties_are_mapping_from_inbound_message_to_database_model()
        {
            // Act
            var model = _sut.MapToGrowthAndSkillsEarningModel(_message);

            // Assert
            model.EarningsId.Should().Be(_message.EarningsId);
            model.UKPRN.Should().Be(_message.UKPRN);
            model.LearnerId.Should().Be(_message.Learner.LearnerId);
            model.LearnerReference.Should().Be(_message.Learner.Reference);
            model.LearnerUln.Should().Be(_message.Learner.ULN);
            var learningTypeValue = (int)model.LearningType;
            learningTypeValue.Should().Be((int)_message.Training.LearningType);
            model.CourseCode.Should().Be(_message.Training.CourseCode);
            model.CourseReference.Should().Be(_message.Training.CourseReference);
            model.StartDate.Should().Be(_message.Training.StartDate);
            model.AgeAtStartOfTraining.Should().Be(_message.Training.AgeAtStartOfTraining);
            model.PlannedEndDate.Should().Be(_message.Training.PlannedEndDate);
            model.ActualEndDate.Should().Be(_message.Training.ActualEndDate);
            var trainingStatusValue = model.TrainingStatus;
            trainingStatusValue.Should().Be((int)_message.Training.TrainingStatus);
            model.EmployerContribution.Should().Be(_message.EmployerContribution);
            var courseTypeValue = (int)model.CourseType;
            courseTypeValue.Should().Be((int)_message.Training.CourseType);
            var pricePeriodModels = model.PricePeriods.ToArray();
            foreach (var earning in _message.Earnings)
            {
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    var pricePeriods = pricePeriod.Periods.ToArray();
                    for (var i = 0; i < pricePeriods.Length; i++)
                    {
                        pricePeriodModels[i].AcademicYear.Should().Be(earning.AcademicYear);
                        pricePeriodModels[i].Price.Should().Be(pricePeriod.Price);
                        pricePeriodModels[i].StartDate.Should().Be(pricePeriod.StartDate);
                        pricePeriodModels[i].EndDate.Should().Be(pricePeriod.EndDate);
                        var earningTypeValue = (int)pricePeriodModels[i].EarningType;
                        earningTypeValue.Should().Be((int)pricePeriods[i].EarningType);
                        pricePeriodModels[i].Amount.Should().Be(pricePeriods[i].Amount);
                        pricePeriodModels[i].EmployerAccountId.Should().Be(pricePeriods[i].Employer.AccountId);
                        var employerTypeValue = (int)pricePeriodModels[i].EmployerType;
                        employerTypeValue.Should().Be((int)pricePeriods[i].Employer.EmployerType);
                        pricePeriodModels[i].FundingAccountId.Should().Be(pricePeriods[i].Employer.FundingAccountId);
                        pricePeriodModels[i].GrowthAndSkillsEarningsId.Should().Be(_message.EarningsId);
                        pricePeriodModels[i].ApprenticeshipId.Should().Be(pricePeriods[i].LearningId);
                    }
                }
            }
        }

        [Test]
        public void Properties_are_mapped_from_inbound_message_to_short_course_earning_event()
        {
            // Arrange
            short academicYear = 2526;
            byte collectionPeriod = 1;

            // Act
            var earningEvent = _sut.MapToShortCourseEarningEvent(_message, academicYear, collectionPeriod);

            // Assert
            var expectedFundingLineType = "GSO Short Courses (Apprenticeship Units) Levy";
            earningEvent.JobId.Should().Be(0);
            earningEvent.EventTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
            earningEvent.EventId.Should().NotBe(Guid.Empty);
            earningEvent.ExternalEarningsId.Should().Be(_message.EarningsId);
            earningEvent.Ukprn.Should().Be(_message.UKPRN);
            earningEvent.Learner.ReferenceNumber.Should().Be(_message.Learner.Reference);
            earningEvent.Learner.Uln.Should().Be(_message.Learner.ULN);
            earningEvent.LearningAim.Reference.Should().Be(_message.Training.CourseReference);
            earningEvent.LearningAim.ProgrammeType.Should().Be(0);
            earningEvent.LearningAim.StandardCode.Should().Be(Convert.ToInt32(_message.Training.CourseCode));
            earningEvent.LearningAim.CourseCode.Should().Be(_message.Training.CourseCode);
            earningEvent.LearningAim.FrameworkCode.Should().Be(0);
            earningEvent.LearningAim.PathwayCode.Should().Be(0);
            earningEvent.LearningAim.FundingLineType.Should().Be("");
            earningEvent.LearningAim.SequenceNumber.Should().Be(0);
            earningEvent.LearningAim.StartDate.Should().Be(_message.Training.StartDate);
            earningEvent.LearningAim.LearningType.Should().Be((Common.TrainingType)_message.Training.LearningType);
            earningEvent.CollectionPeriod.AcademicYear.Should().Be(academicYear);
            earningEvent.CollectionPeriod.Period.Should().Be(collectionPeriod);
            var eventPriceEpisodes = earningEvent.PriceEpisodes.ToArray();
            foreach (var earning in _message.Earnings)
            {
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    var pricePeriods = pricePeriod.Periods.ToArray();
                    for (var i = 0; i < pricePeriods.Length; i++)
                    {
                        var expectedPriceEpisodeIdentifier = $"{_message.Training.CourseCode}-{pricePeriod.StartDate}";
                        eventPriceEpisodes[i].Identifier.Should().Be(expectedPriceEpisodeIdentifier);
                        eventPriceEpisodes[i].AgreedPrice.Should().Be(pricePeriod.Price);
                        eventPriceEpisodes[i].CourseStartDate.Should().Be(_message.Training.StartDate);
                        eventPriceEpisodes[i].EffectiveTotalNegotiatedPriceStartDate.Should().Be(_message.Training.StartDate);
                        eventPriceEpisodes[i].PlannedEndDate.Should().Be(_message.Training.PlannedEndDate);
                        eventPriceEpisodes[i].ActualEndDate.Should().Be(_message.Training.ActualEndDate);
                        eventPriceEpisodes[i].NumberOfInstalments.Should().Be(pricePeriod.NumberOfInstalments);
                        eventPriceEpisodes[i].InstalmentAmount.Should().Be(pricePeriod.InstalmentAmount);
                        eventPriceEpisodes[i].CompletionAmount.Should().Be(pricePeriod.CompletionAmount);
                        eventPriceEpisodes[i].Completed.Should().BeFalse();
                        eventPriceEpisodes[i].FundingLineType.Should().Be(expectedFundingLineType);
                    }
                }
            }

            var eventEarnings = earningEvent.Earnings.ToArray();
            foreach (var earning in _message.Earnings)
            {
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    var pricePeriods = pricePeriod.Periods.ToArray();
                    for (var i = 0; i < pricePeriods.Length; i++)
                    {
                        var earningTypeValue = (int)eventEarnings[i].Type;
                        earningTypeValue.Should().Be((int)pricePeriods[i].EarningType);
                        eventEarnings[i].Periods.Count().Should().Be(1);
                        var earningPeriod = eventEarnings[i].Periods.FirstOrDefault();
                        earningPeriod.AccountId.Should().Be(pricePeriods[i].Employer.AccountId);
                        earningPeriod.Amount.Should().Be(pricePeriods[i].Amount);
                        var employerTypeValue = (int)earningPeriod.ApprenticeshipEmployerType;
                        employerTypeValue.Should().Be((int)pricePeriods[i].Employer.EmployerType);
                        earningPeriod.Period.Should().Be(pricePeriods[i].DeliveryPeriod);
                        earningPeriod.SfaContributionPercentage.Should().Be(0.95m); // 95% funding for Levy employers
                        earningPeriod.ApprenticeshipId.Should().Be(pricePeriods[i].LearningId);
                    }
                }
            }
        }

        [Test]
        public void TrainingStatus_is_mapped_correctly_for_completed_courses()
        {
            // Arrange
            short academicYear = 2526;
            byte collectionPeriod = 1;
            _message.Training.TrainingStatus = TrainingStatus.Completed;

            // Act
            var earningEvent = _sut.MapToShortCourseEarningEvent(_message, academicYear, collectionPeriod);

            // Assert
            earningEvent.PriceEpisodes[0].Completed.Should().BeTrue();
        }

        [Test]
        public void FundingLineType_is_mapped_correctly_for_non_levy_employers()
        {
            // Arrange
            short academicYear = 2526;
            byte collectionPeriod = 1;
            foreach (var earning in _message.Earnings)
            {
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    foreach (var earningPeriod in pricePeriod.Periods)
                    {
                        earningPeriod.Employer.EmployerType = EmployerType.NonLevy;
                    }
                }
            }

            // Act
            var earningEvent = _sut.MapToShortCourseEarningEvent(_message, academicYear, collectionPeriod);

            // Assert
            var expectedFundingLineType = "GSO Short Courses (Apprenticeship Units) Non-Levy";
            foreach (var priceEpisode in earningEvent.PriceEpisodes)
            {
                priceEpisode.FundingLineType.Should().Be(expectedFundingLineType);
            }
        }

        [Test]
        public void SfaContributionPercentage_is_mapped_correctly_for_non_levy_employers()
        {
            short academicYear = 2526;
            byte collectionPeriod = 1;
            foreach (var earning in _message.Earnings)
            {
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    foreach (var earningPeriod in pricePeriod.Periods)
                    {
                        earningPeriod.Employer.EmployerType = EmployerType.NonLevy;
                    }
                }
            }

            // Act
            var earningEvent = _sut.MapToShortCourseEarningEvent(_message, academicYear, collectionPeriod);

            // Assert
            foreach (var earning in earningEvent.Earnings)
            {
                foreach (var earningPeriod in earning.Periods)
                {
                    earningPeriod.SfaContributionPercentage.Should().Be(1m);    // 100%
                }
            }
        }

        [Test]
        public void Properties_are_mapped_from_inbound_message_to_das_earnings_received_event()
        {
            // Arrange
            short academicYear = 2526;
            byte collectionPeriod = 1;

            // Act
            var earningEvent = _sut.MapToDasEarningsReceivedEvent(_message, academicYear, collectionPeriod);

            //  Assert
            earningEvent.EarningsId.Should().Be(_message.EarningsId);
            earningEvent.CourseCode.Should().Be(_message.Training.CourseCode);
            earningEvent.CollectionPeriod.AcademicYear.Should().Be(academicYear);
            earningEvent.CollectionPeriod.Period.Should().Be(collectionPeriod);
            earningEvent.ULN.Should().Be(_message.Learner.ULN);
            earningEvent.UKPRN.Should().Be(_message.UKPRN);
            earningEvent.LearningAimReference.Should().Be(_message.Training.CourseReference);
        }

        [Test]
        public void Properties_are_mapped_from_collection_period_API_response_to_collection_period_models()
        {
            // Arrange
            var collectionPeriodApiResponse = new CollectionYear
            {
                Status = CollectionPeriodStatus.Open,
                Year = 2526,
                Periods = new List<CollectionPeriod>
                {
                    new CollectionPeriod
                    {
                        CalendarMonth = 6,
                        CalendarYear = 2026,
                        Id = 1234,
                        Period = 6,
                        Status = CollectionPeriodStatus.Open
                    },
                    new CollectionPeriod
                    {
                        CalendarMonth = 7,
                        CalendarYear = 2026,
                        Id = 1235,
                        Period = 7,
                        Status = CollectionPeriodStatus.NotStarted
                    }
                }
            };

            // Act
            var models = _sut.MapCollectionYearToCollectionPeriodModels(collectionPeriodApiResponse).ToArray();

            // Assert
            models.Length.Should().Be(collectionPeriodApiResponse.Periods.Count());
            var periods = collectionPeriodApiResponse.Periods.ToArray();
            for (var i = 0; i < models.Length; i++)
            {
                models[i].AcademicYear.Should().Be(collectionPeriodApiResponse.Year);
                models[i].Status.Should().Be(periods[i].Status);
                models[i].Period.Should().Be(periods[i].Period);
                models[i].Id.Should().Be(periods[i].Id);
            }
        }
    }
}

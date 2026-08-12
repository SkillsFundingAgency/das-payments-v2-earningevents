using System.Data.SqlTypes;
using FluentAssertions;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;
using Common = SFA.DAS.Payments.Model.Core;
using CourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]
    public class GSLShortCoursesMapperTests
    {
        private CalculateGrowthAndSkillsPayments _message;
        private GSLShortCoursesMapper _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new GSLShortCoursesMapper();

            _message = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = Guid.NewGuid(),
                EmployerContribution = 1000m,
                UKPRN = 10002233,
                Training = new Training
                {
                    CourseCode = "123456",
                    CourseType = CourseType.ShortCourse,
                    CourseReference = "ZSC00123",
                    LearningType = LearningType.ApprenticeshipUnit,
                    StartDate = new DateTime(2026, 1, 1),
                    TrainingStatus = TrainingStatus.Continuing,
                    AgeAtStartOfTraining = 25,
                    PlannedEndDate = new DateTime(2026, 1, 15),
                    ActualEndDate = new DateTime(2026, 1, 31),
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
        public void Properties_are_mapped_from_inbound_message_to_short_course_earning_events()
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

            var earningEvents = _sut.MapToShortCourseEarningEvents(_message, collectionPeriods);

            earningEvents.Count().Should().Be(1);
            var earningEvent = earningEvents.First();
            var expectedFundingLineType = "GSO Short Courses (Apprenticeship Units) Levy";
            VerifyEarningsAndPricePeriods(earningEvent, collectionPeriods, expectedFundingLineType, collectionPeriods[0].Period, 2526);
        }

        [Test]
        public void Properties_are_mapped_from_inbound_message_to_short_course_earning_events_over_multiple_academic_years()
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

            _message.Earnings = new List<Earnings>
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
                },
                new Earnings
                {
                    AcademicYear = 2627,
                    PricePeriods = new List<PricePeriod>
                    {
                        new PricePeriod
                        {
                            StartDate = new DateTime(2027, 1, 1),
                            Price = 4000m,
                            EndDate = new DateTime(2027, 1, 31),
                            CompletionAmount = 1500m,
                            InstalmentAmount = 1000m,
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
                                    EarningType = EarningType.Completion,
                                    LearningId = 123456
                                }
                            }
                        }
                    }
                }
            };

            var earningEvents = _sut.MapToShortCourseEarningEvents(_message, collectionPeriods);

            earningEvents.Count().Should().Be(2);
            var firstEarningEvent = earningEvents.FirstOrDefault(x => x.CollectionPeriod.AcademicYear == 2526);
            var secondEarningEvent = earningEvents.FirstOrDefault(x => x.CollectionPeriod.AcademicYear == 2627);

            var expectedFundingLineType = "GSO Short Courses (Apprenticeship Units) Levy";
            VerifyEarningsAndPricePeriods(firstEarningEvent, collectionPeriods, expectedFundingLineType, collectionPeriods[0].Period, 2526);
            VerifyEarningsAndPricePeriods(secondEarningEvent, collectionPeriods, expectedFundingLineType, collectionPeriods[1].Period, 2627);
        }

        [Test]
        public void TrainingStatus_is_mapped_correctly_for_completed_courses()
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
            _message.Training.TrainingStatus = TrainingStatus.Completed;

            var earningEvents = _sut.MapToShortCourseEarningEvents(_message, collectionPeriods);

            earningEvents.ToList()[0].PriceEpisodes[0].Completed.Should().BeTrue();
        }

        [Test]
        public void StandardCode_is_zero_when_course_type_is_short_course()
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

            var earningEvents = _sut.MapToShortCourseEarningEvents(_message, collectionPeriods);

            earningEvents.ToList()[0].LearningAim.StandardCode.Should().Be(0);
        }

        [Test]
        public void FundingLineType_is_mapped_correctly_for_non_levy_employers()
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

            var earningEvents = _sut.MapToShortCourseEarningEvents(_message, collectionPeriods);

            var earningEvent = earningEvents.First();
            var expectedFundingLineType = "GSO Short Courses (Apprenticeship Units) Non-Levy";
            foreach (var priceEpisode in earningEvent.PriceEpisodes)
            {
                priceEpisode.FundingLineType.Should().Be(expectedFundingLineType);
            }
        }

        [Test]
        public void SfaContributionPercentage_is_mapped_correctly_for_non_levy_employers()
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

            var earningEvents = _sut.MapToShortCourseEarningEvents(_message, collectionPeriods);

            var earningEvent = earningEvents.First();
            foreach (var earning in earningEvent.Earnings)
            {
                foreach (var earningPeriod in earning.Periods)
                {
                    earningPeriod.SfaContributionPercentage.Should().Be(1m);
                }
            }
        }

        [Test]
        public void TransferSenderAccountId_is_mapped_correctly_when_funding_account_id_is_different_to_employer_account_id()
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
            foreach (var earning in _message.Earnings)
            {
                foreach (var pricePeriod in earning.PricePeriods)
                {
                    foreach (var earningPeriod in pricePeriod.Periods)
                    {
                        earningPeriod.Employer.FundingAccountId = 1234567;
                    }
                }
            }

            var earningEvents = _sut.MapToShortCourseEarningEvents(_message, collectionPeriods);

            var earningEvent = earningEvents.First();
            foreach (var earning in earningEvent.Earnings)
            {
                foreach (var earningPeriod in earning.Periods)
                {
                    earningPeriod.TransferSenderAccountId.Should().Be(1234567);
                }
            }
        }

        private void VerifyEarningsAndPricePeriods(GSLShortCourseEarningsEvent? earningEvent, List<CollectionPeriodModel> collectionPeriods,
                                                   string expectedFundingLineType, byte collectionPeriod, short academicYear)
        {
            earningEvent.JobId.Should().Be(0);
            earningEvent.EventTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
            earningEvent.EventId.Should().NotBe(Guid.Empty);
            earningEvent.ExternalEarningsId.Should().Be(_message.EarningsId);
            earningEvent.Ukprn.Should().Be(_message.UKPRN);
            earningEvent.Learner.ReferenceNumber.Should().Be(_message.Learner.Reference);
            earningEvent.Learner.Uln.Should().Be(_message.Learner.ULN);
            earningEvent.LearningAim.Reference.Should().Be(_message.Training.CourseReference);
            earningEvent.LearningAim.ProgrammeType.Should().Be(0);
            earningEvent.LearningAim.StandardCode.Should().Be(0);
            earningEvent.LearningAim.CourseCode.Should().Be(_message.Training.CourseCode);
            earningEvent.LearningAim.FrameworkCode.Should().Be(0);
            earningEvent.LearningAim.PathwayCode.Should().Be(0);
            earningEvent.LearningAim.FundingLineType.Should().Be("");
            earningEvent.LearningAim.SequenceNumber.Should().Be(0);
            earningEvent.LearningAim.StartDate.Should().Be(_message.Training.StartDate);
            earningEvent.LearningAim.LearningType.Should().Be((Common.Entities.LearningType)_message.Training.LearningType);
            earningEvent.CollectionPeriod.AcademicYear.Should().Be(academicYear);
            earningEvent.CollectionPeriod.Period.Should().Be(collectionPeriod);
            earningEvent.IlrSubmissionDateTime.Should().Be(SqlDateTime.MinValue.Value);

            var eventPriceEpisodes = earningEvent.PriceEpisodes.ToArray();
            foreach (var earning in _message.Earnings.Where(x => x.AcademicYear == academicYear))
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
            foreach (var earning in _message.Earnings.Where(x => x.AcademicYear == academicYear))
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
                        earningPeriod.TransferSenderAccountId.Should().BeNull();
                        var employerTypeValue = (int)earningPeriod.ApprenticeshipEmployerType;
                        employerTypeValue.Should().Be((int)pricePeriods[i].Employer.EmployerType);
                        earningPeriod.Period.Should().Be(pricePeriods[i].DeliveryPeriod);
                        earningPeriod.SfaContributionPercentage.Should().Be(0.95m);
                        earningPeriod.ApprenticeshipId.Should().Be(pricePeriods[i].LearningId);
                        var expectedPriceEpisodeIdentifier = $"{_message.Training.CourseCode}-{pricePeriod.StartDate}";
                        earningPeriod.PriceEpisodeIdentifier.Should().Be(expectedPriceEpisodeIdentifier);
                    }
                }
            }
        }
    }
}
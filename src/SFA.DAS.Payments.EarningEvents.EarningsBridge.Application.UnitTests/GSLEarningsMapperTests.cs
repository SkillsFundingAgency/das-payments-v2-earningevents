using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using Model = SFA.DAS.Payments.EarningEvents.Model;

//Generated test

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]
    public class GSLEarningsMapperTests
    {
        private GSLEarningsMapper _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new GSLEarningsMapper();
        }

        [Test]
        public void MapToShortCourseEarningModel_Maps_TopLevel_Fields()
        {
            var source = CreateSource();

            var result = _sut.MapToGrowthAndSkillsEarningModel(source);

            result.EarningsId.Should().Be(source.EarningsId);
            result.UKPRN.Should().Be(source.UKPRN);
            result.LearnerId.Should().Be(source.Learner.LearnerId);
            result.LearnerUln.Should().Be(source.Learner.ULN);
            result.LearnerReference.Should().Be(source.Learner.Reference);
            result.LearningType.Should().Be((Model.LearningType)source.Training.LearningType);
            result.CourseCode.Should().Be(source.Training.CourseCode);
            result.StartDate.Should().Be(source.Training.StartDate);
            result.AgeAtStartOfTraining.Should().Be(source.Training.AgeAtStartOfTraining);
            result.PlannedEndDate.Should().Be(source.Training.PlannedEndDate);
            result.ActualEndDate.Should().Be(source.Training.ActualEndDate);
            result.TrainingStatus.Should().Be((Model.TrainingStatus)source.Training.TrainingStatus);
            result.EmployerContribution.Should().Be(source.EmployerContribution);
        }

        [Test]
        public void MapToShortCourseEarningModel_Flattens_PricePeriods()
        {
            var source = CreateSource();

            var result = _sut.MapToGrowthAndSkillsEarningModel(source);

            result.PricePeriods.Should().HaveCount(4);

            var mapped = result.PricePeriods.Single(pp => pp.AcademicYear == 2526 && pp.DeliveryPeriod == 2);

            mapped.Price.Should().Be(1000m);
            mapped.StartDate.Should().Be(new DateTime(2026, 1, 1));
            mapped.EndDate.Should().Be(new DateTime(2026, 2, 28));
            mapped.EarningType.Should().Be(Model.EarningType.Milestone1);
            mapped.Amount.Should().Be(500m);
            mapped.EmployerAccountId.Should().Be(5000);
            mapped.EmployerType.Should().Be(Model.EmployerType.Levy);
            mapped.FundingAccountId.Should().Be(5001);
            mapped.GrowthAndSkillsEarningsId.Should().Be(source.EarningsId);
        }

        private static CalculateGSLPayments CreateSource()
        {
            return new CalculateGSLPayments
            {
                EmployerContribution = 123.45m,
                EarningsId = Guid.NewGuid(),
                UKPRN = 10001234,
                Learner = new Learner
                {
                    LearnerId = Guid.NewGuid(),
                    Reference = "ref-1",
                    ULN = 1234567890
                },
                Training = new Training
                {
                    StartDate = new DateTime(2026, 1, 1),
                    AgeAtStartOfTraining = 25,
                    CourseCode = "ABC123",
                    LearningType = LearningType.ApprenticeshipUnit,
                    PlannedEndDate = new DateTime(2026, 3, 31),
                    ActualEndDate = new DateTime(2026, 4, 30),
                    TrainingStatus = TrainingStatus.Continuing
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
                                EndDate = new DateTime(2026, 2, 28),
                                Price = 1000m,
                                Periods = new List<EarningPeriod>
                                {
                                    new EarningPeriod
                                    {
                                        Amount = 250m,
                                        DeliveryPeriod = 1,
                                        EarningType = EarningType.Learning,
                                        Employer = new Employer
                                        {
                                            AccountId = 5000,
                                            EmployerType = EmployerType.Levy,
                                            FundingAccountId = 5001
                                        }
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 500m,
                                        DeliveryPeriod = 2,
                                        EarningType = EarningType.Milestone1,
                                        Employer = new Employer
                                        {
                                            AccountId = 5000,
                                            EmployerType = EmployerType.Levy,
                                            FundingAccountId = 5001
                                        }
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
                                EndDate = new DateTime(2027, 2, 28),
                                Price = 2000m,
                                Periods = new List<EarningPeriod>
                                {
                                    new EarningPeriod
                                    {
                                        Amount = 750m,
                                        DeliveryPeriod = 3,
                                        EarningType = EarningType.Completion,
                                        Employer = new Employer
                                        {
                                            AccountId = 6000,
                                            EmployerType = EmployerType.NonLevy,
                                            FundingAccountId = 6001
                                        }
                                    },
                                    new EarningPeriod
                                    {
                                        Amount = 900m,
                                        DeliveryPeriod = 4,
                                        EarningType = EarningType.Balancing,
                                        Employer = new Employer
                                        {
                                            AccountId = 6000,
                                            EmployerType = EmployerType.NonLevy,
                                            FundingAccountId = 6001
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
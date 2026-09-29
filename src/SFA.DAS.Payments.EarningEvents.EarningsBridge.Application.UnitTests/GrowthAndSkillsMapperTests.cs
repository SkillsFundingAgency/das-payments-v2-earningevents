
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;
using CourseType = SFA.DAS.Payments.EarningEvents.Messages.External.CourseType;
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
        public void Properties_are_mapping_from_inbound_message_to_database_model()
        {
            // Act
            var model = _sut.MapToGrowthAndSkillsEarningModel(_message);

            // Assert
            model.EarningsId.Should().Be(_message.EarningsId);
            model.UKPRN.Should().Be(_message.UKPRN);
            model.LearnerKey.Should().Be(_message.Learner.LearnerKey);
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
            model.LearningKey.Should().Be(_message.Training.LearningKey);
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
        public void Properties_are_mapped_from_inbound_message_to_das_earnings_received_events()
        {
            // Arrange
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel
                {
                    AcademicYear = 2526,
                    Period = 1,
                    Status = CollectionPeriodStatus.Open
                }
            };

            // Act
            var earningEvents = _sut.MapToDasEarningsReceivedEvents(_message, collectionPeriods);

            //  Assert
            var earningEvent = earningEvents.First();
            earningEvent.EarningsId.Should().Be(_message.EarningsId);
            earningEvent.CourseCode.Should().Be(_message.Training.CourseCode);
            earningEvent.CollectionPeriod.AcademicYear.Should().Be(collectionPeriods[0].AcademicYear);
            earningEvent.CollectionPeriod.Period.Should().Be(collectionPeriods[0].Period);
            earningEvent.ULN.Should().Be(_message.Learner.ULN);
            earningEvent.UKPRN.Should().Be(_message.UKPRN);
            earningEvent.LearningAimReference.Should().Be(_message.Training.CourseReference);
        }

        [Test]
        public void Properties_are_mapped_from_inbound_message_to_das_earnings_received_events_for_multiple_academic_years()
        {
            // Arrange
            var collectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel
                {
                    AcademicYear = 2526,
                    Period = 13,
                    Status = CollectionPeriodStatus.Open
                },
                new CollectionPeriodModel
                {
                    AcademicYear = 2627,
                    Period = 1,
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

            // Act
            var earningEvents = _sut.MapToDasEarningsReceivedEvents(_message, collectionPeriods);

            //  Assert
            earningEvents.Count().Should().Be(2);
            var firstEarning = earningEvents.FirstOrDefault(x => x.CollectionPeriod.AcademicYear == 2526);
            var secondEarning = earningEvents.FirstOrDefault(x => x.CollectionPeriod.AcademicYear == 2627);

            firstEarning.EarningsId.Should().Be(_message.EarningsId);
            firstEarning.CourseCode.Should().Be(_message.Training.CourseCode);
            firstEarning.CollectionPeriod.AcademicYear.Should().Be(collectionPeriods[0].AcademicYear);
            firstEarning.CollectionPeriod.Period.Should().Be(collectionPeriods[0].Period);
            firstEarning.ULN.Should().Be(_message.Learner.ULN);
            firstEarning.UKPRN.Should().Be(_message.UKPRN);
            firstEarning.LearningAimReference.Should().Be(_message.Training.CourseReference);


            secondEarning.EarningsId.Should().Be(_message.EarningsId);
            secondEarning.CourseCode.Should().Be(_message.Training.CourseCode);
            secondEarning.CollectionPeriod.AcademicYear.Should().Be(collectionPeriods[1].AcademicYear);
            secondEarning.CollectionPeriod.Period.Should().Be(collectionPeriods[1].Period);
            secondEarning.ULN.Should().Be(_message.Learner.ULN);
            secondEarning.UKPRN.Should().Be(_message.UKPRN);
            secondEarning.LearningAimReference.Should().Be(_message.Training.CourseReference);
        }


        [Test]
        public void MapToCalculateGrowthAndSkillsPayments_MapsPropertiesFromTheStoredEarningModel()
        {
            // Arrange
            var earning = CreateGrowthAndSkillsEarningModel();

            // Act
            var message = _sut.MapToCalculateGrowthAndSkillsPayments(earning);

            // Assert
            message.EarningsId.Should().Be(earning.EarningsId);
            message.UKPRN.Should().Be(earning.UKPRN);
            message.EmployerContribution.Should().Be(earning.EmployerContribution);
            message.Learner.LearnerKey.Should().Be(earning.LearnerKey);
            message.Learner.ULN.Should().Be(earning.LearnerUln);
            message.Learner.Reference.Should().Be(earning.LearnerReference);
            message.Training.CourseCode.Should().Be(earning.CourseCode);
            message.Training.CourseReference.Should().Be(earning.CourseReference);
            message.Training.StartDate.Should().Be(earning.StartDate);
            message.Training.AgeAtStartOfTraining.Should().Be(earning.AgeAtStartOfTraining);
            message.Training.PlannedEndDate.Should().Be(earning.PlannedEndDate);
            message.Training.ActualEndDate.Should().Be(earning.ActualEndDate);
            message.Training.LearningKey.Should().Be(earning.LearningKey!.Value);
            ((int)message.Training.CourseType).Should().Be((int)earning.CourseType);
            ((int)message.Training.LearningType).Should().Be((int)earning.LearningType);
            ((int)message.Training.TrainingStatus).Should().Be((int)earning.TrainingStatus);
        }

        [Test]
        public void MapToCalculateGrowthAndSkillsPayments_WhenLearningKeyIsNull_MapsToEmptyGuid()
        {
            // Arrange
            var earning = CreateGrowthAndSkillsEarningModel();
            earning.LearningKey = null;

            // Act
            var message = _sut.MapToCalculateGrowthAndSkillsPayments(earning);

            // Assert
            message.Training.LearningKey.Should().Be(Guid.Empty);
        }

        [Test]
        public void MapToCalculateGrowthAndSkillsPayments_GroupsPricePeriodsByAcademicYear()
        {
            // Arrange
            var earning = CreateGrowthAndSkillsEarningModel();
            earning.PricePeriods.Add(new GrowthAndSkillsEarningPricePeriodModel
            {
                AcademicYear = 2627,
                Price = 6000m,
                StartDate = new DateTime(2027, 1, 1),
                EndDate = new DateTime(2027, 1, 31),
                DeliveryPeriod = 1,
                EarningType = Model.EarningType.Completion,
                Amount = 3000m,
                EmployerAccountId = 10000,
                EmployerType = Model.EmployerType.Levy,
                FundingAccountId = 10000,
                ApprenticeshipId = 123457
            });

            // Act
            var message = _sut.MapToCalculateGrowthAndSkillsPayments(earning);

            // Assert
            message.Earnings.Should().HaveCount(2);
            message.Earnings.Should().ContainSingle(x => x.AcademicYear == 2526 && x.PricePeriods.Count() == 1);
            message.Earnings.Should().ContainSingle(x => x.AcademicYear == 2627 && x.PricePeriods.Count() == 1);
        }

        [Test]
        public void MapToCalculateGrowthAndSkillsPayments_MapsPricePeriodAndEarningPeriodFields()
        {
            // Arrange
            var earning = CreateGrowthAndSkillsEarningModel();
            var pricePeriod = earning.PricePeriods.Single();

            // Act
            var message = _sut.MapToCalculateGrowthAndSkillsPayments(earning);

            // Assert
            var mappedEarnings = message.Earnings.Single();
            mappedEarnings.AcademicYear.Should().Be(pricePeriod.AcademicYear);

            var mappedPricePeriod = mappedEarnings.PricePeriods.Single();
            mappedPricePeriod.Price.Should().Be(pricePeriod.Price);
            mappedPricePeriod.StartDate.Should().Be(pricePeriod.StartDate);
            mappedPricePeriod.EndDate.Should().Be(pricePeriod.EndDate);

            // NumberOfInstalments/InstalmentAmount/CompletionAmount aren't persisted on the cache table, so they can't be reconstructed.
            mappedPricePeriod.NumberOfInstalments.Should().Be(0);
            mappedPricePeriod.InstalmentAmount.Should().Be(0);
            mappedPricePeriod.CompletionAmount.Should().Be(0);

            var mappedEarningPeriod = mappedPricePeriod.Periods.Single();
            mappedEarningPeriod.DeliveryPeriod.Should().Be(pricePeriod.DeliveryPeriod);
            mappedEarningPeriod.Amount.Should().Be(pricePeriod.Amount);
            mappedEarningPeriod.LearningId.Should().Be(pricePeriod.ApprenticeshipId!.Value);
            ((int)mappedEarningPeriod.EarningType).Should().Be((int)pricePeriod.EarningType);
            mappedEarningPeriod.Employer.AccountId.Should().Be(pricePeriod.EmployerAccountId);
            mappedEarningPeriod.Employer.FundingAccountId.Should().Be(pricePeriod.FundingAccountId);
            ((int)mappedEarningPeriod.Employer.EmployerType).Should().Be((int)pricePeriod.EmployerType);
        }

        private GrowthAndSkillsEarningModel CreateGrowthAndSkillsEarningModel()
        {
            return new GrowthAndSkillsEarningModel
            {
                EarningsId = Guid.NewGuid(),
                UKPRN = 10002233,
                LearnerKey = Guid.NewGuid(),
                LearnerUln = 12345678,
                LearnerReference = "LEARNREF001",
                LearningType = Model.LearningType.ApprenticeshipUnit,
                LearningKey = Guid.NewGuid(),
                CourseCode = "123456",
                CourseReference = "ZSC00123",
                StartDate = new DateTime(2026, 1, 1),
                AgeAtStartOfTraining = 25,
                PlannedEndDate = new DateTime(2026, 1, 15),
                ActualEndDate = new DateTime(2026, 1, 31),
                TrainingStatus = Model.TrainingStatus.Continuing,
                EmployerContribution = 1000m,
                CourseType = Model.CourseType.ShortCourse,
                PricePeriods = new List<GrowthAndSkillsEarningPricePeriodModel>
                {
                    new GrowthAndSkillsEarningPricePeriodModel
                    {
                        AcademicYear = 2526,
                        Price = 5000m,
                        StartDate = new DateTime(2026, 1, 1),
                        EndDate = new DateTime(2026, 1, 31),
                        DeliveryPeriod = 1,
                        EarningType = Model.EarningType.Milestone1,
                        Amount = 2000m,
                        EmployerAccountId = 10000,
                        EmployerType = Model.EmployerType.Levy,
                        FundingAccountId = 10000,
                        ApprenticeshipId = 123456
                    }
                }
            };
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

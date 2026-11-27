using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.Moq;
using Castle.Components.DictionaryAdapter;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.Core.Validation;
using SFA.DAS.Payments.EarningEvents.Domain.Mapping;
using SFA.DAS.Payments.EarningEvents.Domain.Validation.Learner;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.Internal.Commands;
using SFA.DAS.Payments.Model.Core.Incentives;
using FluentAssertions;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.OnProgramme;

namespace SFA.DAS.Payments.EarningEvents.Domain.UnitTests
{
    [TestFixture]
    public class LearnerEarningsTests
    {
        private AutoMock mocker;
        private FM36Learner learner;
        private Mock<ILearnerValidator> validatorMock;
        private Mock<IApprenticeshipContractTypeEarningsEventBuilder> actBuilder;
        private Mock<IFunctionalSkillEarningsEventBuilder> functionalSkillBuilder;
        private Mock<IConfigurationHelper> configurationHelper;
        [SetUp]
        public void SetUp()
        {
            mocker = AutoMock.GetStrict();
            validatorMock = mocker.Mock<ILearnerValidator>();
            actBuilder = mocker.Mock<IApprenticeshipContractTypeEarningsEventBuilder>();
            functionalSkillBuilder = mocker.Mock<IFunctionalSkillEarningsEventBuilder>();
            validatorMock = mocker.Mock<ILearnerValidator>();
            configurationHelper = mocker.Mock<IConfigurationHelper>();

            configurationHelper
                .Setup(x => x.HasSetting("Settings", "DoNotGenerateACT1TransactionType4To16Payments"))
                .Returns(true);

            configurationHelper
                .Setup(x => x.GetSetting("Settings", "DoNotGenerateACT1TransactionType4To16Payments"))
                .Returns("false");

            configurationHelper
                .Setup(x => x.HasSetting("Settings", "DoNotGenerateACT2Payments"))
                .Returns(true);

            configurationHelper
                .Setup(x => x.GetSetting("Settings", "DoNotGenerateACT2Payments"))
                .Returns("false");

            SetupHiringIncentives();

            validatorMock.Setup(x => x.Validate(It.IsAny<FM36Learner>())).Returns(new ValidationResult(new List<ValidationRuleResult>()));
            learner = new FM36Learner();
        }

        [Test]
        public void Fails_If_Validation_Fails()
        {
            mocker.Mock<ILearnerValidator>()
                .Setup(x => x.Validate(It.IsAny<FM36Learner>()))
                .Returns(new ValidationResult(new List<ValidationRuleResult> { ValidationRuleResult.Failure("some failure") }))
                .Verifiable();

            var learnerSubmission =new ProcessLearnerCommand
            {
                Learner = learner,
                CollectionYear = 1819,
                CollectionPeriod = 1,
                Ukprn = 12345,
                JobId = 1
            };
            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);
            Assert.IsTrue(result.Validation.Failed);

            Mock.Verify(validatorMock, actBuilder, functionalSkillBuilder);
        }

        [Test]
        public void Generates_Apprenticeship_Contract_Earnings()
        {
            // arrange
            var learnerSubmission = new ProcessLearnerCommand
            {
                Learner = learner,
                CollectionYear = 1819,
                CollectionPeriod = 1,
                Ukprn = 12345,
                JobId = 1
            };

            var actEarnings = new List<ApprenticeshipContractTypeEarningsEvent>
            {
                new ApprenticeshipContractType2EarningEvent(),
                new ApprenticeshipContractType2EarningEvent()
            };

            var functionalSkillEarnings = new List<FunctionalSkillEarningsEvent>
            {
                new Act2FunctionalSkillEarningsEvent(),
                new Act1FunctionalSkillEarningsEvent()
            };

            actBuilder.Setup(b => b.Build(learnerSubmission)).Returns(actEarnings).Verifiable();
            functionalSkillBuilder.Setup(b => b.Build(learnerSubmission)).Returns(functionalSkillEarnings).Verifiable();

            // act
            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);


            // assert
            Assert.IsFalse(result.Validation.Failed);
            Assert.AreEqual(4, result.EarningEvents.Count);

            Mock.Verify(validatorMock, actBuilder, functionalSkillBuilder);
        }

        [Test]
        public void When_DoNotGenerateACT1TransactionType4To16Payments_Is_True_Payments_Are_Generated_Correctly()
        {
            // arrange
            var learnerSubmission = new ProcessLearnerCommand
            {
                Learner = learner,
                CollectionYear = 1819,
                CollectionPeriod = 1,
                Ukprn = 12345,
                JobId = 1
            };

            var actEarnings = new List<ApprenticeshipContractTypeEarningsEvent>
            {
                new ApprenticeshipContractType1EarningEvent
                {
                    OnProgrammeEarnings = new List<OnProgrammeEarning>
                    {
                        new OnProgrammeEarning()
                    },
                    IncentiveEarnings = new List<IncentiveEarning>
                    {
                        new IncentiveEarning(),
                        new IncentiveEarning()
                    }
                },
                new ApprenticeshipContractType2EarningEvent(),
            };

            var functionalSkillEarnings = new List<FunctionalSkillEarningsEvent>
            {
                new Act2FunctionalSkillEarningsEvent(),
                new Act1FunctionalSkillEarningsEvent()
            };

            configurationHelper
                .Setup(x => x.GetSetting("Settings", "DoNotGenerateACT1TransactionType4To16Payments"))
                .Returns("true");

            actBuilder.Setup(b => b.Build(learnerSubmission)).Returns(actEarnings).Verifiable();
            functionalSkillBuilder.Setup(b => b.Build(learnerSubmission)).Returns(functionalSkillEarnings).Verifiable();

            // act
            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            // assert
            result.EarningEvents.Should().HaveCount(3);


            var act1Payments = result.EarningEvents.OfType<ApprenticeshipContractType1EarningEvent>().ToList();
            act1Payments.Should().HaveCount(1);
            act1Payments.First().IncentiveEarnings.Should().BeEmpty();

            result.EarningEvents.OfType<Act1FunctionalSkillEarningsEvent>().Should().BeEmpty();

        }

        [Test]
        public void When_DoNotGenerateACT2Payments_Is_True_Payments_Are_Generated_Correctly()
        {
            // arrange
            var learnerSubmission = new ProcessLearnerCommand
            {
                Learner = learner,
                CollectionYear = 1819,
                CollectionPeriod = 1,
                Ukprn = 12345,
                JobId = 1
            };

            var actEarnings = new List<ApprenticeshipContractTypeEarningsEvent>
            {
                new ApprenticeshipContractType1EarningEvent
                {
                    OnProgrammeEarnings = new List<OnProgrammeEarning>
                    {
                        new OnProgrammeEarning()
                    },
                    IncentiveEarnings = new List<IncentiveEarning>
                    {
                        new IncentiveEarning(),
                        new IncentiveEarning()
                    }
                },
                new ApprenticeshipContractType2EarningEvent(),
            };

            var functionalSkillEarnings = new List<FunctionalSkillEarningsEvent>
            {
                new Act2FunctionalSkillEarningsEvent(),
                new Act1FunctionalSkillEarningsEvent()
            };

            configurationHelper
                .Setup(x => x.GetSetting("Settings", "DoNotGenerateACT2Payments"))
                .Returns("true");

            actBuilder.Setup(b => b.Build(learnerSubmission)).Returns(actEarnings).Verifiable();
            functionalSkillBuilder.Setup(b => b.Build(learnerSubmission)).Returns(functionalSkillEarnings).Verifiable();

            // act
            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            // assert
            result.EarningEvents.Should().HaveCount(2);


            var act1Payments = result.EarningEvents.OfType<ApprenticeshipContractType1EarningEvent>().ToList();
            act1Payments.Should().HaveCount(1);
            act1Payments.First().IncentiveEarnings.Should().HaveCount(2);

            var functionalSkillAct1Payments = result.EarningEvents.OfType<Act1FunctionalSkillEarningsEvent>().ToList();
            functionalSkillAct1Payments.Should().HaveCount(1);

            result.EarningEvents.OfType<ApprenticeshipContractType2EarningEvent>().Should().BeEmpty();
            result.EarningEvents.OfType<Act2FunctionalSkillEarningsEvent>().Should().BeEmpty();

        }

        [Test]
        public void RemovesFuturePeriodsForAct2()
        {
            // arrange
            var learnerSubmission = new ProcessLearnerCommand
            {
                Learner = learner,
                CollectionYear = 1819,
                CollectionPeriod = 1,
                Ukprn = 12345,
                JobId = 1
            };

            var actEarnings = new List<ApprenticeshipContractTypeEarningsEvent>
            {
                new ApprenticeshipContractType2EarningEvent
                {
                    CollectionPeriod = new CollectionPeriod {AcademicYear = 1, Period = 1},
                    IncentiveEarnings = new List<IncentiveEarning>
                    {
                        new IncentiveEarning
                        {
                            Type = IncentiveEarningType.Balancing16To18FrameworkUplift, Periods = new List<EarningPeriod>
                            {
                                new EarningPeriod {Period = 1},
                                new EarningPeriod {Period = 2}
                            }.AsReadOnly()
                        },
                        new IncentiveEarning
                        {
                            Type = IncentiveEarningType.Balancing16To18FrameworkUplift, Periods = new List<EarningPeriod>
                            {
                                new EarningPeriod {Period = 3},
                                new EarningPeriod {Period = 4}
                            }.AsReadOnly()
                        }
                    },
                    OnProgrammeEarnings = new EditableList<OnProgrammeEarning>
                    {
                        new OnProgrammeEarning
                        {
                            Periods = new List<EarningPeriod>()
                            {
                                new EarningPeriod {Period = 1},
                                new EarningPeriod {Period = 2}
                            }.AsReadOnly()
                        },
                        new OnProgrammeEarning
                        {
                            Periods = new List<EarningPeriod>()
                            {
                                new EarningPeriod {Period = 3},
                                new EarningPeriod {Period = 4}
                            }.AsReadOnly()
                        }
                    }
                }
            };

            var functionalSkillEarnings = new List<FunctionalSkillEarningsEvent>
            {
                new Act2FunctionalSkillEarningsEvent
                {
                    CollectionPeriod = new CollectionPeriod {AcademicYear = 1, Period = 1},
                    Earnings = new List<FunctionalSkillEarning>
                    {
                        new FunctionalSkillEarning
                        {
                            Periods = new List<EarningPeriod>
                            {
                                new EarningPeriod {Period = 1},
                                new EarningPeriod {Period = 2}
                            }.AsReadOnly()
                        }
                    }.AsReadOnly()
                }
            };

            actBuilder.Setup(b => b.Build(learnerSubmission)).Returns(actEarnings).Verifiable();
            functionalSkillBuilder.Setup(b => b.Build(learnerSubmission)).Returns(functionalSkillEarnings).Verifiable();

            // act
            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            // assert
            Assert.AreEqual(2, result.EarningEvents.Count);
            Assert.IsInstanceOf<ApprenticeshipContractTypeEarningsEvent>(result.EarningEvents[0]);
            var act2Earning = (ApprenticeshipContractTypeEarningsEvent)result.EarningEvents[0];

            Assert.AreEqual(1, act2Earning.IncentiveEarnings.Count);
            Assert.AreEqual(1, act2Earning.IncentiveEarnings[0].Periods.Count);
            Assert.AreEqual(1, act2Earning.IncentiveEarnings[0].Periods[0].Period);

            Assert.AreEqual(1, act2Earning.OnProgrammeEarnings.Count);
            Assert.AreEqual(1, act2Earning.OnProgrammeEarnings[0].Periods.Count);
            Assert.AreEqual(1, act2Earning.OnProgrammeEarnings[0].Periods[0].Period);

            Assert.IsInstanceOf<Act2FunctionalSkillEarningsEvent>(result.EarningEvents[1]);
            var mathsEarning = (Act2FunctionalSkillEarningsEvent)result.EarningEvents[1];
            Assert.AreEqual(1, mathsEarning.Earnings.Count);
            Assert.AreEqual(1, mathsEarning.Earnings[0].Periods.Count);
            Assert.AreEqual(1, mathsEarning.Earnings[0].Periods[0].Period);

            Mock.Verify(validatorMock, actBuilder, functionalSkillBuilder);

        }

        [Test]
        public void RemovesFuturePeriodsForAct1()
        {
            // arrange
            var learnerSubmission = new ProcessLearnerCommand
            {
                Learner = learner,
                CollectionYear = 1819,
                CollectionPeriod = 1,
                Ukprn = 12345,
                JobId = 1
            };

            var actEarnings = new List<ApprenticeshipContractTypeEarningsEvent>
            {
                new ApprenticeshipContractType1EarningEvent
                {
                    CollectionPeriod = new CollectionPeriod {AcademicYear = 1, Period = 1},
                    IncentiveEarnings = new List<IncentiveEarning>
                    {
                        new IncentiveEarning
                        {
                            Type = IncentiveEarningType.Balancing16To18FrameworkUplift, Periods = new List<EarningPeriod>
                            {
                                new EarningPeriod {Period = 1},
                                new EarningPeriod {Period = 2}
                            }.AsReadOnly()
                        },
                        new IncentiveEarning
                        {
                            Type = IncentiveEarningType.Balancing16To18FrameworkUplift, Periods = new List<EarningPeriod>
                            {
                                new EarningPeriod {Period = 3},
                                new EarningPeriod {Period = 4}
                            }.AsReadOnly()
                        }
                    },
                    OnProgrammeEarnings = new EditableList<OnProgrammeEarning>
                    {
                        new OnProgrammeEarning
                        {
                            Periods = new List<EarningPeriod>
                            {
                                new EarningPeriod {Period = 1},
                                new EarningPeriod {Period = 2}
                            }.AsReadOnly()
                        },
                        new OnProgrammeEarning
                        {
                            Periods = new List<EarningPeriod>
                            {
                                new EarningPeriod {Period = 3},
                                new EarningPeriod {Period = 4}
                            }.AsReadOnly()
                        }
                    }
                }
            };

            var functionalSkillEarnings = new List<FunctionalSkillEarningsEvent>
            {
                new Act1FunctionalSkillEarningsEvent
                {
                    CollectionPeriod = new CollectionPeriod {AcademicYear = 1, Period = 1},
                    Earnings = new List<FunctionalSkillEarning>
                    {
                        new FunctionalSkillEarning
                        {
                            Periods = new List<EarningPeriod>
                            {
                                new EarningPeriod {Period = 1},
                                new EarningPeriod {Period = 2}
                            }.AsReadOnly()
                        }
                    }.AsReadOnly()
                }
            };

            actBuilder.Setup(b => b.Build(learnerSubmission)).Returns(actEarnings).Verifiable();
            functionalSkillBuilder.Setup(b => b.Build(learnerSubmission)).Returns(functionalSkillEarnings).Verifiable();

            // act
            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            // assert
            Assert.AreEqual(2, result.EarningEvents.Count);
            Assert.IsInstanceOf<ApprenticeshipContractTypeEarningsEvent>(result.EarningEvents[0]);
            var act2Earning = (ApprenticeshipContractTypeEarningsEvent)result.EarningEvents[0];

            Assert.AreEqual(1, act2Earning.IncentiveEarnings.Count);
            Assert.AreEqual(1, act2Earning.IncentiveEarnings[0].Periods.Count);
            Assert.AreEqual(1, act2Earning.IncentiveEarnings[0].Periods[0].Period);

            Assert.AreEqual(1, act2Earning.OnProgrammeEarnings.Count);
            Assert.AreEqual(1, act2Earning.OnProgrammeEarnings[0].Periods.Count);
            Assert.AreEqual(1, act2Earning.OnProgrammeEarnings[0].Periods[0].Period);

            Assert.IsInstanceOf<FunctionalSkillEarningsEvent>(result.EarningEvents[1]);
            var mathsEarning = (FunctionalSkillEarningsEvent)result.EarningEvents[1];
            Assert.AreEqual(1, mathsEarning.Earnings.Count);
            Assert.AreEqual(1, mathsEarning.Earnings[0].Periods.Count);
            Assert.AreEqual(1, mathsEarning.Earnings[0].Periods[0].Period);

            Mock.Verify(validatorMock, actBuilder, functionalSkillBuilder);
        }

        [Test]
        public void Hiring_Payments_In_Future_Delivery_Periods_Are_Removed_From_The_Act1_Event()
        {
            var learnerSubmission = new ProcessLearnerCommand
            {
                Learner = learner,
                CollectionYear = 2627,
                CollectionPeriod = 3,
                Ukprn = 12345,
                JobId = 1
            };

            var act1Event = new ApprenticeshipContractType1EarningEvent
            {
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2627, Period = 3 },
                IncentiveEarnings = new List<IncentiveEarning>
                {
                    new IncentiveEarning
                    {
                        Type = IncentiveEarningType.FirstEmployerHiringPayment,
                        Periods = new List<EarningPeriod>
                        {
                            new EarningPeriod { Period = 3, Amount = 500m },
                            new EarningPeriod { Period = 5, Amount = 500m }
                        }.AsReadOnly()
                    },
                    new IncentiveEarning
                    {
                        Type = IncentiveEarningType.SecondEmployerHiringPayment,
                        Periods = new List<EarningPeriod>
                        {
                            new EarningPeriod { Period = 7, Amount = 250m }
                        }.AsReadOnly()
                    }
                }
            };

            actBuilder.Setup(b => b.Build(learnerSubmission)).Returns(new List<ApprenticeshipContractTypeEarningsEvent> { act1Event });
            functionalSkillBuilder.Setup(b => b.Build(learnerSubmission)).Returns(new List<FunctionalSkillEarningsEvent>());

            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            var published = result.EarningEvents.OfType<ApprenticeshipContractType1EarningEvent>().Single();
            published.IncentiveEarnings.Should().HaveCount(1);
            published.IncentiveEarnings[0].Type.Should().Be(IncentiveEarningType.FirstEmployerHiringPayment);
            published.IncentiveEarnings[0].Periods.Select(p => p.Period).Should().Equal(3);
        }

        [Test]
        public void Hiring_Incentives_Are_Not_Requested_For_An_Act1_Event_Starting_Before_The_Eligibility_Date()
        {
            AssertProviderNotCalledFor(new DateTime(2026, 9, 30), 18);
        }

        [Test]
        public void Hiring_Incentives_Are_Not_Requested_For_An_Act1_Event_For_A_Learner_Aged_25_Or_Over()
        {
            AssertProviderNotCalledFor(new DateTime(2026, 11, 1), 25);
        }

        [Test]
        public void Hiring_Incentives_Are_Not_Requested_For_An_Act1_Event_Without_An_Age()
        {
            AssertProviderNotCalledFor(new DateTime(2026, 11, 1), null);
        }

        [Test]
        public void Hiring_Incentives_Are_Not_Requested_For_Events_That_Are_Not_Act1()
        {
            var learnerSubmission = CreateCommand();
            var act2Event = new ApprenticeshipContractType2EarningEvent
            {
                StartDate = new DateTime(2026, 11, 1),
                AgeAtStartOfLearning = 18
            };
            actBuilder.Setup(b => b.Build(learnerSubmission)).Returns(new List<ApprenticeshipContractTypeEarningsEvent> { act2Event });
            functionalSkillBuilder.Setup(b => b.Build(learnerSubmission)).Returns(new List<FunctionalSkillEarningsEvent>());

            mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            VerifyProviderNeverCalled();
        }

        [Test]
        public void Only_The_Act1_Events_Meeting_The_Start_Date_And_Age_Criteria_Are_Sent_To_The_Provider()
        {
            var learnerSubmission = CreateCommand();
            var eligible = CreateEligibleAct1Event();
            eligible.StartDate = new DateTime(2026, 10, 1);
            eligible.AgeAtStartOfLearning = 24;
            var tooOld = CreateEligibleAct1Event();
            tooOld.AgeAtStartOfLearning = 25;
            SetupBuilders(learnerSubmission, eligible, tooOld);

            mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            mocker.Mock<IHiringIncentiveEarningsProvider>().Verify(
                x => x.GetHiringIncentiveEarnings(It.Is<List<ApprenticeshipContractType1EarningEvent>>(l => l.Count == 1 && ReferenceEquals(l[0], eligible))),
                Times.Once);
        }

        [Test]
        public void Hiring_Incentives_Returned_By_The_Provider_Are_Merged_Into_The_Act1_Event_Keeping_Its_Existing_Incentives()
        {
            var learnerSubmission = CreateCommand();
            var act1Event = CreateEligibleAct1Event();
            SetupBuilders(learnerSubmission, act1Event);
            SetupHiringIncentives((act1Event, new List<IncentiveEarning>
            {
                CreateIncentive((IncentiveEarningType)18, 3),
                CreateIncentive((IncentiveEarningType)19, 3)
            }));

            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            result.EarningEvents.Should().ContainSingle().Which.Should().BeSameAs(act1Event);
            act1Event.IncentiveEarnings.Select(x => x.Type).Should()
                .BeEquivalentTo(new[] { IncentiveEarningType.LearningSupport, (IncentiveEarningType)18, (IncentiveEarningType)19 });
        }

        [Test]
        public void Hiring_Incentives_Of_A_Type_Already_On_The_Event_Are_Combined_By_Type_In_Period_Order()
        {
            var learnerSubmission = CreateCommand();
            var act1Event = CreateEligibleAct1Event();
            act1Event.IncentiveEarnings.Add(CreateIncentive((IncentiveEarningType)18, 2));
            SetupBuilders(learnerSubmission, act1Event);
            SetupHiringIncentives((act1Event, new List<IncentiveEarning> { CreateIncentive((IncentiveEarningType)18, 3) }));

            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            var published = result.EarningEvents.OfType<ApprenticeshipContractType1EarningEvent>().Single();
            published.IncentiveEarnings.Where(x => x.Type == (IncentiveEarningType)18).Should().ContainSingle()
                .Which.Periods.Select(p => p.Period).Should().Equal(2, 3);
            published.IncentiveEarnings.Should().Contain(x => x.Type == IncentiveEarningType.LearningSupport);
        }

        [Test]
        public void Hiring_Incentives_Returned_By_The_Provider_Are_Trimmed_To_The_Current_Collection_Period()
        {
            var learnerSubmission = CreateCommand();
            var act1Event = CreateEligibleAct1Event();
            SetupBuilders(learnerSubmission, act1Event);
            SetupHiringIncentives((act1Event, new List<IncentiveEarning> { CreateIncentive((IncentiveEarningType)18, 3, 5, 7) }));

            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            var published = result.EarningEvents.OfType<ApprenticeshipContractType1EarningEvent>().Single();
            published.IncentiveEarnings.Single(x => x.Type == (IncentiveEarningType)18)
                .Periods.Select(p => p.Period).Should().Equal(3);
        }

        [Test]
        public void Hiring_Incentives_Are_Added_After_The_Config_Setting_That_Clears_Act1_Incentives()
        {
            configurationHelper
                .Setup(x => x.GetSetting("Settings", "DoNotGenerateACT1TransactionType4To16Payments"))
                .Returns("true");
            var learnerSubmission = CreateCommand();
            var act1Event = CreateEligibleAct1Event();
            SetupBuilders(learnerSubmission, act1Event);
            SetupHiringIncentives((act1Event, new List<IncentiveEarning> { CreateIncentive((IncentiveEarningType)18, 3) }));

            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            var published = result.EarningEvents.OfType<ApprenticeshipContractType1EarningEvent>().Single();
            published.IncentiveEarnings.Select(x => x.Type).Should().Equal((IncentiveEarningType)18);
        }

        private void SetupHiringIncentives(params (ApprenticeshipContractType1EarningEvent Event, List<IncentiveEarning> IncentiveEarnings)[] results)
        {
            mocker.Mock<IHiringIncentiveEarningsProvider>()
                .Setup(x => x.GetHiringIncentiveEarnings(It.IsAny<List<ApprenticeshipContractType1EarningEvent>>()))
                .Returns(results.ToList());
        }

        private void AssertProviderNotCalledFor(DateTime startDate, int? age)
        {
            var learnerSubmission = CreateCommand();
            var act1Event = CreateEligibleAct1Event();
            act1Event.StartDate = startDate;
            act1Event.AgeAtStartOfLearning = age;
            SetupBuilders(learnerSubmission, act1Event);

            var result = mocker.Create<LearnerSubmissionProcessor>().GenerateEarnings(learnerSubmission);

            result.EarningEvents.Should().ContainSingle().Which.Should().BeSameAs(act1Event);
            VerifyProviderNeverCalled();
        }

        private void VerifyProviderNeverCalled()
        {
            mocker.Mock<IHiringIncentiveEarningsProvider>()
                .Verify(x => x.GetHiringIncentiveEarnings(It.IsAny<List<ApprenticeshipContractType1EarningEvent>>()), Times.Never);
        }

        private void SetupBuilders(ProcessLearnerCommand learnerSubmission, params ApprenticeshipContractType1EarningEvent[] act1Events)
        {
            actBuilder.Setup(b => b.Build(learnerSubmission)).Returns(act1Events.Cast<ApprenticeshipContractTypeEarningsEvent>().ToList());
            functionalSkillBuilder.Setup(b => b.Build(learnerSubmission)).Returns(new List<FunctionalSkillEarningsEvent>());
        }

        private ProcessLearnerCommand CreateCommand()
        {
            return new ProcessLearnerCommand
            {
                Learner = learner,
                CollectionYear = 2627,
                CollectionPeriod = 3,
                Ukprn = 12345,
                JobId = 1
            };
        }

        private static ApprenticeshipContractType1EarningEvent CreateEligibleAct1Event()
        {
            return new ApprenticeshipContractType1EarningEvent
            {
                StartDate = new DateTime(2026, 11, 1),
                AgeAtStartOfLearning = 18,
                CollectionPeriod = new CollectionPeriod { AcademicYear = 2627, Period = 3 },
                IncentiveEarnings = new List<IncentiveEarning> { CreateIncentive(IncentiveEarningType.LearningSupport, 1) }
            };
        }

        private static IncentiveEarning CreateIncentive(IncentiveEarningType type, params byte[] periods)
        {
            return new IncentiveEarning
            {
                Type = type,
                Periods = periods.Select(period => new EarningPeriod { Period = period, Amount = 100m }).ToList().AsReadOnly()
            };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.Model.Core.Entities;
using Common = SFA.DAS.Payments.Model.Core;
using EarningType = SFA.DAS.Payments.EarningEvents.Messages.External.EarningType;
using EmployerType = SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType;
using LearningType = SFA.DAS.Payments.EarningEvents.Messages.External.LearningType;
using TrainingStatus = SFA.DAS.Payments.EarningEvents.Messages.External.TrainingStatus;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{

    [TestFixture]
    public class GSLShortCoursePaymentsProcessorTests
    {
        private CalculateGrowthAndSkillsPayments _message;
        private List<CollectionPeriodModel> _openCollectionPeriods;
        private Mock<IGSLShortCoursesMapper> _mapper;
        private Mock<IPaymentsServiceBusPublisher> _publisher;
        private GSLShortCoursePaymentsProcessor _sut;

        [SetUp]
        public void SetUp()
        {
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

            _openCollectionPeriods = new List<CollectionPeriodModel>
            {
                new CollectionPeriodModel
                {
                    AcademicYear = 2526,
                    Period = 2,
                    Status = CollectionPeriodStatus.Open
                }
            };

            _mapper = new Mock<IGSLShortCoursesMapper>();
            _publisher = new Mock<IPaymentsServiceBusPublisher>();
            _sut = new GSLShortCoursePaymentsProcessor(_mapper.Object, _publisher.Object);
        }

        [Test]
        public async Task Process_Publishes_short_course_and_DAS_events_for_open_collection_periods()
        {
            _mapper.Setup(x => x.MapToShortCourseEarningEvents(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()))
                .Returns(new[]
                {
                    new GSLShortCourseEarningsEvent
                    {
                        ExternalEarningsId = _message.EarningsId,
                        CollectionPeriod = new Common.CollectionPeriod
                        {
                            AcademicYear = 2526,
                            Period = 2
                        }
                    }
                });

            _mapper.Setup(x => x.MapToDasEarningsReceivedEvents(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()))
                .Returns(new[]
                {
                    new DasEarningsReceivedEvent
                    {
                        EarningsId = _message.EarningsId,
                        CollectionPeriod = new Common.CollectionPeriod
                        {
                            AcademicYear = 2526,
                            Period = 2
                        }
                    }
                });

            var shortCourseEvents = new List<GSLShortCourseEarningsEvent>();
            var dasEarningsReceivedEvents = new List<DasEarningsReceivedEvent>();

            _publisher.Setup(x => x.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()))
                .Callback<GSLShortCourseEarningsEvent>(shortCourseEvents.Add)
                .Returns(Task.CompletedTask);

            _publisher.Setup(x => x.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()))
                .Callback<DasEarningsReceivedEvent>(dasEarningsReceivedEvents.Add)
                .Returns(Task.CompletedTask);

            await _sut.Process(_message, _openCollectionPeriods);

            shortCourseEvents.Should().ContainSingle();
            dasEarningsReceivedEvents.Should().ContainSingle();

            shortCourseEvents[0].ExternalEarningsId.Should().Be(_message.EarningsId);
            shortCourseEvents[0].CollectionPeriod.AcademicYear.Should().Be(2526);
            shortCourseEvents[0].CollectionPeriod.Period.Should().Be(2);

            dasEarningsReceivedEvents[0].EarningsId.Should().Be(_message.EarningsId);
            dasEarningsReceivedEvents[0].CollectionPeriod.AcademicYear.Should().Be(2526);
            dasEarningsReceivedEvents[0].CollectionPeriod.Period.Should().Be(2);

            _publisher.Verify(x => x.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()), Times.Once);
            _publisher.Verify(x => x.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()), Times.Once);
        }

        [Test]
        public async Task Process_Does_not_publish_events_when_short_course_events_are_null()
        {
            _mapper.Setup(x => x.MapToShortCourseEarningEvents(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()))
                .Returns((IEnumerable<GSLShortCourseEarningsEvent>)null);

            await _sut.Process(_message, _openCollectionPeriods);

            _publisher.Verify(x => x.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()), Times.Never);
            _publisher.Verify(x => x.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()), Times.Never);
        }

        [Test]
        public async Task Process_Does_not_publish_events_when_short_course_events_are_empty()
        {
            _mapper.Setup(x => x.MapToShortCourseEarningEvents(It.IsAny<CalculateGrowthAndSkillsPayments>(), It.IsAny<IEnumerable<CollectionPeriodModel>>()))
                .Returns(Array.Empty<GSLShortCourseEarningsEvent>());

            await _sut.Process(_message, _openCollectionPeriods);

            _publisher.Verify(x => x.Publish<GSLShortCourseEarningsEvent>(It.IsAny<GSLShortCourseEarningsEvent>()), Times.Never);
            _publisher.Verify(x => x.Publish<DasEarningsReceivedEvent>(It.IsAny<DasEarningsReceivedEvent>()), Times.Never);
        }
    }
}
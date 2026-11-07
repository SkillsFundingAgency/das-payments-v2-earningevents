using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using System.Security.Cryptography;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests.Mapping
{
    [TestFixture]
    public class ReceivedEarningsMapperTests
    {
        [Test]
        public void Maps_EarningsId()
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = Guid.NewGuid()
            };
            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.EarningsId.Should().Be(source.EarningsId);
        }

        [Test]
        public void Maps_UKPRN()
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                UKPRN = 12345678
            };
            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.UKPRN.Should().Be(source.UKPRN);
        }

        [Test]
        public void Maps_LearnerUln()
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                Learner = new Learner
                {
                    ULN = 1234567890
                }
            };
            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.LearnerUln.Should().Be(source.Learner.ULN);
        }

        [Test]
        public void Maps_LearnerReference()
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                Learner = new Learner
                {
                    Reference = "LearnerRef"
                }
            };
            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.LearnerReference.Should().Be(source.Learner.Reference);
        }

        [Test]
        public void Maps_LearnerKey()
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                Learner = new Learner
                {
                    LearnerKey = Guid.NewGuid()
                }
            };
            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.LearnerKey.Should().Be(source.Learner.LearnerKey);
        }

        [TestCase(Messages.External.CourseType.Apprenticeship)]
        [TestCase(Messages.External.CourseType.FunctionalSkill)]
        [TestCase(Messages.External.CourseType.ShortCourse)]
        public void Maps_CourseType(CourseType courseType)
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                Training = new Training
                {
                    CourseType = courseType
                }
            };
            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.CourseType.Should().Be(source.Training.CourseType.ToModelCourseType());
        }

        [Test]
        public void Maps_LearningKey()
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                Training = new Training
                {
                    LearningKey = Guid.NewGuid()
                }
            };
            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.LearningKey.Should().Be(source.Training.LearningKey);
        }

        [TestCase(Messages.External.LearningType.Apprenticeship)]
        [TestCase(Messages.External.LearningType.FoundationApprenticeship)]
        [TestCase(Messages.External.LearningType.MathsAndEnglish)]
        [TestCase(Messages.External.LearningType.ApprenticeshipUnit)]
        public void maps_LearningType(LearningType learningType)
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                Training = new Training
                {
                    LearningType = learningType
                }
            };
            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.LearningType.Should().Be(source.Training.LearningType.ToModelLearningType());
        }

        [Test]
        public void Maps_CourseCode()
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                Training = new Training
                {
                    CourseCode = "CourseCode"
                }
            };
            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.CourseCode.Should().Be(source.Training.CourseCode);
        }

        [Test]
        public void Maps_CourseReference()
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                Training = new Training
                {
                    CourseReference = "CourseReference"
                }
            };
            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.CourseReference.Should().Be(source.Training.CourseReference);
        }

        [Test]
        public void Maps_StartDate()
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                Training = new Training
                {
                    StartDate = DateTime.UtcNow
                }
            };
            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.StartDate.Should().Be(source.Training.StartDate);
        }

        [Test]
        public void Map_ShouldMapAllProperties()
        {
            // Arrange
            var source = new CalculateGrowthAndSkillsPayments
            {
                EarningsId = Guid.NewGuid(),
                UKPRN = 12345678,
                Learner = new Learner
                {
                    ULN = 1234567890,
                    Reference = "LearnerRef",
                    LearnerKey = Guid.NewGuid()
                },
                Training = new Training
                {
                    CourseType = CourseType.Apprenticeship,
                    LearningKey = Guid.NewGuid(),
                    LearningType = LearningType.FoundationApprenticeship,
                    CourseCode = "123456",
                    CourseReference = "CourseReference",
                    StartDate = DateTime.UtcNow
                }
            };
            var json = System.Text.Json.JsonSerializer.Serialize(source);

            var mapper = new ReceivedEarningsMapper();
            // Act
            var result = mapper.Map(source);
            // Assert
            result.Payload.Should().Be(json);
        }
    }
}

using FluentAssertions;
using Moq;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.Model;
using SFA.DAS.Payments.Model.Core.Entities;
using System.Linq;

//one test checking standard process
//one test checking for duplicate open period but different academic years and making sure both get produced
namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    public class CollectionPeriodServiceTests
    {
        private Mock<ICollectionPeriodApiClient> _collectionPeriodApiClient;
        private GrowthAndSkillsMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _collectionPeriodApiClient = new Mock<ICollectionPeriodApiClient>();
            _mapper = new GrowthAndSkillsMapper();
        }


        [Test]
        public async Task check_that_accurate_data_comes_back_for_one_open_period()
        {
            //Arrange
            var collectionYears = new List<CollectionYear>
            {
                new()
                {
                    Year = 2526,
                    Status = CollectionPeriodStatus.Open,
                },
            };

            var collectionYearWithPeriods = new CollectionYear
            {
                Year = 2526,
                Status = CollectionPeriodStatus.Open,
                Periods = new List<CollectionPeriod>
                {
                    new CollectionPeriod
                    {
                        Id = 1234,
                        Period = 6,
                        CalendarMonth = 6,
                        CalendarYear = 2026,
                        Status = CollectionPeriodStatus.Open
                    }
                }
            };


            _collectionPeriodApiClient.Setup(x => x.GetOpenCollectionYears())
                .ReturnsAsync(collectionYears);

            _collectionPeriodApiClient.Setup(x => x.GetOpenCollectionPeriods("2526"))
                .ReturnsAsync(collectionYearWithPeriods);

            var collectionPeriodService = new CollectionPeriodService(_collectionPeriodApiClient.Object, _mapper);

            //Act
            var result = (await collectionPeriodService.GetOpenCollectionPeriods()).ToList();

            //Assert
            result.Should().HaveCount(1);
            var period = result[0];
            period.AcademicYear.Should().Be(2526);
            period.Period.Should().Be(6);
            period.Status.Should().Be(CollectionPeriodStatus.Open);
            period.Id.Should().Be(1234);

            _collectionPeriodApiClient.Verify(x => x.GetOpenCollectionYears(), Times.Once);
            _collectionPeriodApiClient.Verify(x => x.GetOpenCollectionPeriods("2526"), Times.Once);
        }

        [Test]
        public async Task check_that_duplicate_periods_different_academic_years_are_returned()
        {
            //Arrange
            var collectionYears = new List<CollectionYear>
            {
                new()
                {
                    Year = 2425,
                    Status = CollectionPeriodStatus.Open,
                },
                new()
                {
                    Year = 2526,
                    Status = CollectionPeriodStatus.Open,
                },
            };

            var collectionYear2425 = new CollectionYear
            {
                Year = 2425,
                Status = CollectionPeriodStatus.Open,
                Periods = new[]
                {
                    new CollectionPeriod
                    {
                        Id = 1234,
                        Period = 13,
                        CalendarMonth = 8,
                        CalendarYear = 2025,
                        Status = CollectionPeriodStatus.Open
                    },
                },
            };

            var collectionYear2526 = new CollectionYear
            {
                Year = 2526,
                Status = CollectionPeriodStatus.Open,
                Periods = new[]
                {
                    new CollectionPeriod
                    {
                        Id = 1235,
                        Period = 1,
                        CalendarMonth = 1,
                        CalendarYear = 2026,
                        Status = CollectionPeriodStatus.Open,
                    },
                },
            };

            _collectionPeriodApiClient.Setup(x => x.GetOpenCollectionYears())
                .ReturnsAsync(collectionYears);

            _collectionPeriodApiClient.Setup(x => x.GetOpenCollectionPeriods("2425"))
                .ReturnsAsync(collectionYear2425);

            _collectionPeriodApiClient.Setup(x => x.GetOpenCollectionPeriods("2526"))
                .ReturnsAsync(collectionYear2526);

            var collectionPeriodService = new CollectionPeriodService(_collectionPeriodApiClient.Object, _mapper);

            //Act
            var result = (await collectionPeriodService.GetOpenCollectionPeriods()).ToList();

            //Assert
            result.Should().HaveCount(2);

            var period2425 = result[0];
            period2425.Id.Should().Be(1234);
            period2425.AcademicYear.Should().Be(2425);
            period2425.Period.Should().Be(13);
            period2425.Status.Should().Be(CollectionPeriodStatus.Open);


            var period2526 = result[1];
            period2526.Id.Should().Be(1235);
            period2526.AcademicYear.Should().Be(2526);
            period2526.Period.Should().Be(1);
            period2526.Status.Should().Be(CollectionPeriodStatus.Open);


            _collectionPeriodApiClient.Verify(x => x.GetOpenCollectionYears(), Times.Once);
            _collectionPeriodApiClient.Verify(x => x.GetOpenCollectionPeriods("2425"), Times.Once);
            _collectionPeriodApiClient.Verify(x => x.GetOpenCollectionPeriods("2526"), Times.Once);
        }
    }
}



//var collectionYearWithPeriods = new CollectionYear
//{
//    Year = 2526,
//    Status = CollectionPeriodStatus.Open,
//    Periods = new List<CollectionPeriod>
//    {
//        new CollectionPeriod
//        {
//            CalendarMonth = 6,
//            CalendarYear = 2026,
//            Id = 1234,
//            Period = 6,
//            Status = CollectionPeriodStatus.Open
//        },
//        new CollectionPeriod
//        {
//            CalendarMonth = 7,
//            CalendarYear = 2026,
//            Id = 1235,
//            Period = 7,
//            Status = CollectionPeriodStatus.NotStarted
//        }
//    }
//};
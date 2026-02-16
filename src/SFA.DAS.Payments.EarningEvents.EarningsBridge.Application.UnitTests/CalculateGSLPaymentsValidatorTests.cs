using FluentAssertions;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class CalculateGSLPaymentsValidatorTests
    {
        private CalculateGSLPaymentsValidator _sut;
        private CalculateGrowthAndSkillsPayments _message;

        [SetUp]
        public void Setup()
        {
            _sut = new CalculateGSLPaymentsValidator();
            _message = new CalculateGrowthAndSkillsPayments
            {
                EmployerContribution = 0m,
                EarningsId = Guid.NewGuid(),
                UKPRN = 10001234,
                Learner = new Learner
                {
                    LearnerId = Guid.NewGuid(),
                    Reference = "ref",
                    ULN = 123456789
                },
                Training = new Training
                {
                    StartDate = new DateTime(2026, 1, 1),
                    AgeAtStartOfTraining = 25,
                    CourseCode = "ABC123",
                    LearningType = LearningType.ApprenticeshipUnit,
                    PlannedEndDate = new DateTime(2026, 03, 31),
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
                                Periods = new List<EarningPeriod>()
                                {
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
                    }
                }
            };
        }
        
        [Test]
        public void Validate_rejects_empty_earnings_id()
        {
            _message.EarningsId = Guid.Empty;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings Id is required");
        }

        [Test]
        public void Validate_rejects_empty_UKPRN()
        {
            _message.UKPRN = 0;
        
            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("UKPRN is required");
        }

        [Test]
        public void Validate_rejects_null_learner()
        {
            _message.Learner = null;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Learner is required");
        }

        [Test]
        public void Validate_rejects_null_training()
        {
            _message.Training = null;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Training is required");
        }

        [Test]
        public void Validate_rejects_null_earnings()
        {
            _message.Earnings = null;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings are required");
        }

        [Test]
        public void Validate_rejects_empty_earnings()
        {
            _message.Earnings = new List<Earnings>();

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings are required");
        }

        [Test]
        public void Validate_rejects_empty_learner_id()
        {
            _message.Learner.LearnerId = Guid.Empty;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Learner Id is required");
        }

        [Test]
        public void Validate_rejects_empty_ULN()
        {
            _message.Learner.ULN = 0;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("ULN is required");
        }

        [Test]
        public void Validate_rejects_empty_learner_reference()
        {
            _message.Learner.Reference = string.Empty;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Learner Reference is required");
        }

        [Test]
        public void Validate_rejects_null_learner_reference()
        {
            _message.Learner.Reference = null;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Learner Reference is required");
        }

        [Test]
        public void Validate_rejects_empty_course_code()
        {
            _message.Training.CourseCode = string.Empty;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Course Code is required");
        }

        [Test]
        public void Validate_rejects_empty_start_date()
        {
            _message.Training.StartDate = DateTime.MinValue;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Training Start Date is required");
        }

        [Test]
        public void Validate_rejects_empty_age_at_start_of_training()
        {
            _message.Training.AgeAtStartOfTraining = 0;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Age At Start Of Learning is required");
        }

        [Test]
        public void Validate_rejects_empty_planned_end_date()
        {
            _message.Training.PlannedEndDate = DateTime.MinValue;

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Training Planned End Date is required");
        }

        [Test]
        public void Validate_rejects_empty_earnings_academic_year()
        {
            _message.Earnings = new List<Earnings>
            {
                new Earnings
                {
                    AcademicYear = 0,
                    PricePeriods = new List<PricePeriod>
                    {
                        new PricePeriod
                        {
                            StartDate = new DateTime(2026, 1, 1),
                            EndDate = new DateTime(2026, 2, 28),
                            Price = 1000m,
                            Periods = new List<EarningPeriod>()
                            {
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
                }
            };

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings Academic Year is required");
        }

        [Test]
        public void Validate_rejects_null_earnings_price_periods()
        {
            _message.Earnings = new List<Earnings>
            {
                new Earnings
                {
                    AcademicYear = 2526,
                    PricePeriods = null
                }
            };

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings Price Periods are required");
        }

        [Test]
        public void Validate_rejects_empty_earnings_price_periods()
        {
            _message.Earnings = new List<Earnings>
            {
                new Earnings
                {
                    AcademicYear = 2526,
                    PricePeriods = new List<PricePeriod>()
                }
            };

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings Price Periods are required");
        }

        [Test]
        public void Validate_rejects_empty_earnings_price_period_start_date()
        {
            _message.Earnings = new List<Earnings>
            {
                new Earnings
                {
                    AcademicYear = 2526,
                    PricePeriods = new List<PricePeriod>
                    {
                        new PricePeriod
                        {
                            StartDate = DateTime.MinValue,
                            EndDate = new DateTime(2026, 2, 28),
                            Price = 1000m,
                            Periods = new List<EarningPeriod>()
                            {
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
                }
            };

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings Price Period Start Date is required");
        }

        [Test]
        public void Validate_rejects_empty_earnings_price_period_earnings()
        {
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
                            EndDate = new DateTime(2026, 2, 28),
                            Price = 1000m,
                            Periods = new List<EarningPeriod>()
                        }
                    }
                }
            };

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings are required");
        }

        [Test]
        public void Validate_rejects_null_earnings_price_period_earnings()
        {
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
                            EndDate = new DateTime(2026, 2, 28),
                            Price = 1000m,
                            Periods = null
                        }
                    }
                }
            };

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings are required");
        }

        [Test]
        public void Validate_rejects_empty_earnings_delivery_period()
        {
            _message.Earnings = new List<Earnings>
            {
                new Earnings
                {
                    AcademicYear = 2526,
                    PricePeriods = new List<PricePeriod>
                    {
                        new PricePeriod
                        {
                            StartDate = new DateTime(2026, 1,1),
                            EndDate = new DateTime(2026, 2, 28),
                            Price = 1000m,
                            Periods = new List<EarningPeriod>()
                            {
                                new EarningPeriod
                                {
                                    Amount = 500m,
                                    DeliveryPeriod = 0,
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
                }
            };

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings Delivery Period is required");
        }

        [Test]
        public void Validate_rejects_empty_earnings_employer()
        {
            _message.Earnings = new List<Earnings>
            {
                new Earnings
                {
                    AcademicYear = 2526,
                    PricePeriods = new List<PricePeriod>
                    {
                        new PricePeriod
                        {
                            StartDate = new DateTime(2026, 1,1),
                            EndDate = new DateTime(2026, 2, 28),
                            Price = 1000m,
                            Periods = new List<EarningPeriod>()
                            {
                                new EarningPeriod
                                {
                                    Amount = 0m,
                                    DeliveryPeriod = 2,
                                    EarningType = EarningType.Milestone1,
                                    Employer = null
                                }
                            }
                        }
                    }
                }
            };

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings Employer is required");
        }

        [Test]
        public void Validate_rejects_empty_earnings_employer_account_id()
        {
            _message.Earnings = new List<Earnings>
            {
                new Earnings
                {
                    AcademicYear = 2526,
                    PricePeriods = new List<PricePeriod>
                    {
                        new PricePeriod
                        {
                            StartDate = new DateTime(2026, 1,1),
                            EndDate = new DateTime(2026, 2, 28),
                            Price = 1000m,
                            Periods = new List<EarningPeriod>()
                            {
                                new EarningPeriod
                                {
                                    Amount = 0m,
                                    DeliveryPeriod = 2,
                                    EarningType = EarningType.Milestone1,
                                    Employer = new Employer
                                    {
                                        AccountId = 0,
                                        EmployerType = EmployerType.NonLevy,
                                        FundingAccountId = 1000
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings Employer Account Id is required");
        }

        [Test]
        public void Validate_rejects_empty_earnings_employer_funding_account_id()
        {
            _message.Earnings = new List<Earnings>
            {
                new Earnings
                {
                    AcademicYear = 2526,
                    PricePeriods = new List<PricePeriod>
                    {
                        new PricePeriod
                        {
                            StartDate = new DateTime(2026, 1,1),
                            EndDate = new DateTime(2026, 2, 28),
                            Price = 1000m,
                            Periods = new List<EarningPeriod>()
                            {
                                new EarningPeriod
                                {
                                    Amount = 0m,
                                    DeliveryPeriod = 2,
                                    EarningType = EarningType.Milestone1,
                                    Employer = new Employer
                                    {
                                        AccountId = 30000,
                                        EmployerType = EmployerType.NonLevy,
                                        FundingAccountId = 0
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Action act = () => _sut.Validate(_message);

            act.Should().Throw<ArgumentException>()
                .WithMessage("Earnings Employer Funding Account Id is required");
        }

        [Test]
        public void Validate_rejects_null_command()
        {
            Action act = () => _sut.Validate(null);

            act.Should().Throw<ArgumentException>()
                .WithMessage("CalculateGrowthAndSkillsPayments is required");
        }

        [Test]
        public void Validate_accepts_valid_command()
        {
            var result = _sut.Validate(_message);

            result.Should().BeTrue();
        }
    }
}

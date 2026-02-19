using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators
{
    // ReSharper disable once InconsistentNaming
    public class CalculateGSLPaymentsValidator: ICalculateGSLPaymentsValidator
    {
        public bool Validate(CalculateGrowthAndSkillsPayments command)
        {
            if (command == null)
            {
                throw new ArgumentException("CalculateGrowthAndSkillsPayments is required");
            }

            if (command.EarningsId == Guid.Empty)
            {
                throw new ArgumentException("Earnings Id is required");
            }

            if (command.UKPRN == 0)
            {
                throw new ArgumentException("UKPRN is required");
            }

            ValidateLearner(command);
            ValidateTraining(command);
            ValidateEarnings(command);

            return true;
        }

        private void ValidateLearner(CalculateGrowthAndSkillsPayments command)
        {
            var learner = command.Learner;
            if (learner == null)
            {
                throw new ArgumentException("Learner is required");
            }

            if (learner.LearnerId == Guid.Empty)
            {
                throw new ArgumentException("Learner Id is required");
            }

            if (learner.ULN == 0)
            {
                throw new ArgumentException("ULN is required");
            }

            if (String.IsNullOrWhiteSpace(learner.Reference))
            {
                throw new ArgumentException("Learner Reference is required");
            }
        }

        private void ValidateTraining(CalculateGrowthAndSkillsPayments command)
        {
            var training = command.Training;
            if (training == null)
            {
                throw new ArgumentException("Training is required");
            }

            if (String.IsNullOrWhiteSpace(training.CourseCode))
            {
                throw new ArgumentException("Course Code is required");
            }

            if (String.IsNullOrWhiteSpace(training.CourseReference))
            {
                throw new ArgumentException("Course Reference is required");
            }

            if (training.StartDate == DateTime.MinValue)
            {
                throw new ArgumentException("Training Start Date is required");
            }

            if (training.AgeAtStartOfTraining == 0)
            {
                throw new ArgumentException("Age At Start Of Learning is required");
            }

            if (training.PlannedEndDate == DateTime.MinValue)
            {
                throw new ArgumentException("Training Planned End Date is required");
            }
        }

        private void ValidateEarnings(CalculateGrowthAndSkillsPayments command)
        {
            var earnings = command.Earnings;
            if ((earnings == null) || !earnings.Any())
            {
                throw new ArgumentException("Earnings are required");
            }

            foreach (var earning in earnings)
            {
                if (earning.AcademicYear == 0)
                {
                    throw new ArgumentException("Earnings Academic Year is required");
                }

                if (earning.PricePeriods == null || !earning.PricePeriods.Any())
                {
                    throw new ArgumentException("Earnings Price Periods are required");
                }

                foreach (var pricePeriod in earning.PricePeriods)
                {
                    if (pricePeriod.StartDate == DateTime.MinValue)
                    {
                        throw new ArgumentException("Earnings Price Period Start Date is required");
                    }

                    if (pricePeriod.Periods == null || !pricePeriod.Periods.Any())
                    {
                        throw new ArgumentException("Earnings are required");
                    }

                    foreach (var earningPeriod in pricePeriod.Periods)
                    {
                        if (earningPeriod.DeliveryPeriod == 0)
                        {
                            throw new ArgumentException("Earnings Delivery Period is required");
                        }

                        if (earningPeriod.Employer == null)
                        {
                            throw new ArgumentException("Earnings Employer is required");
                        }

                        if (earningPeriod.Employer.AccountId == 0)
                        {
                            throw new ArgumentException("Earnings Employer Account Id is required");
                        }

                        if (earningPeriod.Employer.FundingAccountId == 0)
                        {
                            throw new ArgumentException("Earnings Employer Funding Account Id is required");
                        }

                        if (earningPeriod.LearningId == 0)
                        {
                            throw new ArgumentException("Earnings Learning Id is required");
                        }
                    }
                }
            }
        }

    }
}

using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{
    public interface IgSLEarningsProcessor
    {
        ShortCourseEarningModel MapToShortCourseEarningModel(CalculateGSLPayments source);
    }

    //Rename to mapper
    public class GSLEarningsProcessor : IgSLEarningsProcessor
    {
        public ShortCourseEarningModel MapToShortCourseEarningModel(CalculateGSLPayments source)
        {
            return new ShortCourseEarningModel
            {
                EarningsId = source.EarningsId,
                UKPRN = source.UKPRN,
                LearnerId = source.Learner.LearnerId,
                LearnerUln = source.Learner.ULN,
                LearnerReference = source.Learner.Reference,
                LearningType = (Model.LearningType)source.Training.LearningType,
                CourseCode = source.Training.CourseCode,
                StartDate = source.Training.StartDate,
                AgeAtStartOfTraining = source.Training.AgeAtStartOfTraining,
                PlannedEndDate = source.Training.PlannedEndDate,
                ActualEndDate = source.Training.ActualEndDate,
                TrainingStatus = (Model.TrainingStatus)source.Training.TrainingStatus,
                EmployerContribution = source.EmployerContribution,
                PricePeriods = MapToPricePeriodModels(source)
            };
        }

        //Naming needs to be refined
        private List<ShortCourseEarningPricePeriodModel> MapToPricePeriodModels(CalculateGSLPayments source )
            {
                IEnumerable<Earnings> earningsEnumerable = source.Earnings;
                var output = new List<ShortCourseEarningPricePeriodModel>();
                
                foreach (var earning in earningsEnumerable)
                {
                    var shortCourseEarningPricePeriodRecord = new ShortCourseEarningPricePeriodModel();
                    shortCourseEarningPricePeriodRecord.AcademicYear = earning.AcademicYear; 
                    foreach (var pricePeriod in earning.PricePeriods)
                    {
                        shortCourseEarningPricePeriodRecord.Price = pricePeriod.Price;
                        shortCourseEarningPricePeriodRecord.StartDate = pricePeriod.StartDate;
                        shortCourseEarningPricePeriodRecord.EndDate = pricePeriod.EndDate;

                        foreach (var earningPeriod in pricePeriod.Periods)
                        {
                            shortCourseEarningPricePeriodRecord.DeliveryPeriod = earningPeriod.DeliveryPeriod; 
                            shortCourseEarningPricePeriodRecord.EarningType = (Model.EarningType)earningPeriod.EarningType;
                            shortCourseEarningPricePeriodRecord.Amount = earningPeriod.Amount;
                            shortCourseEarningPricePeriodRecord.EmployerAccountId = earningPeriod.Employer.AccountId; 
                            shortCourseEarningPricePeriodRecord.EmployerType = (Model.EmployerType)earningPeriod.Employer.EmployerType; 
                            shortCourseEarningPricePeriodRecord.FundingAccountId = earningPeriod.Employer.FundingAccountId;
                            shortCourseEarningPricePeriodRecord.ShortCourseEarningsId = source.EarningsId; 
                        }

                    }

                    output.Add(shortCourseEarningPricePeriodRecord);

                }
                return output;
            }
        }


}


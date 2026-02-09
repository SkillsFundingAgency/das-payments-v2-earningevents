using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services
{


    //Rename to mapper /
    public class GSLEarningsMapper : IGSLEarningsMapper
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

        private List<ShortCourseEarningPricePeriodModel> MapToPricePeriodModels(CalculateGSLPayments source )
            {
                    var output = new List<ShortCourseEarningPricePeriodModel>();

                    foreach (var earning in source.Earnings)
                    {
                        foreach (var pricePeriod in earning.PricePeriods)
                        {
                            foreach (var earningPeriod in pricePeriod.Periods)
                            {
                                output.Add(new ShortCourseEarningPricePeriodModel
                                {
                                    AcademicYear = earning.AcademicYear,
                                    Price = pricePeriod.Price,
                                    StartDate = pricePeriod.StartDate,
                                    EndDate = pricePeriod.EndDate,
                                    DeliveryPeriod = earningPeriod.DeliveryPeriod,
                                    EarningType = (Model.EarningType)earningPeriod.EarningType,
                                    Amount = earningPeriod.Amount,
                                    EmployerAccountId = earningPeriod.Employer.AccountId,
                                    EmployerType = (Model.EmployerType)earningPeriod.Employer.EmployerType,
                                    FundingAccountId = earningPeriod.Employer.FundingAccountId,
                                    ShortCourseEarningsId = source.EarningsId
                                });
                            }
                        }
                    }

                    return output;
                }
    }


}


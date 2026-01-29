namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Messages.Events
{
    public class CalculateApprenticeshipPayments : Events.CalculateGSLPayments
    {
        //It needs to be evaluated whether decimal should be used, additionally
        //Do we need to add decimal place range here?

        public decimal EmployerContribution { get; set; }
    }

}

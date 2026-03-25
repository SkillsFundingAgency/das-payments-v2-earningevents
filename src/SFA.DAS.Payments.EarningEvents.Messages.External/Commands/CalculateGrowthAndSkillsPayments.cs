namespace SFA.DAS.Payments.EarningEvents.Messages.External.Commands
{
    public class CalculateGrowthAndSkillsPayments
    {
        public decimal EmployerContribution { get; set; }
        public Guid EarningsId { get; set; } // Will be generated as UUID Version 7
        public long UKPRN { get; set; }
        public Learner Learner { get; set; }
        public Training Training { get; set; }
        public IEnumerable<Earnings> Earnings { get; set; }
    }
}

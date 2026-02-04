
namespace SFA.DAS.Payments.EarningEvents.Model
{
    public class ShortCourseEarningPricePeriodModel
    {
        public long Id { get; set; }
        public Guid ShortCourseEarningsId { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte DeliveryPeriod { get; set; }
        public short AcademicYear { get; set; }
        public EarningType EarningType { get; set; }
        public decimal Amount { get; set; }
        public long EmployerAccountId { get; set; }
        public EmployerType EmployerType { get; set; }
        public long FundingAccountId { get; set; }
    }
}

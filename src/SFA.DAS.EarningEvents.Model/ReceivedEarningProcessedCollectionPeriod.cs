namespace SFA.DAS.Payments.EarningEvents.Model
{
    public class ReceivedEarningProcessedCollectionPeriod
    {
        public long Id { get; set; }
        public long ReceivedEarningId { get; set; }
        public ReceivedEarning ReceivedEarning { get; set; }
        public short AcademicYear { get; set; }
        public byte CollectionPeriod { get; set; }
        public Guid InteranlEarningId { get; set; }
        public DateTime ProcessedOn { get; set; }
    }
}

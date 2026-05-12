
using UUIDNext;
using UUIDNext.Tools;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]
    public class UUIDComparisionTests
    {
        private const int Iterations = 100;

        [Test]
        public void UUID_CompareTo_reliably_compares_two_UUID_v8_values()
        {
            for(int i = 1; i <= Iterations; i++)
            {
                var firstUUID = Uuid.NewDatabaseFriendly(Database.SqlServer);
                Thread.Sleep(1);
                var secondUUID = Uuid.NewDatabaseFriendly(Database.SqlServer);

                var result = firstUUID.CompareTo(secondUUID);

                Assert.That(result, Is.LessThan(0), "Comparison failed on iteration " + i);
            }
        }

        [Test]
        public void GuidComparer_reliably_compares_two_UUID_v8_values()
        {
            var comparer = new GuidComparer();
            for (int i = 1; i <= Iterations; i++)
            {
                var firstUUID = Uuid.NewDatabaseFriendly(Database.SqlServer);
                Thread.Sleep(1);
                var secondUUID = Uuid.NewDatabaseFriendly(Database.SqlServer);

                var result = comparer.Compare(secondUUID, firstUUID);

                Assert.That(result, Is.GreaterThan(0), "Comparison failed on iteration " + i);
            }
        }

        [Test]
        public void Timestamp_comparison_reliably_compares_two_UUID_v8_values()
        {
            for (int i = 1; i <= Iterations; i++)
            {
                var firstUUID = Uuid.NewDatabaseFriendly(Database.SqlServer);
                Thread.Sleep(1);
                var secondUUID = Uuid.NewDatabaseFriendly(Database.SqlServer);

                var firstEventIdDecodesToTimestamp = UuidDecoder.TryDecodeTimestamp(firstUUID, out var firstEventDateTime);
                var secondEventIdDecodesToTimestamp = UuidDecoder.TryDecodeTimestamp(secondUUID, out var secondEventDateTime);
                
                Assert.That(secondEventDateTime, Is.GreaterThan(firstEventDateTime), "Comparison failed on iteration " + i);
            }
        }
    }
}

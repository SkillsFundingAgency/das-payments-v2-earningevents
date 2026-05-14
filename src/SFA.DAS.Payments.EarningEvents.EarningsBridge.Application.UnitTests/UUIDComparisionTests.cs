
using System.Data.SqlTypes;
using System.Diagnostics;
using UUIDNext;
using UUIDNext.Tools;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]
    public class UUIDComparisionTests
    {
        private const int Iterations = 1000000;

        [Test,Ignore("fails in first few iterations")]
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

        [Test, Ignore("fails in first few iterations")]
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
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 1; i <= Iterations; i++)
            {
                var firstUUID = Uuid.NewDatabaseFriendly(Database.SqlServer);
                //Thread.Sleep(1);
                var secondUUID = Uuid.NewDatabaseFriendly(Database.SqlServer);

                var firstEventIdDecodesToTimestamp = UuidDecoder.TryDecodeTimestamp(firstUUID, out var firstEventDateTime);
                var secondEventIdDecodesToTimestamp = UuidDecoder.TryDecodeTimestamp(secondUUID, out var secondEventDateTime);
                
                Assert.That(secondEventDateTime, Is.GreaterThanOrEqualTo(firstEventDateTime), "Comparison failed on iteration " + i);
            }

            Console.WriteLine($"{Iterations} comparisons completed in {stopwatch.ElapsedMilliseconds} milliseconds");
        }

        [Test]
        public void SqlGuid_reliably_compares_two_SQL_Server_UUID_v8_values() 
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 1; i <= Iterations; i++)
            {
                var firstUUID = Uuid.NewDatabaseFriendly(Database.SqlServer);
                //Thread.Sleep(1);
                var secondUUID = Uuid.NewDatabaseFriendly(Database.SqlServer);

                var result = new SqlGuid(secondUUID).CompareTo(new SqlGuid(firstUUID));

                Assert.That(result, Is.GreaterThanOrEqualTo(0), "Comparison failed on iteration " + i);
            }

            Console.WriteLine($"{Iterations} comparisons completed in {stopwatch.ElapsedMilliseconds} milliseconds");
        }
    }
}

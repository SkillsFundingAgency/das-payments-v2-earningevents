using System;
using System.Collections.Generic;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.EarningEvents.Messages.Events
{
    // ReSharper disable once InconsistentNaming
    public class GSLShortCourseEarningsEvent : IEarningEvent
    {
        public long JobId { get; set; }
        public Guid EventId { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public Guid ExternalEarningsId { get; set; }
        public long Ukprn { get; set; }
        public Learner Learner { get; set; }
        public LearningAim LearningAim { get; set; }
        public DateTime IlrSubmissionDateTime { get; set; }
        public CollectionPeriod CollectionPeriod { get; set; }
        public List<PriceEpisode> PriceEpisodes { get; set; }
        public short CollectionYear { get; set; }
        public int AgeAtStartOfLearning { get; set; }
        public IEnumerable<ShortCourseEarning> Earnings { get; set; }
        public FundingPlatformType FundingPlatformType { get; set; }
    }
}

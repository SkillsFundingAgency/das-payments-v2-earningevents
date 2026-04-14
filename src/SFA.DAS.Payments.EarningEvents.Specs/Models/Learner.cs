using SFA.DAS.Payments.EarningEvents.Specs.Models;

namespace SFA.DAS.Payments.EarningEvents.Specs.Models
{
    public class Learner
    {
        public long Ukprn { get; set; }
        public string LearnRefNumber { get; set; }
        public long Uln { get; set; }
        public Course Course { get; set; }

        public string LearnerIdentifier { get; set; }

        public string SmallEmployer { get; set; }

        public bool IsLevyLearner { get; set; }
        public override string ToString()
        {
            return $"Learn Ref Number: [ {LearnRefNumber} ]\tUln: [ {Uln} ]\t\tLearner Identifier: [ {LearnerIdentifier} ]";
        }
    }
}

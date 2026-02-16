using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.EarningEvents.Model
{
    public class ReceivedDASEarningsMessageModel
    {
        public Guid EarningsId { get; set; }

        public string CourseCode { get; set; }

        public int CollectionPeriod { get; set; }

        public short AcademicYear { get; set; }


    }
}

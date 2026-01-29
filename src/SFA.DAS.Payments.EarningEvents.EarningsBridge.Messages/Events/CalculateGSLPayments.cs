using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Text;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Messages.Models;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Messages.Events
{
    public class CalculateGSLPayments
    {

        public int EarningsId { get; set; }
        public long UKPRN { get; set; }
        [Required]
        public Learner Learner { get; set; }
        [Required]
        public Training Training { get; set; }
        [Required]
        public YearlyEarnings[] Earnings { get; set; }

    }
}

//Why required isn't doing what I expect:
//Short answer: Because [Required] is metadata only — it does not automatically throw or fail tests. You must invoke validation (or use a framework that does) or enforce initialization at compile time.
//    Details and options
//    •	[Required] is a DataAnnotations attribute. It only has an effect when you run a validator (e.g., Validator.ValidateObject, ASP.NET Core model binding that checks ModelState, or a test that explicitly validates the object).
//    •	JSON deserialization leaves missing properties at their default values (null for reference types) unless you use a serializer that enforces required fields (Newtonsoft's Required = Required.Always) or explicitly validate after deserialization.
//    •	If you want compile-time enforcement, use C# required properties (C# 11+) or a constructor that requires those values.
//    •	Note: [Required] on value types (like long UKPRN) does nothing because value types are never null — use long? or a custom check (e.g., UKPRN > 0) if you need to assert presence/validity.
//    Example: validate in a unit test
//    Short answer: Because [Required] is metadata only — it does not automatically throw or fail tests. You must invoke validation (or use a framework that does) or enforce initialization at compile time.
//    Details and options
//    •	[Required] is a DataAnnotations attribute. It only has an effect when you run a validator (e.g., Validator.ValidateObject, ASP.NET Core model binding that checks ModelState, or a test that explicitly validates the object).
//    •	JSON deserialization leaves missing properties at their default values (null for reference types) unless you use a serializer that enforces required fields (Newtonsoft's Required = Required.Always) or explicitly validate after deserialization.
//    •	If you want compile-time enforcement, use C# required properties (C# 11+) or a constructor that requires those values.
//    •	Note: [Required] on value types (like long UKPRN) does nothing because value types are never null — use long? or a custom check (e.g., UKPRN > 0) if you need to assert presence/validity.
//    Example: validate in a unit test

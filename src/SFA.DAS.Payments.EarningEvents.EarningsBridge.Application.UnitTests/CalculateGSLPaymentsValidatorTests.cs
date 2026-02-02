using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]
    public class CalculateGSLPaymentsValidatorTests
    {
        [Test]
        public void Validate_returns_true()
        {
            var result = CalculateGSLPaymentsValidator.Validate(new CalculateGSLPayments());
        }
    }
}

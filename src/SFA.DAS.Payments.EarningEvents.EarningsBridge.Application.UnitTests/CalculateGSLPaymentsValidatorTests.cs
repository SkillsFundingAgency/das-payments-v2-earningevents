using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.UnitTests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class CalculateGSLPaymentsValidatorTests
    {
        private CalculateGSLPaymentsValidator _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new CalculateGSLPaymentsValidator();
        }

        [Test]
        public void Validate_returns_true()
        {
            var result = _sut.Validate(new CalculateGSLPayments());
        }
    }
}

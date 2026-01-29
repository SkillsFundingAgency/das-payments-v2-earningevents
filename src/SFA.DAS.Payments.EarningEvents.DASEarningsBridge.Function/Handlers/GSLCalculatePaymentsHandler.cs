using System;
using System.Collections.Generic;
using System.Text;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Function.Handlers
{
    public interface IGslCalculatePaymentsHandler
    {
        public void HandleGslCalculatePaymentsMessage(CalculateGSLPayments message);



    }

    public class GSLCalculatePaymentsHandler : IGslCalculatePaymentsHandler
    {
        //takes in message object
        public GSLCalculatePaymentsHandler() { }

        public void HandleGslCalculatePaymentsMessage(CalculateGSLPayments message)
        {
            throw new NotImplementedException();
        }

        //pings API collection 

        //SQL db store

        //Sends off requiredpayments and earning audit message
    }
}

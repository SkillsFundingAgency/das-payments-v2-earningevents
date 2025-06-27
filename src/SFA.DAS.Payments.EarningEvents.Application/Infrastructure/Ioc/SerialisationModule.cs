using Autofac;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Xml;
using SFA.DAS.Payments.EarningEvents.Application.Infrastructure.Serialization;

namespace SFA.DAS.Payments.EarningEvents.Application.Infrastructure.Ioc
{
    public class SerialisationModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Fm36JsonSerializationService>().As<IFm36JsonSerializationService>();
            builder.RegisterType<XmlSerializationService>().As<IXmlSerializationService>();
        }
    }
}

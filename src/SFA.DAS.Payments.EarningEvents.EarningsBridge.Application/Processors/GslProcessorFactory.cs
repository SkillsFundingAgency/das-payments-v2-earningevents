using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.EarningEvents.Model;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Processors
{
    public class GSLProcessorFactory : IGSLProcessorFactory
    {
        private IServiceProvider _serviceProvider;

        public GSLProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IGSLProcessor CreateGSLProcessor(CourseType courseType)
        {
            return courseType switch
            {
                CourseType.Apprenticeship => _serviceProvider.GetRequiredService<GSLApprenticeshipPaymentsProcessor>(),
                CourseType.ShortCourse => _serviceProvider.GetRequiredService<GSLShortCoursePaymentsProcessor>(),
                CourseType.FunctionalSkill => _serviceProvider.GetRequiredService<GSLFunctionalSkillProcessor>(),
                _ => _serviceProvider.GetRequiredService<UnsupportedLearningTypeProcessor>(),// Route unsupported learning types to a pass-through processor so message handling can continue safely
            };
        }
    }
}

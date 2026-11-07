using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.EarningEvents.EarningsBridge.Application
{
    public static class Extensions
    {
        public static SFA.DAS.Payments.EarningEvents.Model.CourseType ToModelCourseType(this Messages.External.CourseType courseType)
        {
            return courseType switch
            {
                Messages.External.CourseType.Apprenticeship => Model.CourseType.Apprenticeship,
                Messages.External.CourseType.FunctionalSkill => Model.CourseType.FunctionalSkill,
                Messages.External.CourseType.ShortCourse => Model.CourseType.ShortCourse,
                _ => throw new ArgumentOutOfRangeException(nameof(courseType), courseType, null)
            };
        }

        public static Model.LearningType ToModelLearningType(this Messages.External.LearningType learningType)
        {
            return learningType switch
            {
                Messages.External.LearningType.Apprenticeship => Model.LearningType.Apprenticeship,
                Messages.External.LearningType.FoundationApprenticeship => Model.LearningType.FoundationApprenticeship,
                Messages.External.LearningType.MathsAndEnglish => Model.LearningType.MathsAndEnglish,
                Messages.External.LearningType.ApprenticeshipUnit => Model.LearningType.ApprenticeshipUnit,
                _ => throw new ArgumentOutOfRangeException(nameof(learningType), learningType, null)
            };
        }
    }
}

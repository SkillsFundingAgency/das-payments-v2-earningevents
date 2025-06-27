using System.Linq;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using SFA.DAS.Payments.EarningEvents.Messages.Internal.Commands;
using System;
using System.Collections.Generic;
using AutoFixture;

namespace SFA.DAS.Payments.EarningEvents.Domain.UnitTests
{
    public class LearnerBuilder
    {
        readonly Fixture fixture = new Fixture();
        private int numberOfDeliveries = 1;

        public static LearnerBuilder LearnerWithBasicLevyAim() => new LearnerBuilder();

        public LearnerBuilder WithMultipleDeliveries(int number = 3)
        {
            numberOfDeliveries = number;
            return this;
        }

        public ProcessLearnerCommand BuildLearnerCommand()
        {
            var testDate = DateOnly.Parse("2025-01-01");

            fixture.Customize<FM36Learner>(c => c
                .Without(x => x.PriceEpisodes)
                .Without(x => x.LearningDeliveries)
                .Without(x => x.HistoricEarningOutputValues)
                .Do(x =>
                {
                    x.PriceEpisodes = fixture.Build<PriceEpisode>()
                        .With(y => y.PriceEpisodeValues, fixture.Build<PriceEpisodeValues>()
                            .With(z => z.EpisodeStartDate, testDate)
                            .With(z => z.PriceEpisodeActualEndDate, testDate)
                            .With(z => z.PriceEpisodePlannedEndDate, testDate)
                            .With(z => z.EpisodeEffectiveTNPStartDate, testDate)
                            .With(z => z.PriceEpisodeFirstAdditionalPaymentThresholdDate, testDate)
                            .With(z => z.PriceEpisodeSecondAdditionalPaymentThresholdDate, testDate)
                            .With(z => z.PriceEpisodeLearnerAdditionalPaymentThresholdDate, testDate)
                            .With(z => z.PriceEpisodeRedStartDate, testDate)
                            .With(z => z.PriceEpisodeActualEndDateIncEPA, testDate)
                            .Create())
                        .CreateMany(1).ToList();

                    x.HistoricEarningOutputValues = fixture.Build<HistoricEarningOutputValues>()
                        .With(x => x.HistoricEffectiveTNPStartDateOutput, testDate)
                        .With(x => x.HistoricProgrammeStartDateIgnorePathwayOutput, testDate)
                        .With(x => x.HistoricProgrammeStartDateMatchPathwayOutput, testDate)
                        .With(x => x.HistoricUptoEndDateOutput, testDate)
                        .With(x => x.HistoricLearnDelProgEarliestACT2DateOutput, testDate)
                        .CreateMany(1).ToList();

                    x.LearningDeliveries = fixture.Build<LearningDelivery>()
                        .With(y => y.AimSeqNumber, Convert.ToInt32(x.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeAimSeqNumber))
                        .With(y => y.LearningDeliveryValues, fixture.Build<LearningDeliveryValues>()
                            .With(z => z.LearnAimRef, "ZPROG001")
                            .With(z => z.AdjStartDate, testDate)
                            .With(z => z.AppAdjLearnStartDate, testDate)
                            .With(z => z.AppAdjLearnStartDateMatchPathway, testDate)
                            .With(z => z.ApplicCompDate, testDate)
                            .With(z => z.FirstIncentiveThresholdDate, testDate)
                            .With(z => z.LearnDelApplicEmpDate, testDate)
                            .With(z => z.LearnDelProgEarliestACT2Date, testDate)
                            .With(z => z.SecondIncentiveThresholdDate, testDate)
                            .With(z => z.LearnDelLearnerAddPayThresholdDate, testDate)
                            .With(z => z.LearnDelRedStartDate, testDate)
                            
                            .Create())
                        .CreateMany(numberOfDeliveries).ToList();
                }));

            return fixture.Build<ProcessLearnerCommand>()
                .With(x => x.CollectionYear, 1920)
                .With(x => x.CollectionPeriod, 1)
                .Create();
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.Application.Mapping;
using SFA.DAS.Payments.EarningEvents.Application.Messages;
using SFA.DAS.Payments.EarningEvents.Domain;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using PriceEpisode = ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output.PriceEpisode;

namespace SFA.DAS.Payments.EarningEvents.Application.UnitTests.Mapping
{
    //public class LearnerResolver : IValueResolver<FM36Learner, EarningEvent, Learner>
    //{
    //    public Learner Resolve(FM36Learner source, EarningEvent destination, Learner destMember, ResolutionContext context)
    //    {
    //        return Mapper.Instance.Map<FM36Learner, Learner>(source);
    //    }
    //}

    [TestFixture]
    public class MappingTest
    {
        //[Test]
        //public void Test()
        //{
        //    Mapper.Initialize(cfg =>
        //    {
        //        cfg.CreateMap<FM36Learner, EarningEvent>()
        //            .Include<FM36Learner, ApprenticeshipContractTypeEarningsEvent>()
        //            .Include<FM36Learner, FunctionalSkillEarningsEvent>()
        //            .ForMember(destinationMember => destinationMember.CollectionYear, opt => opt.Ignore())
        //            .ForMember(destinationMember => destinationMember.EventTime, opt => opt.UseValue(DateTimeOffset.UtcNow))
        //            .ForMember(destinationMember => destinationMember.LearningAim, opt => opt.Ignore())
        //            .ForMember(destinationMember => destinationMember.Ukprn, opt => opt.Ignore())
        //            .ForMember(destinationMember => destinationMember.JobId, opt => opt.Ignore())
        //            .ForMember(destinationMember => destinationMember.Learner, opt => opt.ResolveUsing((l, ee) => Mapper.Map<FM36Learner, Learner>(l)));

        //        cfg.CreateMap<FM36Learner, ApprenticeshipContractTypeEarningsEvent>()
        //            .Include<FM36Learner, ApprenticeshipContractType1EarningEvent>()
        //            .Include<FM36Learner, ApprenticeshipContractType2EarningEvent>()
        //            .ForMember(destinationMember => destinationMember.IncentiveEarnings, opt => opt.Ignore())
        //            .ForMember(destinationMember => destinationMember.OnProgrammeEarnings, opt => opt.Ignore())
        //            .ForMember(destinationMember => destinationMember.SfaContributionPercentage, opt => opt.Ignore());

        //        cfg.CreateMap<FM36Learner, ApprenticeshipContractType1EarningEvent>()
        //            .ForMember(destinationMember => destinationMember.AgreementId, opt => opt.Ignore());

        //        cfg.CreateMap<FM36Learner, ApprenticeshipContractType2EarningEvent>();

        //        cfg.CreateMap<FM36Learner, FunctionalSkillEarningsEvent>()
        //            .ForMember(dest => dest.Earnings, opt => opt.Ignore());

        //        cfg.CreateMap<FM36Learner, Learner>()
        //            .ForMember(dest => dest.ReferenceNumber, opt => opt.MapFrom(source => source.LearnRefNumber))
        //            .ForMember(dest => dest.Ukprn, opt => opt.Ignore())
        //            .ForMember(dest => dest.Uln, opt => opt.Ignore());

        //        cfg.CreateMap<ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output.PriceEpisode, Model.Core.PriceEpisode>()
        //            .ForMember(dest => dest.Identifier, opt => opt.MapFrom(source => source.PriceEpisodeIdentifier))
        //            .ForMember(dest => dest.TotalNegotiatedPrice1, opt => opt.MapFrom(source => source.PriceEpisodeValues.TNP1))
        //            .ForMember(dest => dest.TotalNegotiatedPrice2, opt => opt.MapFrom(source => source.PriceEpisodeValues.TNP2))
        //            .ForMember(dest => dest.TotalNegotiatedPrice3, opt => opt.MapFrom(source => source.PriceEpisodeValues.TNP3))
        //            .ForMember(dest => dest.TotalNegotiatedPrice4, opt => opt.MapFrom(source => source.PriceEpisodeValues.TNP4))
        //            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(source => source.PriceEpisodeValues.EpisodeStartDate))
        //            .ForMember(dest => dest.PlannedEndDate, opt => opt.MapFrom(source => source.PriceEpisodeValues.PriceEpisodePlannedEndDate))
        //            .ForMember(dest => dest.ActualEndDate, opt => opt.MapFrom(source => source.PriceEpisodeValues.PriceEpisodeActualEndDate))
        //            .ForMember(dest => dest.NumberOfInstalments, opt => opt.MapFrom(source => source.PriceEpisodeValues.PriceEpisodePlannedInstalments)) //TODO: should this be actual if there is an actual end date?
        //            .ForMember(dest => dest.InstalmentAmount, opt => opt.MapFrom(source => source.PriceEpisodeValues.PriceEpisodeInstalmentValue))
        //            .ForMember(dest => dest.CompletionAmount, opt => opt.MapFrom(source => source.PriceEpisodeValues.PriceEpisodeCompletionElement))
        //            .ForMember(dest => dest.Completed, opt => opt.MapFrom(source => source.PriceEpisodeValues.PriceEpisodeCompleted));

        //    });
        //    Assert_Mappings();
        //}
        private FM36Learner fm36Learner;
        private ProcessLearnerCommand processLearnerCommand;

        [OneTimeSetUp]
        public void InitialiseMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<EarningsEventProfile>();
            });
            Mapper.AssertConfigurationIsValid();
        }

        [SetUp]
        public void SetUp()
        {
            fm36Learner = new FM36Learner
            {
                LearnRefNumber = "learner-a",
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        PriceEpisodeIdentifier = "pe-1",
                        PriceEpisodeValues = new PriceEpisodeValues
                        {

                            EpisodeStartDate = DateTime.Today.AddMonths(-12),
                            PriceEpisodePlannedEndDate = DateTime.Today,
                            PriceEpisodeActualEndDate = DateTime.Today,
                            PriceEpisodePlannedInstalments = 12,
                            PriceEpisodeCompletionElement = 3000,
                            PriceEpisodeInstalmentValue = 1000,
                            TNP1 = 15000,
                            TNP2 = 15000,
                            PriceEpisodeCompleted = true
                        },
                        PriceEpisodePeriodisedValues = new List<PriceEpisodePeriodisedValues>
                        {
                            new PriceEpisodePeriodisedValues
                            {
                                AttributeName = "PriceEpisodeOnProgPayment",
                                Period1 = 1000,
                                Period2 = 1000,
                                Period3 = 1000,
                                Period4 = 1000,
                                Period5 = 1000,
                                Period6 = 1000,
                                Period7 = 1000,
                                Period8 = 1000,
                                Period9 = 1000,
                                Period10 = 1000,
                                Period11 = 1000,
                                Period12 = 1000,
                            },
                            new PriceEpisodePeriodisedValues
                            {
                                AttributeName = "PriceEpisodeCompletionPayment",
                                Period1 = 0,
                                Period2 = 0,
                                Period3 = 0,
                                Period4 = 0,
                                Period5 = 0,
                                Period6 = 0,
                                Period7 = 0,
                                Period8 = 0,
                                Period9 = 0,
                                Period10 = 0,
                                Period11 = 0,
                                Period12 = 3000,
                            },
                        }
                    }
                }
            };

            processLearnerCommand = new ProcessLearnerCommand
            {
                Learner = fm36Learner,
                CollectionYear = "1819",
                Ukprn = 12345,
                JobId = "job-1"
            };
        }

        [Test]
        public void Maps_Learner()
        {
            var earningEvent = Mapper.Instance.Map<ProcessLearnerCommand, ApprenticeshipContractType2EarningEvent>(processLearnerCommand);
            earningEvent.Learner.Should().NotBeNull();
            earningEvent.Learner.ReferenceNumber.Should().Be(fm36Learner.LearnRefNumber);
        }

        [Test]
        public void Maps_Price_Episodes()
        {
            var earningEvent = Mapper.Instance.Map<ProcessLearnerCommand, ApprenticeshipContractType2EarningEvent>(processLearnerCommand);
            earningEvent.PriceEpisodes.Should().NotBeEmpty();
            earningEvent.PriceEpisodes.First().Identifier.Should().Be("pe-1");
            earningEvent.PriceEpisodes.First().TotalNegotiatedPrice1.Should().Be(15000);
            earningEvent.PriceEpisodes.First().TotalNegotiatedPrice2.Should().Be(15000);
            earningEvent.PriceEpisodes.First().CompletionAmount.Should().Be(3000);
            earningEvent.PriceEpisodes.First().InstalmentAmount.Should().Be(1000);
            earningEvent.PriceEpisodes.First().NumberOfInstalments.Should().Be(12);
            earningEvent.PriceEpisodes.First().TotalNegotiatedPrice3.Should().BeNull();
        }

        [Test]
        public void Maps_Price_Episode_Periodised_Values_To_Earning_Periods()
        {
            var earningEvent = Mapper.Instance.Map<ProcessLearnerCommand, ApprenticeshipContractType2EarningEvent>(processLearnerCommand);
            earningEvent.OnProgrammeEarnings.Should().NotBeNullOrEmpty();
            earningEvent.OnProgrammeEarnings.Should().HaveCount(2);
        }

        [Test]
        public void Maps_On_Programme_Earnings()
        {
            var earningEvent = Mapper.Instance.Map<ProcessLearnerCommand, ApprenticeshipContractType2EarningEvent>(processLearnerCommand);
            var learning =
                earningEvent.OnProgrammeEarnings.FirstOrDefault(earnings =>
                    earnings.Type == OnProgrammeEarningType.Learning);
            learning.Should().NotBeNull();
            learning.Periods.Should().HaveCount(12);
            learning.Periods.All(period => period.Amount == 1000).Should().BeTrue();
        }

        [Test]
        public void Maps_Completion_Earnings()
        {
            var earningEvent = Mapper.Instance.Map<ProcessLearnerCommand, ApprenticeshipContractType2EarningEvent>(processLearnerCommand);
            var completion =
                earningEvent.OnProgrammeEarnings.FirstOrDefault(earnings =>
                    earnings.Type == OnProgrammeEarningType.Completion);
            completion.Should().NotBeNull();
            completion.Periods.Should().HaveCount(1);
            completion.Periods.FirstOrDefault().Amount.Should().Be(3000);
        }
    }
}
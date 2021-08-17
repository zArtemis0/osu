// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Gameplay
{
    [HeadlessTest]
    public class TestSceneMasterGameplayClockContainer : OsuTestScene
    {
        [Test]
        public void TestStartThenElapsedTime()
        {
            GameplayClockContainer gcc = null;

            AddStep("create container", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.LoadTrack();

                Add(gcc = new MasterGameplayClockContainer(working, 0));
            });

            AddStep("start clock", () => gcc.Start());
            AddUntilStep("elapsed greater than zero", () => gcc.GameplayClock.ElapsedFrameTime > 0);
        }

        [Test]
        public void TestElapseThenReset()
        {
            GameplayClockContainer gcc = null;

            AddStep("create container", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.LoadTrack();

                Add(gcc = new MasterGameplayClockContainer(working, 0));
            });

            AddStep("start clock", () => gcc.Start());
            AddUntilStep("current time greater 2000", () => gcc.GameplayClock.CurrentTime > 2000);

            double timeAtReset = 0;
            AddStep("reset clock", () =>
            {
                timeAtReset = gcc.GameplayClock.CurrentTime;
                gcc.Reset();
            });

            AddAssert("current time < time at reset", () => gcc.GameplayClock.CurrentTime < timeAtReset);
        }

        [Test]
        public void TestTrueGameplayRate()
        {
            MasterGameplayClockContainer gcc = null;

            AddStep("create container", () =>
            {
                var working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.LoadTrack();

                Add(gcc = new MasterGameplayClockContainer(working, 0));

                gcc.Start();
            });

            AddUntilStep("gameplay clock rate = 1", () => gcc.GameplayClock.Rate == 1f);
            AddAssert("true gameplay rate = 1", () => gcc.TrueGameplayRate == 1f);

            AddStep("stop gcc", () => gcc.Stop());
            AddUntilStep("gameplay clock rate = 0", () => gcc.GameplayClock.Rate == 0f);
            AddAssert("true gameplay rate = 1", () => gcc.TrueGameplayRate == 1f);

            AddStep("change user playback rate to 2", () => gcc.UserPlaybackRate.Value = 2f);
            AddAssert("gameplay clock rate = 0", () => gcc.GameplayClock.Rate == 0f);
            AddAssert("true gameplay rate = 1", () => gcc.TrueGameplayRate == 1f);

            AddStep("add 1.5 gameplay freq adjustment", () => gcc.GameplayTrack.AddAdjustment(AdjustableProperty.Frequency, new BindableDouble(1.5f)));
            AddAssert("gameplay clock rate = 0", () => gcc.GameplayClock.Rate == 0f);
            AddAssert("true gameplay rate = 1.5", () => gcc.TrueGameplayRate == 1.5f);
        }

        [Test]
        public void TestHardwareCorrectionOffsetClock()
        {
            OsuConfigManager config = null;
            WorkingBeatmap working = null;
            TestMasterGameplayClockContainer gcc = null;

            AddStep("create container", () =>
            {
                config = new OsuConfigManager(LocalStorage);

                working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.LoadTrack();

                Add(new DependencyProvidingContainer
                {
                    CachedDependencies = new (Type, object)[] { (typeof(OsuConfigManager), config) },
                    Child = gcc = new TestMasterGameplayClockContainer(working, 0),
                });

                gcc.Stop();
                gcc.Reset();
            });

            AddStep("set audio offset to 100ms", () => config.SetValue(OsuSetting.AudioOffset, 100.0));

            AddAssert("current time has 100% offset", () => gcc.GameplayClock.CurrentTime == 100);

            AddStep("add 0 track freq adjustment", () => working.Track.AddAdjustment(AdjustableProperty.Frequency, new BindableDouble()));
            AddAssert("current time still has 100% offset", () => gcc.GameplayClock.CurrentTime == 100);

            AddStep("start gcc", () => gcc.Start());
            AddAssert("current time still has 100% offset", () => gcc.GameplayClock.CurrentTime == 100);

            AddStep("change user playback rate to 2", () => gcc.UserPlaybackRate.Value = 2f);
            AddAssert("current time has 200% offset", () => gcc.GameplayClock.CurrentTime == 200);

            AddStep("change user playback rate to 0.5", () => gcc.UserPlaybackRate.Value = 0.5f);
            AddAssert("current time has 50% offset", () => gcc.GameplayClock.CurrentTime == 50);

            AddStep("add 0.5 gameplay freq adjustment", () => gcc.GameplayTrack.AddAdjustment(AdjustableProperty.Frequency, new BindableDouble(0.5f)));
            AddAssert("current time has 25% offset", () => gcc.GameplayClock.CurrentTime == 25);
        }

        [Test]
        public void TestSeekPerformsInGameplayTime(
            [Values(1.0, 0.5, 2.0, 0.0)] double clockRate,
            [Values(0.0, 500.0, -500.0)] double userOffset)
        {
            OsuConfigManager config = null;
            WorkingBeatmap working = null;
            GameplayClockContainer gcc = null;

            AddStep("create container", () =>
            {
                config = new OsuConfigManager(LocalStorage);

                working = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                working.LoadTrack();

                Add(new DependencyProvidingContainer
                {
                    CachedDependencies = new (Type, object)[] { (typeof(OsuConfigManager), config) },
                    Child = gcc = new MasterGameplayClockContainer(working, 0),
                });

                gcc.Stop();
                gcc.Reset();
            });

            if (clockRate != 1.0)
                AddStep($"set clock rate to {clockRate}", () => working.Track.AddAdjustment(AdjustableProperty.Frequency, new BindableDouble(clockRate)));

            if (userOffset != 0f)
                AddStep($"set audio offset to {userOffset}", () => config.SetValue(OsuSetting.AudioOffset, userOffset));

            AddStep("seek to 0", () => gcc.Seek(0));
            AddAssert("gameplay clock time = 0", () => gcc.CurrentTime == 0);

            AddStep("seek to 2500", () => gcc.Seek(2500));
            AddAssert("gameplay clock time = 2500", () => gcc.CurrentTime == 2500);

            AddStep("seek to -2500", () => gcc.Seek(-2500));
            AddAssert("gameplay clock time = -2500", () => gcc.GameplayClock.CurrentTime == -2500);
        }

        private class TestMasterGameplayClockContainer : MasterGameplayClockContainer
        {
            protected override bool ApplyPlatformOffset => false;

            public new DecoupleableInterpolatingFramedClock AdjustableSource => base.AdjustableSource;

            public TestMasterGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStartTime, bool startAtGameplayStart = false)
                : base(beatmap, gameplayStartTime, startAtGameplayStart)
            {
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A <see cref="GameplayClockContainer"/> which uses a <see cref="WorkingBeatmap"/> as a source.
    /// <para>
    /// This is the most complete <see cref="GameplayClockContainer"/> which takes into account all user and platform offsets,
    /// and provides implementations for user actions such as skipping or adjusting playback rates that may occur during gameplay.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This is intended to be used as a single controller for gameplay, or as a reference source for other <see cref="GameplayClockContainer"/>s.
    /// </remarks>
    public class MasterGameplayClockContainer : GameplayClockContainer, ITrack
    {
        /// <summary>
        /// Duration before gameplay start time required before skip button displays.
        /// </summary>
        public const double MINIMUM_SKIP_TIME = 1000;

        protected Track Track => (Track)SourceClock;

        public readonly BindableNumber<double> UserPlaybackRate = new BindableDouble(1)
        {
            Default = 1,
            MinValue = 0.5,
            MaxValue = 2,
            Precision = 0.1,
        };

        private readonly AudioAdjustments gameplayAdjustments = new AudioAdjustments();

        /// <summary>
        /// The rate of gameplay when playback is at 100%.
        /// This excludes any seeking / user adjustments.
        /// </summary>
        public double TrueGameplayRate => gameplayAdjustments.AggregateFrequency.Value * gameplayAdjustments.AggregateTempo.Value;

        /// <summary>
        /// The true gameplay rate combined with the <see cref="UserPlaybackRate"/> value.
        /// </summary>
        public double PlaybackRate => TrueGameplayRate * UserPlaybackRate.Value;

        private double totalOffset => userOffsetClock.Offset + platformOffsetClock.Offset;

        private readonly BindableDouble pauseFreqAdjust = new BindableDouble(1);

        private readonly WorkingBeatmap beatmap;
        private readonly double gameplayStartTime;
        private readonly bool startAtGameplayStart;
        private readonly double firstHitObjectTime;

        private FramedOffsetClock userOffsetClock;
        private FramedOffsetClock platformOffsetClock;
        private Bindable<double> userAudioOffset;
        private double startOffset;

        public MasterGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStartTime, bool startAtGameplayStart = false)
            : base(beatmap.Track)
        {
            this.beatmap = beatmap;
            this.gameplayStartTime = gameplayStartTime;
            this.startAtGameplayStart = startAtGameplayStart;

            firstHitObjectTime = beatmap.Beatmap.HitObjects.First().StartTime;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
            userAudioOffset.BindValueChanged(offset => userOffsetClock.Offset = offset.NewValue, true);

            // sane default provided by ruleset.
            startOffset = gameplayStartTime;

            if (!startAtGameplayStart)
            {
                startOffset = Math.Min(0, startOffset);

                // if a storyboard is present, it may dictate the appropriate start time by having events in negative time space.
                // this is commonly used to display an intro before the audio track start.
                double? firstStoryboardEvent = beatmap.Storyboard.EarliestEventTime;
                if (firstStoryboardEvent != null)
                    startOffset = Math.Min(startOffset, firstStoryboardEvent.Value);

                // some beatmaps specify a current lead-in time which should be used instead of the ruleset-provided value when available.
                // this is not available as an option in the live editor but can still be applied via .osu editing.
                if (beatmap.BeatmapInfo.AudioLeadIn > 0)
                    startOffset = Math.Min(startOffset, firstHitObjectTime - beatmap.BeatmapInfo.AudioLeadIn);
            }

            Seek(startOffset);
        }

        protected override void OnIsPausedChanged(ValueChangedEvent<bool> isPaused)
        {
            // The source is stopped by a frequency fade first.
            if (isPaused.NewValue)
            {
                this.TransformBindableTo(pauseFreqAdjust, 0, 200, Easing.Out).OnComplete(_ =>
                {
                    if (IsPaused.Value == isPaused.NewValue)
                        AdjustableSource.Stop();
                });
            }
            else
                this.TransformBindableTo(pauseFreqAdjust, 1, 200, Easing.In);
        }

        public override void Start()
        {
            addSourceClockAdjustments();
            base.Start();
        }

        /// <summary>
        /// Seek to a specific time in gameplay.
        /// </summary>
        /// <remarks>
        /// Adjusts for any offsets which have been applied (so the seek may not be the expected point in time on the underlying audio track).
        /// </remarks>
        /// <param name="time">The destination time to seek to.</param>
        public override void Seek(double time)
        {
            // remove the offset component here because most of the time we want the seek to be aligned to gameplay, not the audio track.
            // we may want to consider reversing the application of offsets in the future as it may feel more correct.
            base.Seek(time - totalOffset * PlaybackRate);
        }

        /// <summary>
        /// Skip forward to the next valid skip point.
        /// </summary>
        public void Skip()
        {
            if (GameplayClock.CurrentTime > gameplayStartTime - MINIMUM_SKIP_TIME)
                return;

            double skipTarget = gameplayStartTime - MINIMUM_SKIP_TIME;

            if (GameplayClock.CurrentTime < 0 && skipTarget > 6000)
                // double skip exception for storyboards with very long intros
                skipTarget = 0;

            Seek(skipTarget);
        }

        public override void Reset()
        {
            base.Reset();
            Seek(startOffset);
        }

        protected override GameplayClock CreateGameplayClock(IFrameBasedClock source)
        {
            // Lazer's audio timings in general doesn't match stable. This is the result of user testing, albeit limited.
            // This only seems to be required on windows. We need to eventually figure out why, with a bit of luck.
            platformOffsetClock = new HardwareCorrectionOffsetClock(source, this) { Offset = RuntimeInfo.OS == RuntimeInfo.Platform.Windows ? 15 : 0 };

            // the final usable gameplay clock with user-set offsets applied.
            userOffsetClock = new HardwareCorrectionOffsetClock(platformOffsetClock, this);

            return new MasterGameplayClock(userOffsetClock, this);
        }

        /// <summary>
        /// Changes the backing clock to avoid using the originally provided track.
        /// </summary>
        public void StopUsingBeatmapClock()
        {
            removeSourceClockAdjustments();
            ChangeSource(new TrackVirtual(beatmap.Track.Length));
            addSourceClockAdjustments();
        }

        private bool speedAdjustmentsApplied;

        private void addSourceClockAdjustments()
        {
            if (speedAdjustmentsApplied)
                return;

            Track.BindAdjustments(gameplayAdjustments);
            Track.AddAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
            Track.AddAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);

            speedAdjustmentsApplied = true;
        }

        private void removeSourceClockAdjustments()
        {
            if (!speedAdjustmentsApplied)
                return;

            Track.RemoveAdjustment(AdjustableProperty.Frequency, pauseFreqAdjust);
            Track.RemoveAdjustment(AdjustableProperty.Tempo, UserPlaybackRate);
            Track.UnbindAdjustments(gameplayAdjustments);

            speedAdjustmentsApplied = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            removeSourceClockAdjustments();
        }

        #region Delegated IAdjustableAudioComponent implementation (gameplay adjustments)

        public IBindable<double> AggregateVolume => gameplayAdjustments.AggregateVolume;
        public IBindable<double> AggregateBalance => gameplayAdjustments.AggregateBalance;
        public IBindable<double> AggregateFrequency => gameplayAdjustments.AggregateFrequency;
        public IBindable<double> AggregateTempo => gameplayAdjustments.AggregateTempo;

        public void BindAdjustments(IAggregateAudioAdjustment component) => gameplayAdjustments.BindAdjustments(component);
        public void UnbindAdjustments(IAggregateAudioAdjustment component) => gameplayAdjustments.UnbindAdjustments(component);

        public void AddAdjustment(AdjustableProperty type, IBindable<double> adjustBindable) => gameplayAdjustments.AddAdjustment(type, adjustBindable);
        public void RemoveAdjustment(AdjustableProperty type, IBindable<double> adjustBindable) => gameplayAdjustments.RemoveAdjustment(type, adjustBindable);
        public void RemoveAllAdjustments(AdjustableProperty type) => gameplayAdjustments.RemoveAllAdjustments(type);

        public BindableNumber<double> Volume => gameplayAdjustments.Volume;
        public BindableNumber<double> Balance => gameplayAdjustments.Balance;
        public BindableNumber<double> Frequency => gameplayAdjustments.Frequency;
        public BindableNumber<double> Tempo => gameplayAdjustments.Tempo;

        #endregion

        #region Delegated ITrack implementation

        ChannelAmplitudes IHasAmplitudes.CurrentAmplitudes => Track.CurrentAmplitudes;

        void ITrack.Restart()
        {
            Track.Restart();
        }

        bool ITrack.Looping
        {
            get => Track.Looping;
            set => Track.Looping = value;
        }

        bool ITrack.IsDummyDevice => Track.IsDummyDevice;

        double ITrack.RestartPoint
        {
            get => Track.RestartPoint;
            set => Track.RestartPoint = value;
        }

        double ITrack.Length
        {
            get => Track.Length;
            set => Track.Length = value;
        }

        int? ITrack.Bitrate => Track.Bitrate;

        bool ITrack.IsReversed => Track.IsReversed;

        bool ITrack.HasCompleted => Track.HasCompleted;

        event Action ITrack.Completed
        {
            add => Track.Completed += value;
            remove => Track.Completed -= value;
        }

        event Action ITrack.Failed
        {
            add => Track.Failed += value;
            remove => Track.Failed -= value;
        }

        #endregion

        private class HardwareCorrectionOffsetClock : FramedOffsetClock
        {
            private readonly MasterGameplayClockContainer gameplayClockContainer;

            // we always want to apply the same real-time offset, so it should be adjusted by the difference in playback rate (from realtime) to achieve this.
            // base implementation already adds offset at 1.0 rate, so we only add the difference from that here.
            public override double CurrentTime => base.CurrentTime + Offset * (gameplayClockContainer.PlaybackRate - 1);

            public HardwareCorrectionOffsetClock(IClock source, MasterGameplayClockContainer gameplayClockContainer)
                : base(source)
            {
                this.gameplayClockContainer = gameplayClockContainer;
            }
        }

        private class MasterGameplayClock : GameplayClock
        {
            private readonly MasterGameplayClockContainer gameplayClockContainer;

            public override double TrueGameplayRate => gameplayClockContainer.TrueGameplayRate;

            public MasterGameplayClock(FramedOffsetClock underlyingClock, MasterGameplayClockContainer gameplayClockContainer)
                : base(underlyingClock)
            {
                this.gameplayClockContainer = gameplayClockContainer;
            }
        }
    }
}

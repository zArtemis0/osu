﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using osu.Framework.Caching;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class Slider : OsuHitObject, IHasPathWithRepeats
    {
        public double EndTime => StartTime + this.SpanCount() * Path.Distance / Velocity;

        [JsonIgnore]
        public double Duration
        {
            get => EndTime - StartTime;
            set => throw new System.NotSupportedException($"Adjust via {nameof(RepeatCount)} instead"); // can be implemented if/when needed.
        }

        private readonly Cached<Vector2> endPositionCache = new Cached<Vector2>();

        public override Vector2 EndPosition => endPositionCache.IsValid ? endPositionCache.Value : endPositionCache.Value = Position + this.CurvePositionAt(1);

        public Vector2 StackedPositionAt(double t) => StackedPosition + this.CurvePositionAt(t);

        private readonly SliderPath path = new SliderPath();

        public SliderPath Path
        {
            get => path;
            set
            {
                path.ControlPoints.Clear();
                path.ExpectedDistance.Value = null;

                if (value != null)
                {
                    path.ControlPoints.AddRange(value.ControlPoints.Select(c => new PathControlPoint(c.Position.Value, c.Type.Value)));
                    path.ExpectedDistance.Value = value.ExpectedDistance.Value;
                }
            }
        }

        public double Distance => Path.Distance;

        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                updateNestedPositions();
            }
        }

        public double? LegacyLastTickOffset { get; set; }

        public List<IList<HitSampleInfo>> NodeSamples { get; set; } = new List<IList<HitSampleInfo>>();

        private int repeatCount;

        public int RepeatCount
        {
            get => repeatCount;
            set
            {
                repeatCount = value;
                updateNestedPositions();
            }
        }

        /// <summary>
        /// The length of one span of this <see cref="Slider"/>.
        /// </summary>
        public double SpanDuration => Duration / this.SpanCount();

        /// <summary>
        /// Velocity of this <see cref="Slider"/>.
        /// </summary>
        public double Velocity { get; private set; }

        /// <summary>
        /// Spacing between <see cref="SliderTick"/>s of this <see cref="Slider"/>.
        /// </summary>
        public double TickDistance { get; private set; }

        /// <summary>
        /// An extra multiplier that affects the number of <see cref="SliderTick"/>s generated by this <see cref="Slider"/>.
        /// An increase in this value increases <see cref="TickDistance"/>, which reduces the number of ticks generated.
        /// </summary>
        public double TickDistanceMultiplier = 1;

        [JsonIgnore]
        public HitCircle HeadCircle { get; protected set; }

        [JsonIgnore]
        public SliderTailCircle TailCircle { get; protected set; }

        public Slider()
        {
            SamplesBindable.CollectionChanged += (_, __) => updateNestedSamples();
            Path.Version.ValueChanged += _ => updateNestedPositions();
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(StartTime);

            double scoringDistance = BASE_SCORING_DISTANCE * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            Velocity = scoringDistance / timingPoint.BeatLength;
            TickDistance = scoringDistance / difficulty.SliderTickRate * TickDistanceMultiplier;

            // The samples should be attached to the slider tail, however this can only be done after LegacyLastTick is removed otherwise they would play earlier than they're intended to.
            // For now, the samples are attached to and played by the slider itself at the correct end time.
            Samples = this.GetNodeSamples(repeatCount + 1);
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            foreach (var e in
                SliderEventGenerator.Generate(StartTime, SpanDuration, Velocity, TickDistance, Path.Distance, this.SpanCount(), LegacyLastTickOffset, cancellationToken))
            {
                switch (e.Type)
                {
                    case SliderEventType.Tick:
                        AddNested(new SliderTick
                        {
                            SpanIndex = e.SpanIndex,
                            SpanStartTime = e.SpanStartTime,
                            StartTime = e.Time,
                            Position = Position + Path.PositionAt(e.PathProgress),
                            StackHeight = StackHeight,
                            Scale = Scale,
                        });
                        break;

                    case SliderEventType.Head:
                        AddNested(HeadCircle = new SliderHeadCircle
                        {
                            StartTime = e.Time,
                            Position = Position,
                            StackHeight = StackHeight,
                            SampleControlPoint = SampleControlPoint,
                        });
                        break;

                    case SliderEventType.LegacyLastTick:
                        // we need to use the LegacyLastTick here for compatibility reasons (difficulty).
                        // it is *okay* to use this because the TailCircle is not used for any meaningful purpose in gameplay.
                        // if this is to change, we should revisit this.
                        AddNested(TailCircle = new SliderTailCircle(this)
                        {
                            RepeatIndex = e.SpanIndex,
                            StartTime = e.Time,
                            Position = EndPosition,
                            StackHeight = StackHeight
                        });
                        break;

                    case SliderEventType.Repeat:
                        AddNested(new SliderRepeat(this)
                        {
                            RepeatIndex = e.SpanIndex,
                            StartTime = StartTime + (e.SpanIndex + 1) * SpanDuration,
                            Position = Position + Path.PositionAt(e.PathProgress),
                            StackHeight = StackHeight,
                            Scale = Scale,
                        });
                        break;
                }
            }

            updateNestedSamples();
        }

        private void updateNestedPositions()
        {
            endPositionCache.Invalidate();

            if (HeadCircle != null)
                HeadCircle.Position = Position;

            if (TailCircle != null)
                TailCircle.Position = EndPosition;
        }

        private void updateNestedSamples()
        {
            var firstSample = Samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL)
                              ?? Samples.FirstOrDefault(); // TODO: remove this when guaranteed sort is present for samples (https://github.com/ppy/osu/issues/1933)
            var sampleList = new List<HitSampleInfo>();

            if (firstSample != null)
            {
                sampleList.Add(new HitSampleInfo
                {
                    Bank = firstSample.Bank,
                    Volume = firstSample.Volume,
                    Name = @"slidertick",
                });
            }

            foreach (var tick in NestedHitObjects.OfType<SliderTick>())
                tick.Samples = sampleList;

            foreach (var repeat in NestedHitObjects.OfType<SliderRepeat>())
                repeat.Samples = this.GetNodeSamples(repeat.RepeatIndex + 1);

            if (HeadCircle != null)
                HeadCircle.Samples = this.GetNodeSamples(0);
        }

        public override Judgement CreateJudgement() => new OsuIgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}

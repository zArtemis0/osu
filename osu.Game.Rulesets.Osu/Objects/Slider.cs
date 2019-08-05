﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class Slider : OsuHitObject, IHasCurve
    {
        /// <summary>
        /// Scoring distance with a speed-adjusted beat length of 1 second.
        /// </summary>
        private const float base_scoring_distance = 100;

        public double EndTime => StartTime + this.SpanCount() * Path.Distance / Velocity;
        public double Duration => EndTime - StartTime;

        private Cached<Vector2> endPositionCache;

        public override Vector2 EndPosition => endPositionCache.IsValid ? endPositionCache.Value : endPositionCache.Value = Position + this.CurvePositionAt(1);

        public Vector2 StackedPositionAt(double t) => StackedPosition + this.CurvePositionAt(t);

        /// <summary>
        /// Invoked when a change in this <see cref="Slider"/> occurs that requires all <see cref="SliderTick"/>s to be recalculated.
        /// </summary>
        public Action OnTicksRegenerated;

        public override int ComboIndex
        {
            get => base.ComboIndex;
            set
            {
                base.ComboIndex = value;
                foreach (var n in NestedHitObjects.OfType<IHasComboInformation>())
                    n.ComboIndex = value;
            }
        }

        public override int IndexInCurrentCombo
        {
            get => base.IndexInCurrentCombo;
            set
            {
                base.IndexInCurrentCombo = value;
                foreach (var n in NestedHitObjects.OfType<IHasComboInformation>())
                    n.IndexInCurrentCombo = value;
            }
        }

        public readonly Bindable<SliderPath> PathBindable = new Bindable<SliderPath>();

        public SliderPath Path
        {
            get => PathBindable.Value;
            set
            {
                PathBindable.Value = value;

                foreach (var tick in NestedHitObjects.OfType<SliderTick>())
                    RemoveNested(tick);

                foreach (var e in
                    SliderEventGenerator.Generate(StartTime, SpanDuration, Velocity, TickDistance, Path.Distance, this.SpanCount(), LegacyLastTickOffset))
                {
                    if (e.Type == SliderEventType.Tick)
                        addNestedTick(e);
                }

                OnTicksRegenerated?.Invoke();

                endPositionCache.Invalidate();
            }
        }

        public double Distance => Path.Distance;

        public override Vector2 Position
        {
            get => base.Position;
            set
            {
                base.Position = value;

                if (HeadCircle != null)
                    HeadCircle.Position = value;

                if (TailCircle != null)
                    TailCircle.Position = EndPosition;

                endPositionCache.Invalidate();
            }
        }

        public double? LegacyLastTickOffset { get; set; }

        /// <summary>
        /// The position of the cursor at the point of completion of this <see cref="Slider"/> if it was hit
        /// with as few movements as possible. This is set and used by difficulty calculation.
        /// </summary>
        internal Vector2? LazyEndPosition;

        /// <summary>
        /// The distance travelled by the cursor upon completion of this <see cref="Slider"/> if it was hit
        /// with as few movements as possible. This is set and used by difficulty calculation.
        /// </summary>
        internal float LazyTravelDistance;

        public List<List<HitSampleInfo>> NodeSamples { get; set; } = new List<List<HitSampleInfo>>();

        private int repeatCount;

        public int RepeatCount
        {
            get => repeatCount;
            set
            {
                repeatCount = value;
                endPositionCache.Invalidate();
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

        public HitCircle HeadCircle;
        public SliderTailCircle TailCircle;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(StartTime);

            double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            Velocity = scoringDistance / timingPoint.BeatLength;
            TickDistance = scoringDistance / difficulty.SliderTickRate * TickDistanceMultiplier;
        }

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            foreach (var e in
                SliderEventGenerator.Generate(StartTime, SpanDuration, Velocity, TickDistance, Path.Distance, this.SpanCount(), LegacyLastTickOffset))
            {
                switch (e.Type)
                {
                    case SliderEventType.Tick:
                        addNestedTick(e);
                        break;

                    case SliderEventType.Head:
                        AddNested(HeadCircle = new SliderCircle
                        {
                            StartTime = e.Time,
                            Position = Position,
                            Samples = getNodeSamples(0),
                            SampleControlPoint = SampleControlPoint,
                            IndexInCurrentCombo = IndexInCurrentCombo,
                            ComboIndex = ComboIndex,
                        });
                        break;

                    case SliderEventType.LegacyLastTick:
                        // we need to use the LegacyLastTick here for compatibility reasons (difficulty).
                        // it is *okay* to use this because the TailCircle is not used for any meaningful purpose in gameplay.
                        // if this is to change, we should revisit this.
                        AddNested(TailCircle = new SliderTailCircle(this)
                        {
                            StartTime = e.Time,
                            Position = EndPosition,
                            IndexInCurrentCombo = IndexInCurrentCombo,
                            ComboIndex = ComboIndex,
                        });
                        break;

                    case SliderEventType.Repeat:
                        AddNested(new RepeatPoint
                        {
                            RepeatIndex = e.SpanIndex,
                            SpanDuration = SpanDuration,
                            StartTime = StartTime + (e.SpanIndex + 1) * SpanDuration,
                            Position = Position + Path.PositionAt(e.PathProgress),
                            StackHeight = StackHeight,
                            Scale = Scale,
                            Samples = getNodeSamples(e.SpanIndex + 1)
                        });
                        break;
                }
            }
        }

        private void addNestedTick(SliderEventDescriptor eventDescriptor)
        {
            var firstSample = Samples.Find(s => s.Name == HitSampleInfo.HIT_NORMAL)
                              ?? Samples.FirstOrDefault(); // TODO: remove this when guaranteed sort is present for samples (https://github.com/ppy/osu/issues/1933)
            var sampleList = new List<HitSampleInfo>();

            if (firstSample != null)
                sampleList.Add(new HitSampleInfo
                {
                    Bank = firstSample.Bank,
                    Volume = firstSample.Volume,
                    Name = @"slidertick",
                });

            AddNested(new SliderTick
            {
                SpanIndex = eventDescriptor.SpanIndex,
                SpanStartTime = eventDescriptor.SpanStartTime,
                StartTime = eventDescriptor.Time,
                Position = Position + Path.PositionAt(eventDescriptor.PathProgress),
                StackHeight = StackHeight,
                Scale = Scale,
                Samples = sampleList
            });
        }

        private List<HitSampleInfo> getNodeSamples(int nodeIndex) =>
            nodeIndex < NodeSamples.Count ? NodeSamples[nodeIndex] : Samples;

        public override Judgement CreateJudgement() => new OsuJudgement();
    }
}

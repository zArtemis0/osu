﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class JuiceStream : CatchHitObject, IHasPathWithRepeats, IHasSliderVelocity
    {
        /// <summary>
        /// Positional distance that results in a duration of one second, before any speed adjustments.
        /// </summary>
        private const float base_scoring_distance = 100;

        public override JudgementCriteria CreateJudgement() => new IgnoreJudgementCriteria();

        public int RepeatCount { get; set; }

        public BindableNumber<double> SliderVelocityMultiplierBindable { get; } = new BindableDouble(1)
        {
            Precision = 0.01,
            MinValue = 0.1,
            MaxValue = 10
        };

        public double SliderVelocityMultiplier
        {
            get => SliderVelocityMultiplierBindable.Value;
            set => SliderVelocityMultiplierBindable.Value = value;
        }

        /// <summary>
        /// An extra multiplier that affects the number of <see cref="Droplet"/>s generated by this <see cref="JuiceStream"/>.
        /// An increase in this value increases <see cref="TickDistance"/>, which reduces the number of ticks generated.
        /// </summary>
        public double TickDistanceMultiplier = 1;

        [JsonIgnore]
        private double velocityFactor;

        [JsonIgnore]
        private double tickDistanceFactor;

        [JsonIgnore]
        public double Velocity => velocityFactor * SliderVelocityMultiplier;

        [JsonIgnore]
        public double TickDistance => tickDistanceFactor * TickDistanceMultiplier;

        /// <summary>
        /// The length of one span of this <see cref="JuiceStream"/>.
        /// </summary>
        public double SpanDuration => Duration / this.SpanCount();

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);

            velocityFactor = base_scoring_distance * difficulty.SliderMultiplier / timingPoint.BeatLength;
            tickDistanceFactor = base_scoring_distance * difficulty.SliderMultiplier / difficulty.SliderTickRate;
        }

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            var dropletSamples = Samples.Select(s => s.With(@"slidertick")).ToList();

            int nodeIndex = 0;
            SliderEventDescriptor? lastEvent = null;

            foreach (var e in SliderEventGenerator.Generate(StartTime, SpanDuration, Velocity, TickDistance, Path.Distance, this.SpanCount(), cancellationToken))
            {
                // generate tiny droplets since the last point
                if (lastEvent != null)
                {
                    double sinceLastTick = (int)e.Time - (int)lastEvent.Value.Time;

                    if (sinceLastTick > 80)
                    {
                        double timeBetweenTiny = sinceLastTick;
                        while (timeBetweenTiny > 100)
                            timeBetweenTiny /= 2;

                        for (double t = timeBetweenTiny; t < sinceLastTick; t += timeBetweenTiny)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            AddNested(new TinyDroplet
                            {
                                StartTime = t + lastEvent.Value.Time,
                                X = EffectiveX + Path.PositionAt(lastEvent.Value.PathProgress + (t / sinceLastTick) * (e.PathProgress - lastEvent.Value.PathProgress)).X,
                            });
                        }
                    }
                }

                // this also includes LastTick and this is used for TinyDroplet generation above.
                // this means that the final segment of TinyDroplets are increasingly mistimed where LastTick is being applied.
                lastEvent = e;

                switch (e.Type)
                {
                    case SliderEventType.Tick:
                        AddNested(new Droplet
                        {
                            Samples = dropletSamples,
                            StartTime = e.Time,
                            X = EffectiveX + Path.PositionAt(e.PathProgress).X,
                        });
                        break;

                    case SliderEventType.Head:
                    case SliderEventType.Tail:
                    case SliderEventType.Repeat:
                        AddNested(new Fruit
                        {
                            Samples = this.GetNodeSamples(nodeIndex++),
                            StartTime = e.Time,
                            X = EffectiveX + Path.PositionAt(e.PathProgress).X,
                        });
                        break;
                }
            }
        }

        public float EndX => EffectiveX + this.CurvePositionAt(1).X;

        [JsonIgnore]
        public double Duration
        {
            get => this.SpanCount() * Path.Distance / Velocity;
            set => throw new NotSupportedException($"Adjust via {nameof(RepeatCount)} instead"); // can be implemented if/when needed.
        }

        public double EndTime => StartTime + Duration;

        private readonly SliderPath path = new SliderPath();

        public SliderPath Path
        {
            get => path;
            set
            {
                path.ControlPoints.Clear();
                path.ControlPoints.AddRange(value.ControlPoints.Select(c => new PathControlPoint(c.Position, c.Type)));
                path.ExpectedDistance.Value = value.ExpectedDistance.Value;
            }
        }

        public double Distance => Path.Distance;

        public IList<IList<HitSampleInfo>> NodeSamples { get; set; } = new List<IList<HitSampleInfo>>();
    }
}

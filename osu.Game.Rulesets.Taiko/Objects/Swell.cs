﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;
using System;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class Swell : TaikoHitObject, IHasDuration
    {
        public double EndTime
        {
            get => StartTime + Duration;
            set => Duration = value - StartTime;
        }

        public double Duration { get; set; }

        /// <summary>
        /// The number of hits required to complete the swell successfully.
        /// </summary>
        public int RequiredHits = 10;

        protected override void CreateNestedHitObjects(CancellationToken cancellationToken)
        {
            base.CreateNestedHitObjects(cancellationToken);

            for (int i = 0; i < RequiredHits; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                AddNested(new SwellTick
                {
                    Samples = Samples
                });
            }
        }

        public override Judgement CreateJudgement() => new TaikoSwellJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        protected override void CopyFrom(HitObject other, IDictionary<object, object>? referenceLookup = null)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not Swell swell)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(Swell)}");

            Duration = swell.Duration;
            RequiredHits = swell.RequiredHits;
        }

        protected override HitObject CreateInstance() => new Swell();
    }
}

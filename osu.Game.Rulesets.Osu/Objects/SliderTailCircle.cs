﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SliderTailCircle : SliderEndCircle
    {
        /// <summary>
        /// Whether to treat this <see cref="SliderHeadCircle"/> as a normal <see cref="HitCircle"/> for judgement purposes.
        /// If <c>false</c>, this <see cref="SliderHeadCircle"/> will be judged as a <see cref="SliderTick"/> instead.
        /// </summary>
        public bool ClassicSliderBehaviour;

        public SliderTailCircle(Slider slider)
            : base(slider)
        {
        }

        public override JudgementCriteria CreateJudgement() => ClassicSliderBehaviour ? new LegacyTailJudgementCriteria() : new TailJudgementCriteria();

        public class LegacyTailJudgementCriteria : OsuJudgementCriteria
        {
            public override HitResult MaxResult => HitResult.SmallTickHit;
        }

        public class TailJudgementCriteria : SliderEndJudgementCriteria
        {
            public override HitResult MaxResult => HitResult.SliderTailHit;
            public override HitResult MinResult => HitResult.IgnoreMiss;
        }
    }
}

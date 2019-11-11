﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoDrumRollTickJudgement : TaikoJudgement
    {
        public override bool AffectsCombo => false;

        protected override int NumericResultFor(HitResult result)
            => result switch
            {
                HitResult.Great => 200,
                _ => 0,
            };

        protected override double HealthIncreaseFor(HitResult result)
            => result switch
            {
                HitResult.Great => 0.15,
                _ => 0,
            };
    }
}

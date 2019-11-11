﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuHitWindows : HitWindows
    {
        private static readonly DifficultyRange[] osu_ranges =
        {
            new DifficultyRange(HitResult.Great, 80, 50, 20),
            new DifficultyRange(HitResult.Good, 140, 100, 60),
            new DifficultyRange(HitResult.Meh, 200, 150, 100),
            new DifficultyRange(HitResult.Miss, 200, 200, 200),
        };

        public override bool IsHitResultAllowed(HitResult result)
            => result switch
            {
                HitResult.Great => true,
                HitResult.Good => true,
                HitResult.Meh => true,
                HitResult.Miss => true,
                _ => false,
            };

        protected override DifficultyRange[] GetRanges() => osu_ranges;
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Pre
{
    public class SpeedBonus : PreStrainSkill
    {
        private const double single_spacing_threshold = 125;
        private const double min_speed_bonus = 75; // ~200BPM
        private const double speed_balancing_factor = 40;

        private SpeedStrainTime speedStrainTime;

        public SpeedBonus(Mod[] mods, SpeedStrainTime speedStrainTime) : base(mods)
        {
            this.speedStrainTime = speedStrainTime;
        }

        protected override double SkillMultiplier => 1.0;

        protected override double StrainDecayBase => 0.0;

        protected override double StrainValueOf(int index, DifficultyHitObject current)
        {
            var osuCurrObj = (OsuDifficultyHitObject)current;

            double strainTime = speedStrainTime[index];

            double speedBonus = 1.0;

            if (strainTime < min_speed_bonus)
                speedBonus = 1 + 0.75 * Math.Pow((min_speed_bonus - strainTime) / speed_balancing_factor, 2);

            double distance = Math.Min(single_spacing_threshold, osuCurrObj.TravelDistance + osuCurrObj.JumpDistance);

            return (speedBonus + speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5)) / strainTime;
        }
    }
}

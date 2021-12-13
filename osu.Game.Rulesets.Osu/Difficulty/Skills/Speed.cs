﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Skills.Pre;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : OsuStrainSkill
    {
        private double skillMultiplier => 1375;
        private double strainDecayBase => 0.3;
        protected override int ReducedSectionCount => 5;
        protected override double DifficultyMultiplier => 1.04;

        private double currentStrain;

        private double currentRhythm;

        private readonly SpeedRhythmBonus speedRhythmBonus;

        private readonly SpeedStrainTime speedStrainTime;

        private readonly SpeedBonus speedBonus;

        public Speed(Mod[] mods, double greatWindow)
            : base(mods)
        {
            speedRhythmBonus = new SpeedRhythmBonus(mods, greatWindow);
            speedStrainTime = new SpeedStrainTime(mods, greatWindow);
            speedBonus = new SpeedBonus(mods, speedStrainTime);
        }

        private double strainValueOf(DifficultyHitObject current)
        {
            speedStrainTime.ProcessInternal(current);
            speedBonus.ProcessInternal(current);

            return speedBonus.GetCurrentStrain();
        }

        private double strainDecay(double ms) => Math.Pow(strainDecayBase, ms / 1000);

        protected override double CalculateInitialStrain(double time) => (currentStrain * currentRhythm) * strainDecay(time - Previous[0].StartTime);

        protected override double StrainValueAt(DifficultyHitObject current)
        {
            speedRhythmBonus.ProcessInternal(current);

            currentStrain *= strainDecay(current.DeltaTime);
            currentStrain += strainValueOf(current) * skillMultiplier;

            currentRhythm = speedRhythmBonus.GetCurrentStrain();

            return currentStrain * currentRhythm;
        }
    }
}

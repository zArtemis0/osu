﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using System.Linq;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public abstract class OsuStrainSkill : Skill
    {
        /// <summary>
        /// The number of sections with the highest strains, which the peak strain reductions will apply to.
        /// This is done in order to decrease their impact on the overall difficulty of the map for this skill.
        /// </summary>
        protected virtual int ReducedSectionCount => 10;

        /// <summary>
        /// The baseline multiplier applied to the section with the biggest strain.
        /// </summary>
        protected virtual double ReducedStrainBaseline => 0.75;

        /// <summary>
        /// The final multiplier to be applied to <see cref="DifficultyValue"/> after all other calculations.
        /// </summary>
        protected virtual double DifficultyMultiplier => 1.06;

        protected virtual double DifficultySumWeight => 0.9;

        protected DecayingStrainPeaks Strain;

        protected OsuStrainSkill(Mod[] mods, double strainDecayBase, int sectionLength = 400)
            : base(mods)
        {
            Strain = new DecayingStrainPeaks(strainDecayBase, sectionLength);
        }

        protected double IncrementStrainAtTime(double time, double strainIncrement)
        {
            return Strain.IncrementStrainAtTime(time, strainIncrement);
        }

        public override double DifficultyValue()
        {
            List<double> strains = Strain.StrainPeaks.OrderByDescending(d => d).ToList();

            // We are reducing the highest strains first to account for extreme difficulty spikes
            for (int i = 0; i < Math.Min(strains.Count, ReducedSectionCount); i++)
            {
                double scale = Math.Log10(Interpolation.Lerp(1, 10, Math.Clamp((float)i / ReducedSectionCount, 0, 1)));
                strains[i] *= Interpolation.Lerp(ReducedStrainBaseline, 1.0, scale);
            }

            return strains.SortedExponentialWeightedSum(DifficultySumWeight) * DifficultyMultiplier;
        }
    }
}

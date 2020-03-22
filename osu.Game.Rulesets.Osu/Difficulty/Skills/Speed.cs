﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using static System.Math;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    /// <summary>
    /// Represents the skill required to press keys with regards to keeping up with the speed at which objects need to be hit.
    /// </summary>
    public class Speed : Skill
    {
        private const double single_spacing_threshold = 125;

        private const double angle_bonus_begin = 5 * Math.PI / 6;
        private const double pi_over_4 = Math.PI / 4;
        private const double pi_over_2 = Math.PI / 2;

        private const double streamaimconst = 2.42;

        //  private const double stdevconst = 0.149820;

        private int sdsplitcounter = 0;
        // private int runcheck1 = 0;

        private double sdstrainmult = 0;
        // private double sdstrainmult2;

        public readonly List<double> JumpDistances = new List<double>();
        public readonly List<double> StrainTimes = new List<double>();
        public List<double> JumpDistances2 = new List<double>();

        protected override double SkillMultiplier => 1400;
        protected override double StrainDecayBase => 0.3;

        private const double min_speed_bonus = 75; // ~200BPM
        private const double max_speed_bonus = 45; // ~330BPM
        private const double speed_balancing_factor = 40;

        private double calculateStandardDeviation(IEnumerable<double> values)
        {
            double standardDeviation = 0;

            if (values.Any())
            {
                // Compute the average.     
                double avg = values.Average();

                // Perform the Sum of (value-avg)_2_2.      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));

                // Put it all together.      
                standardDeviation = Math.Sqrt((sum) / (values.Count()));
            }

            return standardDeviation;
        }
        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            double sectionvelocity = osuCurrent.JumpDistance / osuCurrent.StrainTime;

            if (osuCurrent.JumpDistance < 150 && sectionvelocity < streamaimconst && Previous.Count > 0 && osuCurrent.Angle != null && osuCurrent.Angle.Value >= Math.PI / 2 && osuCurrent.StrainTime < 75)
            {

                JumpDistances2.Add(sectionvelocity);

                StrainTimes.Add(osuCurrent.StrainTime);

                sdsplitcounter++;

                if (JumpDistances2.Count > 1)
                {
                    sdstrainmult = calculateStandardDeviation(JumpDistances2);
                    sdstrainmult *= Log(osuCurrent.StrainTime, 2);
                    sdstrainmult = Pow(1.2, sdstrainmult);
                    //    JumpDistances.Add(sdstrainmult);
                }
            }
            else
            {
                if (sdsplitcounter > 0)
                {
                    JumpDistances2.Clear();
                    JumpDistances2.TrimExcess();
                    sdsplitcounter = 0;
                }

            }

            // if (osuCurrent.LastObject != null)
            // {
            //     sdstrainmult = calculateStandardDeviation(JumpDistances);
            //     runcheck1++;
            // }

            // Console.WriteLine(runcheck1);

            double distance = Math.Min(single_spacing_threshold, osuCurrent.TravelDistance + osuCurrent.JumpDistance);
            double deltaTime = Math.Max(max_speed_bonus, current.DeltaTime);

            double speedBonus = 1.0;
            if (deltaTime < min_speed_bonus)
                speedBonus = 1 + Math.Pow((min_speed_bonus - deltaTime) / speed_balancing_factor, 2);

            double angleBonus = 1.0;

            if (osuCurrent.Angle != null && osuCurrent.Angle.Value < angle_bonus_begin)
            {
                angleBonus = 1 + Math.Pow(Math.Sin(1.5 * (angle_bonus_begin - osuCurrent.Angle.Value)), 2) / 3.57;

                if (osuCurrent.Angle.Value < pi_over_2)
                {
                    angleBonus = 1.28;
                    if (distance < 90 && osuCurrent.Angle.Value < pi_over_4)
                        angleBonus += (1 - angleBonus) * Math.Min((90 - distance) / 10, 1);
                    else if (distance < 90)
                        angleBonus += (1 - angleBonus) * Math.Min((90 - distance) / 10, 1) * Math.Sin((pi_over_2 - osuCurrent.Angle.Value) / pi_over_4);
                }
            }

            return ((1 + (speedBonus - 1) * 0.75) * angleBonus * (0.95 + speedBonus * Math.Pow(distance / single_spacing_threshold, 3.5)) / osuCurrent.StrainTime) * Math.Max(1.0, sdstrainmult);
        }
    }
}

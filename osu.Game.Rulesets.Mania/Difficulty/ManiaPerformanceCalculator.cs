﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using MathNet.Numerics;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    public class ManiaPerformanceCalculator : PerformanceCalculator
    {
        private int countPerfect;
        private int countGreat;
        private int countGood;
        private int countOk;
        private int countMeh;
        private int countMiss;
        private double estimatedUR;

        public ManiaPerformanceCalculator()
            : base(new ManiaRuleset())
        {
        }

        protected override PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes)
        {
            var maniaAttributes = (ManiaDifficultyAttributes)attributes;

            countPerfect = score.Statistics.GetValueOrDefault(HitResult.Perfect);
            countGreat = score.Statistics.GetValueOrDefault(HitResult.Great);
            countGood = score.Statistics.GetValueOrDefault(HitResult.Good);
            countOk = score.Statistics.GetValueOrDefault(HitResult.Ok);
            countMeh = score.Statistics.GetValueOrDefault(HitResult.Meh);
            countMiss = score.Statistics.GetValueOrDefault(HitResult.Miss);
            estimatedUR = computeEstimatedUR(score, maniaAttributes) * 10;

            // Arbitrary initial value for scaling pp in order to standardize distributions across game modes.
            // The specific number has no intrinsic meaning and can be adjusted as needed.
            double multiplier = 8.0;

            if (score.Mods.Any(m => m is ModNoFail))
                multiplier *= 0.75;
            if (score.Mods.Any(m => m is ModEasy))
                multiplier *= 0.5;

            double difficultyValue = computeDifficultyValue(maniaAttributes);
            double totalValue = difficultyValue * multiplier;

            return new ManiaPerformanceAttributes
            {
                Difficulty = difficultyValue,
                Total = totalValue,
                EstimatedUR = estimatedUR
            };
        }

        private double computeDifficultyValue(ManiaDifficultyAttributes attributes)
        {
            double difficultyValue = Math.Pow(Math.Max(attributes.StarRating - 0.15, 0.05), 2.2); // Star rating to pp curve

            difficultyValue *= Math.Max(1.2 * Math.Pow(SpecialFunctions.Erf(300 / estimatedUR), 1.6) - 0.2, 0);

            return difficultyValue;
        }

        private double totalHits => countPerfect + countOk + countGreat + countGood + countMeh + countMiss;
        private double totalSuccessfulHits => countPerfect + countOk + countGreat + countGood + countMeh;

        /// <summary>
        /// Accuracy used to weight judgements independently from the score's actual accuracy.
        /// </summary>
        private double computeEstimatedUR(ScoreInfo score, ManiaDifficultyAttributes attributes)
        {
            if (totalSuccessfulHits == 0)
                return Double.PositiveInfinity;

            double overallDifficulty = attributes.OverallDifficulty;

            if (attributes.Convert)
                overallDifficulty = 10;

            double windowMultiplier = 1;

            if (score.Mods.Any(m => m is ModHardRock))
                windowMultiplier *= 1 / 1.4;
            else if (score.Mods.Any(m => m is ModEasy))
                windowMultiplier *= 1.4;

            // We need the size of every hit window in order to calculate deviation accurately.
            double hMax = Math.Floor(16 * windowMultiplier);
            double h300 = Math.Floor((64 - 3 * overallDifficulty) * windowMultiplier);
            double h200 = Math.Floor((97 - 3 * overallDifficulty) * windowMultiplier);
            double h100 = Math.Floor((127 - 3 * overallDifficulty) * windowMultiplier);
            double h50 = Math.Floor((151 - 3 * overallDifficulty) * windowMultiplier);

            double root2 = Math.Sqrt(2);

            // Returns the likelihood of any deviation resulting in the play
            double likelihoodGradient(double d)
            {
                if (d <= 0)
                    return double.PositiveInfinity;

                double pMax = 1 - erfcApprox(hMax / (d * root2));
                double p300 = erfcApprox(hMax / (d * root2)) - erfcApprox(h300 / (d * root2));
                double p200 = erfcApprox(h300 / (d * root2)) - erfcApprox(h200 / (d * root2));
                double p100 = erfcApprox(h200 / (d * root2)) - erfcApprox(h100 / (d * root2));
                double p50 = erfcApprox(h100 / (d * root2)) - erfcApprox(h50 / (d * root2));
                double p0 = erfcApprox(h50 / (d * root2));

                double gradient = Math.Pow(pMax, countPerfect / totalHits)
                * Math.Pow(p300, (countGreat + 0.5) / totalHits)
                * Math.Pow(p200, countGood / totalHits)
                * Math.Pow(p100, countOk / totalHits)
                * Math.Pow(p50, countMeh / totalHits)
                * Math.Pow(p0, countMiss / totalHits);

                return -gradient;
            }

            // Finding the minimum of the inverse likelihood function returns the most likely deviation for a play
            return FindMinimum.OfScalarFunction(likelihoodGradient, 30);
        }
 
        double erfcApprox(double x)
        {
            if (x <= 5)
                return SpecialFunctions.Erfc(x);

            // Approximation is most accurate with values over 5, and is much more performant than the Erfc function
            return Math.Pow(Math.E, -Math.Pow(x, 2) - Math.Log(x * Math.Sqrt(Math.PI)));
        } 
    }
}

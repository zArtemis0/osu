// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class CognitionEvaluator
    {
        private const double cognition_window_size = 3000;

        public static double EvaluateDifficultyOf(DifficultyHitObject current, bool hidden)
        {
            if (current.BaseObject is Spinner || current.Index == 0)
                return 0;

            var currObj = (OsuDifficultyHitObject)current;
            var prevObj = (OsuDifficultyHitObject)current.Previous(0);

            double currVelocity = currObj.LazyJumpDistance / currObj.StrainTime;

            // Maybe I should just pass in clockrate...
            var clockRateEstimate = current.BaseObject.StartTime / currObj.StartTime;

            List<OsuDifficultyHitObject> pastVisibleObjects = retrievePastVisibleObjects(currObj);

            // Rather than note density being the number of on-screen objects visible at the current object,
            // consider it as how many objects the current object has been visible for.
            double noteDensityDifficulty = 1.0;

            double pastObjectDifficultyInfluence = 1.0;

            foreach (var loopObj in pastVisibleObjects)
            {
                var prevLoopObj = loopObj.Previous(0) as OsuDifficultyHitObject;

                double loopDifficulty = currObj.OpacityAt(loopObj.BaseObject.StartTime, false);

                // Small distances means objects may be cheesed, so it doesn't matter whether they are arranged confusingly.
                loopDifficulty *= logistic((loopObj.MinimumJumpDistance - 90) / 15);

                double timeBetweenCurrAndLoopObj = (currObj.BaseObject.StartTime - loopObj.BaseObject.StartTime) / clockRateEstimate;
                loopDifficulty *= getTimeNerfFactor(timeBetweenCurrAndLoopObj);

                pastObjectDifficultyInfluence += loopDifficulty;
            }

            noteDensityDifficulty = Math.Pow(3 * Math.Log(Math.Max(1, pastObjectDifficultyInfluence - 1)), 2.3);
            noteDensityDifficulty *= getConstantAngleNerfFactor(currObj);

            double hiddenDifficulty = 0;

            if (hidden)
            {
                var timeSpentInvisible = getDurationSpentInvisible(currObj) / clockRateEstimate;
                var isRhythmChange = (currObj.StrainTime - prevObj.StrainTime < 5);

                var timeDifficultyFactor = 800 / pastObjectDifficultyInfluence;

                hiddenDifficulty += Math.Pow(7 * timeSpentInvisible / timeDifficultyFactor, 1);

                if (isRhythmChange)
                    hiddenDifficulty *= 1.1;

                hiddenDifficulty += 2 * currVelocity;
            }

            double difficulty = hiddenDifficulty + noteDensityDifficulty;

            // While there is slider leniency...
            if (currObj.BaseObject is Slider)
                difficulty *= 0.2;

            return difficulty;
        }

        // Returns a list of objects that are visible on screen at
        // the point in time at which the current object becomes visible.
        private static List<OsuDifficultyHitObject> retrievePastVisibleObjects(OsuDifficultyHitObject current)
        {
            List<OsuDifficultyHitObject> objects = new List<OsuDifficultyHitObject>();

            for (int i = 0; i < current.Index; i++)
            {
                OsuDifficultyHitObject loopObj = (OsuDifficultyHitObject)current.Previous(i);

                if (loopObj.IsNull() || current.StartTime - loopObj.StartTime > cognition_window_size)
                    break;

                objects.Add(loopObj);
            }

            return objects;
        }

        private static double getDurationSpentInvisible(OsuDifficultyHitObject current)
        {
            var baseObject = (OsuHitObject)current.BaseObject;

            double fadeOutStartTime = baseObject.StartTime - baseObject.TimePreempt + baseObject.TimeFadeIn;
            double fadeOutDuration = baseObject.TimePreempt * OsuModHidden.FADE_OUT_DURATION_MULTIPLIER;

            return (fadeOutStartTime + fadeOutDuration) - (baseObject.StartTime - baseObject.TimePreempt);
        }

        private static double getConstantAngleNerfFactor(OsuDifficultyHitObject current)
        {
            const double time_limit = 2000;
            const double time_limit_low = 500;

            double constantAngleCount = 0;

            int index = 0;
            double currentTimeGap = 0;

            while (currentTimeGap < time_limit)
            {
                var loopObj = (OsuDifficultyHitObject)current.Previous(index);

                if (loopObj.IsNull())
                    break;

                double longIntervalFactor = Math.Clamp(1 - (loopObj.StrainTime - time_limit_low) / (time_limit - time_limit_low), 0, 1);

                if (loopObj.Angle.IsNotNull() && current.Angle.IsNotNull())
                {
                    double angleDifference = Math.Abs(current.Angle.Value - loopObj.Angle.Value);

                    constantAngleCount += Math.Cos(4 * Math.Min(Math.PI / 8, angleDifference)) * longIntervalFactor;
                }

                currentTimeGap = current.StartTime - loopObj.StartTime;
                index++;
            }

            double difficulty = Math.Pow(Math.Min(1, 2 / constantAngleCount), 2);

            return difficulty;
        }

        private static double getTimeNerfFactor(double deltaTime)
        {
            return Math.Clamp(2 - (deltaTime / 1500), 0, 1);
        }

        private static double logistic(double x) => 1 / (1 + Math.Pow(Math.E, -x));
    }
}

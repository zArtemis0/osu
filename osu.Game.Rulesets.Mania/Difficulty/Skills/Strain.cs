// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Utils;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

/* The calculation of strain is sequential, following rules:
 * 1) The current strain value can only depend on past notes
 * 2) Notes on the same offset may see different strain values, as column order sorting is not guaranteed.
 * 3) The first hit object is omitted as it acts as a reference for the following note.
 *
 * E.g. A:(100ms @ Col 1), B:(200ms @ Col 1), C:(200ms @ Col 2)
 * The following sequences are possible: B C, C B.
 * A is omitted because of rule 3.
 */

/* Previous Note States
 * When calculating strain by its history, there's these possible states.
 *
 * The last row of each cell shows the current note
 * the 2nd last              shows the previous note
 *
 * The column describes where the previous note's end time is.
 * E.g. E3 states that the previous note's end time is on the body (of the current note)
 *
 * Invalid/Impossible states are marked with X
 * These states are not possible as their head is AFTER our current offset.
 *
 * E.g. D2 implies that our current note is a Hold. The previous note is a note, on the same offset.
 *
 *                 +-------------+-------------+-------------+-------------+--------------+
 *                 | Before Head | On Head     | On Body     |  On Tail    | After Tail   |
 *                 | (1)         | (2)         | (3)         |  (4)        | (5)          |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Note      (A) | (A1)        | (A2)        |             |             |              |
 * | Before        | O           |      O      |      X      |      X      |      X       |
 * | Note          |      O      |      O      |             |             |              |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Long Note (B) | (B1)        | (B2)        |             |             | (B5)         |
 * | Before        | [==]        | [====]      |      X      |      X      | [==========] |
 * | Note          |      O      |      O      |             |             |      O       |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Long Note (C) |             |             |             |             | (C5)         |
 * | Before        |      X      |      x      |      X      |      X      |      [===]   |
 * | Note          |             |             |             |             |      O       |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Note      (D) | (D1)        | (D2)        |             |             |              |
 * | Before        | O           |      O      |      X      |      X      |      X       |
 * | Long Note     |      [===]  |      [===]  |             |             |              |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Long Note (E) | (E1)        | (E2)        | (E3)        | (E4)        | (E5)         |
 * | Before        | [==]        | [====]      | [======]    | [=========] | [==========] |
 * | Long Note     |      [===]  |      [===]  |      [===]  |       [===] |      [===]   |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 * | Long Note (F) |             |             | (F3)        | (F4)        | (F5)         |
 * | Before        |      X      |      X      |      [=]    |       [===] |      [=====] |
 * | Long Note     |             |             |      [===]  |       [===] |      [===]   |
 * +---------------+-------------+-------------+-------------+-------------+--------------+
 *
 */

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Strain : StrainDecaySkill
    {
        /// <summary>
        /// Individual Strain Decay Exponent Base. Used in <see cref="applyDecay"/>
        /// </summary>
        private const double decay_base = 0.125;

        /// <summary>
        /// Global Strain Decay Exponent Base. Used in <see cref="applyDecay"/>
        /// </summary>
        private const double global_decay_base = 0.30;

        /// <summary>
        /// Center of our On Body Bias sigmoid function.
        /// </summary>
        private const double release_threshold = 24;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 1;

        /// <summary>
        /// Previous notes' start times. Indices correspond to columns
        /// </summary>
        private readonly double[] prevStartTimes;

        /// <summary>
        /// Previous notes' end times. Indices correspond to columns
        /// </summary>
        private readonly double[] prevEndTimes;

        /// <summary>
        /// Column note strains. Indices correspond to columns.
        /// The semantic meaning is the "strain" of each column.
        /// </summary>
        private readonly double[] columnStrains;

        /// <summary>
        /// Previous Strain processed.
        /// </summary>
        private double prevColumnStrain;

        /// <summary>
        /// Current Global Strain.
        /// The semantic meaning is the "strain" of all fingers.
        /// </summary>
        private double globalStrain;

        public Strain(Mod[] mods, int totalColumns)
            : base(mods)
        {
            prevStartTimes = new double[totalColumns];
            prevEndTimes = new double[totalColumns];
            prevStrains = new double[totalColumns];
            globalStrain = 1;
        }

        /// <summary>
        /// Calculates the strain value of a <see cref="DifficultyHitObject"/>. This value is affected by previously processed objects.
        ///
        /// <param name="current">Current Hit Object to evaluate strain</param>
        /// <remarks>
        /// 1) The first hitObject is not considered in the calculation.
        /// <see cref="ManiaDifficultyCalculator.CreateDifficultyHitObjects"/>
        /// 2) Strain is binned/bucketed by sections by Section Length, with a Max aggregation
        /// <see cref="StrainSkill.SectionLength"/>
        /// <see cref="StrainSkill.Process"/>
        /// </remarks>
        /// </summary>
        protected override double StrainValueOf(DifficultyHitObject current)
        {
            var hitObject = (ManiaDifficultyHitObject)current;

            // Given a note, startTime == endTime.
            double startTime = hitObject.StartTime;
            double endTime = hitObject.EndTime;
            int column = hitObject.BaseObject.Column;

            double holdLength = Math.Abs(endTime - startTime);
            double endOnBodyBias = 0; // Addition to the current note in case it's a hold and has to be released awkwardly
            double endAfterTailWeight = 1.0; // Factor to all additional strains in case something else is held

            // The closest end time, currently, is the current note's end time, which is its length
            double closestEndTime = holdLength;

            bool isEndOnBody = false;
            bool isEndAfterTail = false;

            for (int i = 0; i < prevEndTimes.Length; ++i)
            {
                /* True for Column 3 Scenarios:
                 *      Criterion 1 accepts Col 3-5
                 *      Criterion 2 accepts Col 1, D2:F3,
                 *      Thus, AND accepts Col 3 Only.
                 */
                isEndOnBody |= Precision.DefinitelyBigger(prevEndTimes[i], startTime, 1) &&
                               Precision.DefinitelyBigger(endTime, prevEndTimes[i], 1);

                // True for Column 5 Scenarios
                isEndAfterTail |= Precision.DefinitelyBigger(prevEndTimes[i], endTime, 1);

                // Update closest end time by looking through previous LNs
                closestEndTime = Math.Min(closestEndTime, Math.Abs(endTime - prevEndTimes[i]));
            }

            /* Give Hold Addition for Scenario Column 3.
             * Releasing multiple notes is as easy as releasing one.
             * Halves hold addition if closest release is release_threshold away.
             *
             * End on Body Bias
             *     ^
             * 1.0 + - - - - - -+-----------
             *     |           /
             * 0.5 + - - - - -/   Sigmoid Curve
             *     |         /|
             * 0.0 +--------+-+---------------> Release Difference / ms
             *         release_threshold
             */
            if (isEndOnBody)
                endOnBodyBias = 1 / (1 + Math.Exp(0.5 * (release_threshold - closestEndTime)));

            // Bonus for Holds that end after our tail.
            // We give a slight bonus to everything if something is held meanwhile
            if (isEndAfterTail)
                endAfterTailWeight = 1.25;

            /* Update Column & Global Strain given context of note
             * 1) We decay the strain given deltaTime
             * 2) Increase strain given information of note and surroundings
             *
             * Only in Global Strain, we include endOnBodyBias as a design choice.
             */
            columnStrains[column] = applyDecay(columnStrains[column], startTime - prevStartTimes[column], decay_base);
            columnStrains[column] += 2 * endAfterTailWeight;
            globalStrain = applyDecay(globalStrain, current.DeltaTime, global_decay_base);
            globalStrain += (1 + endOnBodyBias) * endAfterTailWeight;

            /* For notes at the same time (in a chord), the strain should be the highest column strain + global strain out of those columns
             *
             * --- Problem
             *
             * Given a scenario with a 4 note chord with:
             * - Global Strains (GS) [A, B, C, D]  <- Strains are different in a chord as we add to global strain every note. A < B < C < D
             * - Column Strains (CS) [1, 2, 3, 4]
             *
             * The maximum strain evaluated should be D + 4 = max(GS) + max(CS)
             * However, remember that strains are not necessarily increasing
             *
             * - Column Strains [4, 3, 2, 1] may yield A + 4 as the maxima.
             *
             * Shows that: max(GS + CS) != max(GS) + max(CS)
             * This caused the issue where strain was column-variant.
             *
             * --- Solution
             *
             * This mechanism counters this effect by taking the running maximum of both arrays.
             * (!) Global Strain is strictly increasing so it's not necessary to track it.
             *
             * With this mechanism:
             * - Global Strains                    [A, B, C, D]
             * - Column Strains                    [3, 4, 3, 2]
             * - Column Strains w/ running Maximum [3, 4, 4, 4]
             *
             * Shows that: run_max(GS + CS) = max(GS) + max(CS)
             *
             * We can also see that the last entry in a chord is always the peak strain.
             */
            double columnStrain = hitObject.DeltaTime <= 1 ? Math.Max(prevColumnStrain, columnStrains[column]) : columnStrains[column];

            // The final strain is the sum
            double strain = columnStrain + globalStrain;

            // Update prev arrays
            prevStartTimes[column] = startTime;
            prevEndTimes[column] = endTime;
            prevColumnStrain = columnStrain;

            // By subtracting CurrentStrain, this skill effectively only considers the maximum strain of any one hitobject within each strain section.
            return strain - CurrentStrain;
        }

        protected override double CalculateInitialStrain(double offset, DifficultyHitObject current)
            => applyDecay(prevStrain, offset - current.Previous(0).StartTime, decay_base)
               + applyDecay(globalStrain, offset - current.Previous(0).StartTime, global_decay_base);

        private double applyDecay(double value, double deltaTime, double decayBase)
            => value * Math.Pow(decayBase, deltaTime / 1000);
    }
}

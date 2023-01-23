// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Scoring
{
    public partial class ManiaHealthProcessor : DrainingHealthProcessor
    {
        /// <inheritdoc/>
        public ManiaHealthProcessor(double drainStartTime)
            : base(drainStartTime, 1.0)
        {
        }

        protected override HitResult GetSimulatedHitResult(Judgement judgement)
        {
            // Users are not expected to attain perfect judgements for all notes due to the tighter hit window.
            return judgement.MaxResult == HitResult.Perfect ? HitResult.Great : judgement.MaxResult;
        }
    }
}

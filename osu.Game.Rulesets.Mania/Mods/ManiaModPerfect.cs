﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModPerfect : ModPerfect
    {
        protected override bool FailCondition(HealthProcessor healthProcessor, Judgement result)
        {
            if (!isRelevantResult(result.JudgementCriteria.MinResult) && !isRelevantResult(result.JudgementCriteria.MaxResult) && !isRelevantResult(result.Type))
                return false;

            // Mania allows imperfect "Great" hits without failing.
            if (result.JudgementCriteria.MaxResult == HitResult.Perfect)
                return result.Type < HitResult.Great;

            return result.Type != result.JudgementCriteria.MaxResult;
        }

        private bool isRelevantResult(HitResult result) => result.AffectsAccuracy() || result.AffectsCombo();
    }
}

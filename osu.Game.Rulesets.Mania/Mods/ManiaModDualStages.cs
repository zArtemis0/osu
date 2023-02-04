﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Localisation.Mods;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModDualStages : Mod, IPlayfieldTypeMod, IApplicableToBeatmapConverter
    {
        public override string Name => "Dual Stages";
        public override string Acronym => "DS";
        public override LocalisableString Description => ConversionModsStrings.ManiaDualStagesDescription;
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1;

        private bool isForCurrentRuleset;

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var mbc = (ManiaBeatmapConverter)beatmapConverter;

            isForCurrentRuleset = mbc.IsForCurrentRuleset;

            // Although this can work, for now let's not allow keymods for mania-specific beatmaps
            if (isForCurrentRuleset)
                return;

            mbc.Dual = true;
        }

        public PlayfieldType PlayfieldType => PlayfieldType.Dual;
    }
}

﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class VariantBindingsSubsection : KeyBindingsSubsection
    {
        protected override bool AutoAdvanceTarget => true;

        protected override LocalisableString Header { get; }

        public VariantBindingsSubsection(RulesetInfo ruleset, int variant)
            : base(variant)
        {
            Ruleset = ruleset;

            var rulesetInstance = ruleset.CreateInstance();

            Header = rulesetInstance.GetVariantName(variant);
            Defaults = rulesetInstance.GetDefaultKeyBindings(variant);
        }
    }
}

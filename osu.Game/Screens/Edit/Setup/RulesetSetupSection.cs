// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Localisation;
using osu.Game.Rulesets;

namespace osu.Game.Screens.Edit.Setup
{
    public abstract class RulesetSetupSection : SetupSection
    {
        public sealed override LocalisableString Title => $"游戏模式 ({rulesetInfo.Name})";

        private readonly RulesetInfo rulesetInfo;

        protected RulesetSetupSection(RulesetInfo rulesetInfo)
        {
            this.rulesetInfo = rulesetInfo;
        }
    }
}

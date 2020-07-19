﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsOverlayHeader : TabControlOverlayHeader<RankingsScope>
    {
        public Bindable<RulesetInfo> Ruleset => rulesetSelector.Current;

        private OverlayRulesetSelector rulesetSelector;

        protected override OverlayTitle CreateTitle() => new RankingsTitle();

        protected override Drawable CreateTitleContent() => rulesetSelector = new OverlayRulesetSelector();

        protected override Drawable CreateBackground() => new OverlayHeaderBackground("Headers/rankings");

        public RankingsOverlayHeader()
        {
            ContentSidePadding = 50;
        }

        private class RankingsTitle : OverlayTitle
        {
            public RankingsTitle()
            {
                Title = "ranking";
                IconTexture = "Icons/rankings";
            }
        }
    }

    public enum RankingsScope
    {
        Performance,
        Spotlights,
        Score,
        Country
    }
}

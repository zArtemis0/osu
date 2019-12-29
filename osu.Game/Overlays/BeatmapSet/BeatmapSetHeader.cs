﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapSetHeader : OverlayHeader
    {
        public readonly Bindable<BeatmapSetInfo> BeatmapSet = new Bindable<BeatmapSetInfo>();
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        public BeatmapRulesetSelector RulesetSelector;
        public BeatmapHeaderContent HeaderContent;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            TitleBackgroundColour = colours.Gray2;
            RulesetSelector.AccentColour = colours.Blue;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            BeatmapSet.BindValueChanged(beatmapSet => RulesetSelector.BeatmapSet = beatmapSet.NewValue, true);
        }

        protected override ScreenTitle CreateTitle() => new BeatmapSetTitle();

        protected override Drawable CreateTitleContent() => RulesetSelector = new BeatmapRulesetSelector
        {
            Current = Ruleset
        };

        protected override Drawable CreateContent() => HeaderContent = new BeatmapHeaderContent
        {
            BeatmapSet = { BindTarget = BeatmapSet },
            Ruleset = { BindTarget = Ruleset }
        };

        private class BeatmapSetTitle : ScreenTitle
        {
            public BeatmapSetTitle()
            {
                Title = "beatmap";
                Section = "info";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Blue;
            }

            protected override Drawable CreateIcon() => new ScreenTitleTextureIcon(@"Icons/changelog");
        }
    }
}

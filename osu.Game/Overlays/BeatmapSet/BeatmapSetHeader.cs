﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapSetHeader : OverlayHeader
    {
        public BeatmapSetHeader()
            : base(true)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            TitleBackgroundColour = colours.Gray2;
            RulesetSelector.AccentColour = colours.Blue;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ((BeatmapHeaderContent)HeaderContent).BeatmapSet.BindValueChanged(beatmapSet => ((BeatmapRulesetSelector)RulesetSelector).BeatmapSet = beatmapSet.NewValue, true);
            ((BeatmapHeaderContent)HeaderContent).Ruleset.BindTo(Ruleset);
        }

        protected override ScreenTitle CreateTitle() => new BeatmapSetTitle();

        protected override OverlayRulesetSelector CreateRulesetSelector() => new BeatmapRulesetSelector();

        protected override Drawable CreateContent() => new BeatmapHeaderContent();

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

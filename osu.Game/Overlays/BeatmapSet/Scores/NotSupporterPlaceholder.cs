﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class NotSupporterPlaceholder : Container
    {
        public NotSupporterPlaceholder()
        {
            LinkFlowContainer text;

            AutoSizeAxes = Axes.Both;
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = @"你需要成为一名osu!supporter来查看好友和国内/区内排名!",
                        Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold),
                    },
                    text = new LinkFlowContainer(t => t.Font = t.Font.With(size: 18))
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.Both,
                    }
                }
            };

            text.AddText("点击");
            text.AddLink("这里", "/home/support");
            text.AddText("来查看你可以得到的所有超棒功能!");
        }
    }
}

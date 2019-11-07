﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Comments
{
    public class TotalCommentsCounter : CompositeDrawable
    {
        private SpriteText counter;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            Height = 50;
            AddInternal(new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Margin = new MarginPadding { Left = 50 },
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(size: 20, italics: true),
                        Text = @"Comments",
                        Colour = colours.BlueLighter
                    },
                    new CircularContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colours.CommentsGrayDarker
                            },
                            counter = new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Margin = new MarginPadding { Horizontal = 10, Vertical = 5 },
                                Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                                Colour = colours.BlueLighter
                            }
                        },
                    }
                }
            });

            SetValue(0);
        }

        public void SetValue(int value)
        {
            counter.Text = value.ToString("N0");
        }
    }
}

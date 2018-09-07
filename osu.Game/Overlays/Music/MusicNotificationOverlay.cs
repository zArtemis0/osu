﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays
{
    public class MusicNotificationOverlay : Container
    {
        private readonly Container box;

        public override bool HandleKeyboardInput => false;
        public override bool HandleMouseInput => false;

        private readonly SpriteText text;
        private readonly SpriteIcon icon;

        private const float height = 52;
        private const float height_contracted = height * 0.9f;

        public MusicNotificationOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                box = new Container
                {
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(0.5f, 0.75f),
                    Masking = true,
                    AutoSizeAxes = Axes.X,
                    Alpha = 0,
                    CornerRadius = 20,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.7f,
                        },
                        new Container // purely to add a minimum width
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Width = 80,
                            RelativeSizeAxes = Axes.Y,
                        },
                        text = new OsuSpriteText
                        {
                            Padding = new MarginPadding(10),
                            Font = @"Exo2.0-Black",
                            Spacing = new Vector2(1, 0),
                            TextSize = 11,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        icon = new SpriteIcon
                        {
                            Size = new Vector2(12),
                            Margin = new MarginPadding(16),
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                        },
                    }
                },
            };
        }

        public void Display(string _text, FontAwesome _icon)
        {
            Schedule(() =>
            {
                text.Text = _text;
                icon.Icon = _icon;

                DisplayTemporarily(box);
            });
        }

        private TransformSequence<Drawable> fadeIn;
        private ScheduledDelegate fadeOut;

        protected virtual void DisplayTemporarily(Drawable toDisplay)
        {
            // avoid starting a new fade-in if one is already active.
            if (fadeIn == null)
            {
                fadeIn = toDisplay.Animate(
                    b => b.FadeIn(500, Easing.OutQuint),
                    b => b.ResizeHeightTo(height, 500, Easing.OutQuint)
                );

                fadeIn.Finally(_ => fadeIn = null);
            }

            fadeOut?.Cancel();
            fadeOut = Scheduler.AddDelayed(() =>
            {
                toDisplay.Animate(
                    b => b.FadeOutFromOne(1500, Easing.InQuint),
                    b => b.ResizeHeightTo(height_contracted, 1500, Easing.InQuint));
            }, 500);
        }
    }
}

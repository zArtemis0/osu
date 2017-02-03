﻿//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuButton : Button
    {
        private Box hover;

        public OsuButton()
        {
            Height = 40;
        }

        protected override SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = @"Exo2.0-Bold",
        };

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueDark;

            Content.Masking = true;
            Content.CornerRadius = 5;

            Add(new Drawable[]
            {
                new Triangles
                {
                    RelativeSizeAxes = Axes.Both,
                    ColourDark = colours.BlueDarker,
                    ColourLight = colours.Blue,
                },
                hover = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    BlendingMode = BlendingMode.Additive,
                    Colour = Color4.White.Opacity(0.1f),
                    Alpha = 0,
                },
            });
        }

        protected override bool OnHover(InputState state)
        {
            hover.FadeIn(200);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            hover.FadeOut(200);
            base.OnHoverLost(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            Content.ScaleTo(0.9f, 4000, EasingTypes.OutQuint);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            Content.ScaleTo(1, 1000, EasingTypes.OutElastic);
            return base.OnMouseUp(state, args);
        }
    }
}
﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Select
{
    public class FooterButton : OsuClickableContainer
    {
        private static readonly Vector2 shearing = new Vector2(0.15f, 0);

        public string Text
        {
            get => SpriteText?.Text;
            set
            {
                if (SpriteText != null)
                    SpriteText.Text = value;
            }
        }

        private Color4 deselectedColour;

        public Color4 DeselectedColour
        {
            get => deselectedColour;
            set
            {
                deselectedColour = value;
                if (light.Colour != SelectedColour)
                    light.Colour = value;
            }
        }

        private Color4 selectedColour;

        public Color4 SelectedColour
        {
            get => selectedColour;
            set
            {
                selectedColour = value;
                box.Colour = selectedColour;
            }
        }

        protected readonly Vector2 DEFAULT_SIZE = new Vector2(100, 50);
        protected readonly Container TextContainer;
        protected readonly SpriteText SpriteText;
        private readonly Box box;
        private readonly Box light;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => box.ReceivePositionalInputAt(screenSpacePos);

        public FooterButton()
        {
            Size = DEFAULT_SIZE;
            Children = new Drawable[]
            {
                TextContainer = new Container
                {
                    Size = DEFAULT_SIZE,
                    Child = SpriteText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                },
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Shear = shearing,
                    EdgeSmoothness = new Vector2(2, 0),
                    Colour = Color4.White,
                    Alpha = 0,
                },
                light = new Box
                {
                    Shear = shearing,
                    Height = 4,
                    EdgeSmoothness = new Vector2(2, 0),
                    RelativeSizeAxes = Axes.X,
                },
            };
        }

        public Action Hovered;
        public Action HoverLost;
        public Key? Hotkey;

        protected override bool OnHover(HoverEvent e)
        {
            Hovered?.Invoke();
            light.ScaleTo(new Vector2(1, 2), Footer.TRANSITION_LENGTH, Easing.OutQuint);
            light.FadeColour(SelectedColour, Footer.TRANSITION_LENGTH, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            HoverLost?.Invoke();
            light.ScaleTo(new Vector2(1, 1), Footer.TRANSITION_LENGTH, Easing.OutQuint);
            light.FadeColour(DeselectedColour, Footer.TRANSITION_LENGTH, Easing.OutQuint);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            box.FadeTo(0.3f, Footer.TRANSITION_LENGTH * 2, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            box.FadeOut(Footer.TRANSITION_LENGTH, Easing.OutQuint);
            return base.OnMouseUp(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            box.ClearTransforms();
            box.Alpha = 1;
            box.FadeOut(Footer.TRANSITION_LENGTH * 3, Easing.OutQuint);
            return base.OnClick(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.Repeat && e.Key == Hotkey)
            {
                Click();
                return true;
            }

            return base.OnKeyDown(e);
        }
    }
}

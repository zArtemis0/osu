﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings.Sections.General;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Overlays
{
    internal class LoginOverlay : OsuFocusedOverlayContainer
    {
        private LoginSettings settingsSection;

        private const float transition_time = 400;

        public LoginOverlay()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.6f,
                },
                new Container
                {
                    Width = 360,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    AutoSizeDuration = transition_time,
                    AutoSizeEasing = Easing.OutQuint,
                    Children = new Drawable[]
                    {
                        settingsSection = new LoginSettings
                        {
                            Padding = new MarginPadding(10),
                            RequestHide = Hide,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Height = 3,
                            Colour = colours.Yellow,
                            Alpha = 1,
                        },
                    }
                }
            };
        }

        protected override void PopIn()
        {
            base.PopIn();

            settingsSection.Bounding = true;
            this.FadeIn(transition_time, Easing.OutQuint);

            GetContainingInputManager().ChangeFocus(settingsSection);
        }

        protected override void PopOut()
        {
            base.PopOut();

            settingsSection.Bounding = false;
            this.FadeOut(transition_time);
        }

        protected override bool OnDragStart(InputState state)
        {
            if (!state.Mouse.IsPressed(MouseButton.Left)) return false;
            return base.OnDragStart(state);
        }

        protected override bool OnDrag(InputState state)
        {
            if (!state.Mouse.IsPressed(MouseButton.Left)) return false;
            return base.OnDrag(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            if (args.Button != MouseButton.Left) return false;
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (args.Button != MouseButton.Left) return false;
            return base.OnMouseUp(state, args);
        }
    }
}

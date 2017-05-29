﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Game.Graphics.UserInterface;
using System.Linq;

namespace osu.Game.Graphics.Cursor
{
    public class ContextMenuContainer : Container
    {
        private readonly CursorContainer cursor;
        private readonly ContextMenu menu;

        private const float fade_duration = 250;

        private UserInputManager inputManager;

        public ContextMenuContainer(CursorContainer cursor)
        {
            this.cursor = cursor;
            AlwaysPresent = true;
            RelativeSizeAxes = Axes.Both;
            Add(menu = new ContextMenu { Alpha = 0 });
        }

        [BackgroundDependencyLoader]
        private void load(UserInputManager input)
        {
            inputManager = input;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            switch (args.Button)
            {
                case MouseButton.Right:
                    var menuTarget = inputManager.HoveredDrawables.OfType<IHasContextMenu>().FirstOrDefault();
                    if (menuTarget == null)
                    {
                        menu.FadeOut(fade_duration, EasingTypes.OutQuint);
                        return false;
                    }
                    menu.Items = menuTarget.Items;
                    menu.Position = ToLocalSpace(cursor.ActiveCursor.ScreenSpaceDrawQuad.TopLeft);
                    menu.FadeIn(fade_duration, EasingTypes.OutQuint);
                    return true;
            }

            menu.FadeOut(fade_duration, EasingTypes.OutQuint);
            return true;
        }

        public class ContextMenu : Container
        {
            private readonly FillFlowContainer content;

            private ContextMenuItem[] items;
            public ContextMenuItem[] Items
            {
                set
                {
                    if(items != null)
                    {
                        foreach (var item in items)
                            content.Remove(item);
                    }

                    items = value;

                    foreach (var item in value)
                        content.Add(item);
                }
            }

            public ContextMenu()
            {
                AutoSizeAxes = Axes.Both;

                CornerRadius = 5;
                Masking = true;
                EdgeEffect = new EdgeEffect
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(40),
                    Radius = 5,
                };

                Children = new Drawable[]
                {
                    content = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                    }
                };
            }
        }
    }
}

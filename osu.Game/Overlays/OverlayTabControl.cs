// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract class OverlayTabControl<T> : OsuTabControl<T>
    {
        private readonly Box bar;

        protected float BarHeight
        {
            set => bar.Height = value;
        }

        public override Color4 AccentColour
        {
            get => base.AccentColour;
            set
            {
                base.AccentColour = value;
                bar.Colour = value;
            }
        }

        private Color4 textColour;

        public Color4 TextColour
        {
            get => textColour;
            set
            {
                textColour = value;

                foreach (var i in TabContainer.Children.OfType<OverlayTabItem>())
                    i.TextColour = value;
            }
        }

        protected OverlayTabControl()
        {
            TabContainer.Masking = false;
            TabContainer.Spacing = new Vector2(15, 0);

            AddInternal(bar = new Box
            {
                RelativeSizeAxes = Axes.X,
                Height = 2,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.CentreLeft
            });
        }

        protected override Dropdown<T> CreateDropdown() => null;

        protected override TabItem<T> CreateTabItem(T value) => new OverlayTabItem(value);

        protected class OverlayTabItem : TabItem<T>, IHasAccentColour
        {
            protected readonly ExpandingBar Bar;
            protected readonly OsuSpriteText Text;

            public Color4 AccentColour
            {
                get => Bar.Colour;
                set => Bar.Colour = value;
            }

            private Color4 textColour;

            public Color4 TextColour
            {
                get => textColour;
                set
                {
                    if (textColour == value)
                        return;

                    textColour = value;
                    updateState();
                }
            }

            public OverlayTabItem(T value)
                : base(value)
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                Children = new Drawable[]
                {
                    Text = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Vertical = 16.5f }, // 15px padding + 1.5px line-height difference compensation
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        Font = OsuFont.GetFont(),
                    },
                    Bar = new ExpandingBar
                    {
                        Anchor = Anchor.BottomCentre,
                        ExpandedSize = 7.5f,
                        CollapsedSize = 0
                    },
                    new HoverClickSounds()
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);

                if (!Active.Value)
                    HoverAction();

                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);

                if (!Active.Value)
                    UnhoverAction();
            }

            protected override void OnActivated()
            {
                HoverAction();
                Text.Font = Text.Font.With(weight: FontWeight.Bold);
            }

            protected override void OnDeactivated()
            {
                UnhoverAction();
                Text.Font = Text.Font.With(weight: FontWeight.Medium);
            }

            private void updateState()
            {
                if (Active.Value)
                    OnActivated();
                else
                    OnDeactivated();
            }

            protected virtual void HoverAction()
            {
                Bar.Expand();
                Text.FadeColour(Color4.White, 120, Easing.InQuad);
            }

            protected virtual void UnhoverAction()
            {
                Bar.Collapse();
                Text.FadeColour(TextColour, 120, Easing.InQuad);
            }
        }
    }
}

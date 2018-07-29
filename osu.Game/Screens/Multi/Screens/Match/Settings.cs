// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class Settings : Container
    {
        private readonly Box bg;

        public Settings()
        {
            Children = new Drawable[]
            {
                bg = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING, Vertical = 15 },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = @"ROOM NAME",
                            Colour = Color4.White,
                        },
                        new OsuTextBox
                        {
                            Width = 300,
                            Margin = new MarginPadding { Top = 20 },
                        },
                        new OsuSpriteText
                        {
                            Text = @"MAX PARTICIPANTS",
                            Colour = Color4.White,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        },
                        new OsuTextBox
                        {
                            Width = 300,
                            Margin = new MarginPadding { Top = 20 },
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING, Vertical = 15 },
                    Margin = new MarginPadding { Top = 100 },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = @"ROOM VISIBILITY",
                            Colour = Color4.White,
                        },
                        new VisibilityTabControl
                        {
                            Margin = new MarginPadding { Top = 20 },
                            Width = 400,
                            Height = 40,
                        },
                        new OsuSpriteText
                        {
                            Text = @"PASSWORD (OPTIONAL)",
                            Colour = Color4.White,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        },
                        new SettingsPasswordTextBox
                        {
                            Width = 300,
                            Margin = new MarginPadding { Top = 20 },
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING, Vertical = 15 },
                    Margin = new MarginPadding { Top = 200 },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = @"GAME TYPE",
                            Colour = Color4.White,
                        },
                        new GameTypeTabControl
                        {
                            Margin = new MarginPadding { Top = 20 },
                            Width = 400,
                            Height = 80,
                        },
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            bg.Colour = colours.Gray2;
        }

        private class SettingsPasswordTextBox : OsuPasswordTextBox
        {
            private readonly Box labelBox;

            public SettingsPasswordTextBox()
            {
                TextContainer.Margin = new MarginPadding { Left = 80 };

                Content.Add(new Container
                {
                    Masking = true,
                    CornerRadius = 3,
                    RelativeSizeAxes = Axes.Y,
                    Width = 80,
                    Children = new Drawable[]
                    {
                        labelBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                        },
                        new OsuSpriteText
                        {
                            Text = @"Password",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                });
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                labelBox.Colour = colours.Gray4;
            }
        }

        private class VisibilityTabControl : OsuTabControl<RoomAvailability>
        {
            protected override TabItem<RoomAvailability> CreateTabItem(RoomAvailability value) => new VisibilityTabItem(value);

            private class VisibilityTabItem : OsuTabItem
            {
                private readonly Box bg;

                private OsuColour colours;

                public VisibilityTabItem(RoomAvailability value)
                    : base(value)
                {
                    AutoSizeAxes = Axes.None;
                    CornerRadius = 7;
                    Masking = true;
                    Width = 110;

                    Children = new Drawable[]
                    {
                        bg = new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new OsuSpriteText
                        {
                            Text = value.GetDescription(),
                            Colour = Color4.White,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    };
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    this.colours = colours;

                    bg.Colour = Active.Value ? colours.Green : colours.Gray4;
                }
                protected override bool OnHover(InputState state)
                {
                    bg.FadeTo(0.8f, 200);

                    return base.OnHover(state);
                }

                protected override void OnHoverLost(InputState state)
                {
                    bg.FadeTo(1f, 200);

                    base.OnHoverLost(state);
                }

                private void fadeActive()
                {
                    bg.FadeColour(colours.Green, 150, Easing.InSine);
                }

                private void fadeInactive()
                {
                    bg.FadeColour(colours.Gray4, 150, Easing.InSine);
                }

                protected override void OnActivated()
                {
                    fadeActive();

                    base.OnActivated();
                }

                protected override void OnDeactivated()
                {
                    fadeInactive();

                    base.OnDeactivated();
                }
            }
        }

        private class GameTypeTabControl : OsuTabControl<GameType>
        {
            protected override TabItem<GameType> CreateTabItem(GameType value) => new GameTypeTabItem(value);

            protected override Dropdown<GameType> CreateDropdown() => null;

            private readonly OsuSpriteText activeTabText;

            public GameTypeTabControl()
            {
                Add(activeTabText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                });

                AddItem(new GameTypeVersus());
                AddItem(new GameTypeTag());
                AddItem(new GameTypeTagTeam());
                AddItem(new GameTypeTeamVersus());
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                activeTabText.Colour = colours.Yellow;
            }

            protected override void SelectTab(TabItem<GameType> tab)
            {
                base.SelectTab(tab);

                activeTabText.Text = tab.Value.Name;
            }

            private class GameTypeTabItem : OsuTabItem
            {
                private readonly CircularContainer circle;
                private readonly Box circleBg;
                private readonly GameType gameType;

                private OsuColour colours;

                public GameTypeTabItem(GameType value)
                    : base(value)
                {
                    this.gameType = value;

                    AutoSizeAxes = Axes.None;
                    RelativeSizeAxes = Axes.None;

                    Height = 60;
                    Width = 60;

                    Child = circle = new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            circleBg = new Box
                            {
                                Colour = Color4.White,
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    };

                    Colour = Color4.White;
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    this.colours = colours;

                    circleBg.Colour = colours.Gray4;
                    circle.BorderColour = colours.Yellow;

                    circle.Add(gameType.GetIcon(colours, 30));
                }

                private void fadeActive()
                {
                    circle.BorderThickness = 5;
                }

                private void fadeInactive()
                {
                    circle.BorderThickness = 0;
                }

                protected override void OnActivated()
                {
                    base.OnActivated();

                    fadeActive();
                }

                protected override void OnDeactivated()
                {
                    base.OnDeactivated();

                    fadeInactive();
                }
            }
        }
    }
}

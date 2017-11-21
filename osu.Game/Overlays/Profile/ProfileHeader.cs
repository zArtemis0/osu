﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using System.Diagnostics;
using System.Collections.Generic;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Profile
{
    public class ProfileHeader : Container
    {
        private const int count_duration = 300;

        private readonly OsuTextFlowContainer infoTextLeft;
        private readonly LinkFlowContainer infoTextRight;

        private readonly Container coverContainer, supporterTag;
        private readonly Sprite levelBadge;
        private readonly StatisticsCounter levelCounter;
        private readonly GradeBadge gradeSSPlus, gradeSS, gradeSPlus, gradeS, gradeA;
        private readonly Box colourBar;
        private readonly DrawableFlag countryFlag;
        private readonly RankChart graph;

        private readonly StatisticsCounter rankedScoreCounter;
        private readonly PercentageCounter accuracyCounter;
        private readonly StatisticsCounter playCountCounter;
        private readonly StatisticsCounter totalScoreCounter;
        private readonly StatisticsCounter totalHitsCounter;
        private readonly StatisticsCounter maxComboCounter;
        private readonly StatisticsCounter replaysCounter;

        private readonly Box levelLoader;
        private readonly Box graphLoader;
        private readonly Box statisticsLoader;

        private const float cover_height = 350;
        private const float info_height = 150;
        private const float info_width = 220;
        private const float avatar_size = 110;
        private const float level_position = 30;
        private const float level_height = 60;

        public ProfileHeader(User user)
        {
            RelativeSizeAxes = Axes.X;
            Height = cover_height + info_height;

            Children = new Drawable[]
            {
                coverContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = cover_height,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.1f), Color4.Black.Opacity(0.75f))
                        },
                        new Container
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            X = UserProfileOverlay.CONTENT_X_MARGIN,
                            Y = -20,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new UpdateableAvatar
                                {
                                    User = user,
                                    Size = new Vector2(avatar_size),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Masking = true,
                                    CornerRadius = 5,
                                    EdgeEffect = new EdgeEffectParameters
                                    {
                                        Type = EdgeEffectType.Shadow,
                                        Colour = Color4.Black.Opacity(0.25f),
                                        Radius = 4,
                                    },
                                },
                                new Container
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    X = avatar_size + 10,
                                    AutoSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        supporterTag = new CircularContainer
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Y = -75,
                                            Size = new Vector2(25, 25),
                                            Masking = true,
                                            BorderThickness = 3,
                                            BorderColour = Color4.White,
                                            Alpha = 0,
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Alpha = 0,
                                                    AlwaysPresent = true
                                                },
                                                new SpriteIcon
                                                {
                                                    Icon = FontAwesome.fa_heart,
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Size = new Vector2(12),
                                                }
                                            }
                                        },
                                        new LinkFlowContainer.ProfileLink(user)
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Y = -48,
                                        },
                                        countryFlag = new DrawableFlag(user.Country?.FlagName)
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Width = 30,
                                            Height = 20
                                        }
                                    }
                                }
                            }
                        },
                        colourBar = new Box
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            X = UserProfileOverlay.CONTENT_X_MARGIN,
                            Height = 5,
                            Width = info_width,
                            Alpha = 0
                        }
                    }
                },
                infoTextLeft = new OsuTextFlowContainer(t =>
                {
                    t.TextSize = 14;
                    t.Alpha = 0.8f;
                })
                {
                    X = UserProfileOverlay.CONTENT_X_MARGIN,
                    Y = cover_height + 20,
                    Width = info_width,
                    AutoSizeAxes = Axes.Y,
                    ParagraphSpacing = 0.8f,
                    LineSpacing = 0.2f
                },
                infoTextRight = new LinkFlowContainer(t =>
                {
                    t.TextSize = 14;
                    t.Font = @"Exo2.0-RegularItalic";
                })
                {
                    X = UserProfileOverlay.CONTENT_X_MARGIN + info_width + 20,
                    Y = cover_height + 20,
                    Width = info_width,
                    AutoSizeAxes = Axes.Y,
                    ParagraphSpacing = 0.8f,
                    LineSpacing = 0.2f
                },
                new Container
                {
                    X = -UserProfileOverlay.CONTENT_X_MARGIN,
                    RelativeSizeAxes = Axes.Y,
                    Width = 280,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Y = level_position,
                            Height = level_height,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.Black.Opacity(0.5f),
                                    RelativeSizeAxes = Axes.Both
                                },
                                levelBadge = new Sprite
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Height = 50,
                                    Width = 50,
                                },
                                levelCounter = new StatisticsCounter(20, false)
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Y = 11,
                                },
                                levelLoader = new Box
                                {
                                    Colour = Color4.Black.Opacity(0.5f),
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Y = cover_height,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.BottomCentre,
                            Height = cover_height - level_height - level_position - 5,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.Black.Opacity(0.5f),
                                    RelativeSizeAxes = Axes.Both
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 2),
                                    Padding = new MarginPadding { Horizontal = 20, Vertical = 18 },
                                    Children = new Drawable[]
                                    {
                                        new StatisticsLine("Ranked Score") { Child = rankedScoreCounter = new StatisticsCounter() },
                                        new StatisticsLine("Accuracy") { Child = accuracyCounter = new PercentageCounter() },
                                        new StatisticsLine("Play Count") { Child = playCountCounter = new StatisticsCounter() },
                                        new StatisticsLine("Total Score") { Child = totalScoreCounter = new StatisticsCounter() },
                                        new StatisticsLine("Total Hits") { Child = totalHitsCounter = new StatisticsCounter() },
                                        new StatisticsLine("Max Combo") { Child = maxComboCounter = new StatisticsCounter() },
                                        new StatisticsLine("Replays Watched by Others") { Child = replaysCounter = new StatisticsCounter() },
                                    },
                                },
                                new FillFlowContainer<GradeBadge>
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Y = -64,
                                    Spacing = new Vector2(20, 0),
                                    Children = new[]
                                    {
                                        gradeSSPlus = new GradeBadge("SSPlus") { Alpha = 0 },
                                        gradeSS = new GradeBadge("SS"),
                                    }
                                },
                                new FillFlowContainer<GradeBadge>
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Y = -18,
                                    Spacing = new Vector2(20, 0),
                                    Children = new[]
                                    {
                                        gradeSPlus = new GradeBadge("SPlus") { Alpha = 0 },
                                        gradeS = new GradeBadge("S"),
                                        gradeA = new GradeBadge("A"),
                                    }
                                },
                                statisticsLoader = new Box
                                {
                                    Colour = Color4.Black.Opacity(0.5f),
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Height = info_height - 15,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.Black.Opacity(0.25f),
                                    RelativeSizeAxes = Axes.Both
                                },
                                graph = new RankChart
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                graphLoader = new Box
                                {
                                    Colour = Color4.Black.Opacity(0.5f),
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            levelBadge.Texture = textures.Get(@"Profile/levelbadge");
        }

        private User user;
        public User User
        {
            get { return user; }
            set
            {
                IsReloading = false;
                if (value == null)
                {
                    if (user != null)
                        reloadStatistics(null);

                    return;
                }

                if (user != null && user.Id == value.Id && user.Statistics != value.Statistics)
                {
                    reloadStatistics(value);
                    return;
                }

                user = value;
                loadUser();
            }
        }

        private void loadUser()
        {
            coverContainer.Add(new AsyncLoadWrapper(new UserCoverBackground(user)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
                OnLoadComplete = d => d.FadeInFromZero(200)
            })
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Depth = float.MaxValue
            });

            if (user.IsSupporter) supporterTag.Show();

            if (!string.IsNullOrEmpty(user.Colour))
            {
                colourBar.Colour = OsuColour.FromHex(user.Colour);
                colourBar.Show();
            }

            Action<SpriteText> boldItalic = t =>
            {
                t.Font = @"Exo2.0-BoldItalic";
                t.Alpha = 1;
            };

            if (user.Age != null)
            {
                infoTextLeft.AddText($"{user.Age} years old ", boldItalic);
            }
            if (user.Country != null)
            {
                infoTextLeft.AddText("from ");
                infoTextLeft.AddText(user.Country.FullName, boldItalic);
                countryFlag.FlagName = user.Country.FlagName;
            }
            infoTextLeft.NewParagraph();

            if (user.JoinDate.ToUniversalTime().Year < 2008)
            {
                infoTextLeft.AddText("Here since the beginning", boldItalic);
            }
            else
            {
                infoTextLeft.AddText("Joined ");
                infoTextLeft.AddText(user.JoinDate.LocalDateTime.ToShortDateString(), boldItalic);
            }
            infoTextLeft.NewLine();
            infoTextLeft.AddText("Last seen ");
            infoTextLeft.AddText(user.LastVisit.LocalDateTime.ToShortDateString(), boldItalic);
            infoTextLeft.NewParagraph();

            if (user.PlayStyle?.Length > 0)
            {
                infoTextLeft.AddText("Plays with ");
                infoTextLeft.AddText(string.Join(", ", user.PlayStyle), boldItalic);
            }

            string websiteWithoutProtcol = user.Website;
            if (!string.IsNullOrEmpty(websiteWithoutProtcol))
            {
                int protocolIndex = websiteWithoutProtcol.IndexOf("//", StringComparison.Ordinal);
                if (protocolIndex >= 0)
                    websiteWithoutProtcol = websiteWithoutProtcol.Substring(protocolIndex + 2);
            }

            tryAddInfoRightLine(FontAwesome.fa_map_marker, user.Location);
            tryAddInfoRightLine(FontAwesome.fa_heart_o, user.Intrerests);
            tryAddInfoRightLine(FontAwesome.fa_suitcase, user.Occupation);
            infoTextRight.NewParagraph();
            if (!string.IsNullOrEmpty(user.Twitter))
                tryAddInfoRightLine(FontAwesome.fa_twitter, "@" + user.Twitter, $@"https://twitter.com/{user.Twitter}");
            tryAddInfoRightLine(FontAwesome.fa_globe, websiteWithoutProtcol, user.Website);
            tryAddInfoRightLine(FontAwesome.fa_skype, user.Skype, @"skype:" + user.Skype + @"?chat");

            reloadStatistics(user);
        }

        public bool IsReloading
        {
            set
            {
                levelLoader.FadeTo(value ? 1 : 0, count_duration);
                graphLoader.FadeTo(value ? 1 : 0, count_duration);
                statisticsLoader.FadeTo(value ? 1 : 0, count_duration);
            }
        }

        private void reloadStatistics(User user)
        {
            var statistics = user?.Statistics;

            levelCounter.CountTo(statistics?.Level.Current ?? 0, count_duration, Easing.OutQuad);

            rankedScoreCounter.CountTo(statistics?.RankedScore ?? 0, count_duration, Easing.OutQuad);
            accuracyCounter.CountTo((double)(statistics?.Accuracy ?? 0), count_duration, Easing.OutQuad);
            playCountCounter.CountTo(statistics?.PlayCount ?? 0, count_duration, Easing.OutQuad);
            totalScoreCounter.CountTo(statistics?.TotalScore ?? 0, count_duration, Easing.OutQuad);
            totalHitsCounter.CountTo(statistics?.TotalHits ?? 0, count_duration, Easing.OutQuad);
            maxComboCounter.CountTo(statistics?.MaxCombo ?? 0, count_duration, Easing.OutQuad);
            replaysCounter.CountTo(statistics?.ReplaysWatched ?? 0, count_duration, Easing.OutQuad);

            gradeSS.UpdateValue(statistics?.GradesCount.SS ?? 0);
            gradeS.UpdateValue(statistics?.GradesCount.S ?? 0);
            gradeA.UpdateValue(statistics?.GradesCount.A ?? 0);

            gradeSPlus.UpdateValue(0);
            gradeSSPlus.UpdateValue(0);

            graph.Redraw(user);
        }

        private void tryAddInfoRightLine(FontAwesome icon, string str, string url = null)
        {
            if (string.IsNullOrEmpty(str)) return;

            infoTextRight.AddIcon(icon);
            infoTextRight.AddLink(" " + str, url);
            infoTextRight.NewLine();
        }

        private class StatisticsLine : Container
        {
            private readonly Container content;
            protected override Container<Drawable> Content => content;

            public StatisticsLine(string title)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                AddInternal(new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = 14,
                    Text = title,
                });
                AddInternal(content = new Container
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                });
            }
        }

        private class StatisticsCounter : Counter
        {
            protected readonly OsuSpriteText Counter;

            public StatisticsCounter(int textsize = 14, bool isBold = true)
            {
                AutoSizeAxes = Axes.Both;
                InternalChild = Counter = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = textsize,
                    Text = "0",
                };

                if (isBold)
                    Counter.Font = @"Exo2.0-Bold";
            }

            protected override void OnCountChanged(double count) => Counter.Text = $"{count:n0}";
        }

        private class PercentageCounter : StatisticsCounter
        {
            public PercentageCounter(int textsize = 14, bool isBold = true)
                :base(textsize, isBold)
            {
            }

            protected override void OnCountChanged(double count) => Counter.Text = $"{count:0.##}%";
        }

        private class GradeBadge : Container
        {
            private const float width = 50;
            private readonly string grade;
            private readonly Sprite badge;
            private readonly StatisticsCounter counter;

            public GradeBadge(string grade)
            {
                this.grade = grade;
                Width = width;
                Height = 41;
                Add(badge = new Sprite
                {
                    Width = width,
                    Height = 26
                });
                Add(counter = new StatisticsCounter
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                });
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                badge.Texture = textures.Get($"Grades/{grade}");
            }

            public void UpdateValue(int newValue) => counter.CountTo(newValue, count_duration, Easing.OutQuad);
        }

        private class LinkFlowContainer : OsuTextFlowContainer
        {
            public override bool HandleInput => true;

            public LinkFlowContainer(Action<SpriteText> defaultCreationParameters = null) : base(defaultCreationParameters)
            {
            }

            protected override SpriteText CreateSpriteText() => new LinkText();

            public void AddLink(string text, string url) => AddText(text, link => ((LinkText)link).Url = url);

            public class LinkText : OsuSpriteText
            {
                private readonly OsuHoverContainer content;

                public override bool HandleInput => content.Action != null;

                protected override Container<Drawable> Content => content ?? (Container<Drawable>)this;

                protected override IEnumerable<Drawable> FlowingChildren => Children;

                public string Url
                {
                    set
                    {
                        if(value != null)
                            content.Action = () => Process.Start(value);
                    }
                }

                public LinkText()
                {
                    AddInternal(content = new OsuHoverContainer
                    {
                        AutoSizeAxes = Axes.Both,
                    });
                }
            }

            public class ProfileLink : LinkText, IHasTooltip
            {
                public string TooltipText => "View Profile in Browser";

                public ProfileLink(User user)
                {
                    Text = user.Username;
                    Url = $@"https://osu.ppy.sh/users/{user.Id}";
                    Font = @"Exo2.0-RegularItalic";
                    TextSize = 30;
                }
            }
        }
    }
}

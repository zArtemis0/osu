// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.TeamIntro
{
    public class SeedingScreen : TournamentScreen, IProvideVideo
    {
        private Container mainContainer;

        private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();

        private readonly Bindable<TournamentTeam> currentTeam = new Bindable<TournamentTeam>();

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new TourneyVideo(storage.GetStream(@"videos/seeding.m4v"))
                {
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "显示第一队",
                            Action = () => currentTeam.Value = currentMatch.Value.Team1.Value,
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "显示第二队",
                            Action = () => currentTeam.Value = currentMatch.Value.Team2.Value,
                        },
                        new SettingsTeamDropdown(LadderInfo.Teams)
                        {
                            LabelText = "显示指定队伍",
                            Bindable = currentTeam,
                        }
                    }
                }
            };

            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(LadderInfo.CurrentMatch);

            currentTeam.BindValueChanged(teamChanged, true);
        }

        private void teamChanged(ValueChangedEvent<TournamentTeam> team)
        {
            if (team.NewValue == null)
            {
                mainContainer.Clear();
                return;
            }

            showTeam(team.NewValue);
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch> match) =>
            currentTeam.Value = currentMatch.Value.Team1.Value;

        private void showTeam(TournamentTeam team)
        {
            mainContainer.Children = new Drawable[]
            {
                new LeftInfo(team) { Position = new Vector2(55, 150), },
                new RightInfo(team) { Position = new Vector2(500, 150), },
            };
        }

        private class RightInfo : CompositeDrawable
        {
            public RightInfo(TournamentTeam team)
            {
                FillFlowContainer fill;

                Width = 400;

                InternalChildren = new Drawable[]
                {
                    fill = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                    },
                };

                foreach (var seeding in team.SeedingResults)
                {
                    fill.Add(new ModRow(seeding.Mod.Value, seeding.Seed.Value));
                    foreach (var beatmap in seeding.Beatmaps)
                        fill.Add(new BeatmapScoreRow(beatmap));
                }
            }

            private class BeatmapScoreRow : CompositeDrawable
            {
                public BeatmapScoreRow(SeedingBeatmap beatmap)
                {
                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChildren = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText { Text = beatmap.BeatmapInfo.Metadata.Title, Colour = Color4.Black, },
                                new TournamentSpriteText { Text = "by", Colour = Color4.Black, Font = OsuFont.Torus.With(weight: FontWeight.Regular) },
                                new TournamentSpriteText { Text = beatmap.BeatmapInfo.Metadata.Artist, Colour = Color4.Black, Font = OsuFont.Torus.With(weight: FontWeight.Regular) },
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(40),
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText { Text = beatmap.Score.ToString("#,0"), Colour = Color4.Black, Width = 80 },
                                new TournamentSpriteText { Text = "#" + beatmap.Seed.Value.ToString("#,0"), Colour = Color4.Black, Font = OsuFont.Torus.With(weight: FontWeight.Regular) },
                            }
                        },
                    };
                }
            }

            private class ModRow : CompositeDrawable
            {
                private readonly string mods;
                private readonly int seeding;

                public ModRow(string mods, int seeding)
                {
                    this.mods = mods;
                    this.seeding = seeding;

                    Padding = new MarginPadding { Vertical = 10 };

                    AutoSizeAxes = Axes.Y;
                }

                [BackgroundDependencyLoader]
                private void load(TextureStore textures)
                {
                    InternalChildren = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                new Sprite
                                {
                                    Texture = textures.Get($"mods/{mods.ToLower()}"),
                                    Scale = new Vector2(0.5f)
                                },
                                new Container
                                {
                                    Size = new Vector2(50, 16),
                                    CornerRadius = 10,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Color4.Black,
                                        },
                                        new TournamentSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = seeding.ToString("#,0"),
                                        },
                                    }
                                },
                            }
                        },
                    };
                }
            }
        }

        private class LeftInfo : CompositeDrawable
        {
            public LeftInfo(TournamentTeam team)
            {
                FillFlowContainer fill;

                Width = 200;

                if (team == null) return;

                InternalChildren = new Drawable[]
                {
                    fill = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new TeamDisplay(team) { Margin = new MarginPadding { Bottom = 30 } },
                            new RowDisplay("平均Rank:", $"#{team.AverageRank:#,0}"),
                            new RowDisplay("种子:", team.Seed.Value),
                            new RowDisplay("去年的位置:", team.LastYearPlacing.Value > 0 ? $"#{team.LastYearPlacing:#,0}" : "0"),
                            new Container { Margin = new MarginPadding { Bottom = 30 } },
                        }
                    },
                };

                foreach (var p in team.Players)
                    fill.Add(new RowDisplay(p.Username, p.Statistics?.Ranks.Global?.ToString("\\##,0") ?? "-"));
            }

            internal class RowDisplay : CompositeDrawable
            {
                public RowDisplay(string left, string right)
                {
                    AutoSizeAxes = Axes.Y;
                    RelativeSizeAxes = Axes.X;

                    var colour = OsuColour.Gray(0.3f);

                    InternalChildren = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = left,
                            Colour = colour,
                            Font = OsuFont.Torus.With(size: 22),
                        },
                        new TournamentSpriteText
                        {
                            Text = right,
                            Colour = colour,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopLeft,
                            Font = OsuFont.Torus.With(size: 22, weight: FontWeight.Regular),
                        },
                    };
                }
            }

            private class TeamDisplay : DrawableTournamentTeam
            {
                public TeamDisplay(TournamentTeam team)
                    : base(team)
                {
                    AutoSizeAxes = Axes.Both;

                    Flag.RelativeSizeAxes = Axes.None;
                    Flag.Size = new Vector2(300, 200);
                    Flag.Scale = new Vector2(0.3f);

                    InternalChild = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            Flag,
                            new OsuSpriteText
                            {
                                Text = team?.FullName.Value ?? "???",
                                Font = OsuFont.Torus.With(size: 32, weight: FontWeight.SemiBold),
                                Colour = Color4.Black,
                            },
                        }
                    };
                }
            }
        }
    }
}
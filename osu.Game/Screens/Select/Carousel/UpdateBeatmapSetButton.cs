// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select.Carousel
{
    public class UpdateBeatmapSetButton : OsuAnimatedButton
    {
        private readonly BeatmapSetInfo beatmapSetInfo;
        private SpriteIcon icon = null!;
        private Box progressFill = null!;

        public UpdateBeatmapSetButton(BeatmapSetInfo beatmapSetInfo)
        {
            this.beatmapSetInfo = beatmapSetInfo;

            AutoSizeAxes = Axes.Both;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
        }

        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            const float icon_size = 14;

            Content.Anchor = Anchor.CentreLeft;
            Content.Origin = Anchor.CentreLeft;

            Content.AddRange(new Drawable[]
            {
                progressFill = new Box
                {
                    Colour = Color4.White,
                    Alpha = 0.2f,
                    Blending = BlendingParameters.Additive,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0,
                },
                new FillFlowContainer
                {
                    Padding = new MarginPadding { Horizontal = 5, Vertical = 3 },
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(4),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Size = new Vector2(icon_size),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children = new Drawable[]
                            {
                                icon = new SpriteIcon
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.SyncAlt,
                                    Size = new Vector2(icon_size),
                                },
                            }
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.Default.With(weight: FontWeight.Bold),
                            Text = "Update",
                        }
                    }
                },
            });

            beatmapDownloader.DownloadFailed += _ =>
            {
                Hide();

                // Download might have failed because BeatmapSetOnlineAvailability is DownloadDisabled
                // We want to check that so reset online info for now, background processing will update the info
                realm.Run(r =>
                {
                    using (var transaction = r.BeginWrite())
                    {
                        var matchingSet = r.Find<BeatmapSetInfo>(beatmapSetInfo.ID);

                        foreach (BeatmapInfo beatmap in matchingSet.Beatmaps)
                        {
                            // Background processing checks LastOnlineUpdate, that's the only field we need to null
                            beatmap.LastOnlineUpdate = null;
                        }

                        transaction.Commit();
                    }
                });
            };

            Action = () =>
            {
                beatmapDownloader.DownloadAsUpdate(beatmapSetInfo);
                attachExistingDownload();
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            icon.Spin(4000, RotationDirection.Clockwise);
        }

        private void attachExistingDownload()
        {
            var download = beatmapDownloader.GetExistingDownload(beatmapSetInfo);

            if (download != null)
            {
                Enabled.Value = false;
                TooltipText = string.Empty;

                download.DownloadProgressed += progress => progressFill.ResizeWidthTo(progress, 100, Easing.OutQuint);
                download.Failure += _ => attachExistingDownload();
            }
            else
            {
                Enabled.Value = true;
                TooltipText = "Update beatmap with online changes";

                progressFill.ResizeWidthTo(0, 100, Easing.OutQuint);
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            icon.Spin(400, RotationDirection.Clockwise);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            icon.Spin(4000, RotationDirection.Clockwise);
            base.OnHoverLost(e);
        }
    }
}

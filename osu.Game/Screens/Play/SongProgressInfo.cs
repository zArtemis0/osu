﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;

namespace osu.Game.Screens.Play
{
    public class SongProgressInfo : Container
    {
        private OsuSpriteText timeCurrent;
        private OsuSpriteText timeLeft;
        private OsuSpriteText progress;

        private double startTime;
        private double endTime;

        private int? previousPercent;
        private int? previousSecond;

        private double songLength => endTime - startTime;

        private const int margin = 10;

        public double StartTime
        {
            set => startTime = value;
        }

        public double EndTime
        {
            set => endTime = value;
        }

        private GameplayClock gameplayClock;
        private SongProgress songProgressParent;

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, GameplayClock clock, SongProgress parent)
        {
            if (clock != null)
                gameplayClock = clock;

            if (parent != null)
                songProgressParent = parent;

            Children = new Drawable[]
            {
                timeCurrent = new OsuSpriteText
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomLeft,
                    Colour = colours.BlueLighter,
                    Font = OsuFont.Numeric,
                    Margin = new MarginPadding
                    {
                        Left = margin,
                    },
                },
                progress = new OsuSpriteText
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Colour = colours.BlueLighter,
                    Font = OsuFont.Numeric,
                },
                timeLeft = new OsuSpriteText
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomRight,
                    Colour = colours.BlueLighter,
                    Font = OsuFont.Numeric,
                    Margin = new MarginPadding
                    {
                        Right = margin,
                    },
                }
            };

            SongProgressParent.OnUpdate += songProgress =>
            {
                timeCurrent.Rotation = -songProgress.Rotation;
                progress.Rotation = -songProgress.Rotation;
                timeLeft.Rotation = -songProgress.Rotation;

                bool flipVectors = (((int)Math.Floor(songProgress.Rotation / 90)) & 1) == 1;

                if (!flipVectors)
                {
                    timeCurrent.Scale = new osuTK.Vector2 { X = Math.Sign(songProgress.Scale.X), Y = Math.Sign(songProgress.Scale.Y) };
                    progress.Scale = new osuTK.Vector2 { X = Math.Sign(songProgress.Scale.X), Y = Math.Sign(songProgress.Scale.Y) };
                    timeLeft.Scale = new osuTK.Vector2 { X = Math.Sign(songProgress.Scale.X), Y = Math.Sign(songProgress.Scale.Y) };
                }
                else
                {
                    timeCurrent.Scale = new osuTK.Vector2 { Y = Math.Sign(songProgress.Scale.X), X = Math.Sign(songProgress.Scale.Y) };
                    progress.Scale = new osuTK.Vector2 { Y = Math.Sign(songProgress.Scale.X), X = Math.Sign(songProgress.Scale.Y) };
                    timeLeft.Scale = new osuTK.Vector2 { Y = Math.Sign(songProgress.Scale.X), X = Math.Sign(songProgress.Scale.Y) };
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            double time = gameplayClock?.CurrentTime ?? Time.Current;

            double songCurrentTime = time - startTime;
            int currentPercent = Math.Max(0, Math.Min(100, (int)(songCurrentTime / songLength * 100)));
            int currentSecond = (int)Math.Floor(songCurrentTime / 1000.0);

            if (currentPercent != previousPercent)
            {
                progress.Text = currentPercent.ToString() + @"%";
                previousPercent = currentPercent;
            }

            if (currentSecond != previousSecond && songCurrentTime < songLength)
            {
                timeCurrent.Text = formatTime(TimeSpan.FromSeconds(currentSecond));
                timeLeft.Text = formatTime(TimeSpan.FromMilliseconds(endTime - time));

                previousSecond = currentSecond;
            }
        }

        private string formatTime(TimeSpan timeSpan) => $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{Math.Floor(timeSpan.Duration().TotalMinutes)}:{timeSpan.Duration().Seconds:D2}";
    }
}

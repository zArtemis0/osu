﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using System;
using System.Collections.Generic;

namespace osu.Game.Screens.Play
{
    public class SectionTrackOverlay : Container
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION_FOR_EFFECT / 2;
        private const int arrows_appear_offset = 900;

        private List<BreakPeriod> breaks = new List<BreakPeriod>();

        private readonly BindableDouble healthBindable = new BindableDouble
        {
            MinValue = 0,
            MaxValue = 1
        };

        private int currentBreakIndex;
        private double iconAppearTime;
        private bool isBreak;
        private bool iconHasBeenShown;
        private bool arrowsHasBeenShown;
        private bool startArrowsHasBeenShown;
        private double health;
        private readonly double startTime;

        private SampleChannel samplePass;
        private SampleChannel sampleFail;

        private readonly TextAwesome resultIcon;
        private readonly ArrowsOverlay arrows;

        private IClock audioClock;
        public IClock AudioClock { set { audioClock = value; } }

        public List<BreakPeriod> Breaks { set { breaks = value; } }

        public Action BreakIn;
        public Action BreakOut;

        public SectionTrackOverlay(double startTime)
        {
            this.startTime = startTime;

            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                resultIcon = new TextAwesome
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = 100,
                    Alpha = 0,
                },
                arrows = new ArrowsOverlay(),
            };

            healthBindable.ValueChanged += newValue => { health = newValue; };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            samplePass = audio.Sample.Get(@"SectionResult/sectionpass");
            sampleFail = audio.Sample.Get(@"SectionResult/sectionfail");
        }

        public void BindHealth(BindableDouble health) => healthBindable.BindTo(health);

        protected override void Update()
        {
            if (breaks == null) return;
            if (currentBreakIndex == breaks.Count) return;

            double currentTime = audioClock?.CurrentTime ?? Time.Current;

            // Show arrows if we are in the lead-in part
            if (!startArrowsHasBeenShown && startTime - SkipButton.SKIP_REQUIRED_CUTOFF > 0 && currentTime > startTime - arrows_appear_offset)
            {
                arrows.PlayWarning();
                startArrowsHasBeenShown = true;
            }

            var currentBreak = breaks[currentBreakIndex];

            if (!isBreak)
            {
                if (currentTime > currentBreak.StartTime)
                {
                    isBreak = true;

                    if (currentBreak.HasEffect)
                    {
                        iconHasBeenShown = false;
                        arrowsHasBeenShown = false;
                        iconAppearTime = currentTime + (currentBreak.EndTime - currentBreak.StartTime) / 2;
                        BreakIn?.Invoke();
                    }
                }
            }
            else
            {
                // Show icon depends on HP
                if (currentTime > iconAppearTime && !iconHasBeenShown && currentBreak.HasPeriodResult)
                {
                    if (health < 0.3)
                    {
                        resultIcon.Icon = FontAwesome.fa_close;
                        sampleFail.Play();
                    }
                    else
                    {
                        resultIcon.Icon = FontAwesome.fa_check;
                        samplePass.Play();
                    }

                    resultIcon.FadeTo(1);
                    Delay(100);
                    Schedule(() => resultIcon.FadeTo(0));
                    Delay(100);
                    Schedule(() => resultIcon.FadeTo(1));
                    Delay(1000);
                    Schedule(() => resultIcon.FadeTo(0, 200));

                    iconHasBeenShown = true;
                }

                // Show warning arrows
                if (currentBreak.EndTime - currentTime < arrows_appear_offset && !arrowsHasBeenShown)
                {
                    if (currentBreak.HasEffect)
                        arrows.PlayWarning();

                    arrowsHasBeenShown = true;
                }

                // Exit from break
                if (currentBreak.EndTime - currentTime < fade_duration)
                {
                    if (currentBreak.HasEffect)
                        BreakOut?.Invoke();

                    currentBreakIndex++;
                    isBreak = false;
                }
            }
        }

        private class ArrowsOverlay : Container
        {
            private const int appear_duration = 140;
            private const int margin_vertical = 120;
            private const int margin_horizontal = 90;

            private readonly Container content;

            public ArrowsOverlay()
            {
                RelativeSizeAxes = Axes.Both;
                Add(content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new TextAwesome
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Colour = Color4.Red,
                            TextSize = 90,
                            Icon = FontAwesome.fa_arrow_right,
                            Margin = new MarginPadding { Top = margin_vertical, Left = margin_horizontal },
                        },
                        new TextAwesome
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Colour = Color4.Red,
                            TextSize = 90,
                            Icon = FontAwesome.fa_arrow_left,
                            Margin = new MarginPadding { Top = margin_vertical, Right = margin_horizontal },
                        },
                        new TextAwesome
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Colour = Color4.Red,
                            TextSize = 90,
                            Icon = FontAwesome.fa_arrow_right,
                            Margin = new MarginPadding { Bottom = margin_vertical, Left = margin_horizontal },
                        },
                        new TextAwesome
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Colour = Color4.Red,
                            TextSize = 90,
                            Icon = FontAwesome.fa_arrow_left,
                            Margin = new MarginPadding { Bottom = margin_vertical, Right = margin_horizontal },
                        },
                    }
                });
            }

            public void PlayWarning()
            {
                content.FadeTo(1);
                Delay(appear_duration);
                Schedule(() => content.FadeTo(0));
                Delay(appear_duration);
                Schedule(() => content.FadeTo(1));
                Delay(appear_duration);
                Schedule(() => content.FadeTo(0));
                Delay(appear_duration);
                Schedule(() => content.FadeTo(1));
                Delay(appear_duration);
                Schedule(() => content.FadeTo(0));
            }
        }
    }
}

﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using System;
using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using System.Linq;
using osu.Framework.Timing;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Types;
using osu.Game.Configuration;
using osu.Framework.Configuration;

namespace osu.Game.Screens.Play
{
    public class SongProgress : OverlayContainer
    {
        private const int progress_height = 5;

        protected override bool HideOnEscape => false;

        private static readonly Vector2 handle_size = new Vector2(14, 25);

        private const float transition_duration = 200;

        private Bindable<bool> userHidden;

        private readonly SongProgressBar bar;
        private readonly SongProgressGraph graph;

        public Action<double> OnSeek;

        public IClock AudioClock;

        private double lastHitTime => ((objects.Last() as IHasEndTime)?.EndTime ?? objects.Last().StartTime) + 1;

        private IEnumerable<HitObject> objects;

        public IEnumerable<HitObject> Objects
        {
            set
            {
                objects = value;

                const int granularity = 200;

                var interval = lastHitTime / granularity;

                var values = new int[granularity];

                foreach (var h in objects)
                {
                    IHasEndTime end = h as IHasEndTime;

                    int startRange = (int)(h.StartTime / interval);
                    int endRange = (int)((end?.EndTime ?? h.StartTime) / interval);
                    for (int i = startRange; i <= endRange; i++)
                        values[i]++;
                }

                graph.Values = values;
            }
        }

        private bool hasForcedVisibility;
        public bool HasForcedVisibility
        {
            set
            {
                hasForcedVisibility = value;
                userHidden.TriggerChange();
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, OsuColour colours)
        {
            graph.FillColour = bar.FillColour = colours.BlueLighter;

            userHidden = config.GetBindable<bool>(OsuConfig.UserHidden);
            userHidden.ValueChanged += progressVisibility =>
            {
                if (userHidden && !hasForcedVisibility)
                    FadeOut(transition_duration);
                else
                    FadeIn(transition_duration);
            };
            userHidden.TriggerChange();
        }

        public SongProgress()
        {
            RelativeSizeAxes = Axes.X;
            Height = progress_height + SongProgressGraph.Column.HEIGHT + handle_size.Y;
            Y = progress_height;

            Children = new Drawable[]
            {
                graph = new SongProgressGraph
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Height = SongProgressGraph.Column.HEIGHT,
                    Margin = new MarginPadding { Bottom = progress_height },
                },
                bar = new SongProgressBar(progress_height, SongProgressGraph.Column.HEIGHT, handle_size)
                {
                    Alpha = 0,
                    Anchor = Anchor.BottomLeft,
                    Origin =  Anchor.BottomLeft,
                    SeekRequested = delegate (float position)
                    {
                        OnSeek?.Invoke(position);
                    },
                },
            };

            State = Visibility.Visible;
        }

        private bool barVisible;

        public void ToggleBar()
        {
            barVisible = !barVisible;
            updateBarVisibility();
        }

        private void updateBarVisibility()
        {
            bar.FadeTo(barVisible ? 1 : 0, transition_duration, EasingTypes.In);
            MoveTo(new Vector2(0, barVisible ? 0 : progress_height), transition_duration, EasingTypes.In);
        }

        protected override void PopIn()
        {
            updateBarVisibility();
            FadeIn(500, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            FadeOut(100);
        }

        protected override void Update()
        {
            base.Update();

            double progress = (AudioClock?.CurrentTime ?? Time.Current) / lastHitTime;

            bar.UpdatePosition((float)progress);
            graph.Progress = (int)(graph.ColumnCount * progress);

        }
    }
}

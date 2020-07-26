﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays.Rankings;
using osu.Game.Users;
using osu.Game.Rulesets;
using System.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Rankings.Displays;
using System;
using osu.Game.Online.API;

namespace osu.Game.Overlays
{
    public class RankingsOverlay : FullscreenOverlay
    {
        public const float CONTENT_X_MARGIN = 50;

        protected Bindable<RankingsScope> Scope => header.Current;

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; }

        private readonly Container contentContainer;
        private readonly LoadingLayer loading;
        private readonly Box background;
        private readonly RankingsOverlayHeader header;
        private readonly OverlayScrollContainer scrollFlow;

        private CancellationTokenSource cancellationToken;

        private PerformanceRankingsDisplay performanceDisplay;
        private Country lastRequested;

        public RankingsOverlay()
            : base(OverlayColourScheme.Green)
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                scrollFlow = new OverlayScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            header = new RankingsOverlayHeader
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Depth = -float.MaxValue
                            },
                            contentContainer = new Container
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                            }
                        }
                    }
                },
                loading = new LoadingLayer()
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = ColourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            header.Ruleset.BindTo(ruleset);

            Scope.BindValueChanged(scope =>
            {
                if (scope.NewValue != RankingsScope.Performance)
                    lastRequested = null;

                Scheduler.AddOnce(changeDisplay);
            });
        }

        private bool displayUpdateRequired = true;

        protected override void PopIn()
        {
            base.PopIn();

            // We don't want to create a new display on every call, only when exiting from fully closed state.
            if (displayUpdateRequired)
            {
                header.Current.TriggerChange();
                displayUpdateRequired = false;
            }
        }

        protected override void PopOutComplete()
        {
            base.PopOutComplete();
            loadDisplayAsync(Empty());
            displayUpdateRequired = true;
        }

        public void ShowCountry(Country requested)
        {
            if (requested == null)
                return;

            if (performanceDisplay != null)
                performanceDisplay.Country.Value = requested;
            else
                Scope.Value = RankingsScope.Performance;

            lastRequested = requested;
            Show();
        }

        public void ShowSpotlights()
        {
            Scope.Value = RankingsScope.Spotlights;
            Show();
        }

        private void changeDisplay()
        {
            performanceDisplay = null;
            cancellationToken?.Cancel();

            loading.Show();

            if (!API.IsLoggedIn)
            {
                loadDisplayAsync(Empty());
                return;
            }

            loadDisplayAsync(createDisplay());
        }

        private void loadDisplayAsync(Drawable display)
        {
            scrollFlow.ScrollToStart();

            LoadComponentAsync(display, loaded =>
            {
                contentContainer.Child = loaded;
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private Drawable createDisplay()
        {
            switch (Scope.Value)
            {
                case RankingsScope.Country:
                    return new CountryRankingsDisplay
                    {
                        Current = ruleset,
                        StartLoading = loading.Show,
                        FinishLoading = loading.Hide
                    };

                case RankingsScope.Performance:
                    return performanceDisplay = new PerformanceRankingsDisplay
                    {
                        Country = { Value = lastRequested },
                        Current = ruleset,
                        StartLoading = loading.Show,
                        FinishLoading = loading.Hide
                    };

                case RankingsScope.Score:
                    return new ScoreRankingsDisplay
                    {
                        Current = ruleset,
                        StartLoading = loading.Show,
                        FinishLoading = loading.Hide
                    };

                case RankingsScope.Spotlights:
                    return new SpotlightsRankingsDisplay
                    {
                        Current = ruleset,
                        StartLoading = loading.Show,
                        FinishLoading = loading.Hide
                    };
            }

            throw new NotImplementedException($"Display for {Scope.Value} is not implemented.");
        }

        public override void APIStateChanged(IAPIProvider api, APIState state)
        {
            if (State.Value == Visibility.Hidden)
                return;

            Scope.TriggerChange();
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}

﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play;
using System;
using osu.Game.Modes.Scoring;

namespace osu.Game.Modes.UI
{
    public abstract class HudOverlay : Container
    {
        public readonly KeyCounterCollection KeyCounter;
        public readonly ComboCounter ComboCounter;
        public readonly ScoreCounter ScoreCounter;
        public readonly PercentageCounter AccuracyCounter;
        public readonly HealthDisplay HealthDisplay;

        private Bindable<bool> showKeyCounter;
        private Bindable<bool> showHud;

        private bool isVisible;

        public bool IsVisible
        {
            set { isVisible = value; }
            get { return isVisible; }
        }

        protected abstract KeyCounterCollection CreateKeyCounter();
        protected abstract ComboCounter CreateComboCounter();
        protected abstract PercentageCounter CreateAccuracyCounter();
        protected abstract ScoreCounter CreateScoreCounter();
        protected abstract HealthDisplay CreateHealthDisplay();

        protected HudOverlay()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                KeyCounter = CreateKeyCounter(),
                ComboCounter = CreateComboCounter(),
                ScoreCounter = CreateScoreCounter(),
                AccuracyCounter = CreateAccuracyCounter(),
                HealthDisplay = CreateHealthDisplay(),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            showKeyCounter = config.GetBindable<bool>(OsuConfig.KeyOverlay);
            showKeyCounter.ValueChanged += keyCounterVisibilityChanged;
            showKeyCounter.TriggerChange();

            showHud = config.GetBindable<bool>(OsuConfig.ShowInterface);
            showHud.ValueChanged += hudVisibilityChanged;
            showHud.TriggerChange();
        }

        private void keyCounterVisibilityChanged(object sender, EventArgs e)
        {
            if (showKeyCounter)
                KeyCounter.Show();
            else
                KeyCounter.Hide();
        }

        private void hudVisibilityChanged(object sender, EventArgs e)
        {
            if (showHud)
            {
                IsVisible = true;
                Show();
            }
            else
            {
                IsVisible = false;
                Hide();
            }
        }

        public void ChangeVisibility()
        {
            showHud.Value = !showHud.Value;
        }

        public void BindProcessor(ScoreProcessor processor)
        {
            //bind processor bindables to combocounter, score display etc.
            //TODO: these should be bindable binds, not events!
            ScoreCounter?.Current.BindTo(processor.TotalScore);
            AccuracyCounter?.Current.BindTo(processor.Accuracy);
            ComboCounter?.Current.BindTo(processor.Combo);
            HealthDisplay?.Current.BindTo(processor.Health);
        }

        public void BindHitRenderer(HitRenderer hitRenderer)
        {
            hitRenderer.InputManager.Add(KeyCounter.GetReceptor());
        }
    }
}

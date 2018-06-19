﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Screens.Backgrounds;

namespace osu.Game.Screens.Play
{
    public abstract class ScreenWithBeatmapBackground : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

        public override bool AllowBeatmapRulesetChange => false;

        protected const float BACKGROUND_FADE_DURATION = 800;

        protected float BackgroundOpacity => 1 - (float)DimLevel;

        #region User Settings

        protected Bindable<double> DimLevel;
        protected Bindable<double> DimColour;
        protected Bindable<double> BlurLevel;
        protected Bindable<bool> ShowStoryboard;

        #endregion

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            DimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            DimColour = config.GetBindable<double>(OsuSetting.DimColour);
            BlurLevel = config.GetBindable<double>(OsuSetting.BlurLevel);
            ShowStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            DimLevel.ValueChanged += _ => UpdateBackgroundElements();
            DimColour.ValueChanged += _ => UpdateBackgroundElements();
            BlurLevel.ValueChanged += _ => UpdateBackgroundElements();
            ShowStoryboard.ValueChanged += _ => UpdateBackgroundElements();
            UpdateBackgroundElements();
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            UpdateBackgroundElements();
        }

        protected virtual void UpdateBackgroundElements()
        {
            if (!IsCurrentScreen) return;
            if (Background is BackgroundScreenBeatmap bg && bg != null) {
                bg.FadeTo(BackgroundOpacity, BACKGROUND_FADE_DURATION, Easing.OutQuint);
                bg.BlurTo(new Vector2((float)BlurLevel.Value * 25), BACKGROUND_FADE_DURATION, Easing.OutQuint);
                bg.DimColour = OsuColour.Gray((float)DimColour);
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;

namespace osu.Game.Skinning
{
    public partial class LegacyKiaiFlashingDrawable : BeatSyncedContainer
    {
        private readonly Drawable flashingDrawable;
        private Bindable<float> flashOpacity = new Bindable<float>();
        private float opacityValue => flashOpacity.Value;

        public LegacyKiaiFlashingDrawable(Func<Drawable?> creationFunc)
        {
            AutoSizeAxes = Axes.Both;

            Children = new[]
            {
                (creationFunc.Invoke() ?? Empty()).With(d =>
                {
                    d.Anchor = Anchor.Centre;
                    d.Origin = Anchor.Centre;
                }),
                flashingDrawable = (creationFunc.Invoke() ?? Empty()).With(d =>
                {
                    d.Anchor = Anchor.Centre;
                    d.Origin = Anchor.Centre;
                    d.Alpha = 0;
                    d.Blending = BlendingParameters.Additive;
                })
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            flashOpacity = config.GetBindable<float>(OsuSetting.GlowStrength);
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
        {
            if (!effectPoint.KiaiMode)
                return;

            Logger.Log($"Current Kiai Glow: {opacityValue}");

            flashingDrawable
                .FadeTo(opacityValue)
                .Then()
                .FadeOut(Math.Max(80, timingPoint.BeatLength - 80), Easing.OutSine);
        }
    }
}

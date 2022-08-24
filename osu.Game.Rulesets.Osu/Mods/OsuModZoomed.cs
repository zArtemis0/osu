// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModZoomed : Mod, IUpdatableByPlayfield, IApplicableToScoreProcessor, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Zoomed";
        public override string Acronym => "ZM";
        public override IconUsage? Icon => FontAwesome.Solid.Glasses;
        public override ModType Type => ModType.Fun;
        public override string Description => "Big brother is watching your cursor.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModBarrelRoll) };

        private const int apply_zoom_duration = 1000;

        [SettingSource("Delay", "Delay in milliseconds for the view to catch up to the cursor")]
        public BindableInt MovementDelay { get; } = new BindableInt
        {
            MinValue = 0,
            MaxValue = 1000,
            Precision = 100,
        };

        [SettingSource("Initial zoom", "The starting zoom level")]
        public BindableDouble InitialZoom { get; } = new BindableDouble(1.8)
        {
            MinValue = 1.5,
            MaxValue = 2,
            Precision = 0.05
        };

        [SettingSource("Final zoom combo", "The combo count at which point the zoom level stops increasing.", SettingControlType = typeof(SettingsSlider<int, OsuSliderBar<int>>))]
        public BindableInt FinalZoomCombo { get; } = new BindableInt(200)
        {
            MinValue = 0,
            MaxValue = 500,
            Precision = 100
        };

        private double currentZoom;

        private readonly BindableInt currentCombo = new BindableInt();

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            currentCombo.MaxValue = FinalZoomCombo.Value;
            currentZoom = InitialZoom.Value;
        }

        public void Update(Playfield playfield)
        {
            double currentScale = Interpolation.ValueAt(Math.Min(Math.Abs(playfield.Clock.ElapsedFrameTime), apply_zoom_duration), playfield.Scale.X, currentZoom, 0, apply_zoom_duration, Easing.Out);

            playfield.Scale = new Vector2((float)currentScale);

            moveDrawablesFollowingCursor(playfield);
        }

        private Vector2 getTrackingPosition(Playfield playfield)
        {
            var position = playfield.Cursor.ActiveCursor.DrawPosition;
            return Vector2.Clamp(playfield.OriginPosition - position, -playfield.OriginPosition, playfield.OriginPosition);
        }

        private void moveDrawablesFollowingCursor(Playfield playfield)
        {
            var trackingPosition = getTrackingPosition(playfield);

            if (MovementDelay.Value == 0)
                playfield.Position = trackingPosition;
            else
            {
                playfield.Position = Interpolation.ValueAt(
                    Math.Min(Math.Abs(playfield.Clock.ElapsedFrameTime), MovementDelay.Value), playfield.Position, trackingPosition, 0, MovementDelay.Value, Easing.Out);
            }
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            currentCombo.BindTo(scoreProcessor.Combo);
            currentCombo.ValueChanged += e => currentZoom = getZoomForCombo(e.NewValue);
        }

        private double getZoomForCombo(int combo)
        {
            double increaseRatio = combo / 1000;
            return InitialZoom.Value + increaseRatio;
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy)
        {
            switch (rank)
            {
                case ScoreRank.X:
                    return ScoreRank.XH;

                case ScoreRank.S:
                    return ScoreRank.SH;

                default:
                    return rank;
            }
        }
    }
}

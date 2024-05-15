// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Localisation;
using osu.Game.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuAnalysisSettings : AnalysisSettings
    {
        protected new DrawableOsuRuleset DrawableRuleset => (DrawableOsuRuleset)base.DrawableRuleset;

        private readonly PlayerCheckbox hitMarkerToggle;
        private readonly PlayerCheckbox aimMarkerToggle;
        private readonly PlayerCheckbox aimLinesToggle;

        public OsuAnalysisSettings(DrawableRuleset drawableRuleset)
            : base(drawableRuleset)
        {
            PlayerCheckbox hideCursorToggle;

            Children = new Drawable[]
            {
                hitMarkerToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.HitMarkers },
                aimMarkerToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.AimMarkers },
                aimLinesToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.AimLines },
                hideCursorToggle = new PlayerCheckbox { LabelText = PlayerSettingsOverlayStrings.HideCursor }
            };

            hideCursorToggle.Current.BindValueChanged(onCursorToggle);
        }

        private void onCursorToggle(ValueChangedEvent<bool> hide)
        {
            // this only hides half the cursor
            if (hide.NewValue)
            {
                DrawableRuleset.Playfield.Cursor.FadeOut();
            }
            else
            {
                DrawableRuleset.Playfield.Cursor.FadeIn();
            }
        }

        public override AnalysisContainer CreateAnalysisContainer(Replay replay)
        {
            var analysisContainer = new OsuAnalysisContainer(replay);
            analysisContainer.HitMarkerEnabled.BindTo(hitMarkerToggle.Current);
            analysisContainer.AimMarkersEnabled.BindTo(aimMarkerToggle.Current);
            analysisContainer.AimLinesEnabled.BindTo(aimLinesToggle.Current);
            return analysisContainer;
        }
    }
}

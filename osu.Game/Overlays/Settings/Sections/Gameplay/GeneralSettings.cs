// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public partial class GeneralSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.GeneralHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsEnumDropdown<ScoringMode>
                {
                    ClassicDefault = ScoringMode.Classic,
                    LabelText = GameplaySettingsStrings.ScoreDisplayMode,
                    Current = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode),
                    Keywords = new[] { "scoring" }
                },
                new SettingsCheckbox
                {
                    LabelText = GraphicsSettingsStrings.HitLighting,
                    Current = config.GetBindable<bool>(OsuSetting.HitLighting)
                },
                new SettingsSlider<float>
                {
                    LabelText = GraphicsSettingsStrings.KiaiFlashIntensity,
                    TransferValueOnCommit = true,
                    Current = config.GetBindable<float>(OsuSetting.KiaiFlashIntensity),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                    Keywords = new[] { @"glow", @"highlight" },
                }
            };
        }
    }
}

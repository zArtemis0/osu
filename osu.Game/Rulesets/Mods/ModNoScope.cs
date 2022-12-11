// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNoScope : Mod, IApplicableToScoreProcessor, IApplicableToPlayer, ICanBeToggledDuringReplay
    {
        public override string Name => "No Scope";
        public override string Acronym => "NS";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => FontAwesome.Solid.EyeSlash;
        public override double ScoreMultiplier => 1;

        /// <summary>
        /// Slightly higher than the cutoff for <see cref="Drawable.IsPresent"/>.
        /// </summary>
        protected const float MIN_ALPHA = 0.0002f;

        protected const float TRANSITION_DURATION = 100;

        protected readonly BindableNumber<int> CurrentCombo = new BindableInt();

        protected readonly IBindable<bool> IsBreakTime = new Bindable<bool>();

        protected float ComboBasedAlpha = MIN_ALPHA;

        public BindableBool IsDisabled { get; } = new BindableBool();

        [SettingSource(
            "Hidden at combo",
            "The combo count at which the cursor becomes completely hidden",
            SettingControlType = typeof(SettingsSlider<int, HiddenComboSlider>)
        )]
        public abstract BindableInt HiddenComboCount { get; }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void ApplyToPlayer(Player player)
        {
            IsBreakTime.BindTo(player.IsBreakTime);
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (HiddenComboCount.Value == 0)
            {
                IsDisabled.BindValueChanged(s =>
                {
                    ComboBasedAlpha = IsDisabled.Value ? 1 : MIN_ALPHA;
                });
                return;
            }

            CurrentCombo.BindTo(scoreProcessor.Combo);
            CurrentCombo.BindValueChanged(combo =>
            {
                if (IsDisabled.Value) return;

                ComboBasedAlpha = updateComboBasedAlpha(combo.NewValue);
            }, true);

            IsDisabled.BindValueChanged(s =>
            {
                ComboBasedAlpha = s.NewValue ? 1 : updateComboBasedAlpha(CurrentCombo.Value);
            });

            float updateComboBasedAlpha(int combo) => Math.Max(MIN_ALPHA, 1 - (float)combo / HiddenComboCount.Value);
        }
    }

    public partial class HiddenComboSlider : OsuSliderBar<int>
    {
        public override LocalisableString TooltipText => Current.Value == 0 ? "always hidden" : base.TooltipText;
    }
}

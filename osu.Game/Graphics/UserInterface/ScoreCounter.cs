﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Graphics.UserInterface
{
    public class ScoreCounter : RollingCounter<double>
    {
        protected override double RollingDuration => 1000;
        protected override Easing RollingEasing => Easing.Out;
        public bool UseCommaSeparator;

        private Bindable<double> overlayDim = new Bindable<double>();

        /// <summary>
        /// How many leading zeroes the counter has.
        /// </summary>
        public uint LeadingZeroes { get; }

        /// <summary>
        /// Displays score.
        /// </summary>
        /// <param name="leading">How many leading zeroes the counter will have.</param>
        public ScoreCounter(uint leading = 0)
        {
            DisplayedCountSpriteText.Font = DisplayedCountSpriteText.Font.With(fixedWidth: true);
            Console.WriteLine(overlayDim.Value);
            LeadingZeroes = leading;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager config) {
            AccentColour = colours.BlueLighter;
            overlayDim.BindTo(config.GetBindable<double>(OsuSetting.OverlayDim));
            DisplayedCountSpriteText.Alpha = 1 - (float)overlayDim.Value;
        }

        protected override double GetProportionalDuration(double currentValue, double newValue)
        {
            return currentValue > newValue ? currentValue - newValue : newValue - currentValue;
        }

        protected override string FormatCount(double count)
        {
            string format = new string('0', (int)LeadingZeroes);

            if (UseCommaSeparator)
            {
                for (int i = format.Length - 3; i > 0; i -= 3)
                    format = format.Insert(i, @",");
            }

            return ((long)count).ToString(format);
        }

        public override void Increment(double amount)
        {
            Current.Value += amount;
        }
    }
}

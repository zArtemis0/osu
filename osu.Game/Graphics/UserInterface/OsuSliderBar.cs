﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Overlays;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public partial class OsuSliderBar<T> : SliderBar<T>, IHasTooltip, IHasAccentColour
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        /// <summary>
        /// Maximum number of decimal digits to be displayed in the tooltip.
        /// </summary>
        private const int max_decimal_digits = 5;

        protected readonly Nub Nub;
        private readonly SliderSounds<T> sounds;
        private readonly Container nubContainer;
        protected Box RightBox;
        protected Box LeftBox;

        public virtual LocalisableString TooltipText { get; private set; }

        public bool PlaySamplesOnAdjust { get; set; } = true;

        private readonly HoverClickSounds hoverClickSounds;

        /// <summary>
        /// Whether to format the tooltip as a percentage or the actual value.
        /// </summary>
        public bool DisplayAsPercentage { get; set; }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                LeftBox.Colour = value;
            }
        }

        private Colour4 backgroundColour;

        public Color4 BackgroundColour
        {
            get => backgroundColour;
            set
            {
                backgroundColour = value;
                RightBox.Colour = value;
            }
        }

        public OsuSliderBar()
        {
            Height = Nub.HEIGHT;
            RangePadding = Nub.EXPANDED_SIZE / 2;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Padding = new MarginPadding { Horizontal = 2 },
                    Child = new CircularContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Masking = true,
                        CornerRadius = 5f,
                        Children = new Drawable[]
                        {
                            RightBox = new Box
                            {
                                Height = 5,
                                EdgeSmoothness = new Vector2(0, 0.5f),
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight
                            },
                            LeftBox = new Box
                            {
                                Height = 5,
                                EdgeSmoothness = new Vector2(0, 0.5f),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft
                            }
                        }
                    },
                },
                nubContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = Nub = new Nub
                    {
                        Origin = Anchor.TopCentre,
                        RelativePositionAxes = Axes.X,
                        Current = { Value = true }
                    },
                },
                sounds = new SliderSounds<T>(),
                hoverClickSounds = new HoverClickSounds()
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? colourProvider, OsuColour colours)
        {
            AccentColour = colourProvider?.Highlight1 ?? colours.Pink;
            BackgroundColour = colourProvider?.Background5 ?? colours.PinkDarker.Darken(1);
        }

        protected override void Update()
        {
            base.Update();

            nubContainer.Padding = new MarginPadding { Horizontal = RangePadding };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CurrentNumber.BindValueChanged(current => TooltipText = getTooltipText(current.NewValue), true);

            Current.BindDisabledChanged(disabled =>
            {
                Alpha = disabled ? 0.3f : 1;
                hoverClickSounds.Enabled.Value = !disabled;
            }, true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateGlow();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateGlow();
            base.OnHoverLost(e);
        }

        protected override bool ShouldHandleAsRelativeDrag(MouseDownEvent e)
            => Nub.ReceivePositionalInputAt(e.ScreenSpaceMouseDownPosition);

        protected override void OnDragEnd(DragEndEvent e)
        {
            updateGlow();
            base.OnDragEnd(e);
        }

        private void updateGlow()
        {
            Nub.Glowing = !Current.Disabled && (IsHovered || IsDragged);
        }

        protected override void OnUserChange(T value)
        {
            base.OnUserChange(value);
            TooltipText = getTooltipText(value);

            if (PlaySamplesOnAdjust)
                sounds.PlaySample(value, NormalizedValue);
        }

        private LocalisableString getTooltipText(T value)
        {
            if (CurrentNumber.IsInteger)
                return value.ToInt32(NumberFormatInfo.InvariantInfo).ToString("N0");

            double floatValue = value.ToDouble(NumberFormatInfo.InvariantInfo);

            if (DisplayAsPercentage)
                return floatValue.ToString("0%");

            decimal decimalPrecision = normalise(CurrentNumber.Precision.ToDecimal(NumberFormatInfo.InvariantInfo), max_decimal_digits);

            // Find the number of significant digits (we could have less than 5 after normalize())
            int significantDigits = FormatUtils.FindPrecision(decimalPrecision);

            return floatValue.ToString($"N{significantDigits}");
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            LeftBox.Scale = new Vector2(Math.Clamp(RangePadding + Nub.DrawPosition.X - Nub.DrawWidth / 2, 0, Math.Max(0, DrawWidth)), 1);
            RightBox.Scale = new Vector2(Math.Clamp(DrawWidth - Nub.DrawPosition.X - RangePadding - Nub.DrawWidth / 2, 0, Math.Max(0, DrawWidth)), 1);
        }

        protected override void UpdateValue(float value)
        {
            Nub.MoveToX(value, 250, Easing.OutQuint);
        }

        /// <summary>
        /// Removes all non-significant digits, keeping at most a requested number of decimal digits.
        /// </summary>
        /// <param name="d">The decimal to normalize.</param>
        /// <param name="sd">The maximum number of decimal digits to keep. The final result may have fewer decimal digits than this value.</param>
        /// <returns>The normalised decimal.</returns>
        private decimal normalise(decimal d, int sd)
            => decimal.Parse(Math.Round(d, sd).ToString(string.Concat("0.", new string('#', sd)), CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }
}

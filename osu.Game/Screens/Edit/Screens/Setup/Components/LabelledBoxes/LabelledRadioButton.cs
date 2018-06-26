﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes
{
    public class LabelledRadioButton : CompositeDrawable
    {
        private readonly Container outerContainer;
        private readonly Box box;
        private readonly Container content;
        private readonly OsuSpriteText label;
        private readonly OsuSpriteText bottomText;
        private readonly OsuRadioButton radioButton;

        public const float DEFAULT_LABEL_TEXT_SIZE = 20;
        public const float DEFAULT_BOTTOM_LABEL_TEXT_SIZE = 14;
        public const float NORMAL_HEIGHT = 50;
        public const float DEFAULT_LABEL_PADDING = 15;
        public const float DEFAULT_TOP_PADDING = 15;
        public const float DEFAULT_BOTTOM_PADDING = 15;

        public event Action<bool> RadioButtonValueChanged;

        public void TriggerRadioButtonValueChanged(bool newValue)
        {
            RadioButtonValueChanged?.Invoke(newValue);
        }

        public bool CurrentValue
        {
            get => radioButton.Current.Value;
            set => radioButton.Current.Value = value;
        }

        private string labelText;
        public string LabelText
        {
            get => labelText;
            set
            {
                labelText = value;
                label.Text = value;
            }
        }

        private string bottomLabelText;
        public string BottomLabelText
        {
            get => bottomLabelText;
            set
            {
                bottomLabelText = value;
                bottomText.Text = value;
                changeHeight(NORMAL_HEIGHT + (value != "" ? 25 : 0));
            }
        }

        private float labelTextSize;
        public float LabelTextSize
        {
            get => labelTextSize;
            set
            {
                labelTextSize = value;
                label.TextSize = value;
            }
        }

        public MarginPadding Padding
        {
            get => base.Padding;
            set
            {
                base.Padding = value;
                Height = Height + base.Padding.Top;
            }
        }

        public MarginPadding LabelPadding
        {
            get => label.Padding;
            set => label.Padding = value;
        }

        public MarginPadding RadioButtonPadding
        {
            get => radioButton.Padding;
            set => radioButton.Padding = value;
        }

        public Color4 LabelTextColour
        {
            get => label.Colour;
            set => label.Colour = value;
        }

        public Color4 BackgroundColour
        {
            get => content.Colour;
            set => content.Colour = value;
        }

        public LabelledRadioButton()
        {
            Masking = true;
            CornerRadius = 15;
            RelativeSizeAxes = Axes.X;
            Height = NORMAL_HEIGHT + Padding.Top;

            InternalChildren = new Drawable[]
            {
                outerContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = NORMAL_HEIGHT,
                    CornerRadius = 15,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        box = new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = NORMAL_HEIGHT,
                            Colour = OsuColour.FromHex("1c2125"),
                        },
                        content = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = NORMAL_HEIGHT,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = NORMAL_HEIGHT,
                                    Children = new Drawable[]
                                    {
                                        label = new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            Padding = new MarginPadding { Left = DEFAULT_LABEL_PADDING, Top = DEFAULT_TOP_PADDING },
                                            Colour = Color4.White,
                                            TextSize = DEFAULT_LABEL_TEXT_SIZE,
                                            Text = LabelText,
                                            Font = @"Exo2.0-Bold",
                                        },
                                        radioButton = new OsuRadioButton
                                        {
                                            Anchor = Anchor.TopLeft,
                                            Origin = Anchor.TopLeft,
                                            Padding = new MarginPadding { Left = DEFAULT_LABEL_PADDING, Top = DEFAULT_TOP_PADDING }, // TODO: Test the right padding
                                            Alpha = 1,
                                            //Colour = Color4.White,
                                        },
                                    },
                                },
                                bottomText = new OsuSpriteText
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Colour = Color4.Yellow,
                                    Padding = new MarginPadding { Left = DEFAULT_LABEL_PADDING, Bottom = DEFAULT_BOTTOM_PADDING },
                                    TextSize = DEFAULT_BOTTOM_LABEL_TEXT_SIZE,
                                    Font = @"Exo2.0-BoldItalic",
                                    Text = BottomLabelText
                                },
                            }
                        }
                    }
                }
            };
            radioButton.Current.ValueChanged += delegate { TriggerRadioButtonValueChanged(radioButton.Current.Value); };
        }

        private void changeHeight(float newHeight)
        {
            Height = newHeight + Padding.Top;
            content.Height = newHeight;
            box.Height = newHeight;
            outerContainer.Height = newHeight;
        }
    }
}

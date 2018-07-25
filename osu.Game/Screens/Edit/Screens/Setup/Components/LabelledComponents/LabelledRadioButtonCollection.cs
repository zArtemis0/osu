﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;
using System.Collections.Generic;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents
{
    public class LabelledRadioButtonCollection : CompositeDrawable
    {
        private readonly Container content;
        private readonly Box box;
        private readonly OsuSpriteText label;
        private readonly OsuSpriteText bottomText;
        private readonly OsuSetupRadioButtonCollection radioButtonCollection;

        public const float LABEL_CONTAINER_WIDTH = 150;
        public const float OUTER_CORNER_RADIUS = 15;
        public const float DEFAULT_LABEL_TEXT_SIZE = 16;
        public const float DEFAULT_BOTTOM_LABEL_TEXT_SIZE = 12;
        public const float NORMAL_HEIGHT = 40;
        public const float DEFAULT_LABEL_PADDING = 15;
        public const float DEFAULT_TOP_PADDING = 12;
        public const float DEFAULT_BOTTOM_PADDING = 12;

        public event Action<OsuSetupRadioButton> SelectedRadioButtonChanged;

        public void TriggerSelectedRadioButtonChanged(OsuSetupRadioButton newSelected)
        {
            SelectedRadioButtonChanged?.Invoke(newSelected);
        }
        
        public string LabelText
        {
            get => label.Text;
            set => label.Text = value;
        }
        
        public string BottomLabelText
        {
            get => bottomText.Text;
            set
            {
                bottomText.Text = value;
                Height = NORMAL_HEIGHT + (value != "" ? 20 : 0);
            }
        }
        
        public float LabelTextSize
        {
            get => label.TextSize;
            set => label.TextSize = value;
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

        public OsuSetupRadioButton CurrentSelection
        {
            get => radioButtonCollection.SelectedRadioButton;
            set => radioButtonCollection.SelectedRadioButton = value;
        }

        public IEnumerable<OsuSetupRadioButton> Items
        {
            get => radioButtonCollection.Items;
            set => radioButtonCollection.Items = value;
        }

        public LabelledRadioButtonCollection()
        {
            Masking = true;
            CornerRadius = OUTER_CORNER_RADIUS;
            RelativeSizeAxes = Axes.X;
            Height = NORMAL_HEIGHT;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex("1c2125"),
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                label = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Padding = new MarginPadding { Left = DEFAULT_LABEL_PADDING, Top = DEFAULT_TOP_PADDING },
                                    Colour = Color4.White,
                                    TextSize = DEFAULT_LABEL_TEXT_SIZE,
                                    Font = @"Exo2.0-Bold",
                                },
                                radioButtonCollection = new OsuSetupRadioButtonCollection
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Position = new Vector2(LABEL_CONTAINER_WIDTH, 10),
                                },
                            },
                        },
                        bottomText = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Padding = new MarginPadding { Left = DEFAULT_LABEL_PADDING, Bottom = DEFAULT_BOTTOM_PADDING },
                            TextSize = DEFAULT_BOTTOM_LABEL_TEXT_SIZE,
                            Font = @"Exo2.0-BoldItalic",
                        },
                    }
                }
            };

            radioButtonCollection.SelectedRadioButtonChanged += SelectedRadioButtonChanged;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            bottomText.Colour = osuColour.Yellow;
        }
    }
}

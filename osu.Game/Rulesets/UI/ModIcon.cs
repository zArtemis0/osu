﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osuTK;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Localisation;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Display the specified mod at a fixed size.
    /// </summary>
    public partial class ModIcon : Container, IHasTooltip
    {
        public readonly BindableBool Selected = new BindableBool();

        private readonly SpriteText modIcon;
        private readonly SpriteText modAcronym;
        private readonly SpriteIcon background;

        private const float size = 80;

        /// <remarks>
        /// The font size for the mods font should be the height of the mod background.
        /// The aspect ratio of the mod background is 10:7.
        /// </remarks>
        private const float mods_icon_font_size = size * .7f;

        /// <remarks>
        /// Other fonts are scaled down to match the apparent height of icons in the mods font.
        /// </remarks>
        private const float other_icon_font_size = mods_icon_font_size * .75f;

        public virtual LocalisableString TooltipText => showTooltip ? ((mod as Mod)?.IconTooltip ?? mod.Name) : null;

        private IMod mod;
        private readonly bool showTooltip;

        public IMod Mod
        {
            get => mod;
            set
            {
                mod = value;

                if (IsLoaded)
                    updateMod(value);
            }
        }

        [Resolved]
        private OsuColour colours { get; set; }

        private Color4 backgroundColour;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="mod">The mod to be displayed</param>
        /// <param name="showTooltip">Whether a tooltip describing the mod should display on hover.</param>
        public ModIcon(IMod mod, bool showTooltip = true)
        {
            this.mod = mod ?? throw new ArgumentNullException(nameof(mod));
            this.showTooltip = showTooltip;

            Size = new Vector2(size);

            Children = new Drawable[]
            {
                background = new SpriteIcon
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(size),
                    Icon = OsuIcon.ModBg,
                    Shadow = true,
                },
                modAcronym = new OsuSpriteText
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 0,
                    Font = OsuFont.Numeric.With(null, 22f),
                    UseFullGlyphHeight = false,
                },
                modIcon = new OsuSpriteText
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 0,
                    Shadow = false,
                    UseFullGlyphHeight = false,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Selected.BindValueChanged(_ => updateColour());

            updateMod(mod);
        }

        private void updateMod(IMod value)
        {
            var foregroundColour = value.Type == ModType.System ? Color4Extensions.FromHex(@"fc2") : colours.Gray5;

            if (value.Icon is IconUsage icon)
            {
                modIcon.Colour = foregroundColour;
                modIcon.Font = new FontUsage
                (
                    icon.Family,
                    icon.Family == @"osuModsFont" ? mods_icon_font_size : other_icon_font_size,
                    icon.Weight
                );
                modIcon.Text = icon.Icon.ToString();

                modIcon.FadeIn();
                modAcronym.FadeOut();
            }
            else
            {
                modAcronym.Colour = foregroundColour;
                modAcronym.Text = value.Acronym;

                modIcon.FadeOut();
                modAcronym.FadeIn();
            }

            backgroundColour = colours.ForModType(value.Type);
            updateColour();
        }

        private void updateColour()
        {
            background.Colour = Selected.Value ? backgroundColour.Lighten(0.2f) : backgroundColour;
        }
    }
}

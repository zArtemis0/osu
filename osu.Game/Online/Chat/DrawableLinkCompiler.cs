﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// An invisible drawable that brings multiple <see cref="Drawable"/> pieces together to form a consumable clickable link.
    /// </summary>
    public class DrawableLinkCompiler : OsuHoverContainer
    {
        /// <summary>
        /// Each word part of a chat link (split for word-wrap support).
        /// </summary>
        public List<Drawable> Parts;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Parts.Any(d => d.ReceivePositionalInputAt(screenSpacePos));

        protected override HoverClickSounds CreateHoverClickSounds(HoverSampleSet sampleSet) => new LinkHoverSounds(sampleSet, Parts);

        private readonly bool ignoreColourProvider;

        public DrawableLinkCompiler(IEnumerable<Drawable> parts, bool ignoreColourProvider = false)
        {
            Parts = parts.ToList();
            this.ignoreColourProvider = ignoreColourProvider;
        }

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] OverlayColourProvider colourProvider, [NotNull] OsuColour colours)
        {
            IdleColour = (ignoreColourProvider || colourProvider == null) ? colours.Blue : colourProvider.Light2;
        }

        protected override IEnumerable<Drawable> EffectTargets => Parts;

        private class LinkHoverSounds : HoverClickSounds
        {
            private readonly List<Drawable> parts;

            public LinkHoverSounds(HoverSampleSet sampleSet, List<Drawable> parts)
                : base(sampleSet)
            {
                this.parts = parts;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => parts.Any(d => d.ReceivePositionalInputAt(screenSpacePos));
        }
    }
}

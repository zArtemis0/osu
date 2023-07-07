﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osuTK;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// An item to be displayed in a row of statistics inside the results screen.
    /// </summary>
    public class StatisticItem
    {
        /// <summary>
        /// The name of this item.
        /// </summary>
        public readonly LocalisableString Name;

        /// <summary>
        /// A function returning the <see cref="Drawable"/> content to be displayed.
        /// </summary>
        public readonly Func<Drawable> CreateContent;

        /// <summary>
        /// The relative size of the statistics panel which should be consumed by this item.
        /// </summary>
        /// <remarks>
        /// A relative size of (1, 1) implies one full screen (before scrolling).
        /// </remarks>
        public readonly Vector2 RelativeSize;

        /// <summary>
        /// Whether this item requires hit events. If true, <see cref="CreateContent"/> will not be called if no hit events are available.
        /// </summary>
        public readonly bool RequiresHitEvents;

        /// <summary>
        /// Creates a new <see cref="StatisticItem"/>, to be displayed in the results screen.
        /// </summary>
        /// <param name="name">The name of the item. Can be <see langword="null"/> to hide the item header.</param>
        /// <param name="createContent">A function returning the <see cref="Drawable"/> content to be displayed.</param>
        /// <param name="requiresHitEvents">Whether this item requires hit events. If true, <see cref="CreateContent"/> will not be called if no hit events are available.</param>
        /// <param name="relativeSize">The relative size of the statistics panel which should be consumed by this item.</param>
        public StatisticItem(LocalisableString name, Func<Drawable> createContent, bool requiresHitEvents = false, Vector2? relativeSize = null)
        {
            Name = name;
            RequiresHitEvents = requiresHitEvents;
            CreateContent = createContent;
            RelativeSize = relativeSize ?? Vector2.One;
        }
    }
}

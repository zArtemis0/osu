﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Beatmaps.ControlPoints
{
    public abstract class ControlPoint : IComparable<ControlPoint>, IEquatable<ControlPoint>
    {
        /// <summary>
        /// The time at which the control point takes effect.
        /// </summary>
        public double Time;

        /// <summary>
        /// Whether this timing point was generated internally, as opposed to parsed from the underlying beatmap.
        /// </summary>
        internal bool AutoGenerated;

        public int CompareTo(ControlPoint other) => Time.CompareTo(other.Time);

        /// <summary>
        /// Whether this control point is equivalent to another, ignoring time.
        /// </summary>
        /// <param name="other">Another control point to compare with.</param>
        /// <returns>Whether equivalent.</returns>
        public abstract bool EquivalentTo(ControlPoint other);

        public bool Equals(ControlPoint other) => Time.Equals(other?.Time) && EquivalentTo(other);
    }
}

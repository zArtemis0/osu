// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace osu.Game.Rulesets.Difficulty.Utils
{
    internal class ObjectLink<T> : IReadOnlyList<T>
    {
        /// <summary>
        /// The number of linked objects.
        /// </summary>
        public int Count { get; private set; }

        private readonly T obj;
        private readonly ObjectLink<T>? previous;

        public ObjectLink(T obj, ObjectLink<T>? previous)
        {
            this.obj = obj;
            this.previous = previous;

            Count = (previous?.Count + 1) ?? 0;
        }

        public T this[int index]
        {
            get
            {
                if (previous == null)
                    throw new InvalidOperationException("Cannot index when there are no previous objects.");

                return index == 0 ? previous.obj : previous[index - 1];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException();
    }
}

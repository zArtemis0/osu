// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableList<T> : RearrangeableListContainer<T>, IDrawableListItem<T>
        where T : Drawable
    {
        protected Action<SelectionState> SelectAll;

        Action<SelectionState> IDrawableListItem<T>.SelectAll
        {
            get => SelectAll;
            set => SelectAll = value;
        }

        public DrawableList()
        {
            SelectAll = ((IDrawableListItem<T>)this).SelectableOnStateChanged;
            RelativeSizeAxes = Axes.X;
            //todo: compute this somehow add runtime
            Height = 100f;
            ListContainer.Spacing = new Vector2(2.5f);
            SelectAll = ((IDrawableListItem<T>)this).SelectableOnStateChanged;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button == MouseButton.Left)
                SelectAll(SelectionState.NotSelected);
            base.OnClick(e);
            return e.Button == MouseButton.Left;
        }

        public void AddRange(IEnumerable<T>? drawables)
        {
            if (drawables is null) return;

            foreach (T drawable in drawables)
            {
                Add(drawable);
            }
        }

        public void Add(T? drawable)
        {
            if (drawable is null) return;

            addInternal(drawable);
        }

        private void addInternal(T drawable)
        {
            if (Items.Contains(drawable)) return;

            Items.Add(drawable);
        }

        public bool Remove(T? drawable) => RemoveInternal(drawable, false);

        protected bool RemoveInternal(T? drawable, bool disposeImmediately)
        {
            if (drawable is null || !Items.Contains(drawable)) return false;

            bool remove = Items.Remove(drawable);
            if (disposeImmediately) drawable.Dispose();
            return remove;
        }

        public virtual Drawable GetDrawableListItem() => this;

        public void UpdateItem()
        {
            foreach (var item in ItemMap.Values)
            {
                if (item is IRearrangableListItem<T> rearrangableItem)
                {
                    rearrangableItem.UpdateItem();
                    rearrangableItem.SelectAll = SelectAll;
                }
            }
        }

        /// <summary>
        /// Selects obj, if it can be cast to a IRearrangableListItem.
        /// </summary>
        /// <param name="obj">the object to call Select on</param>
        /// <param name="value">The value to pass to the Select call of IRearrangableListItem</param>
        /// <returns>If Select was actually called</returns>
        private static bool select(object obj, bool value)
        {
            if (obj is IRearrangableListItem<T> item)
            {
                item.Select(value);
                return true;
            }

            return false;
        }

        public bool Select(T drawable, bool value = true)
        {
            if (!ItemMap.ContainsKey(drawable)) return false;

            return select(ItemMap[drawable], value);
        }

        public virtual void Select(bool value)
        {
            foreach (var listItem in ItemMap.Values)
            {
                select(listItem, value);
            }
        }

        public void SelectInternal(bool value) => Select(value);

        protected override ScrollContainer<Drawable> CreateScrollContainer() => new OsuScrollContainer();

        protected override RearrangeableListItem<T> CreateDrawable(T item)
        {
            var drawable = new DrawableListItem<T>(item);
            drawable.SelectAll = SelectAll;
            return drawable.GetRearrangeableListItem();
        }
    }
}

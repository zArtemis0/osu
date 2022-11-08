// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public class DrawableContainer<T> : CompositeDrawable, IDrawableListItem<T>
        where T : Drawable
    {
        public event Action<SelectionState> SelectAll;

        private readonly OsuCheckbox button;
        private readonly Box box;
        private readonly BindableBool enabled = new BindableBool();
        private readonly DrawableList<T> list = new DrawableList<T>();

        public DrawableContainer()
        {
            SelectAll = ((IDrawableListItem<T>)list).SelectableOnStateChanged;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2),
                Children = new Drawable[]
                {
                    box = new Box
                    {
                        Colour = new Colour4(255, 255, 0, 0.25f),
                    },
                    button = new OsuCheckbox
                    {
                        LabelText = @"SkinnableContainer",
                        Current = enabled
                    },
                    new GridContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 10),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable?[]
                            {
                                null,
                                list,
                            }
                        }
                    }
                }
            };
            enabled.BindValueChanged(v => SetShown(v.NewValue), true);
            Select(false);
        }

        public void SelectableOnStateChanged(SelectionState obj) =>
            ((IDrawableListItem<T>)list).SelectableOnStateChanged(obj);

        public void Toggle() => SetShown(!enabled.Value, true);

        public void SetShown(bool value, bool setValue = false)
        {
            if (value) list.Show();
            else list.Hide();

            if (setValue) enabled.Value = value;
        }

        public void UpdateText() => list.UpdateText();

        public void Select(bool value)
        {
            if (value)
            {
                box.Show();
                box.Width = button.Width;
                box.Height = button.Height;
            }
            else
            {
                box.Hide();
                box.Width = button.Width;
                box.Height = button.Height;
            }

            list.Select(value);
        }

        public void AddRange(IEnumerable<T>? drawables) => list.AddRange(drawables);
        public void Add(DrawableListItem<T> drawableListItem) => list.Add(drawableListItem);
        public void Add(DrawableContainer<T> container) => list.Add(container);
        public void Add(DrawableList<T> list) => list.Add(list);
        public void Add(T? item) => list.Add(item);
        public void Remove(T? item) => list.Remove(item);

        public void SelectInternal(bool value) => list.SelectInternal(value);

        public Drawable GetDrawableListItem() => this;
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Edit.List
{
    public partial class DrawableMinimisableList<T> : AbstractListItem<T>
        where T : Drawable
    {
        public BindableBool Enabled { get; } = new BindableBool();
        public readonly DrawableList<T> List;

        private readonly DrawableListItem<T> representedListItem;
        public RectangleF ListHeadBoundingBox => representedListItem.BoundingBox;
        public LocalisableString ListHeadText => representedListItem.Text;
        private readonly Action updateListPositionAndIcon;

        internal DrawableMinimisableList(T item)
            : this(item, new DrawableListProperties<T>())
        {
            setProperties();
        }

        internal DrawableMinimisableList(T item, DrawableListProperties<T> properties)
            : this(new DrawableListRepresetedItem<T>(item, DrawableListEntryType.MinimisableList), properties)
        {
        }

        internal DrawableMinimisableList(DrawableListRepresetedItem<T> item)
            : this(item, new DrawableListProperties<T>())
        {
            setProperties();
        }

        internal DrawableMinimisableList(DrawableListRepresetedItem<T> item, DrawableListProperties<T> properties)
            : base(item, properties)
        {
            SpriteIcon icon;
            Container head;
            ClickableContainer headClickableContainer;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                // RelativeSizeAxes = Axes.X,
                // AutoSizeAxes = Axes.Y,
                head = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        headClickableContainer = new ClickableContainer
                        {
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Child = icon = new SpriteIcon
                            {
                                Size = new Vector2(8),
                                Icon = FontAwesome.Solid.ChevronRight,
                                Margin = new MarginPadding { Left = 3, Right = 3 },
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                            },
                        },
                        representedListItem = new DrawableListItem<T>(Model, Properties)
                        {
                            X = icon.LayoutSize.X,
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                        }
                    },
                    Margin = new MarginPadding { Bottom = 2.5f }
                },
                List = new DrawableList<T>(Properties)
                {
                    X = icon.LayoutSize.X,
                    Y = head.LayoutSize.Y,
                }
            };

            updateListPositionAndIcon = () =>
            {
                icon.Icon = Enabled.Value ? FontAwesome.Solid.ChevronDown : FontAwesome.Solid.ChevronRight;

                List.X = icon.LayoutSize.X;
                List.Y = head.LayoutSize.Y;
            };
            headClickableContainer.Action = () =>
            {
                Enabled.Toggle();

                updateListPositionAndIcon();
            };

            List.ItemAdded += t =>
            {
                if (t is IRearrangableDrawableListItem<T> listItem)
                {
                    listItem.Deselected += () =>
                    {
                        //If all elements are not selected we want to also deselect this element
                        if (checkAllSelectedState(SelectionState.NotSelected)) Deselect();
                        //If some elements are still selected, keep them selected, but deselect the representedListItem.
                        else representedListItem.Deselect();
                    };
                    listItem.Selected += () =>
                    {
                        //if all elements of a List are selected, the representedListItem should also be selected
                        if (checkAllSelectedState(SelectionState.Selected)) Select();
                    };
                }
            };
            representedListItem.Selected += Select;
            representedListItem.Deselected += () =>
            {
                //if all items are selected, then actually deselect all items.
                if (checkAllSelectedState(SelectionState.Selected)) Deselect();
                //else we just want to deselect the representedListItem, because we don't actually know if the representedListItem gotClicked
                //or we deselected it manually through a deselection of a child element
                InvokeStateChanged(SelectionState.NotSelected);
            };

            Enabled.BindValueChanged(v =>
            {
                if (v.NewValue) ShowList(false);
                else HideList(false);
            }, true);

            Deselect();
            Scheduler.Add(UpdateItem);
        }

        private void setProperties()
        {
            Properties.TopLevelItem = this;
        }

        private bool checkAllSelectedState(SelectionState state)
        {
            foreach (var item in List.ItemMaps.Values)
            {
                if (item is IRearrangableDrawableListItem<T> rearrangeableItem && rearrangeableItem.State != state) return false;
            }

            return true;
        }

        public void ShowList(bool setValue = true)
        {
            List.Show();
            updateListPositionAndIcon();
            UpdateItem();
            if (setValue) Enabled.Value = true;
        }

        public void HideList(bool setValue = true)
        {
            List.Hide();
            updateListPositionAndIcon();
            UpdateItem();
            if (setValue) Enabled.Value = false;
        }

        public override void UpdateItem()
        {
            representedListItem.Properties = Properties;
            representedListItem.UpdateItem();

            List.Properties = Properties;
            List.UpdateItem();
        }

        public override void Select()
        {
            representedListItem.Select();
            List.Select();
            InvokeStateChanged(SelectionState.Selected);
        }

        public override void Deselect()
        {
            representedListItem.Deselect();
            List.Deselect();
            InvokeStateChanged(SelectionState.NotSelected);
        }

        public override void ApplyAction(Action<IDrawableListItem<T>> action)
        {
            representedListItem.ApplyAction(action);
            List.ApplyAction(action);
        }

        public override void SelectInternal(bool invokeChildMethods = true)
        {
            if (invokeChildMethods)
            {
                representedListItem.SelectInternal();
                List.SelectInternal();
            }
        }

        public override void DeselectInternal(bool invokeChildMethods = true)
        {
            if (invokeChildMethods)
            {
                representedListItem.DeselectInternal();
                List.DeselectInternal();
            }
        }

        public override SelectionState State
        {
            get => representedListItem.State;
            set
            {
                switch (value)
                {
                    case SelectionState.Selected:
                        Select();
                        break;

                    case SelectionState.NotSelected:
                        Deselect();
                        break;
                }
            }
        }
    }
}

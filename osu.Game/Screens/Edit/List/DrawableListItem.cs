// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK.Input;

namespace osu.Game.Screens.Edit.List
{
    public partial class DrawableListItem<T> : RearrangeableListItem<IDrawableListRepresetedItem<T>>, IRearrangableDrawableListItem<T>
        where T : Drawable
    {
        private readonly OsuSpriteText text = new OsuSpriteText();
        private readonly Box box;
        public bool IsSelected { get; private set; }

        public Action<T, int> SetItemDepth { get; set; } = IDrawableListItem<T>.DEFAULT_SET_ITEM_DEPTH;
        public Action OnDragAction { get; set; } = () => { };

        public Action<Action<IDrawableListItem<T>>> ApplyAll { get; set; }

        private Func<T, LocalisableString> getName;

        public Func<T, LocalisableString> GetName
        {
            get => getName;
            set
            {
                getName = value;
                UpdateItem();
            }
        }

        internal DrawableListItem(IDrawableListRepresetedItem<T> represetedItem, LocalisableString name)
            : base(represetedItem)
        {
            StateChanged = t =>
            {
                switch (t)
                {
                    case SelectionState.Selected:
                        Selected.Invoke();
                        break;

                    case SelectionState.NotSelected:
                        Deselected.Invoke();
                        break;
                }
            };
            getName = IDrawableListItem<T>.GetDefaultText;
            ApplyAll = e => e(this);
            text.Text = name;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1f,
                    Height = 1f,
                    Colour = new Colour4(255, 255, 0, 0.25f),
                },
                text
            };
            updateText(RepresentedItem);

            if (RepresentedItem is IStateful<SelectionState> selectable)
            {
                selectable.StateChanged += this.ApplySelectionState;
                this.ApplySelectionState(selectable.State);
            }

            if (!((IDrawableListItem<T>)this).EnableSelection)
            {
                box.RemoveAndDisposeImmediately();
                box = new Box();
            }

            box.Hide();
        }

        public DrawableListItem(IDrawableListRepresetedItem<T> represetedItem)
            //select the name of the from the drawable as it's name, if it is set
            //otherwise use the
            : this(represetedItem, string.Empty)
        {
        }

        //imitate the existing SelectionHandler implementation
        protected override bool OnClick(ClickEvent e)
        {
            // while holding control, we only want to add to selection, not replace an existing selection.
            if (e.ControlPressed && e.Button == MouseButton.Left)
            {
                if (IsSelected) Deselect();
                else Select();
            }
            else
            {
                ApplyAll(element => element.Deselect());
                Select();
            }

            base.OnClick(e);
            return true;
        }

        public Drawable GetDrawableListItem() => this;

        public RearrangeableListItem<IDrawableListRepresetedItem<T>> GetRearrangeableListItem() => this;

        public void UpdateItem()
        {
            updateText(RepresentedItem!);

            if (RepresentedItem is IStateful<SelectionState> selectable)
            {
                if (selectable.State == SelectionState.Selected) SelectInternal();
                else DeselectInternal();
            }
        }

        private void updateText(T? target)
        {
            if (target is null) return;
            //Set the text to the target's name, if set. Else try and get the name of the class that defined T
            Scheduler.Add(() => text.Text = getName(target));
        }

        public void Select()
        {
            if (IsSelected) return;

            SelectInternal();
            if (RepresentedItem is IStateful<SelectionState> selectable)
                selectable.State = SelectionState.Selected;

            StateChanged.Invoke(SelectionState.Selected);
        }

        public void Deselect()
        {
            if (!IsSelected) return;

            DeselectInternal();
            if (RepresentedItem is IStateful<SelectionState> selectable)
                selectable.State = SelectionState.NotSelected;

            StateChanged.Invoke(SelectionState.NotSelected);
        }

        public void ApplyAction(Action<IDrawableListItem<T>> action) => action(this);

        public void SelectInternal()
        {
            if (IsSelected) return;

            IsSelected = true;
            Scheduler.Add(box.Show);
        }

        public void DeselectInternal()
        {
            if (!IsSelected) return;

            IsSelected = false;
            Scheduler.Add(box.Hide);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            ApplyAll(element => element.Deselect());
            Select();
            return base.OnDragStart(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            OnDragAction();
            base.OnDrag(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            OnDragAction();
            base.OnDragEnd(e);
        }

        public T? RepresentedItem => Model.RepresentedItem;

        public SelectionState State
        {
            get => IsSelected ? SelectionState.Selected : SelectionState.NotSelected;
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

        public event Action<SelectionState> StateChanged;
        public event Action Selected = () => { };
        public event Action Deselected = () => { };
    }
}

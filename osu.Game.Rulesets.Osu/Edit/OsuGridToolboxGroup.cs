// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuGridToolboxGroup : EditorToolboxGroup, IKeyBindingHandler<GlobalAction>
    {
        private static readonly int[] grid_sizes = { 4, 8, 16, 32 };

        private int currentGridSizeIndex = grid_sizes.Length - 1;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        /// <summary>
        /// X position of the grid's origin.
        /// </summary>
        public BindableFloat StartPositionX { get; } = new BindableFloat(OsuPlayfield.BASE_SIZE.X / 2)
        {
            MinValue = 0f,
            MaxValue = OsuPlayfield.BASE_SIZE.X,
        };

        /// <summary>
        /// Y position of the grid's origin.
        /// </summary>
        public BindableFloat StartPositionY { get; } = new BindableFloat(OsuPlayfield.BASE_SIZE.Y / 2)
        {
            MinValue = 0f,
            MaxValue = OsuPlayfield.BASE_SIZE.Y,
        };

        /// <summary>
        /// The spacing between grid lines.
        /// </summary>
        public BindableFloat Spacing { get; } = new BindableFloat(4f)
        {
            MinValue = 4f,
            MaxValue = 128f,
        };

        /// <summary>
        /// Rotation of the grid lines in degrees.
        /// </summary>
        public BindableFloat GridLinesRotation { get; } = new BindableFloat(0f)
        {
            MinValue = -45f,
            MaxValue = 45f,
        };

        /// <summary>
        /// Read-only bindable representing the grid's origin.
        /// Equivalent to <code>new Vector2(StartPositionX, StartPositionY)</code>
        /// </summary>
        public Bindable<Vector2> StartPosition { get; } = new Bindable<Vector2>();

        /// <summary>
        /// Read-only bindable representing the grid's spacing in both the X and Y dimension.
        /// Equivalent to <code>new Vector2(Spacing)</code>
        /// </summary>
        public Bindable<Vector2> SpacingVector { get; } = new Bindable<Vector2>();

        public Bindable<PositionSnapGridType> GridType { get; } = new Bindable<PositionSnapGridType>();

        private ExpandableSlider<float> startPositionXSlider = null!;
        private ExpandableSlider<float> startPositionYSlider = null!;
        private ExpandableSlider<float> spacingSlider = null!;
        private ExpandableSlider<float> gridLinesRotationSlider = null!;
        private EditorRadioButtonCollection gridTypeButtons = null!;

        public event Action? GridFromPointsClicked;

        public OsuGridToolboxGroup()
            : base("grid")
        {
        }

        public void SetGridFromPoints(Vector2 point1, Vector2 point2)
        {
            StartPositionX.Value = point1.X;
            StartPositionY.Value = point1.Y;

            // Get the angle between the two points and normalize to the valid range.
            float period = GridType.Value == PositionSnapGridType.Triangle ? 60 : 90;
            GridLinesRotation.Value = (MathHelper.RadiansToDegrees(MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X))
                                       + 360 + period / 2) % period - period / 2;

            // Divide the distance so that there is a good density of grid lines.
            float dist = Vector2.Distance(point1, point2);
            while (dist > 32)
                dist /= 2;
            Spacing.Value = dist;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                startPositionXSlider = new ExpandableSlider<float>
                {
                    Current = StartPositionX,
                },
                startPositionYSlider = new ExpandableSlider<float>
                {
                    Current = StartPositionY
                },
                spacingSlider = new ExpandableSlider<float>
                {
                    Current = Spacing
                },
                gridLinesRotationSlider = new ExpandableSlider<float>
                {
                    Current = GridLinesRotation
                },
                new RoundedButton
                {
                    Action = () => GridFromPointsClicked?.Invoke(),
                    RelativeSizeAxes = Axes.X,
                    Text = "Grid from points",
                },
                gridTypeButtons = new EditorRadioButtonCollection
                {
                    RelativeSizeAxes = Axes.X,
                    Items = new[]
                    {
                        new RadioButton("Square",
                            () => GridType.Value = PositionSnapGridType.Square,
                            () => new SpriteIcon { Icon = FontAwesome.Regular.Square }),
                        new RadioButton("Triangle",
                            () => GridType.Value = PositionSnapGridType.Triangle,
                            () => new Triangle()),
                        new RadioButton("Circle",
                            () => GridType.Value = PositionSnapGridType.Circle,
                            () => new SpriteIcon { Icon = FontAwesome.Regular.Circle }),
                    }
                },
            };

            Spacing.Value = editorBeatmap.BeatmapInfo.GridSize;
            int gridSizeIndex = Array.IndexOf(grid_sizes, editorBeatmap.BeatmapInfo.GridSize);
            if (gridSizeIndex >= 0)
                currentGridSizeIndex = gridSizeIndex;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            gridTypeButtons.Items.First().Select();

            StartPositionX.BindValueChanged(x =>
            {
                startPositionXSlider.ContractedLabelText = $"X: {x.NewValue:#,0.##}";
                startPositionXSlider.ExpandedLabelText = $"X Offset: {x.NewValue:#,0.##}";
                StartPosition.Value = new Vector2(x.NewValue, StartPosition.Value.Y);
            }, true);

            StartPositionY.BindValueChanged(y =>
            {
                startPositionYSlider.ContractedLabelText = $"Y: {y.NewValue:#,0.##}";
                startPositionYSlider.ExpandedLabelText = $"Y Offset: {y.NewValue:#,0.##}";
                StartPosition.Value = new Vector2(StartPosition.Value.X, y.NewValue);
            }, true);

            StartPosition.BindValueChanged(pos =>
            {
                StartPositionX.Value = pos.NewValue.X;
                StartPositionY.Value = pos.NewValue.Y;
            });

            Spacing.BindValueChanged(spacing =>
            {
                spacingSlider.ContractedLabelText = $"S: {spacing.NewValue:#,0.##}";
                spacingSlider.ExpandedLabelText = $"Spacing: {spacing.NewValue:#,0.##}";
                SpacingVector.Value = new Vector2(spacing.NewValue);
                editorBeatmap.BeatmapInfo.GridSize = (int)spacing.NewValue;
            }, true);

            GridLinesRotation.BindValueChanged(rotation =>
            {
                gridLinesRotationSlider.ContractedLabelText = $"R: {rotation.NewValue:#,0.##}";
                gridLinesRotationSlider.ExpandedLabelText = $"Rotation: {rotation.NewValue:#,0.##}";
            }, true);
        }

        private void nextGridSize()
        {
            currentGridSizeIndex = (currentGridSizeIndex + 1) % grid_sizes.Length;
            Spacing.Value = grid_sizes[currentGridSizeIndex];
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorCycleGridDisplayMode:
                    nextGridSize();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }

    public enum PositionSnapGridType
    {
        Square,
        Triangle,
        Circle,
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Rendering.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Timing;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.OpenGL.Vertices;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModFlashlight : Mod
    {
        public override string Name => "Flashlight";
        public override string Acronym => "FL";
        public override IconUsage? Icon => OsuIcon.ModFlashlight;
        public override ModType Type => ModType.DifficultyIncrease;
        public override LocalisableString Description => "Restricted view area.";

        [SettingSource("Flashlight size", "Multiplier applied to the default flashlight size.")]
        public abstract BindableFloat SizeMultiplier { get; }

        [SettingSource("Change size based on combo", "Decrease the flashlight size as combo increases.")]
        public abstract BindableBool ComboBasedSize { get; }

        [SettingSource("Change size after how many combo", "Changes after how many combo does flashlight change size")]
        public BindableFloat ChangeSizeAfterHowManyCombo { get; } = new BindableFloat
        {
            MinValue = 1,
            Value = 100,
            MaxValue = 300,
            Precision = 1
        };

        [SettingSource("Final change size combo", "Changes on which combo the flashlight size reaches it final combo based size.")]
        public BindableInt FinalChangeSizeCombo { get; } = new BindableInt
        {
            MinValue = 100,
            Value = 200,
            MaxValue = 300,
            Precision = 50
        };

        [SettingSource("Final flashlight size", "The final multiplier fully applied when the final change size combo is reached.")]
        public BindableFloat FinalFlashlightSize { get; } = new BindableFloat
        {
            MinValue = 0.1f,
            Value = 0.8f,
            MaxValue = 1,
            Precision = 0.1f,
        };

        /// <summary>
        /// The default size of the flashlight in ruleset-appropriate dimensions.
        /// <see cref="SizeMultiplier"/> and <see cref="ComboBasedSize"/> will apply their adjustments on top of this size.
        /// </summary>
        public abstract float DefaultFlashlightSize { get; }
    }

    public abstract class ModFlashlight<T> : ModFlashlight, IApplicableToDrawableRuleset<T>, IApplicableToScoreProcessor
        where T : HitObject
    {
        public const double FLASHLIGHT_FADE_DURATION = 800;
        protected readonly BindableInt Combo = new BindableInt();

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            Combo.BindTo(scoreProcessor.Combo);

            // Default value of ScoreProcessor's Rank in Flashlight Mod should be SS+
            scoreProcessor.Rank.Value = ScoreRank.XH;
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy)
        {
            switch (rank)
            {
                case ScoreRank.X:
                    return ScoreRank.XH;

                case ScoreRank.S:
                    return ScoreRank.SH;

                default:
                    return rank;
            }
        }

        public virtual void ApplyToDrawableRuleset(DrawableRuleset<T> drawableRuleset)
        {
            var flashlight = CreateFlashlight();

            flashlight.RelativeSizeAxes = Axes.Both;
            flashlight.Colour = Color4.Black;

            flashlight.Combo.BindTo(Combo);
            drawableRuleset.KeyBindingInputManager.Add(flashlight);

            flashlight.Breaks = drawableRuleset.Beatmap.Breaks;
        }

        protected abstract Flashlight CreateFlashlight();

        public abstract class Flashlight : Drawable
        {
            public readonly BindableInt Combo = new BindableInt();

            private IShader shader = null!;

            protected override DrawNode CreateDrawNode() => new FlashlightDrawNode(this);

            public override bool RemoveCompletedTransforms => false;

            public List<BreakPeriod> Breaks = new List<BreakPeriod>();

            private readonly float appliedSize;
            private readonly float finalFlashlightDecreasing;
            private readonly float maximumChangeSizeComboReachedTimes;

            private readonly bool comboBasedSize;
            private readonly int finalChangeSizeCombo;
            private readonly float changeSizeAfterHowManyCombo;

            private float getChangeSizeComboReachedTimesForCombo(int combo) => MathF.Floor(combo / changeSizeAfterHowManyCombo);

            protected Flashlight(ModFlashlight modFlashlight)
            {
                changeSizeAfterHowManyCombo = modFlashlight.ChangeSizeAfterHowManyCombo.Value;
                finalChangeSizeCombo = modFlashlight.FinalChangeSizeCombo.Value;
                comboBasedSize = modFlashlight.ComboBasedSize.Value;

                finalFlashlightDecreasing = 1 - modFlashlight.FinalFlashlightSize.Value;
                appliedSize = modFlashlight.DefaultFlashlightSize * modFlashlight.SizeMultiplier.Value;
                maximumChangeSizeComboReachedTimes = getChangeSizeComboReachedTimesForCombo(finalChangeSizeCombo);
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaderManager)
            {
                shader = shaderManager.Load("PositionAndColour", FragmentShader);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Combo.ValueChanged += OnComboChange;

                using (BeginAbsoluteSequence(0))
                {
                    foreach (var breakPeriod in Breaks)
                    {
                        if (!breakPeriod.HasEffect)
                            continue;

                        if (breakPeriod.Duration < FLASHLIGHT_FADE_DURATION * 2) continue;

                        this.Delay(breakPeriod.StartTime + FLASHLIGHT_FADE_DURATION).FadeOutFromOne(FLASHLIGHT_FADE_DURATION);
                        this.Delay(breakPeriod.EndTime - FLASHLIGHT_FADE_DURATION).FadeInFromZero(FLASHLIGHT_FADE_DURATION);
                    }
                }
            }

            protected abstract void OnComboChange(ValueChangedEvent<int> e);

            protected abstract string FragmentShader { get; }

            protected float GetSizeFor(int combo)
            {
                if (!comboBasedSize) return appliedSize;

                int comboForSize = Math.Min(finalChangeSizeCombo, combo);

                float changeSizeComboReachedTimes = getChangeSizeComboReachedTimesForCombo(comboForSize);
                float changeSizeComboReachedTimesRatio = changeSizeComboReachedTimes / maximumChangeSizeComboReachedTimes;

                return appliedSize * (finalFlashlightDecreasing * changeSizeComboReachedTimesRatio);
            }

            private Vector2 flashlightPosition;

            protected Vector2 FlashlightPosition
            {
                get => flashlightPosition;
                set
                {
                    if (flashlightPosition == value) return;

                    flashlightPosition = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            private Vector2 flashlightSize;

            protected Vector2 FlashlightSize
            {
                get => flashlightSize;
                set
                {
                    if (flashlightSize == value) return;

                    flashlightSize = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            private float flashlightDim;

            public float FlashlightDim
            {
                get => flashlightDim;
                set
                {
                    if (flashlightDim == value) return;

                    flashlightDim = value;
                    Invalidate(Invalidation.DrawNode);
                }
            }

            private class FlashlightDrawNode : DrawNode
            {
                protected new Flashlight Source => (Flashlight)base.Source;

                private IShader shader = null!;
                private Quad screenSpaceDrawQuad;
                private Vector2 flashlightPosition;
                private Vector2 flashlightSize;
                private float flashlightDim;

                private IVertexBatch<PositionAndColourVertex>? quadBatch;
                private Action<TexturedVertex2D>? addAction;

                public FlashlightDrawNode(Flashlight source)
                    : base(source)
                {
                }

                public override void ApplyState()
                {
                    base.ApplyState();

                    shader = Source.shader;
                    screenSpaceDrawQuad = Source.ScreenSpaceDrawQuad;
                    flashlightPosition = Vector2Extensions.Transform(Source.FlashlightPosition, DrawInfo.Matrix);
                    flashlightSize = Source.FlashlightSize * DrawInfo.Matrix.ExtractScale().Xy;
                    flashlightDim = Source.FlashlightDim;
                }

                public override void Draw(IRenderer renderer)
                {
                    base.Draw(renderer);

                    if (quadBatch == null)
                    {
                        quadBatch = renderer.CreateQuadBatch<PositionAndColourVertex>(1, 1);
                        addAction = v => quadBatch.Add(new PositionAndColourVertex
                        {
                            Position = v.Position,
                            Colour = v.Colour
                        });
                    }

                    shader.Bind();

                    shader.GetUniform<Vector2>("flashlightPos").UpdateValue(ref flashlightPosition);
                    shader.GetUniform<Vector2>("flashlightSize").UpdateValue(ref flashlightSize);
                    shader.GetUniform<float>("flashlightDim").UpdateValue(ref flashlightDim);

                    renderer.DrawQuad(renderer.WhitePixel, screenSpaceDrawQuad, DrawColourInfo.Colour, vertexAction: addAction);

                    shader.Unbind();
                }

                protected override void Dispose(bool isDisposing)
                {
                    base.Dispose(isDisposing);
                    quadBatch?.Dispose();
                }
            }
        }
    }
}

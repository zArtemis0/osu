// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonMainCirclePiece : CompositeDrawable
    {
        public const float BORDER_THICKNESS = (OsuHitObject.OBJECT_RADIUS * 2) * (2f / 58);

        public const float GRADIENT_THICKNESS = BORDER_THICKNESS * 2.5f;

        public const float OUTER_GRADIENT_SIZE = (OsuHitObject.OBJECT_RADIUS * 2) - BORDER_THICKNESS * 4;

        public const float INNER_GRADIENT_SIZE = OUTER_GRADIENT_SIZE - GRADIENT_THICKNESS * 2;
        public const float INNER_FILL_SIZE = INNER_GRADIENT_SIZE - GRADIENT_THICKNESS * 2;

        private readonly Circle outerFill;
        private readonly Circle outerGradient;
        private readonly Circle innerGradient;
        private readonly Circle innerFill;

        private readonly RingPiece border;
        private readonly OsuSpriteText number;

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();
        private readonly IBindable<int> indexInCurrentCombo = new Bindable<int>();
        private readonly FlashPiece flash;
        private IBindable<bool> hitLightingEnabled = null!;

        [Resolved]
        private DrawableHitObject drawableObject { get; set; } = null!;

        public ArgonMainCirclePiece(bool withOuterFill)
        {
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                outerFill = new Circle // renders white outer border and dark fill
                {
                    Size = Size,
                    Alpha = withOuterFill ? 1 : 0,
                },
                outerGradient = new Circle // renders the outer bright gradient
                {
                    Size = new Vector2(OUTER_GRADIENT_SIZE),
                    Alpha = 1,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                innerGradient = new Circle // renders the inner bright gradient
                {
                    Size = new Vector2(INNER_GRADIENT_SIZE),
                    Alpha = 1,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                innerFill = new Circle // renders the inner dark fill
                {
                    Size = new Vector2(INNER_FILL_SIZE),
                    Alpha = 1,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                number = new OsuSpriteText
                {
                    Font = OsuFont.Default.With(size: 52, weight: FontWeight.Bold),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Y = -2,
                    Text = @"1",
                },
                flash = new FlashPiece(),
                border = new RingPiece(BORDER_THICKNESS),
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            hitLightingEnabled = config.GetBindable<bool>(OsuSetting.HitLighting);
            var drawableOsuObject = (DrawableOsuHitObject)drawableObject;

            accentColour.BindTo(drawableObject.AccentColour);
            indexInCurrentCombo.BindTo(drawableOsuObject.IndexInCurrentComboBindable);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            indexInCurrentCombo.BindValueChanged(index => number.Text = (index.NewValue + 1).ToString(), true);

            accentColour.BindValueChanged(colour =>
            {
                // A colour transform is applied.
                // Without removing transforms first, when it is rewound it may apply an old colour.
                outerGradient.ClearTransforms(targetMember: nameof(Colour));
                outerGradient.Colour = ColourInfo.GradientVertical(colour.NewValue, colour.NewValue.Darken(0.1f));

                outerFill.Colour = innerFill.Colour = colour.NewValue.Darken(4);
                innerGradient.Colour = ColourInfo.GradientVertical(colour.NewValue.Darken(0.5f), colour.NewValue.Darken(0.6f));
                flash.Colour = colour.NewValue;

                // Accent colour may be changed many times during a paused gameplay state.
                // Schedule the change to avoid transforms piling up.
                Scheduler.AddOnce(updateStateTransforms);
            }, true);

            drawableObject.ApplyCustomUpdateState += updateStateTransforms;
        }

        private void updateStateTransforms() => updateStateTransforms(drawableObject, drawableObject.State.Value);

        private void updateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
            {
                switch (state)
                {
                    case ArmedState.Hit:
                        // Fade out time is at a maximum of 800. Must match `DrawableHitCircle`'s arbitrary lifetime spec.
                        const double fade_out_time = 800;
                        const double legacy_fade_duration = 240;

                        const double flash_in_duration = 150;
                        const double resize_duration = 400;

                        const float shrink_size = 0.8f;

                        // Animating with the number present is distracting.
                        // The number disappearing is hidden by the bright flash.
                        number.FadeOut(flash_in_duration / 2);

                        // The fill layers add too much noise during the explosion animation.
                        // They will be hidden by the additive effects anyway.
                        outerFill.FadeOut(flash_in_duration, Easing.OutQuint);
                        innerFill.FadeOut(flash_in_duration, Easing.OutQuint);

                        // The inner-most gradient should actually be resizing, but is only visible for
                        // a few milliseconds before it's hidden by the flash, so it's pointless overhead to bother with it.
                        innerGradient.FadeOut(flash_in_duration, Easing.OutQuint);

                        if (hitLightingEnabled.Value)
                        {
                            // The border is always white, but after hit it gets coloured by the skin/beatmap's colouring.
                            // A gradient is applied to make the border less prominent over the course of the animation.
                            // Without this, the border dominates the visual presence of the explosion animation in a bad way.
                            border.TransformTo(nameof
                                (BorderColour), ColourInfo.GradientVertical(
                                accentColour.Value.Opacity(0.5f),
                                accentColour.Value.Opacity(0)), fade_out_time);

                            // The outer ring shrinks immediately, but accounts for its thickness so it doesn't overlap the inner
                            // gradient layers.
                            border.ResizeTo(Size * shrink_size + new Vector2(border.BorderThickness), resize_duration, Easing.OutElasticHalf);

                            // The outer gradient is resize with a slight delay from the border.
                            // This is to give it a bomb-like effect, with the border "triggering" its animation when getting close.
                            using (BeginDelayedSequence(flash_in_duration / 12))
                            {
                                outerGradient.ResizeTo(OUTER_GRADIENT_SIZE * shrink_size, resize_duration, Easing.OutElasticHalf);
                                outerGradient
                                    .FadeColour(Color4.White, 80)
                                    .Then()
                                    .FadeOut(flash_in_duration);
                            }

                            flash.FadeTo(1, flash_in_duration, Easing.OutQuint);

                            this.FadeOut(fade_out_time, Easing.OutQuad);
                        }
                        else
                        {
                            // If hit lighting is disabled, use legacy fade out behavior for border and outer gradient.
                            border.FadeOut(legacy_fade_duration);
                            border.ScaleTo(1.4f, legacy_fade_duration, Easing.Out);

                            outerGradient.FadeOut(legacy_fade_duration);
                            outerGradient.ScaleTo(1.4f, legacy_fade_duration, Easing.Out);

                            this.FadeOut(legacy_fade_duration, Easing.OutQuad);
                        }

                        break;
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableObject.IsNotNull())
                drawableObject.ApplyCustomUpdateState -= updateStateTransforms;
        }

        private partial class FlashPiece : Circle
        {
            public FlashPiece()
            {
                Size = new Vector2(OsuHitObject.OBJECT_RADIUS);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Alpha = 0;
                Blending = BlendingParameters.Additive;

                // The edge effect provides the fill due to not being rendered hollow.
                Child.Alpha = 0;
                Child.AlwaysPresent = true;
            }

            protected override void Update()
            {
                base.Update();

                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Colour,
                    Radius = OsuHitObject.OBJECT_RADIUS * 1.2f,
                };
            }
        }
    }
}

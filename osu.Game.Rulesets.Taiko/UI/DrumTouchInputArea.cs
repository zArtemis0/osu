// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using osu.Game.Configuration;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// An overlay that captures and displays osu!taiko mouse and touch input.
    /// </summary>
    public partial class DrumTouchInputArea : VisibilityContainer
    {
        // visibility state affects our child. we always want to handle input.
        public override bool PropagatePositionalInputSubTree => true;
        public override bool PropagateNonPositionalInputSubTree => true;

        private KeyBindingContainer<TaikoAction> keyBindingContainer = null!;

        private readonly Dictionary<object, TaikoAction> trackedActions = new Dictionary<object, TaikoAction>();

        private Container mainContent = null!;

        private QuarterCircle leftCentre = null!;
        private QuarterCircle rightCentre = null!;
        private QuarterCircle leftRim = null!;
        private QuarterCircle rightRim = null!;

        private Bindable<TaikoTouchControlType> touchControlsType;

        [BackgroundDependencyLoader]
        private void load(TaikoInputManager taikoInputManager, OsuColour colours, OsuConfigManager config)
        {
            Debug.Assert(taikoInputManager.KeyBindingContainer != null);
            keyBindingContainer = taikoInputManager.KeyBindingContainer;

            // Container should handle input everywhere.
            RelativeSizeAxes = Axes.Both;

            touchControlsType = config.GetBindable<TaikoTouchControlType>(OsuSetting.TaikoTouchControlType);

            const float centre_region = 0.80f;
            
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 350,
                    Y = 20,
                    Masking = true,
                    FillMode = FillMode.Fit,
                    Children = new Drawable[]
                    {
                        mainContent = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                leftRim = new QuarterCircle(touchControlsType.Value == TaikoTouchControlType.KDDK ? TaikoAction.LeftRim :
                                                            touchControlsType.Value == TaikoTouchControlType.DDKK ? TaikoAction.LeftCentre :
                                                          /*touchControlsType.Value == TaikoTouchControlType.KKDD*/ TaikoAction.LeftRim,

                                                            touchControlsType.Value == TaikoTouchControlType.KDDK ? colours.Blue :
                                                            touchControlsType.Value == TaikoTouchControlType.DDKK ? colours.Pink :
                                                          /*touchControlsType.Value == TaikoTouchControlType.KKDD*/ colours.Blue)
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomRight,
                                    X = -2,
                                },
                                rightRim = new QuarterCircle(touchControlsType.Value == TaikoTouchControlType.KDDK ? TaikoAction.RightRim :
                                                             touchControlsType.Value == TaikoTouchControlType.DDKK ? TaikoAction.RightRim :
                                                           /*touchControlsType.Value == TaikoTouchControlType.KKDD*/ TaikoAction.RightCentre,

                                                             touchControlsType.Value == TaikoTouchControlType.KDDK ? colours.Blue :
                                                             touchControlsType.Value == TaikoTouchControlType.DDKK ? colours.Blue :
                                                           /*touchControlsType.Value == TaikoTouchControlType.KKDD*/ colours.Pink)
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomRight,
                                    X = 2,
                                    Rotation = 90,
                                },
                                leftCentre = new QuarterCircle(touchControlsType.Value == TaikoTouchControlType.KDDK ? TaikoAction.LeftCentre :
                                                               touchControlsType.Value == TaikoTouchControlType.DDKK ? TaikoAction.RightCentre :
                                                             /*touchControlsType.Value == TaikoTouchControlType.KKDD*/ TaikoAction.RightRim,

                                                               touchControlsType.Value == TaikoTouchControlType.KDDK ? colours.Pink :
                                                               touchControlsType.Value == TaikoTouchControlType.DDKK ? colours.Pink :
                                                             /*touchControlsType.Value == TaikoTouchControlType.KKDD*/ colours.Blue)
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomRight,
                                    X = -2,
                                    Scale = new Vector2(centre_region),
                                },
                                rightCentre = new QuarterCircle(touchControlsType.Value == TaikoTouchControlType.KDDK ? TaikoAction.RightCentre :
                                                                touchControlsType.Value == TaikoTouchControlType.DDKK ? TaikoAction.LeftRim :
                                                              /*touchControlsType.Value == TaikoTouchControlType.KKDD*/ TaikoAction.LeftCentre,

                                                                touchControlsType.Value == TaikoTouchControlType.KDDK ? colours.Pink :
                                                                touchControlsType.Value == TaikoTouchControlType.DDKK ? colours.Blue :
                                                              /*touchControlsType.Value == TaikoTouchControlType.KKDD*/ colours.Pink)
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomRight,
                                    X = 2,
                                    Scale = new Vector2(centre_region),
                                    Rotation = 90,
                                }
                            }
                        },
                    }
                },
            };
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            // Hide whenever the keyboard is used.
            Hide();
            return false;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (!validMouse(e))
                return false;

            handleDown(e.Button, e.ScreenSpaceMousePosition);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (!validMouse(e))
                return;

            handleUp(e.Button);
            base.OnMouseUp(e);
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            handleDown(e.Touch.Source, e.ScreenSpaceTouchDownPosition);
            return true;
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            handleUp(e.Touch.Source);
            base.OnTouchUp(e);
        }
        private TaikoAction convertInput(TaikoAction input, TaikoTouchControlType controlType) {
            switch(controlType) {
                default:
                    return input; // Using default controls; Nothing needs to be done
                case TaikoTouchControlType.DDKK:
                    switch(input) {
                        case TaikoAction.LeftRim: return TaikoAction.LeftCentre;
                        case TaikoAction.LeftCentre: return TaikoAction.RightCentre;
                        case TaikoAction.RightCentre: return TaikoAction.LeftRim;
                      //case TaikoAction.RightRim: return TaikoAction.RightRim;
                    }
                    return TaikoAction.RightRim;
                case TaikoTouchControlType.KKDD:
                    switch(input) {
                        case TaikoAction.RightRim: return TaikoAction.RightCentre;
                        case TaikoAction.LeftCentre: return TaikoAction.RightRim;
                        case TaikoAction.RightCentre: return TaikoAction.LeftCentre;
                      //case TaikoAction.LeftRim: return TaikoAction.LeftRim;
                    }
                    return TaikoAction.LeftRim;
            }
        }
        private void handleDown(object source, Vector2 position)
        {
            Show();

            TaikoAction taikoAction = convertInput(getTaikoActionFromInput(position), touchControlsType.Value);

            // Not too sure how this can happen, but let's avoid throwing.
            if (trackedActions.ContainsKey(source))
                return;

            trackedActions.Add(source, taikoAction);
            keyBindingContainer.TriggerPressed(taikoAction);
        }

        private void handleUp(object source)
        {
            keyBindingContainer.TriggerReleased(trackedActions[source]);
            trackedActions.Remove(source);
        }

        private bool validMouse(MouseButtonEvent e) =>
            leftRim.Contains(e.ScreenSpaceMouseDownPosition) || rightRim.Contains(e.ScreenSpaceMouseDownPosition);

        private TaikoAction getTaikoActionFromInput(Vector2 inputPosition)
        {
            bool centreHit = leftCentre.Contains(inputPosition) || rightCentre.Contains(inputPosition);
            bool leftSide = ToLocalSpace(inputPosition).X < DrawWidth / 2;

            if (leftSide)
                return centreHit ? TaikoAction.LeftCentre : TaikoAction.LeftRim;

            return centreHit ? TaikoAction.RightCentre : TaikoAction.RightRim;
        }

        protected override void PopIn()
        {
            mainContent.FadeIn(500, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            mainContent.FadeOut(300);
        }

        private partial class QuarterCircle : CompositeDrawable, IKeyBindingHandler<TaikoAction>
        {
            private readonly Circle overlay;

            private readonly TaikoAction handledAction;

            private readonly Circle circle;

            public override bool Contains(Vector2 screenSpacePos) => circle.Contains(screenSpacePos);

            public QuarterCircle(TaikoAction handledAction, Color4 colour)
            {
                this.handledAction = handledAction;
                RelativeSizeAxes = Axes.Both;

                FillMode = FillMode.Fit;

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        Masking = true,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            circle = new Circle
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colour.Multiply(1.4f).Darken(2.8f),
                                Alpha = 0.8f,
                                Scale = new Vector2(2),
                            },
                            overlay = new Circle
                            {
                                Alpha = 0,
                                RelativeSizeAxes = Axes.Both,
                                Blending = BlendingParameters.Additive,
                                Colour = colour,
                                Scale = new Vector2(2),
                            }
                        }
                    },
                };
            }

            public bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
            {
                if (e.Action == handledAction)
                    overlay.FadeTo(1f, 80, Easing.OutQuint);
                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
            {
                if (e.Action == handledAction)
                    overlay.FadeOut(1000, Easing.OutQuint);
            }
        }
    }
}

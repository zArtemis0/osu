﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Edit;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderHead : DrawableHitCircle
    {
        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved(canBeNull: true)]
        private IBeatmap beatmap { get; set; }
        private readonly Bindable<bool> configTimingBasedNoteColouring = new Bindable<bool>();
        public new SliderHeadCircle HitObject => (SliderHeadCircle)base.HitObject;

        [CanBeNull]
        public Slider Slider => DrawableSlider?.HitObject;

        public DrawableSlider DrawableSlider => (DrawableSlider)ParentHitObject;

        public override bool DisplayResult => HitObject?.JudgeAsNormalHitCircle ?? base.DisplayResult;

        /// <summary>
        /// Makes this <see cref="DrawableSliderHead"/> track the follow circle when the start time is reached.
        /// If <c>false</c>, this <see cref="DrawableSliderHead"/> will be pinned to its initial position in the slider.
        /// </summary>
        public bool TrackFollowCircle = true;

        private readonly IBindable<int> pathVersion = new Bindable<int>();

        protected override OsuSkinComponents CirclePieceComponent => OsuSkinComponents.SliderHeadHitCircle;

        public DrawableSliderHead()
        {
        }

        public DrawableSliderHead(SliderHeadCircle h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuRulesetConfigManager rulesetConfig)
        {
            rulesetConfig?.BindWith(OsuRulesetSetting.TimingBasedNoteColouring, configTimingBasedNoteColouring);
            PositionBindable.BindValueChanged(_ => updatePosition());
            pathVersion.BindValueChanged(_ => updatePosition());
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();
             configTimingBasedNoteColouring.BindValueChanged(_ => updateSnapColour());
            StartTimeBindable.BindValueChanged(_ => updateSnapColour(), true);
        }

        protected override void OnFree()
        {
            base.OnFree();

            pathVersion.UnbindFrom(DrawableSlider.PathVersion);
        }

        protected override void OnApply()
        {
            base.OnApply();

            updateSnapColour();
            pathVersion.BindTo(DrawableSlider.PathVersion);

            OnShake = DrawableSlider.Shake;
            CheckHittable = (d, t) => DrawableSlider.CheckHittable?.Invoke(d, t) ?? true;
        }

        protected override void Update()
        {
            base.Update();

            Debug.Assert(Slider != null);
            Debug.Assert(HitObject != null);

            if (TrackFollowCircle)
            {
                double completionProgress = Math.Clamp((Time.Current - Slider.StartTime) / Slider.Duration, 0, 1);

                //todo: we probably want to reconsider this before adding scoring, but it looks and feels nice.
                if (!IsHit)
                    Position = Slider.CurvePositionAt(completionProgress);
            }
        }

        protected override HitResult ResultFor(double timeOffset)
        {
            Debug.Assert(HitObject != null);

            if (HitObject.JudgeAsNormalHitCircle)
                return base.ResultFor(timeOffset);

            // If not judged as a normal hitcircle, judge as a slider tick instead. This is the classic osu!stable scoring.
            var result = base.ResultFor(timeOffset);
            return result.IsHit() ? HitResult.LargeTickHit : HitResult.LargeTickMiss;
        }

        public Action<double> OnShake;

        public override void Shake(double maximumLength) => OnShake?.Invoke(maximumLength);

        private void updatePosition()
        {
            if (Slider != null)
                Position = HitObject.Position - Slider.Position;
        }
             private void updateSnapColour()
        {
            if (beatmap == null || HitObject == null) return;

            int snapDivisor = beatmap.ControlPointInfo.GetClosestBeatDivisor(HitObject.StartTime);

            AccentColour.Value = configTimingBasedNoteColouring.Value ? BindableBeatDivisor.GetColourFor(snapDivisor, colours) : Color4.White;
        }
    }
}

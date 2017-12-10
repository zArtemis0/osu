﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using System.Linq;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableOsuHitObject : DrawableHitObject<OsuHitObject>
    {
        public const float TIME_PREEMPT = 600;
        public const float TIME_FADEIN = 400;
        public const float TIME_FADEOUT = 500;

        public double FadeInSpeed = 1;
        public double FadeOutSpeed = 1;
        public double EarlyFadeOutTime = 0;
        protected float FadeOutAlpha = 0.001f;

        protected DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
            AccentColour = HitObject.ComboColour;
            Alpha = 0;
        }

        protected sealed override void UpdateState(ArmedState state)
        {
            double transformTime = HitObject.StartTime - TIME_PREEMPT;

            base.ApplyTransformsAt(transformTime, true);
            base.ClearTransformsAfter(transformTime, true);

            using (BeginAbsoluteSequence(transformTime, true))
            {
                UpdatePreemptState();

                var delay = TIME_PREEMPT + (Judgements.FirstOrDefault()?.TimeOffset ?? 0) - EarlyFadeOutTime;
                using (BeginDelayedSequence(delay, true))
                {
                    UpdateCurrentState(state);
                    UpdatePostState();
                }
            }
        }

        protected virtual void UpdatePreemptState()
        {
            this.FadeIn(TIME_FADEIN / FadeInSpeed);
        }

        protected virtual void UpdateCurrentState(ArmedState state)
        {
        }

        protected virtual void UpdatePostState()
        {
            double duration = ((HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime) - HitObject.StartTime + EarlyFadeOutTime;
            this.Delay(duration).Expire();
        }

        // Todo: At some point we need to move these to DrawableHitObject after ensuring that all other Rulesets apply
        // transforms in the same way and don't rely on them not being cleared
        public override void ClearTransformsAfter(double time, bool propagateChildren = false, string targetMember = null) { }
        public override void ApplyTransformsAt(double time, bool propagateChildren = false) { }

        private OsuInputManager osuActionInputManager;
        internal OsuInputManager OsuActionInputManager => osuActionInputManager ?? (osuActionInputManager = GetContainingInputManager() as OsuInputManager);
    }

    public enum ComboResult
    {
        [Description(@"")]
        None,
        [Description(@"Good")]
        Good,
        [Description(@"Amazing")]
        Perfect
    }
}

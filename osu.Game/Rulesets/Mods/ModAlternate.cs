// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public class ModAlternate : Mod
    {
        public override string Name => "Alternate";
        public override string Acronym => "AL";
        public override ModType Type => ModType.DifficultyIncrease;
        public override IconUsage? Icon => FontAwesome.Solid.Keyboard;
        public override double ScoreMultiplier => 1;
        public override string Description => "Never use the same key twice!";
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay) };
    }

    public abstract class ModAlternate<THitObject, TAction> : ModAlternate, IApplicableToPlayer, IApplicableToDrawableRuleset<THitObject>, IApplicableToScoreProcessor
        where THitObject : HitObject
        where TAction : struct
    {
        protected readonly BindableBool IsBreakTime = new BindableBool();
        protected readonly BindableInt HighestCombo = new BindableInt();
        protected List<THitObject> HitObjects;
        protected InputInterceptor Interceptor;

        public void ApplyToDrawableRuleset(DrawableRuleset<THitObject> drawableRuleset)
        {
            HitObjects = drawableRuleset.Beatmap.HitObjects;
            drawableRuleset.KeyBindingInputManager.Add(Interceptor = new InputInterceptor(this));
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            HighestCombo.BindTo(scoreProcessor.HighestCombo);
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void ApplyToPlayer(Player player)
        {
            IsBreakTime.BindTo((BindableBool)player.IsBreakTime);
            player.IsBreakTime.BindValueChanged(onBreakTimeChanged, true);
        }

        private void onBreakTimeChanged(ValueChangedEvent<bool> isBreakTime)
        {
            if (!isBreakTime.NewValue)
                ResetActionStates();
        }

        protected abstract bool OnPressed(TAction action);

        protected abstract void ResetActionStates();

        public class InputInterceptor : Drawable, IKeyBindingHandler<TAction>
        {
            private readonly ModAlternate<THitObject, TAction> mod;

            public InputInterceptor(ModAlternate<THitObject, TAction> mod)
            {
                this.mod = mod;
            }

            public bool OnPressed(TAction action) => mod.OnPressed(action);

            public void OnReleased(TAction action)
            {
            }
        }
    }
}

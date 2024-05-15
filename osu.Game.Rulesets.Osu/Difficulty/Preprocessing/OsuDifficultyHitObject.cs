﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    public class OsuDifficultyHitObject : DifficultyHitObject
    {
        /// <summary>
        /// A distance by which all distances should be scaled in order to assume a uniform circle size.
        /// </summary>
        public const int NORMALISED_RADIUS = 50; // Change radius to 50 to make 100 the diameter. Easier for mental maths.

        private const int min_delta_time = 25;
        private const float maximum_slider_radius = NORMALISED_RADIUS * 2.4f;
        private const float assumed_slider_radius = NORMALISED_RADIUS * 1.8f;

        protected new OsuHitObject BaseObject => (OsuHitObject)base.BaseObject;

        /// <summary>
        /// Milliseconds elapsed since the start time of the previous <see cref="OsuDifficultyHitObject"/>, with a minimum of 25ms.
        /// </summary>
        public readonly double StrainTime;

        /// <summary>
        /// Normalised distance from the "lazy" end position of the previous <see cref="OsuDifficultyHitObject"/> to the start position of this <see cref="OsuDifficultyHitObject"/>.
        /// <para>
        /// The "lazy" end position is the position at which the cursor ends up if the previous hitobject is followed with as minimal movement as possible (i.e. on the edge of slider follow circles).
        /// </para>
        /// </summary>
        public double LazyJumpDistance { get; private set; }

        /// <summary>
        /// Normalised shortest distance to consider for a jump between the previous <see cref="OsuDifficultyHitObject"/> and this <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        /// <remarks>
        /// This is bounded from above by <see cref="LazyJumpDistance"/>, and is smaller than the former if a more natural path is able to be taken through the previous <see cref="OsuDifficultyHitObject"/>.
        /// </remarks>
        /// <example>
        /// Suppose a linear slider - circle pattern.
        /// <br />
        /// Following the slider lazily (see: <see cref="LazyJumpDistance"/>) will result in underestimating the true end position of the slider as being closer towards the start position.
        /// As a result, <see cref="LazyJumpDistance"/> overestimates the jump distance because the player is able to take a more natural path by following through the slider to its end,
        /// such that the jump is felt as only starting from the slider's true end position.
        /// <br />
        /// Now consider a slider - circle pattern where the circle is stacked along the path inside the slider.
        /// In this case, the lazy end position correctly estimates the true end position of the slider and provides the more natural movement path.
        /// </example>
        public double MinimumJumpDistance { get; private set; }

        /// <summary>
        /// The time taken to travel through <see cref="MinimumJumpDistance"/>, with a minimum value of 25ms.
        /// </summary>
        public double MinimumJumpTime { get; private set; }

        /// <summary>
        /// Normalised distance between the start and end position of this <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public double TravelDistance { get; private set; }

        /// <summary>
        /// The time taken to travel through <see cref="TravelDistance"/>, with a minimum value of 25ms for <see cref="Slider"/> objects.
        /// </summary>
        public double TravelTime { get; private set; }

        /// <summary>
        /// Angle the player has to take to hit this <see cref="OsuDifficultyHitObject"/>.
        /// Calculated as the angle between the circles (current-2, current-1, current).
        /// Ranges from 0 to PI
        /// </summary>
        public double? Angle { get; private set; }

        /// <summary>
        /// Retrieves the full hit window for a Great <see cref="HitResult"/>.
        /// </summary>
        public double HitWindowGreat { get; private set; }

        /// <summary>
        /// Rhythm difficulty of the object. Saved for optimization, rhythm calculation is very expensive.
        /// </summary>
        public double RhythmDifficulty { get; private set; }

        /// <summary>
        /// Predictabiliy of the angle. Gives high values only in exceptionally repetitive patterns.
        /// </summary>
        public double AnglePredictability { get; private set; }

        /// <summary>
        /// Time in ms between appearence of this <see cref="OsuDifficultyHitObject"/> and moment to click on it.
        /// </summary>
        public readonly double Preempt;

        /// <summary>
        /// Preempt of follow line for this <see cref="OsuDifficultyHitObject"/> adjusted by clockrate.
        /// Will be equal to 0 if object is New Combo.
        /// </summary>
        public readonly double FollowLineTime;

        /// <summary>
        /// Playback rate of beatmap.
        /// Will be equal 1.5 on DT and 0.75 on HT.
        /// </summary>
        public readonly double ClockRate;

        private readonly OsuHitObject? lastLastObject;
        private readonly OsuHitObject lastObject;

        public OsuDifficultyHitObject(HitObject hitObject, HitObject lastObject, HitObject? lastLastObject, double clockRate, List<DifficultyHitObject> objects, int index)
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            OsuHitObject currObject = (OsuHitObject)hitObject;
            this.lastObject = (OsuHitObject)lastObject;
            this.lastLastObject = lastLastObject as OsuHitObject;

            Preempt = BaseObject.TimePreempt / clockRate;
            FollowLineTime = 800 / clockRate; // 800ms is follow line appear time
            FollowLineTime *= (currObject.NewCombo ? 0 : 1); // no follow lines when NC
            ClockRate = clockRate;

            // Capped to 25ms to prevent difficulty calculation breaking from simultaneous objects.
            StrainTime = Math.Max(DeltaTime, min_delta_time);

            if (BaseObject is Slider sliderObject)
            {
                HitWindowGreat = 2 * sliderObject.HeadCircle.HitWindows.WindowFor(HitResult.Great) / clockRate;
            }
            else
            {
                HitWindowGreat = 2 * BaseObject.HitWindows.WindowFor(HitResult.Great) / clockRate;
            }

            setDistances(clockRate);

            AnglePredictability = CalculateAnglePredictability();

            RhythmDifficulty = RhythmEvaluator.EvaluateDifficultyOf(this);
        }

        private static double getTimeDifference(double timeA, double timeB)
        {
            double similarity = Math.Min(timeA, timeB) / Math.Max(timeA, timeB);
            if (Math.Max(timeA, timeB) == 0) similarity = 1;

            if (similarity < 0.75) return 1.0;
            if (similarity > 0.9) return 0.0;

            return (Math.Cos((similarity - 0.75) * Math.PI / 0.15) + 1) / 2; // drops from 1 to 0 as similarity increase from 0.75 to 0.9
        }
        public double CalculateAnglePredictability()
        {
            OsuDifficultyHitObject? prevObj0 = (OsuDifficultyHitObject?)Previous(0);
            OsuDifficultyHitObject? prevObj1 = (OsuDifficultyHitObject?)Previous(1);
            OsuDifficultyHitObject? prevObj2 = (OsuDifficultyHitObject?)Previous(2);

            if (Angle.IsNull() || prevObj0.IsNull() || prevObj0.Angle.IsNull())
                return 1.0;

            double angleDifference = Math.Abs(prevObj0.Angle.Value - Angle.Value);

            // Assume that very low spacing difference means that angles don't matter
            if (prevObj0.LazyJumpDistance < NORMALISED_RADIUS)
                angleDifference *= Math.Pow(prevObj0.LazyJumpDistance / NORMALISED_RADIUS, 2);
            if (LazyJumpDistance < NORMALISED_RADIUS)
                angleDifference *= Math.Pow(LazyJumpDistance / NORMALISED_RADIUS, 2);

            // Now research previous angles
            double angleDifferencePrev = 0;

            // How close the smallest angle of curr and prev is to 0
            double zeroAngleFactor = 1.0;

            // Nerf alternating angles case
            if (prevObj1.IsNotNull() && prevObj2.IsNotNull() && prevObj1.Angle.IsNotNull())
            {
                angleDifferencePrev = Math.Abs(prevObj1.Angle.Value - Angle.Value);
                zeroAngleFactor = Math.Pow(1 - Math.Min(Angle.Value, prevObj0.Angle.Value) / Math.PI, 10);
            }

            // Will be close to 1 if angleDifferencePrev is close to 0
            double rescaleFactor = Math.Pow(1 - angleDifferencePrev / Math.PI, 5);

            // 0 on different rhythm, 1 on same rhythm
            double rhythmFactor = 1 - getTimeDifference(StrainTime, prevObj0.StrainTime);

            if (prevObj1.IsNotNull())
                rhythmFactor *= 1 - getTimeDifference(prevObj0.StrainTime, prevObj1.StrainTime);
            if (prevObj1.IsNotNull() && prevObj2.IsNotNull())
                rhythmFactor *= 1 - getTimeDifference(prevObj1.StrainTime, prevObj2.StrainTime);

            // Get the base - how much alternating difference is lower than current difference
            double prevAngleAdjust = Math.Max(angleDifference - angleDifferencePrev, 0);

            // Don't apply the nerf when angleDifferencePrev is too high
            prevAngleAdjust *= rescaleFactor;

            // Don't apply the nerf if rhythm is changing
            prevAngleAdjust *= rhythmFactor;

            // Don't apply the nerf if neither of previous angles isn't close to 0
            prevAngleAdjust *= zeroAngleFactor;

            angleDifference -= prevAngleAdjust;

            // Bandaid to fix Rubik's Cube +EZ
            double wideness = 0;
            if (Angle!.Value > Math.PI * 0.5)
            {
                // Goes from 0 to 1 as angle increasing from 90 degrees to 180
                wideness = (Angle.Value / Math.PI - 0.5) * 2;

                // Transform into cubic scaling
                wideness = 1 - Math.Pow(1 - wideness, 3);
            }

            // Angle difference will be considered as 2 times lower if angle is wide
            angleDifference /= 1 + wideness;

            // Angle difference more than 15 degrees gets no penalty
            double adjustedAngleDifference = Math.Min(Math.PI / 12, angleDifference);
            return rhythmFactor * Math.Cos(Math.Min(Math.PI / 2, 6 * adjustedAngleDifference));
        }

        public double OpacityAt(double time, bool hidden)
        {
            var baseObject = BaseObject; // Optimization

            if (time > baseObject.StartTime)
            {
                // Consider a hitobject as being invisible when its start time is passed.
                // In reality the hitobject will be visible beyond its start time up until its hittable window has passed,
                // but this is an approximation and such a case is unlikely to be hit where this function is used.
                return 0.0;
            }

            double fadeInStartTime = baseObject.StartTime - baseObject.TimePreempt;
            double fadeInDuration = baseObject.TimeFadeInRaw;

            if (hidden)
            {
                // Taken from OsuModHidden.
                double fadeOutStartTime = baseObject.StartTime - baseObject.TimePreempt + baseObject.TimeFadeInRaw;
                double fadeOutDuration = baseObject.TimePreempt * OsuModHidden.FADE_OUT_DURATION_MULTIPLIER;

                return Math.Min
                (
                    Math.Clamp((time - fadeInStartTime) / fadeInDuration, 0.0, 1.0),
                    1.0 - Math.Clamp((time - fadeOutStartTime) / fadeOutDuration, 0.0, 1.0)
                );
            }

            return Math.Clamp((time - fadeInStartTime) / fadeInDuration, 0.0, 1.0);
        }

        private void setDistances(double clockRate)
        {
            if (BaseObject is Slider currentSlider)
            {
                computeSliderCursorPosition(currentSlider);
                // Bonus for repeat sliders until a better per nested object strain system can be achieved.
                TravelDistance = currentSlider.LazyTravelDistance * (float)Math.Pow(1 + currentSlider.RepeatCount / 2.5, 1.0 / 2.5);
                TravelTime = Math.Max(currentSlider.LazyTravelTime / clockRate, min_delta_time);
            }

            // We don't need to calculate either angle or distance when one of the last->curr objects is a spinner
            if (BaseObject is Spinner || lastObject is Spinner)
                return;

            // We will scale distances by this factor, so we can assume a uniform CircleSize among beatmaps.
            float scalingFactor = NORMALISED_RADIUS / (float)BaseObject.Radius;

            if (BaseObject.Radius < 30)
            {
                float smallCircleBonus = Math.Min(30 - (float)BaseObject.Radius, 5) / 50;
                scalingFactor *= 1 + smallCircleBonus;
            }

            Vector2 lastCursorPosition = getEndCursorPosition(lastObject);

            LazyJumpDistance = (BaseObject.StackedPosition * scalingFactor - lastCursorPosition * scalingFactor).Length;
            MinimumJumpTime = StrainTime;
            MinimumJumpDistance = LazyJumpDistance;

            if (lastObject is Slider lastSlider)
            {
                double lastTravelTime = Math.Max(lastSlider.LazyTravelTime / clockRate, min_delta_time);
                MinimumJumpTime = Math.Max(StrainTime - lastTravelTime, min_delta_time);

                //
                // There are two types of slider-to-object patterns to consider in order to better approximate the real movement a player will take to jump between the hitobjects.
                //
                // 1. The anti-flow pattern, where players cut the slider short in order to move to the next hitobject.
                //
                //      <======o==>  ← slider
                //             |     ← most natural jump path
                //             o     ← a follow-up hitcircle
                //
                // In this case the most natural jump path is approximated by LazyJumpDistance.
                //
                // 2. The flow pattern, where players follow through the slider to its visual extent into the next hitobject.
                //
                //      <======o==>---o
                //                  ↑
                //        most natural jump path
                //
                // In this case the most natural jump path is better approximated by a new distance called "tailJumpDistance" - the distance between the slider's tail and the next hitobject.
                //
                // Thus, the player is assumed to jump the minimum of these two distances in all cases.
                //

                float tailJumpDistance = Vector2.Subtract(lastSlider.TailCircle.StackedPosition, BaseObject.StackedPosition).Length * scalingFactor;
                MinimumJumpDistance = Math.Max(0, Math.Min(LazyJumpDistance - (maximum_slider_radius - assumed_slider_radius), tailJumpDistance - maximum_slider_radius));
            }

            if (lastLastObject != null && !(lastLastObject is Spinner))
            {
                Vector2 lastLastCursorPosition = getEndCursorPosition(lastLastObject);

                Vector2 v1 = lastLastCursorPosition - lastObject.StackedPosition;
                Vector2 v2 = BaseObject.StackedPosition - lastCursorPosition;

                float dot = Vector2.Dot(v1, v2);
                float det = v1.X * v2.Y - v1.Y * v2.X;

                Angle = Math.Abs(Math.Atan2(det, dot));
            }
        }

        private void computeSliderCursorPosition(Slider slider)
        {
            if (slider.LazyEndPosition != null)
                return;

            // TODO: This commented version is actually correct by the new lazer implementation, but intentionally held back from
            // difficulty calculator to preserve known behaviour.
            // double trackingEndTime = Math.Max(
            //     // SliderTailCircle always occurs at the final end time of the slider, but the player only needs to hold until within a lenience before it.
            //     slider.Duration + SliderEventGenerator.TAIL_LENIENCY,
            //     // There's an edge case where one or more ticks/repeats fall within that leniency range.
            //     // In such a case, the player needs to track until the final tick or repeat.
            //     slider.NestedHitObjects.LastOrDefault(n => n is not SliderTailCircle)?.StartTime ?? double.MinValue
            // );

            double trackingEndTime = Math.Max(
                slider.StartTime + slider.Duration + SliderEventGenerator.TAIL_LENIENCY,
                slider.StartTime + slider.Duration / 2
            );

            IList<HitObject> nestedObjects = slider.NestedHitObjects;

            SliderTick? lastRealTick = null;

            foreach (var hitobject in slider.NestedHitObjects)
            {
                if (hitobject is SliderTick tick)
                    lastRealTick = tick;
            }

            if (lastRealTick?.StartTime > trackingEndTime)
            {
                trackingEndTime = lastRealTick.StartTime;

                // When the last tick falls after the tracking end time, we need to re-sort the nested objects
                // based on time. This creates a somewhat weird ordering which is counter to how a user would
                // understand the slider, but allows a zero-diff with known diffcalc output.
                //
                // To reiterate, this is definitely not correct from a difficulty calculation perspective
                // and should be revisited at a later date (likely by replacing this whole code with the commented
                // version above).
                List<HitObject> reordered = nestedObjects.ToList();

                reordered.Remove(lastRealTick);
                reordered.Add(lastRealTick);

                nestedObjects = reordered;
            }

            slider.LazyTravelTime = trackingEndTime - slider.StartTime;

            double endTimeMin = slider.LazyTravelTime / slider.SpanDuration;
            if (endTimeMin % 2 >= 1)
                endTimeMin = 1 - endTimeMin % 1;
            else
                endTimeMin %= 1;

            slider.LazyEndPosition = slider.StackedPosition + slider.Path.PositionAt(endTimeMin); // temporary lazy end position until a real result can be derived.

            Vector2 currCursorPosition = slider.StackedPosition;

            double scalingFactor = NORMALISED_RADIUS / slider.Radius; // lazySliderDistance is coded to be sensitive to scaling, this makes the maths easier with the thresholds being used.

            for (int i = 1; i < nestedObjects.Count; i++)
            {
                var currMovementObj = (OsuHitObject)nestedObjects[i];

                Vector2 currMovement = Vector2.Subtract(currMovementObj.StackedPosition, currCursorPosition);
                double currMovementLength = scalingFactor * currMovement.Length;

                // Amount of movement required so that the cursor position needs to be updated.
                double requiredMovement = assumed_slider_radius;

                if (i == nestedObjects.Count - 1)
                {
                    // The end of a slider has special aim rules due to the relaxed time constraint on position.
                    // There is both a lazy end position as well as the actual end slider position. We assume the player takes the simpler movement.
                    // For sliders that are circular, the lazy end position may actually be farther away than the sliders true end.
                    // This code is designed to prevent buffing situations where lazy end is actually a less efficient movement.
                    Vector2 lazyMovement = Vector2.Subtract((Vector2)slider.LazyEndPosition, currCursorPosition);

                    if (lazyMovement.Length < currMovement.Length)
                        currMovement = lazyMovement;

                    currMovementLength = scalingFactor * currMovement.Length;
                }
                else if (currMovementObj is SliderRepeat)
                {
                    // For a slider repeat, assume a tighter movement threshold to better assess repeat sliders.
                    requiredMovement = NORMALISED_RADIUS;
                }

                if (currMovementLength > requiredMovement)
                {
                    // this finds the positional delta from the required radius and the current position, and updates the currCursorPosition accordingly, as well as rewarding distance.
                    currCursorPosition = Vector2.Add(currCursorPosition, Vector2.Multiply(currMovement, (float)((currMovementLength - requiredMovement) / currMovementLength)));
                    currMovementLength *= (currMovementLength - requiredMovement) / currMovementLength;
                    slider.LazyTravelDistance += (float)currMovementLength;
                }

                if (i == nestedObjects.Count - 1)
                    slider.LazyEndPosition = currCursorPosition;
            }
        }

        private Vector2 getEndCursorPosition(OsuHitObject hitObject)
        {
            Vector2 pos = hitObject.StackedPosition;

            if (hitObject is Slider slider)
            {
                computeSliderCursorPosition(slider);
                pos = slider.LazyEndPosition ?? pos;
            }

            return pos;
        }

        public struct ReadingObject
        {
            public OsuDifficultyHitObject HitObject;
            public double Overlapness;

            public ReadingObject(OsuDifficultyHitObject hitObject, double overlapness)
            {
                HitObject = hitObject;
                Overlapness = overlapness;
            }
        }
    }
}

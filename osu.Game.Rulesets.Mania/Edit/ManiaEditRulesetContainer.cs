﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Screens.Compose;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaEditRulesetContainer : ManiaRulesetContainer
    {
        private bool isLoaded;

        public BindableBeatDivisor BeatDivisor;

        public IEnumerable<EditSnapLine> EditSnapLines;

        public ManiaEditRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap, BindableBeatDivisor beatDivisor)
            : base(ruleset, beatmap)
        {
            BeatDivisor = beatDivisor;
            beatDivisor.ValueChanged += OnBeatSnapDivisorChange;
            OnBeatSnapDivisorChange(beatDivisor.Value);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            isLoaded = true;
            EditSnapLines.ForEach(Playfield.Add);
        }

        public void OnBeatSnapDivisorChange(int newDivisor)
        {
            generateEditSnapLines(newDivisor);
        }

        // TODO: Fix this shit
        private void generateEditSnapLines(int newDivisor)
        {
            // Generate the edit lines
            double lastObjectTime = (Objects.LastOrDefault() as IHasEndTime)?.EndTime ?? Objects.LastOrDefault()?.StartTime ?? double.MaxValue;

            var timingPoints = Beatmap.ControlPointInfo.TimingPoints;
            var editSnapLines = new List<EditSnapLine>();

            for (int i = 0; i < timingPoints.Count; i++)
            {
                TimingControlPoint point = timingPoints[i];

                // Stop on the beat before the next timing point, or if there is no next timing point stop slightly past the last object
                double endTime = i < timingPoints.Count - 1 ? timingPoints[i + 1].Time - point.BeatLength : lastObjectTime + point.BeatLength * (int)point.TimeSignature;

                int index = 0;
                double step = point.BeatLength / newDivisor;
                for (double t = timingPoints[i].Time; Precision.DefinitelyBigger(endTime, t); t += step, index++)
                {
                    editSnapLines.Add(new EditSnapLine
                    {
                        StartTime = t,
                        ControlPoint = point,
                        BeatDivisor = BeatDivisor,
                        BeatIndex = index,
                    });
                }
            }

            EditSnapLines = editSnapLines;
            if (isLoaded)
            {
                Playfield.ClearEditSnapLines();
                EditSnapLines.ForEach(Playfield.Add);
            }
        }

        protected override Playfield CreatePlayfield() => new ManiaEditPlayfield(Beatmap.Stages);

        protected override Vector2 PlayfieldArea => Vector2.One;

        protected override CursorContainer CreateCursor() => null;
    }
}

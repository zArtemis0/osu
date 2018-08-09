﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects;
using osu.Framework.Timing;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Screens.Play
{
    public class SongProgressGraph : SquareGraph
    {
        private readonly List<double> strains = new List<double>();

        public List<double> Strains
        {
            get { return strains; }
            set
            {
                for (int x = 0; x < value.Count; x++)
                {
                    if (!strains.Any())
                    {
                        if (x == 0)
                        {
                            strains.Add(value[x]);
                            Values.Add(value[x]);
                        }
                    }
                    strains.Add(value[x]);
                    Values.Add(value[x]);
                }
            }
        }

        private IEnumerable<HitObject> objects;

        public IEnumerable<HitObject> Objects
        {
            get { return objects; }
            set
            {
                objects = value;

                if (!objects.Any())
                {
                    for (int x = 0; x < Strains.Count; x++)
                        Strains[x] = 0;
                }
                else
                {
                    if (!Strains.Any())
                    {
                        Strains.Add(1);
                        Update();
                        return;
                    }
                }

                var values = new List<int>();

                for (int x = 0; x < strains.Count; x++)
                    values.Add(0);

                var startOfLists = (objects.First().StartTime - objects.First().StartTime % (strainStep * audioClock.Rate))/(strainStep * audioClock.Rate);
                if (strainStep == 1)
                    startOfLists = objects.First().StartTime;

                var interval = strainStep;
                if (interval == 1)
                    interval = 400;
                interval = interval * audioClock.Rate;

                foreach (var h in objects)
                {
                    var endTime = (h as IHasEndTime)?.EndTime ?? h.StartTime;

                    Debug.Assert(endTime >= h.StartTime);

                    int startRange = (int)((h.StartTime - startOfLists) / interval);
                    int endRange = (int)((endTime - startOfLists) / interval);
                    for (int i = startRange; i <= endRange && i < values.Count; i++)
                        values[i]++;
                }

                Debug.Assert(values.Count == Strains.Count);

                for (int x = 0; x < Strains.Count; x++)
                {
                    if (values[x]==0)
                        Strains[x] = Strains.Max() * -1;
                    else
                    {
                        if (Strains[x]<0.01)
                        {
                            Strains[x]=1;
                        }
                    }
                }
                Update();
            }
        }

        private double strainStep;

        public double StrainStep
        {
            get { return strainStep; }
            set
            {
                strainStep = value;
            }
        }

        private IClock audioClock;

        public IClock AudioClock
        {
            get { return audioClock; }
            set { audioClock = value; }
        }

        private List<BreakPeriod> breaks;

        public List<BreakPeriod> Breaks
        {
            get { return breaks; }
            set
            {
                breaks = value;

                var startOfLists = (objects.First().StartTime - objects.First().StartTime % (strainStep * audioClock.Rate))/(strainStep * audioClock.Rate);
                if (strainStep == 1)
                {
                    startOfLists = objects.First().StartTime;
                }

                var interval = strainStep;
                if (interval == 1)
                {
                    strainStep = 400;
                }

                for (int x = 0; x < strains.Count; x++)
                {
                    foreach (var br in breaks)
                    {
                        if ( br.Duration >= interval / 2)
                        {
                            double strainStart = x * interval + startOfLists;
                            double strainEnd = (x+1) * interval + startOfLists;

                            double startOfTouch = new double();
                            if (strainStart >= br.StartTime && strainStart < br.EndTime)
                            {
                                startOfTouch = strainStart;
                            }
                            else
                            {
                                if (strainStart < br.StartTime)
                                {
                                    if (br.StartTime - strainStart < interval /2)
                                    {
                                        startOfTouch = br.StartTime;
                                    }
                                    else
                                        continue;
                                }
                                else
                                    continue;
                            }

                            double endOfTouch = new double();
                            if (strainEnd >= br.StartTime && strainEnd < br.EndTime)
                            {
                                endOfTouch = strainEnd;
                            }
                            else
                            {
                                if (strainEnd > br.EndTime)
                                {
                                    if (strainEnd - br.EndTime < interval /2)
                                    {
                                        endOfTouch = br.EndTime;
                                    }
                                    else
                                        continue;
                                }
                                else
                                    continue;
                            }

                            if (endOfTouch - startOfTouch >= interval / 2)
                            {
                                Strains[x] = Strains.Max() * -1;
                                //^^maybe -1000
                            }
                        }
                    }
                }
            }
        }
    }
}

﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;

namespace osu.Game.Screens.Play
{
    public class BreakOverlay : Container, IStateful<Visibility>
    {
        private IClock audioClock;
        public IClock AudioClock { set { audioClock = value; } }

        private Visibility state;
        public Visibility State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;

                switch (state)
                {
                    case Visibility.Visible:
                        break;
                    case Visibility.Hidden:
                        break;
                }
            }
        }

        private double endTime;

        public BreakOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public void Show(double remainingTime)
        {
            endTime = audioClock?.CurrentTime ?? Time.Current + remainingTime;
            State = Visibility.Visible;
        }

        protected override void Update()
        {
            if (State == Visibility.Hidden) return;

            double currentTime = audioClock?.CurrentTime ?? Time.Current;

            if(currentTime >= endTime)
                State = Visibility.Hidden;
        }
    }
}

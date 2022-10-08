// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Bindables;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens
{
    public abstract class TournamentMatchScreen : TournamentScreen
    {
        protected readonly Bindable<TournamentMatch> CurrentMatch = new Bindable<TournamentMatch>();
        private WarningBox noMatchWarning;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentMatch.BindTo(LadderInfo.CurrentMatch);
            CurrentMatch.BindValueChanged(CurrentMatchChanged, true);
        }

        protected virtual void CurrentMatchChanged(ValueChangedEvent<TournamentMatch> match)
        {
            if (match.NewValue == null)
            {
                AddInternal(noMatchWarning = new WarningBox("请先从时间表选择一场比赛"));
                return;
            }

            noMatchWarning?.Expire();
            noMatchWarning = null;
        }
    }
}

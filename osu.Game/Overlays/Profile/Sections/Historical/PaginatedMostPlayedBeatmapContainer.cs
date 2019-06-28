﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class PaginatedMostPlayedBeatmapContainer : PaginatedContainer
    {
        private GetUserMostPlayedBeatmapsRequest request;

        public PaginatedMostPlayedBeatmapContainer(Bindable<User> user)
            : base(user, "Most Played Beatmaps", "No records. :(")
        {
            ItemsContainer.Direction = FillDirection.Vertical;

            ItemsPerPage = 5;
        }

        protected override void ShowMore()
        {
            request = new GetUserMostPlayedBeatmapsRequest(User.Value.Id, VisiblePages++ * ItemsPerPage, ItemsPerPage);
            request.Success += beatmaps => Schedule(() =>
            {
                MoreButton.FadeTo(beatmaps.Count == ItemsPerPage ? 1 : 0);
                MoreButton.IsLoading = false;

                if (!beatmaps.Any() && VisiblePages == 1)
                {
                    MissingText.Show();
                    return;
                }

                MissingText.Hide();

                foreach (var beatmap in beatmaps)
                {
                    ItemsContainer.Add(new DrawableMostPlayedRow(beatmap.GetBeatmapInfo(Rulesets), beatmap.PlayCount));
                }
            });

            Api.Queue(request);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}

﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Online.API.Requests;
using System.Threading.Tasks;

namespace osu.Game.Overlays.Comments
{
    public class OnlineCommentsContainer : CommentsContainer
    {
        private GetCommentsRequest request;
        private int currentPage;
        private CommentBundleParameters parameters;

        public void ShowComments(CommentableType type, long id)
        {
            parameters = new CommentBundleParameters(type, id);
            Sort.TriggerChange();
        }

        protected override void OnSortChanged(ValueChangedEvent<CommentsSortCriteria> sort)
        {
            ClearComments();
            OnShowMoreAction();
        }

        protected override void ClearComments()
        {
            request?.Cancel();
            currentPage = 1;
            base.ClearComments();
        }

        protected override void OnShowMoreAction()
        {
            request = new GetCommentsRequest(parameters, Sort.Value, currentPage++);
            request.Success += AddComments;
            Task.Run(() => request.Perform(API));
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}

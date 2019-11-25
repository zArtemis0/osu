﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users.Drawables;
using osu.Game.Graphics.Containers;
using osu.Game.Utils;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using System.Linq;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osuTK.Graphics;
using System;
using osu.Framework.Allocation;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Comments
{
    public class DrawableComment : CompositeDrawable
    {
        private const int avatar_size = 40;
        private const int margin = 10;

        public Action OnDeleted;
        public readonly Comment Comment;

        public readonly BindableBool ShowDeleted = new BindableBool();

        public string GetMessage()
        {
            if (api.LocalUser.Value.IsAdmin)
                return Comment.Message;

            if (IsDeleted.Value)
                return @"deleted";

            return Comment.Message;
        }

        protected readonly BindableBool IsDeleted = new BindableBool();

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly BindableBool childrenExpanded = new BindableBool(true);

        private readonly FillFlowContainer childCommentsVisibilityContainer;
        private readonly GridContainer content;
        private readonly VotePill votePill;
        private readonly LinkFlowContainer message;
        private readonly OsuSpriteText deletedIndicator;
        private readonly DeletedChildrenPlaceholder deletedChildrenPlaceholder;
        private readonly DeleteCommentButton deleteButton;

        public DrawableComment(Comment comment, DrawableComment drawableParent)
        {
            LinkFlowContainer username;
            FillFlowContainer childCommentsContainer;
            FillFlowContainer info;

            Comment = comment;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(margin) { Left = margin + 5 },
                        Child = content = new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                            },
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Margin = new MarginPadding { Horizontal = margin },
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(5, 0),
                                        Children = new Drawable[]
                                        {
                                            new Container
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 40,
                                                AutoSizeAxes = Axes.Y,
                                                Child = votePill = new VotePill(comment)
                                                {
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                }
                                            },
                                            new UpdateableAvatar(comment.User)
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Size = new Vector2(avatar_size),
                                                Masking = true,
                                                CornerRadius = avatar_size / 2f,
                                                CornerExponent = 2,
                                            },
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Spacing = new Vector2(0, 3),
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                Spacing = new Vector2(7, 0),
                                                Children = new Drawable[]
                                                {
                                                    username = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true))
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                    },
                                                    new ParentUsername(drawableParent)
                                                    {
                                                        Alpha = drawableParent == null ? 0 : 1
                                                    },
                                                    deletedIndicator = new OsuSpriteText
                                                    {
                                                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                                                        Text = @"deleted",
                                                    }
                                                }
                                            },
                                            message = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 14))
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Padding = new MarginPadding { Right = 40 }
                                            },
                                            info = new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                Spacing = new Vector2(10, 0),
                                                Children = new Drawable[]
                                                {
                                                    new OsuSpriteText
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Font = OsuFont.GetFont(size: 12),
                                                        Text = HumanizerUtils.Humanize(comment.CreatedAt),
                                                        Colour = OsuColour.Gray(0.7f),
                                                    },
                                                    deleteButton = CreateDeleteButton(),
                                                    new RepliesButton(comment.RepliesCount)
                                                    {
                                                        Expanded = { BindTarget = childrenExpanded }
                                                    },
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    childCommentsVisibilityContainer = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            childCommentsContainer = new FillFlowContainer
                            {
                                Padding = new MarginPadding { Left = 20 },
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical
                            },
                            deletedChildrenPlaceholder = new DeletedChildrenPlaceholder
                            {
                                ShowDeleted = { BindTarget = ShowDeleted }
                            }
                        }
                    }
                }
            };

            if (comment.UserId.HasValue)
                username.AddUserLink(comment.User);
            else
                username.AddText(comment.LegacyName);

            if (comment.EditedAt.HasValue)
            {
                info.Add(new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = OsuFont.GetFont(size: 12),
                    Text = $@"edited {HumanizerUtils.Humanize(comment.EditedAt.Value)} by {comment.EditedUser.Username}",
                    Colour = OsuColour.Gray(0.7f)
                });
            }

            if (comment.HasMessage)
            {
                var formattedSource = MessageFormatter.FormatText(comment.Message);
                message.AddLinks(formattedSource.Text, formattedSource.Links);
            }

            if (comment.IsTopLevel)
            {
                AddInternal(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1.5f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(0.1f)
                    }
                });

                if (comment.Replies.Any())
                {
                    AddInternal(new ChevronButton(comment)
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Margin = new MarginPadding { Right = 30, Top = margin },
                        Expanded = { BindTarget = childrenExpanded }
                    });
                }
            }

            comment.Replies.ForEach(c => childCommentsContainer.Add(CreateDrawableReply(c)));
        }

        protected override void LoadComplete()
        {
            IsDeleted.Value = Comment.IsDeleted;

            ShowDeleted.BindValueChanged(show =>
            {
                if (IsDeleted.Value)
                    this.FadeTo(show.NewValue ? 1 : 0);
            });

            IsDeleted.BindValueChanged(deleted =>
            {
                ShowDeleted.TriggerChange();
                content.FadeColour(deleted.NewValue ? OsuColour.Gray(0.5f) : Color4.White);
                votePill.FadeTo(deleted.NewValue ? 0 : 1);
                deletedIndicator.FadeTo(deleted.NewValue ? 1 : 0);
                deleteButton.FadeTo(!deleted.NewValue && (api.LocalUser.Value.Id == Comment.UserId || api.LocalUser.Value.IsAdmin) ? 1 : 0);

                if (deleted.NewValue && !api.LocalUser.Value.IsAdmin)
                    message.Clear();

                if (deleted.NewValue)
                    OnDeleted?.Invoke();
            }, true);

            childrenExpanded.BindValueChanged(expanded => childCommentsVisibilityContainer.FadeTo(expanded.NewValue ? 1 : 0), true);
            base.LoadComplete();
        }

        protected virtual DrawableComment CreateDrawableReply(Comment comment) => new DrawableComment(comment, this)
        {
            ShowDeleted = { BindTarget = ShowDeleted },
            OnDeleted = OnReplyDeleted
        };

        protected virtual DeleteCommentButton CreateDeleteButton() => new DeleteCommentButton
        {
            IsDeleted = { BindTarget = IsDeleted }
        };

        protected void OnReplyDeleted() => deletedChildrenPlaceholder.DeletedCount.Value++;

        private class ChevronButton : ShowChildrenButton
        {
            private readonly SpriteIcon icon;

            public ChevronButton(Comment comment)
            {
                Alpha = comment.IsTopLevel && comment.Replies.Any() ? 1 : 0;
                Child = icon = new SpriteIcon
                {
                    Size = new Vector2(12)
                };
            }

            protected override void OnExpandedChanged(ValueChangedEvent<bool> expanded)
            {
                icon.Icon = expanded.NewValue ? FontAwesome.Solid.ChevronUp : FontAwesome.Solid.ChevronDown;
            }
        }

        private class RepliesButton : ShowChildrenButton
        {
            private readonly SpriteText text;
            private readonly int count;

            public RepliesButton(int count)
            {
                this.count = count;

                Alpha = count == 0 ? 0 : 1;
                Child = text = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                };
            }

            protected override void OnExpandedChanged(ValueChangedEvent<bool> expanded)
            {
                text.Text = $@"{(expanded.NewValue ? "[+]" : "[-]")} replies ({count})";
            }
        }

        private class ParentUsername : FillFlowContainer, IHasTooltip
        {
            public string TooltipText => parent?.GetMessage();

            private readonly DrawableComment parent;

            public ParentUsername(DrawableComment parent)
            {
                this.parent = parent;

                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(3, 0);
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Reply,
                        Size = new Vector2(14),
                    },
                    new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                        Text = parent?.Comment.User?.Username ?? parent?.Comment.LegacyName
                    }
                };
            }
        }
    }
}

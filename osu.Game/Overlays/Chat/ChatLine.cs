﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Users;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Cursor;

namespace osu.Game.Overlays.Chat
{
    public class ChatLine : Container
    {
        private static readonly Color4[] username_colours =
        {
            OsuColour.FromHex("588c7e"),
            OsuColour.FromHex("b2a367"),
            OsuColour.FromHex("c98f65"),
            OsuColour.FromHex("bc5151"),
            OsuColour.FromHex("5c8bd6"),
            OsuColour.FromHex("7f6ab7"),
            OsuColour.FromHex("a368ad"),
            OsuColour.FromHex("aa6880"),

            OsuColour.FromHex("6fad9b"),
            OsuColour.FromHex("f2e394"),
            OsuColour.FromHex("f2ae72"),
            OsuColour.FromHex("f98f8a"),
            OsuColour.FromHex("7daef4"),
            OsuColour.FromHex("a691f2"),
            OsuColour.FromHex("c894d3"),
            OsuColour.FromHex("d895b0"),

            OsuColour.FromHex("53c4a1"),
            OsuColour.FromHex("eace5c"),
            OsuColour.FromHex("ea8c47"),
            OsuColour.FromHex("fc4f4f"),
            OsuColour.FromHex("3d94ea"),
            OsuColour.FromHex("7760ea"),
            OsuColour.FromHex("af52c6"),
            OsuColour.FromHex("e25696"),

            OsuColour.FromHex("677c66"),
            OsuColour.FromHex("9b8732"),
            OsuColour.FromHex("8c5129"),
            OsuColour.FromHex("8c3030"),
            OsuColour.FromHex("1f5d91"),
            OsuColour.FromHex("4335a5"),
            OsuColour.FromHex("812a96"),
            OsuColour.FromHex("992861"),
        };

        public const float LEFT_PADDING = message_padding + padding * 2;

        private const float padding = 15;
        private const float message_padding = 200;
        private const float text_size = 20;

        private Action<User> loadProfile;

        private Color4 customUsernameColour;

        private OsuSpriteText timestamp;

        public ChatLine(Message message)
        {
            Message = message;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Left = padding, Right = padding };
        }

        private Message message;
        private OsuSpriteText username;
        private FillFlowContainer contentFlow;

        public Message Message
        {
            get { return message; }
            set
            {
                if (message == value) return;

                message = value;

                if (!IsLoaded)
                    return;

                updateMessageContent();
            }
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, UserProfileOverlay profile)
        {
            customUsernameColour = colours.ChatBlue;
            loadProfile = u => profile?.ShowUser(u);
        }

        private bool senderHasBackground => !string.IsNullOrEmpty(message.Sender.Colour);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            bool hasBackground = senderHasBackground;

            Drawable effectedUsername = username = new OsuSpriteText
            {
                Font = @"Exo2.0-BoldItalic",
                Colour = hasBackground ? customUsernameColour : username_colours[message.Sender.Id % username_colours.Length],
                TextSize = text_size,
            };

            if (hasBackground)
            {
                // Background effect
                effectedUsername = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 4,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Roundness = 1,
                        Offset = new Vector2(0, 3),
                        Radius = 3,
                        Colour = Color4.Black.Opacity(0.3f),
                        Type = EdgeEffectType.Shadow,
                    },
                    // Drop shadow effect
                    Child = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 4,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Radius = 1,
                            Colour = OsuColour.FromHex(message.Sender.Colour),
                            Type = EdgeEffectType.Shadow,
                        },
                        Padding = new MarginPadding { Left = 3, Right = 3, Bottom = 1, Top = -3 },
                        Y = 3,
                        Child = username,
                    }
                };
            }

            Children = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(message_padding, text_size),
                    Children = new Drawable[]
                    {
                        timestamp = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = @"Exo2.0-SemiBold",
                            FixedWidth = true,
                            TextSize = text_size * 0.75f,
                        },
                        new ClickableContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Child = effectedUsername,
                            Action = () => loadProfile(message.Sender),
                        },
                    }
                },
                contentFlow = new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = message_padding + padding },
                }
            };

            updateMessageContent();
            FinishTransforms(true);
        }

        private void updateMessageContent()
        {
            this.FadeTo(message is LocalEchoMessage ? 0.4f : 1.0f, 500, Easing.OutQuint);
            timestamp.FadeTo(message is LocalEchoMessage ? 0 : 1, 500, Easing.OutQuint);

            timestamp.Text = $@"{message.Timestamp.LocalDateTime:HH:mm:ss}";
            username.Text = $@"{message.Sender.Username}" + (senderHasBackground ? "" : ":");

            contentFlow.Clear();

            LinkFormatterResult formatterResults = LinkFormatter.Format(message.Content);

            string finalText = formatterResults.Text;

            int linksCount = formatterResults.Links.Count;
            int depth = 0;

            if (linksCount > 0)
            {
                for (int i = linksCount - 1; i >= 0; i--)
                {
                    Link l = formatterResults.Links[i];

                    addTextPieceToMessage(finalText.Substring(l.Index + l.Length), depth);
                    depth++;
                    contentFlow.Add(new ClickableLink(l, depth));
                    depth++;

                    finalText = finalText.Substring(0, l.Index);
                }
            }

            if (finalText.Length > 0)
                addTextPieceToMessage(finalText, depth);
        }

        private void addTextPieceToMessage(string text, int depth)
        {
            contentFlow.Add(new OsuSpriteText
            {
                Text = text,
                TextSize = text_size,
                Depth = depth,
            });
        }

        private class ClickableLink : ClickableContainer, IHasTooltip
        {
            public string TooltipText => @"Link: " + link.Url;

            private readonly Link link;
            private readonly Box background;

            private ChatOverlay chat;

            public ClickableLink(Link link, int depth)
            {
                this.link = link;

                AutoSizeAxes = Axes.Both;
                Depth = depth;
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.6f,
                    },
                    new OsuSpriteText
                    {
                        Text = link.DisplayText,
                        TextSize = text_size,
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(ChatOverlay chat)
            {
                this.chat = chat;
            }

            protected override bool OnClick(InputState state)
            {
                chat.HandleLink(link.Url);
                return base.OnClick(state);
            }

            protected override bool OnHover(InputState state)
            {
                background.FadeTo(0.8f);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                background.FadeTo(0.6f);
                base.OnHoverLost(state);
            }
        }
    }
}

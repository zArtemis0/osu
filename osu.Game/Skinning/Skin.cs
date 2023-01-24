﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.Screens.Play.HUD;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace osu.Game.Skinning
{
    public abstract class Skin : IDisposable, ISkin
    {
        /// <summary>
        /// A texture store which can be used to perform user file lookups for this skin.
        /// </summary>
        protected TextureStore? Textures { get; }

        /// <summary>
        /// A sample store which can be used to perform user file lookups for this skin.
        /// </summary>
        protected ISampleStore? Samples { get; }

        public readonly Live<SkinInfo> SkinInfo;

        public SkinConfiguration Configuration { get; set; }

        public IDictionary<GlobalSkinComponentLookup.LookupType, SkinnableInfo[]> DrawableComponentInfo => drawableComponentInfo;

        private readonly Dictionary<GlobalSkinComponentLookup.LookupType, SkinnableInfo[]> drawableComponentInfo = new Dictionary<GlobalSkinComponentLookup.LookupType, SkinnableInfo[]>();

        public abstract ISample? GetSample(ISampleInfo sampleInfo);

        public Texture? GetTexture(string componentName) => GetTexture(componentName, default, default);

        public abstract Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT);

        public abstract IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
            where TLookup : notnull
            where TValue : notnull;

        private readonly RealmBackedResourceStore<SkinInfo>? realmBackedStorage;

        /// <summary>
        /// Construct a new skin.
        /// </summary>
        /// <param name="skin">The skin's metadata. Usually a live realm object.</param>
        /// <param name="resources">Access to game-wide resources.</param>
        /// <param name="storage">An optional store which will *replace* all file lookups that are usually sourced from <paramref name="skin"/>.</param>
        /// <param name="configurationFilename">An optional filename to read the skin configuration from. If not provided, the configuration will be retrieved from the storage using "skin.ini".</param>
        protected Skin(SkinInfo skin, IStorageResourceProvider? resources, IResourceStore<byte[]>? storage = null, string configurationFilename = @"skin.ini")
        {
            if (resources != null)
            {
                SkinInfo = skin.ToLive(resources.RealmAccess);

                storage ??= realmBackedStorage = new RealmBackedResourceStore<SkinInfo>(SkinInfo, resources.Files, resources.RealmAccess);

                var samples = resources.AudioManager?.GetSampleStore(storage);
                if (samples != null)
                    samples.PlaybackConcurrency = OsuGameBase.SAMPLE_CONCURRENCY;

                // osu-stable performs audio lookups in order of wav -> mp3 -> ogg.
                // The GetSampleStore() call above internally adds wav and mp3, so ogg is added at the end to ensure expected ordering.
                (storage as ResourceStore<byte[]>)?.AddExtension("ogg");

                Samples = samples;
                Textures = new TextureStore(resources.Renderer, new SquishingTextureLoaderStore(resources.CreateTextureLoaderStore(storage)));
            }
            else
            {
                // Generally only used for tests.
                SkinInfo = skin.ToLiveUnmanaged();
            }

            var configurationStream = storage?.GetStream(configurationFilename);

            if (configurationStream != null)
            {
                // stream will be closed after use by LineBufferedReader.
                ParseConfigurationStream(configurationStream);
                Debug.Assert(Configuration != null);
            }
            else
                Configuration = new SkinConfiguration();

            // skininfo files may be null for default skin.
            foreach (GlobalSkinComponentLookup.LookupType skinnableTarget in Enum.GetValues<GlobalSkinComponentLookup.LookupType>())
            {
                string filename = $"{skinnableTarget}.json";

                byte[]? bytes = storage?.Get(filename);

                if (bytes == null)
                    continue;

                try
                {
                    string jsonContent = Encoding.UTF8.GetString(bytes);

                    // handle namespace changes...

                    // can be removed 2023-01-31
                    jsonContent = jsonContent.Replace(@"osu.Game.Screens.Play.SongProgress", @"osu.Game.Screens.Play.HUD.DefaultSongProgress");
                    jsonContent = jsonContent.Replace(@"osu.Game.Screens.Play.HUD.LegacyComboCounter", @"osu.Game.Skinning.LegacyComboCounter");

                    var deserializedContent = JsonConvert.DeserializeObject<IEnumerable<SkinnableInfo>>(jsonContent);

                    if (deserializedContent == null)
                        continue;

                    DrawableComponentInfo[skinnableTarget] = deserializedContent.ToArray();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to load skin configuration.");
                }
            }
        }

        protected virtual void ParseConfigurationStream(Stream stream)
        {
            using (LineBufferedReader reader = new LineBufferedReader(stream, true))
                Configuration = new LegacySkinDecoder().Decode(reader);
        }

        /// <summary>
        /// Remove all stored customisations for the provided target.
        /// </summary>
        /// <param name="targetContainer">The target container to reset.</param>
        public void ResetDrawableTarget(ISkinnableTarget targetContainer)
        {
            DrawableComponentInfo.Remove(targetContainer.Target);
        }

        /// <summary>
        /// Update serialised information for the provided target.
        /// </summary>
        /// <param name="targetContainer">The target container to serialise to this skin.</param>
        public void UpdateDrawableTarget(ISkinnableTarget targetContainer)
        {
            DrawableComponentInfo[targetContainer.Target] = targetContainer.CreateSkinnableInfo().ToArray();
        }

        public virtual Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            switch (lookup)
            {
                // This fallback is important for user skins which use SkinnableSprites.
                case SkinnableSprite.SpriteComponentLookup sprite:
                    return this.GetAnimation(sprite.LookupName, false, false);

                case GlobalSkinComponentLookup target:
                    if (!DrawableComponentInfo.TryGetValue(target.Lookup, out var skinnableInfo))
                        return null;

                    var components = new List<Drawable>();

                    foreach (var i in skinnableInfo)
                        components.Add(i.CreateInstance());

                    return new SkinnableTargetComponentsContainer
                    {
                        Children = components,
                    };
            }

            return null;
        }

        #region Disposal

        ~Skin()
        {
            // required to potentially clean up sample store from audio hierarchy.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed)
                return;

            isDisposed = true;

            Textures?.Dispose();
            Samples?.Dispose();

            realmBackedStorage?.Dispose();
        }

        #endregion

        public class SquishingTextureLoaderStore : IResourceStore<TextureUpload>
        {
            private readonly IResourceStore<TextureUpload> textureStore;

            public SquishingTextureLoaderStore(IResourceStore<TextureUpload> textureStore)
            {
                this.textureStore = textureStore;
            }

            public void Dispose()
            {
                textureStore.Dispose();
            }

            public TextureUpload Get(string name)
            {
                var textureUpload = textureStore.Get(name);

                // NRT not enabled on framework side classes (IResourceStore / TextureLoaderStore), welp.
                if (textureUpload.IsNull())
                    return null!;

                // So there's a thing where some users have taken it upon themselves to create skin elements of insane dimensions.
                // To the point where GPUs cannot load the textures (along with most image editor apps).
                // To work around this, let's look out for any stupid images and shrink them down into a usable size.
                const int max_supported_texture_size = 8192;

                if (textureUpload.Height > max_supported_texture_size || textureUpload.Width > max_supported_texture_size)
                {
                    var image = Image.LoadPixelData(textureUpload.Data.ToArray(), textureUpload.Width, textureUpload.Height);

                    // The original texture upload will no longer be returned or used.
                    textureUpload.Dispose();

                    image.Mutate(i => i.Resize(new Size(
                        Math.Min(textureUpload.Width, max_supported_texture_size),
                        Math.Min(textureUpload.Height, max_supported_texture_size)
                    )));

                    return new TextureUpload(image);
                }

                return textureUpload;
            }

            public Task<TextureUpload> GetAsync(string name, CancellationToken cancellationToken = new CancellationToken()) => textureStore.GetAsync(name, cancellationToken);

            public Stream GetStream(string name) => textureStore.GetStream(name);

            public IEnumerable<string> GetAvailableResources() => textureStore.GetAvailableResources();
        }
    }
}

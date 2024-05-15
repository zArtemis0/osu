// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass;
using ManagedBass.Loud;
using osu.Framework.Logging;

namespace osu.Game.Audio
{
    /// <summary>
    /// Audio Normalization Implementation using Bass
    /// </summary>
    public class BassAudioNormalization
    {
        /// <summary>
        /// The integrated loudness of the audio
        /// </summary>
        /// <remarks>
        /// Applicable range = -70 to 0<br/>Null if the loudness could not be calculated due to an error
        /// </remarks>
        public float? IntegratedLoudness { get; }

        /// <summary>
        /// Calculate the integrated loudness of an audio file using Bass
        /// </summary>
        /// <param name="filePath">A path to an audio file</param>
        public BassAudioNormalization(string filePath)
        {
            int decodeStream = Bass.CreateStream(filePath, 0, 0, BassFlags.Decode | BassFlags.Float);

            if (decodeStream == 0)
            {
                Logger.Log("Failed to create stream for loudness measurement!\nError Code: " + Bass.LastError, LoggingTarget.Runtime, LogLevel.Error);
                IntegratedLoudness = null;
                Bass.StreamFree(decodeStream);
                return;
            }

            int loudness = BassLoud.Start(decodeStream, BassFlags.BassLoudnessIntegrated | BassFlags.BassLoudnessAutofree, 0);

            if (loudness == 0)
            {
                Logger.Log("Failed to start loudness measurement!\nError Code: " + Bass.LastError, LoggingTarget.Runtime, LogLevel.Error);
                IntegratedLoudness = null;
                Bass.StreamFree(decodeStream);
                return;
            }

            byte[] buffer = new byte[10000];

            while (Bass.ChannelGetData(decodeStream, buffer, buffer.Length) >= 0)
            {
            }

            float integratedLoudness = 1;
            bool gotLevel = BassLoud.GetLevel(loudness, BassFlags.BassLoudnessIntegrated, ref integratedLoudness);

            if (!gotLevel || integratedLoudness > 0)
            {
                Logger.Log("Failed to get loudness level!\nError Code: " + Bass.LastError, LoggingTarget.Runtime, LogLevel.Error);
                IntegratedLoudness = null;
                Bass.StreamFree(decodeStream);
                return;
            }

            IntegratedLoudness = integratedLoudness;

            bool freedStream = Bass.StreamFree(decodeStream);
            if (!freedStream)
                Logger.Log("Failed to free stream!\nError Code: " + Bass.LastError, LoggingTarget.Runtime, LogLevel.Error);
        }
    }
}

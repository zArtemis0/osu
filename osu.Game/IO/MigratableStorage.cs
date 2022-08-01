// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.IO;
using System.Linq;
using System.Threading;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace osu.Game.IO
{
    /// <summary>
    /// A <see cref="WrappedStorage"/> that is migratable to different locations.
    /// </summary>
    public abstract class MigratableStorage : WrappedStorage
    {
        /// <summary>
        /// The number of file copy failures before actually bailing on migration.
        /// This allows some lenience to cover things like temporary files which could not be copied but are also not too important.
        /// </summary>
        private const int allowed_failures = 10;

        /// <summary>
        /// A relative list of directory paths which should not be migrated.
        /// </summary>
        public virtual string[] IgnoreDirectories => Array.Empty<string>();

        /// <summary>
        /// A relative list of file paths which should not be migrated.
        /// </summary>
        public virtual string[] IgnoreFiles => Array.Empty<string>();

        protected MigratableStorage(Storage storage, string subPath = null)
            : base(storage, subPath)
        {
        }

        /// <summary>
        /// A general purpose migration method to move the storage to a different location.
        /// <param name="newStorage">The target storage of the migration.</param>
        /// </summary>
        /// <returns>Whether cleanup could complete.</returns>
        public virtual bool Migrate(Storage newStorage)
        {
            var source = new DirectoryInfo(GetFullPath("."));
            var destination = new DirectoryInfo(newStorage.GetFullPath("."));

            // using Uri is the easiest way to check equality and contains (https://stackoverflow.com/a/7710620)
            var sourceUri = new Uri(source.FullName + Path.DirectorySeparatorChar);
            var destinationUri = new Uri(destination.FullName + Path.DirectorySeparatorChar);

            if (sourceUri == destinationUri)
                throw new ArgumentException("Destination provided is already the current location", destination.FullName);

            if (sourceUri.IsBaseOf(destinationUri))
                throw new ArgumentException("Destination provided is inside the source", destination.FullName);

            // ensure the new location has no files present, else hard abort
            if (destination.Exists)
            {
                if (destination.GetFiles().Length > 0 || destination.GetDirectories().Length > 0)
                    throw new ArgumentException("Destination provided already has files or directories present", destination.FullName);
            }

            CopyRecursive(source, destination);
            ChangeTargetStorage(newStorage);

            return DeleteRecursive(source);
        }

        protected bool DeleteRecursive(DirectoryInfo target, bool topLevelExcludes = true)
        {
            bool allFilesDeleted = true;

            foreach (System.IO.FileInfo fi in target.GetFiles())
            {
                if (topLevelExcludes && IgnoreFiles.Contains(fi.Name))
                    continue;

                allFilesDeleted &= AttemptOperation(() => fi.Delete());
            }

            foreach (DirectoryInfo dir in target.GetDirectories())
            {
                if (topLevelExcludes && IgnoreDirectories.Contains(dir.Name))
                    continue;

                allFilesDeleted &= AttemptOperation(() => dir.Delete(true));
            }

            if (target.GetFiles().Length == 0 && target.GetDirectories().Length == 0)
                allFilesDeleted &= AttemptOperation(target.Delete);

            return allFilesDeleted;
        }

        protected void CopyRecursive(DirectoryInfo source, DirectoryInfo destination, bool topLevelExcludes = true, int totalFailedOperations = 0)
        {
            // based off example code https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo
            if (!destination.Exists)
                Directory.CreateDirectory(destination.FullName);

            foreach (System.IO.FileInfo fi in source.GetFiles())
            {
                if (topLevelExcludes && IgnoreFiles.Contains(fi.Name))
                    continue;

                if (!AttemptOperation(() => fi.CopyTo(Path.Combine(destination.FullName, fi.Name), false)))
                {
                    Logger.Log($"Failed to copy file {fi.Name} during folder migration");
                    totalFailedOperations++;

                    if (totalFailedOperations > allowed_failures)
                        throw new Exception("Aborting due to too many file copy failures during data migration");
                }
            }

            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (topLevelExcludes && IgnoreDirectories.Contains(dir.Name))
                    continue;

                CopyRecursive(dir, destination.CreateSubdirectory(dir.Name), false, totalFailedOperations);
            }
        }

        /// <summary>
        /// Attempt an IO operation multiple times and only throw if none of the attempts succeed.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <param name="attempts">The number of attempts (250ms wait between each).</param>
        protected static bool AttemptOperation(Action action, int attempts = 10)
        {
            while (true)
            {
                try
                {
                    action();
                    return true;
                }
                catch (Exception)
                {
                    if (attempts-- == 0)
                        return false;
                }

                Thread.Sleep(250);
            }
        }
    }
}

// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLScreens.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

using UnityEngine.XR.MagicLeap.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace UnityEngine.XR.MagicLeap
{
    /// <summary>
    /// System screen information.
    /// </summary>
    [System.Obsolete("Deprecated and scheduled for removal.", true)]
    public struct MLScreensScreenInfo
    {
        /// <summary/>
        public Vector3 Position;

        /// <summary/>
        public Quaternion Rotation;

        /// <summary>
        /// Dimensions of the screens' bounding box.
        /// x, y and z represent
        /// width, height and depth respectively.
        /// </summary>
        public Vector3 Dimensions;

        /// <summary/>
        public ulong Id;

        /// <summary>
        /// Scale of the screen in Universe
        /// </summary>
        public Vector3 Scale;
    }

    /// <summary>
    /// Media watch entry for channel application.
    /// </summary>
    [System.Obsolete("Deprecated and scheduled for removal.", true)]
    public struct MLScreensWatchHistoryEntry
    {
        /// <summary>
        /// Identification number.
        /// </summary>
        public long Id;

        /// <summary>
        /// Media title.
        /// </summary>
        public string Title;

        /// <summary>
        /// Media subtitle. Can be left as empty string.
        /// </summary>
        public string Subtitle;

        /// <summary>
        /// Last playback position in milliseconds.
        /// </summary>
        public uint PlaybackPositionMs;

        /// <summary>
        /// Total duration in milliseconds.
        /// </summary>
        public uint PlaybackDurationMs;

        /// <summary>
        /// Entry is valid if:
        /// * PlaybackPositionMs is smaller or equal to PlaybackDurationMs.
        /// * Title and Subtitle are not null.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return Title != null
                    && Subtitle != null
                    && PlaybackPositionMs <= PlaybackDurationMs;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
             string.Format("id({0})," +
                          "Title({1})," +
                          "Subtitle({2})," +
                          "PlaybackPositionMs({3})," +
                          "PlaybackDurationMs({4})",
                          Id,
                          Title == null ? "" : Title,
                          Subtitle == null ? "" : Subtitle,
                          PlaybackPositionMs,
                          PlaybackDurationMs);
        }
    }

    /// <summary>
    /// Manages list of Screens media watch history and
    /// exposes external screen placement.
    /// </summary>
    [System.Obsolete("Deprecated and scheduled for removal.", true)]
    public sealed class MLScreens : MLAPISingleton<MLScreens>
    {
        /// <summary/>
        public const int DefaultThumbnailWidth = 10;

        /// <summary/>
        public const int DefaultThumbnailHeight = 10;

        /// <summary/>
        public const byte DefaultThumbnailIntensityValue = 128;

        /// <summary/>
        public static readonly TextureFormat[] SupportedThumbnailTextureFormats =
            { TextureFormat.RGB24 };

        /// <summary>
        /// Used to cache entries and avoid native calls if unnecessary.
        /// </summary>
        private readonly Dictionary<long, MLScreensWatchHistoryEntry> _watchHistory;

        /// <summary>
        /// Is not kept up to date. Only necessary for getting watch history at
        /// start.
        /// </summary>
        private MLScreensNativeBindings.MLScreensWatchHistoryListNative _watchHistoryList;

        private MLScreensNativeBindings.MLScreensScreenInfoListExNative _screenInfoList;

        private const int RGBBytesPerPixel = 3;

        private MLImageNativeBindings.MLImageNative _defaultGrayThumbnailImage;

        /// <summary>
        /// static instance of the MLScreens class
        /// </summary>
        private static void CreateInstance()
        {
            if (!IsValidInstance())
            {
                _instance = new MLScreens();
            }
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        ///
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        ///
        /// MLResult.Result will be MLResult.Code.ScreensServiceNotAvailable if failed due to the unavailability of the screens service.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return BaseStart();
        }

        private MLScreens()
        {
            DllNotFoundError = "{0} is only available on device.";
            _watchHistory = new Dictionary<long, MLScreensWatchHistoryEntry>();
            _watchHistoryList = MLScreensNativeBindings.MLScreensWatchHistoryListNative.Create();
            _screenInfoList = MLScreensNativeBindings.MLScreensScreenInfoListExNative.Create();
        }

        #if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Starts the screens object requests, Must be called to receive screens data from
        /// the underlying system
        /// </summary>
        protected override MLResult StartAPI()
        {
            MLResult.Code resultCode = MLScreensNativeBindings.MLScreensGetWatchHistoryList(ref _watchHistoryList);
            var result = MLResult.Create(resultCode);
            if (!result.IsOk)
            {
                MLPluginLog.ErrorFormat("MLScreens.StartAPI failed to retrieve saved screen information. Reason: {0}", result);
                return result;
            }

            PopulateWatchHistory();
            resultCode = MLScreensNativeBindings.MLScreensReleaseWatchHistoryList(ref _watchHistoryList);
            result = MLResult.Create(resultCode);
            if (!result.IsOk)
            {
                MLPluginLog.ErrorFormat("MLScreens.StartAPI failed to clean screens data. Reason: {0}", result);
                return result;
            }

            _defaultGrayThumbnailImage = CreateGrayThumbnailImage();

            return result;
        }
        #endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary/>
        protected override void Update()
        {
        }

        private void PopulateWatchHistory()
        {
            long screensArrayAddress = _watchHistoryList.Entries.ToInt64();
            for (var i = 0; i < _watchHistoryList.Count; ++i)
            {
                long entryAddress = screensArrayAddress +
                                    i * Marshal.SizeOf(typeof(MLScreensNativeBindings.MLScreensWatchHistoryEntryNative));
                var entryPtr = new IntPtr(entryAddress);
                var nativeEntry =
                    (MLScreensNativeBindings.MLScreensWatchHistoryEntryNative)Marshal.PtrToStructure(entryPtr,
                                                                       typeof(MLScreensNativeBindings.MLScreensWatchHistoryEntryNative));
                MLScreensWatchHistoryEntry entry = nativeEntry.Data;
                _watchHistory.Add(entry.Id, entry);
            }
        }

        /// <summary>
        /// Cleans up unmanaged memory
        /// </summary>
        protected override void CleanupAPI(bool isSafeToAccessManagedObjects)
        {
            if (!isSafeToAccessManagedObjects)
                return;

            _watchHistory.Clear();
            if (_defaultGrayThumbnailImage.Image != IntPtr.Zero)
                Marshal.FreeHGlobal(_defaultGrayThumbnailImage.Image);
        }

        private MLResult InternalAdd(ref MLScreensWatchHistoryEntry entry, Texture2D thumbnailImage)
        {
            if (!entry.IsValid)
            {
                return MLResult.Create(MLResult.Code.InvalidParam, "Invalid entry parameter");
            }

            if (thumbnailImage != null && thumbnailImage.format != TextureFormat.RGB24)
            {
                return MLResult.Create(MLResult.Code.InvalidParam, "Invalid thumbnail parameter format");
            }

            MLScreensNativeBindings.MLScreensWatchHistoryEntryNative nativeEntry =  MLScreensNativeBindings.MLScreensWatchHistoryEntryNative.Create();
            nativeEntry.Data = entry;

            MLImageNativeBindings.MLImageNative thumbnail =
                                      thumbnailImage == null ? _defaultGrayThumbnailImage : CreateThumbnailImage(thumbnailImage);
            MLResult.Code resultCode =
                 MLScreensNativeBindings.MLScreensInsertWatchHistoryEntry(ref nativeEntry, ref thumbnail);

            if (thumbnail.Image != IntPtr.Zero && thumbnail.Image != _defaultGrayThumbnailImage.Image)
                Marshal.FreeHGlobal(thumbnail.Image);

            Marshal.FreeHGlobal(nativeEntry.Title);
            Marshal.FreeHGlobal(nativeEntry.Subtitle);
            Marshal.FreeHGlobal(nativeEntry.CustomData);

            var result = MLResult.Create(resultCode);
            if (result.IsOk)
            {
                entry.Id = nativeEntry.Id;
                _watchHistory.Add(entry.Id, entry);
            }

            return result;
        }

        /// Note: this function expects thumbnailImage to be 24-bit
        private MLImageNativeBindings.MLImageNative CreateThumbnailImage(Texture2D thumbnailImage)
        {
            // TODO: Change alignment to 4 and modify ConvertToByteArray to account for that.
            byte[] imageData = MLTextureUtils.ConvertToByteArray(thumbnailImage, out int numChannels);
            var thumbnail = new MLImageNativeBindings.MLImageNative
            {
                ImageType = MLImageNativeBindings.MLImageType.RGB24,
                Alignment = 1,
                Width = (uint)thumbnailImage.width,
                Height = (uint)thumbnailImage.height,
                Image = Marshal.AllocHGlobal(imageData.Length)
            };
            Marshal.Copy(imageData, 0, thumbnail.Image, imageData.Length);
            return thumbnail;
        }

        private static MLImageNativeBindings.MLImageNative CreateGrayThumbnailImage()
        {
            byte[] imageData = Enumerable.Repeat(DefaultThumbnailIntensityValue, DefaultThumbnailWidth * DefaultThumbnailHeight * RGBBytesPerPixel).ToArray();
            var thumbnail = new MLImageNativeBindings.MLImageNative
            {
                ImageType = MLImageNativeBindings.MLImageType.RGB24,
                Alignment = 1,
                Width = DefaultThumbnailWidth,
                Height = DefaultThumbnailHeight,
                Image = Marshal.AllocHGlobal(imageData.Length)
            };
            Marshal.Copy(imageData, 0, thumbnail.Image, imageData.Length);
            return thumbnail;
        }

        private MLResult InternalRemove(long entryId)
        {
            if (!_watchHistory.ContainsKey(entryId))
            {
                return MLResult.Create(MLResult.Code.InvalidParam, "Unknown entry Id");
            }

            MLResult.Code resultCode = MLScreensNativeBindings.MLScreensRemoveWatchHistoryEntry(entryId);
            var result = MLResult.Create(resultCode);
            if (result.IsOk)
            {
                _watchHistory.Remove(entryId);
            }

            return result;
        }

        private MLResult InternalUpdateWatchHistory(MLScreensWatchHistoryEntry entry, Texture2D thumbnailImage)
        {
            if (!entry.IsValid)
            {
                return MLResult.Create(MLResult.Code.InvalidParam, "Invalid entry parameter");
            }

            if (!_watchHistory.ContainsKey(entry.Id))
            {
                return MLResult.Create(MLResult.Code.InvalidParam, "Unknown entry Id");
            }

            if (thumbnailImage != null && thumbnailImage.format != TextureFormat.RGB24)
            {
                return MLResult.Create(MLResult.Code.InvalidParam, "Invalid thumbnail parameter format");
            }

            MLScreensNativeBindings.MLScreensWatchHistoryEntryNative nativeEntry =  MLScreensNativeBindings.MLScreensWatchHistoryEntryNative.Create();
            nativeEntry.Data = entry;

            MLImageNativeBindings.MLImageNative thumbnail =
                                      thumbnailImage == null ? _defaultGrayThumbnailImage : CreateThumbnailImage(thumbnailImage);
            MLResult.Code resultCode = MLScreensNativeBindings.MLScreensUpdateWatchHistoryEntry(ref nativeEntry, ref thumbnail);

            if (thumbnail.Image != IntPtr.Zero && thumbnail.Image != _defaultGrayThumbnailImage.Image)
                Marshal.FreeHGlobal(thumbnail.Image);

            Marshal.FreeHGlobal(nativeEntry.Title);
            Marshal.FreeHGlobal(nativeEntry.Subtitle);
            Marshal.FreeHGlobal(nativeEntry.CustomData);

            var result = MLResult.Create(resultCode);
            if (result.IsOk)
            {
                _watchHistory[entry.Id] = entry;
            }

            return result;
        }

        private MLResult InternalGetEntry(long id, ref MLScreensWatchHistoryEntry entry)
        {
            if (!_watchHistory.ContainsKey(id))
            {
                return MLResult.Create(MLResult.Code.InvalidParam, "Unknown entry Id");
            }

            entry = _watchHistory[id];
            return MLResult.Create(MLResult.Code.Ok);
        }

        private MLResult InternalClearAllEntries()
        {
            var result = MLResult.Create(MLResult.Code.Ok);

            foreach (long entryId in _watchHistory.Keys.ToArray())
            {
                result = InternalRemove(entryId);
                if (!result.IsOk)
                {
                    break;
                }
            }

            return result;
        }

        private MLResult InternalGetScreensInfo(out List<MLScreensScreenInfo> info)
        {
            info = new List<MLScreensScreenInfo>();
            _screenInfoList.Initialize();
            MLResult.Code resultCode = MLScreensNativeBindings.MLScreensGetScreenInfoListEx(ref _screenInfoList);
            var result = MLResult.Create(resultCode);
            if (!result.IsOk)
            {
                return result;
            }

            long screensArrayAddress = _screenInfoList.Entries.ToInt64();
            for (var i = 0; i < _screenInfoList.Count; ++i)
            {
                long entryAddress = screensArrayAddress +
                                    i * Marshal.SizeOf(typeof(MLScreensNativeBindings.MLScreensScreenInfoExNative));
                var entryPtr = new IntPtr(entryAddress);
                var entryNative = (MLScreensNativeBindings.MLScreensScreenInfoExNative)Marshal.PtrToStructure(entryPtr,
                    typeof(MLScreensNativeBindings.MLScreensScreenInfoExNative));
                MLScreensScreenInfo entry = entryNative.Data;
                info.Add(entry);
            }
            resultCode = MLScreensNativeBindings.MLScreensReleaseScreenInfoListEx(ref _screenInfoList);
            result = MLResult.Create(resultCode);
            return result;
        }

        private MLResult InternalGetScreensInfo(ulong id, out MLScreensScreenInfo screenInfo)
        {
            var screenNative = MLScreensNativeBindings.MLScreensScreenInfoExNative.Create();
            MLResult.Code resultCode = MLScreensNativeBindings.MLScreensGetScreenInfo(id, ref screenNative);
            var result = MLResult.Create(resultCode);
            if (!result.IsOk)
            {
                screenInfo = new MLScreensScreenInfo();
                return result;
            }
            else
            {
                screenInfo = screenNative.Data;
            }

            return result;
        }

        private MLResult InternalGetThumbnail(long entryId, out Texture2D imageThumbnail)
        {
            imageThumbnail = null;

            if (!_watchHistory.ContainsKey(entryId))
            {
                return MLResult.Create(MLResult.Code.InvalidParam, "Unknown entry Id");
            }

            var thumbnail = new MLImageNativeBindings.MLImageNative();
            MLResult.Code resultCode = MLScreensNativeBindings.MLScreensGetWatchHistoryThumbnail(entryId, ref thumbnail);
            var result = MLResult.Create(resultCode);
            if (!result.IsOk)
            {
                return result;
            }

            imageThumbnail =
                new Texture2D((int)thumbnail.Width,
                    (int)thumbnail.Height,
                    TextureFormat.RGB24,
                    false,
                    true);

            imageThumbnail.LoadRawTextureData(thumbnail.Image,
                                              (int)(thumbnail.Height * thumbnail.Width) * RGBBytesPerPixel);
            resultCode = MLScreensNativeBindings.MLScreensReleaseWatchHistoryThumbnail(ref thumbnail);
            result = MLResult.Create(resultCode);
            return result;
        }

        private MLResult InternalUpdateScreenInfo(MLScreensScreenInfo info)
        {
            MLScreensNativeBindings.MLScreensScreenInfoExNative nativeScreenInfo = MLScreensNativeBindings.MLScreensScreenInfoExNative.Create();
            nativeScreenInfo.Data = info;

            MLResult.Code resultCode = MLScreensNativeBindings.MLScreensUpdateScreenInfo(ref nativeScreenInfo);
            var result = MLResult.Create(resultCode);
            return result;
        }

        /// <summary>
        /// Adds new media watch entry to history with default thumbnail.
        /// </summary>
        /// <param name="entry">
        /// Entry to add. Id is ignored and gets overwritten if operation is successful.
        /// </param>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidParam if failed due to invalid input parameter.
        ///
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        ///
        /// MLResult.Result will be MLResult.Code.ScreensServiceNotAvailable if failed due to the unavailability of the screens service.
        /// </returns>
        public static MLResult Add(ref MLScreensWatchHistoryEntry entry)
        {
            return Instance.InternalAdd(ref entry, null);
        }

        /// <summary>
        /// Adds new media watch entry to history.
        /// </summary>
        /// <param name="entry">
        /// Entry to add. Id is ignored and gets overwritten if operation is successful.
        /// </param>
        /// <param name="thumbnailImage">
        /// Thumbnail image for entry. Format needs to be TextureFormat.RGB24.
        /// </param>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidParam if failed due to invalid input parameter.
        ///
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        ///
        /// MLResult.Result will be MLResult.Code.ScreensServiceNotAvailable if failed due to the unavailability of the screens service.
        /// </returns>
        public static MLResult Add(ref MLScreensWatchHistoryEntry entry, Texture2D thumbnailImage)
        {
            if (thumbnailImage == null)
            {
                return MLResult.Create(MLResult.Code.InvalidParam, "Invalid null thumbnail parameter");
            }

            return Instance.InternalAdd(ref entry, thumbnailImage);
        }

        /// <summary>
        /// Removes entry from media watch history.
        /// </summary>
        /// <param name="entryId">Id of existing media watch entry.</param>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidParam if failed due to invalid input parameter.
        ///
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        ///
        /// MLResult.Result will be MLResult.Code.ScreensServiceNotAvailable if failed due to the unavailability of the screens service.
        /// </returns>
        public static MLResult Remove(long entryId)
        {
            return Instance.InternalRemove(entryId);
        }

        /// <summary>
        /// Updates media watch entry.
        /// </summary>
        /// <param name="entry">Id of existing media watch entry.</param>
        /// <param name="thumbnailImage">
        /// Optional thumbnail image for entry. Format needs to be TextureFormat.RGB24. If left out, default thumbnail will be used.
        /// </param>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidParam if failed due to invalid input parameter.
        ///
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        ///
        /// MLResult.Result will be MLResult.Code.ScreensServiceNotAvailable if failed due to the unavailability of the screens service.
        /// </returns>
        public static MLResult UpdateWatchHistory(MLScreensWatchHistoryEntry entry, Texture2D thumbnailImage = null)
        {
            return Instance.InternalUpdateWatchHistory(entry, thumbnailImage);
        }

        /// <summary>
        /// Gets entry from media watch history.
        /// </summary>
        /// <param name="id">Id of existing media watch entry.</param>
        /// <param name="entry">Output entry if successful, unmodified otherwise.</param>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidParam if failed due to invalid input parameter.
        /// </returns>
        public static MLResult GetEntry(long id, ref MLScreensWatchHistoryEntry entry)
        {
            return Instance.InternalGetEntry(id, ref entry);
        }

        /// <summary>
        /// Gets all media watch history entries.
        /// </summary>
        /// <returns/>
        public static List<MLScreensWatchHistoryEntry> GetAllEntries()
        {
            return Instance._watchHistory.Values.ToList();
        }

        /// <summary>
        /// Clears all media watch history for this app.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        ///
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        ///
        /// MLResult.Result will be MLResult.Code.ScreensServiceNotAvailable if failed due to the unavailability of the screens service.
        /// </returns>
        public static MLResult ClearAllEntries()
        {
            return Instance.InternalClearAllEntries();
        }

        /// <summary>
        /// Gets information for all system saved screens.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidParam if failed due to invalid input parameter.
        ///
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        ///
        /// MLResult.Result will be MLResult.Code.ScreensServiceNotAvailable if failed due to the unavailability of the screens service.
        ///
        /// MLResult.Result will be MLResult.Code.PermissionDenied if application does not have permission to get information about the specific screen.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidScreenId if the id for the screen is invalid.
        /// </returns>
        public static MLResult GetScreensInfo(out List<MLScreensScreenInfo> info)
        {
            return Instance.InternalGetScreensInfo(out info);
        }

        /// <summary>
        /// Gets information for all system saved screens.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidParam if failed due to invalid input parameter.
        ///
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        ///
        /// MLResult.Result will be MLResult.Code.ScreensServiceNotAvailable if failed due to the unavailability of the screens service.
        ///
        /// MLResult.Result will be MLResult.Code.PrivilegeDenied if necessary privilege is missing.
        ///
        /// MLResult.Result will be MLResult.Code.PermissionDenied if application does not have permission to get information about the specific screen.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidScreenId if the id for the screen is invalid.
        /// </returns>
        public static MLResult GetScreensInfo(ulong id, out MLScreensScreenInfo info)
        {
            return Instance.InternalGetScreensInfo(id, out info);
        }

        /// <summary>
        /// Updates screen info.
        /// Only the screen this app was launched from is a valid one.
        /// </summary>
        /// <param name="screenInfo"/>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidParam if failed due to invalid input parameter.
        ///
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        ///
        /// MLResult.Result will be MLResult.Code.ScreensServiceNotAvailable if failed due to the unavailability of the screens service.
        ///
        /// MLResult.Result will be MLResult.Code.PrivilegeDenied if necessary privilege is missing.
        ///
        /// MLResult.Result will be MLResult.Code.PermissionDenied if application does not have permission to get information about the specific screen.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidScreenId if the id for the screen is invalid.
        /// </returns>
        public static MLResult UpdateScreenInfo(MLScreensScreenInfo screenInfo)
        {
            return Instance.InternalUpdateScreenInfo(screenInfo);
        }

        /// <summary>
        /// Gets previously saved thumbnail for history entry.
        /// Avoid using this function, and maintain thumbnails
        /// through app persistent memory instead.
        /// </summary>
        /// <param name="entryId"/>
        /// <param name="imageThumbnail"/>
        /// <returns>
        /// MLResult.Result will be MLResult.Code.Ok if successful.
        ///
        /// MLResult.Result will be MLResult.Code.InvalidParam if failed due to invalid input parameter.
        ///
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if failed due to internal error.
        ///
        /// MLResult.Result will be MLResult.Code.ScreensServiceNotAvailable if failed due to the unavailability of the screens service.
        /// </returns>
        public static MLResult GetThumbnail(long entryId, out Texture2D imageThumbnail)
        {
            return Instance.InternalGetThumbnail(entryId, out imageThumbnail);
        }

        /// <summary>
        /// Gets a readable version of the result code as an ASCII string.
        /// </summary>
        /// <param name="result">The MLResult that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        public static string GetResultString(MLResultCode result)
        {
            return "This function is deprecated. Use MLResult.CodeToString(MLResult.Code) instead.";
        }

        /// <summary>
        /// Gets a readable version of the result code as an ASCII string.
        /// </summary>
        /// <param name="result">The MLResult that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        public static string GetResultString(MLResult.Code result)
        {
            return Marshal.PtrToStringAnsi(MLScreensNativeBindings.MLScreensGetResultString(result));
        }

        /// <summary>
        /// Gets a screen id of the screen that lauched the application.
        /// </summary>
        /// <returns>the screen id if found -1 if not found.</returns>
        public static long GetLauncherScreenId()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            Regex screenIdRegex = new Regex("screenId=(?<screenId>\\d+)");
            foreach (var arg in args)
            {
                Match match = screenIdRegex.Match(arg);
                if (match.Success)
                {
                    string screenIdValue = match.Groups["screenId"].Value;
                    return long.Parse(screenIdValue);
                }
            }
            MLPluginLog.ErrorFormat("MLScreens.GetLauncherScreenId failed to retrieve valid screen id.");
            return -1;
        }
    }
}

#endif

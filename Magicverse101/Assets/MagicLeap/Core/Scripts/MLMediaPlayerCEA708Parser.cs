// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2019-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Developer Agreement, located
// here: https://auth.magicleap.com/terms/developer
//
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeap
{
    /// <summary>
    /// Simple CEA708 parser that will display unformatted subtitle strings onto specified TextMesh.
    /// </summary>
    public class MLMediaPlayerCEA708Parser : MonoBehaviour
    {
#pragma warning disable 414

        [SerializeField, Tooltip("Should force subtitle track selection")]
        private bool _forceTrackSelection = true;

        [SerializeField, Tooltip("The subtitle track to be selected")]
        private uint _forceSelectTrackID = 3;

        [SerializeField, Tooltip("GameObject with MLMediaPlayer")]
        private GameObject _mediaPlayerGameObject = null;

        [SerializeField, Tooltip("Closed captioning text")]
        private TextMesh _subtitleTextMesh = null;
#pragma warning restore 414

#if PLATFORM_LUMIN
        private MLMediaPlayer _mediaPlayer = null;
        private Dictionary<long, MLMediaPlayer.TrackData> _subtitleTracksCache = null;
        private int _selectedTrackID = -1;
        private string _subtitleText = string.Empty;
        private string[] _cea708Strings = new string[2];
        private int _cea708WindowID = -1;

        /// <summary>
        /// Validate necessary objects and setup CEA708 subtitle processing.
        /// </summary>
        void Start()
        {
            if (!_mediaPlayerGameObject)
            {
                Debug.LogError("Error: MLMediaPlayerCEA708Parser._mediaPlayerGameObject is not set, disabling script.");
                enabled = false;
                return;
            }

            if (!_subtitleTextMesh)
            {
                Debug.LogError("Error: MLMediaPlayerCEA708Parser._subtitleTextMesh is not set, disabling script.");
                enabled = false;
                return;
            }

            _mediaPlayer = _mediaPlayerGameObject.GetComponent<MLMediaPlayer>();
            if (!_mediaPlayer)
            {
                Debug.LogError("Error: MLMediaPlayerCEA708Parser._mediaPlayerGameObject doesn't contain an MLMediaPlayer component.");
                enabled = false;
                return;
            }

            _subtitleTextMesh.text = string.Empty;

            _mediaPlayer.OnSubtitleTracksFound += SubtitleTracksFound;
            _mediaPlayer.OnSubtitle708EventFound += Subtitle708Event;
        }

        /// <summary>
        /// Unsubscribe from all events when component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (_mediaPlayer)
            {
                _mediaPlayer.OnSubtitleTracksFound -= SubtitleTracksFound;
                _mediaPlayer.OnSubtitle708EventFound -= Subtitle708Event;
            }
        }

        /// <summary>
        /// Update is called once per frame
        /// </summary>
        void Update()
        {
            if (_mediaPlayer.IsPlaying)
            {
                _subtitleTextMesh.text = _subtitleText;
            }
        }

        /// <summary>
        /// Select the first CEA708 track we find.
        /// </summary>
        private void SubtitleTracksFound(Dictionary<long, MLMediaPlayer.TrackData> tracks)
        {
            _subtitleTracksCache = tracks;
            if (_selectedTrackID < 0)
            {
                foreach (MLMediaPlayer.TrackData track in tracks.Values)
                {
                    if (track.MimeType == MLMediaPlayer.MimeTypeTextCEA708)
                    {
                        if (_forceTrackSelection && track.ID != _forceSelectTrackID)
                        {
                            // If track selecting is enforced, skip all but the desired track
                            continue;
                        }

                        MLResult result = _mediaPlayer.SelectSubtitleTrack(track.ID);
                        if (result.IsOk)
                        {
                            _selectedTrackID = (int)track.ID;
                            Debug.LogFormat("CEA708 track {0} selected.", _selectedTrackID);
                            break;
                        }
                        else
                        {
                            Debug.LogErrorFormat("Error: MLMediaPlayerCEA708Parser.SubtitleTracksFound Failed to select CEA708 track {0}. Result: {1}", _selectedTrackID, result);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process CEA708 events.
        /// </summary>
        private void Subtitle708Event(MLMediaPlayer.Cea708CaptionEvent event_708)
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            int winBitmap;
            switch (event_708.Type)
            {
                case MLMediaPlayer.Cea708CaptionEmitCommand.Buffer:
                    //Debug.Log("Got event: " + event_708.Type + "obj: " + (string)event_708.Object);

                    if (_cea708WindowID == -1)
                    {
                        _cea708Strings[0] += (string)event_708.Object;
                        _subtitleText = _cea708Strings[0];
                        //Debug.LogFormat("CEA708#1: emit buffer: {0}, winId: -1", _cea708Strings[0]);
                    }
                    else
                    {
                        if (_cea708WindowID == 0)
                        {
                            _cea708Strings[0] += (string)event_708.Object;
                            _subtitleText = _cea708Strings[0];
                            //Debug.LogFormat("CEA708#2: emit buffer: {0}, winId: 0", _cea708Strings[0]);
                        }
                        else if (_cea708WindowID == 1)
                        {
                            _cea708Strings[1] += (string)event_708.Object;
                            _subtitleText = _cea708Strings[1];
                            //Debug.LogFormat("CEA708#3: emit buffer: {0}, winId: 1", _cea708Strings[1]);
                        }
                    }

                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.Control:
                    //Debug.LogFormat("Got event: {0}, obj: {1} ", event_708.Type, (byte)event_708.Object);
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.CWX:
                    //Debug.LogFormat("Got event: "+ event_708.Type + " obj: " + (int)event_708.Object);

                    _cea708WindowID = (int)event_708.Object;
                    if (_cea708WindowID == 0)
                    {
                        _cea708Strings[0] = string.Empty;
                    }
                    else if (_cea708WindowID == 1)
                    {
                        _cea708Strings[1] = string.Empty;
                    }
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.CLW:
                    //Debug.LogFormat("Got event: " + event_708.Type + " obj: " + (int)event_708.Object);

                    winBitmap = (int)event_708.Object;
                    if (_cea708WindowID == -1)
                    {
                        _cea708Strings[0] = string.Empty;
                    }
                    for (int i = 0; i < MLMediaPlayer.Cea708CaptionWindowsMax; i++)
                    {
                        if ((winBitmap & (1 << i)) != 0)
                        {
                            if (i == 0 || i == 1)
                            {
                                _cea708Strings[i] = string.Empty;
                            }
                        }
                    }
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.DSW:
                    //Debug.LogFormat("Got event: " + event_708.Type + " obj: " + (int)event_708.Object);

                    winBitmap = (int)event_708.Object;
                    for (int i = 0; i < MLMediaPlayer.Cea708CaptionWindowsMax; i++)
                    {
                        if ((winBitmap & (1 << i)) != 0)
                        {
                            //Debug.LogFormat("DisplayWindow: bmp: {0}, winID: {1}", winBitmap, i);
                        }
                    }
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.HDW:
                    //Debug.LogFormat("Got event: " + event_708.Type + " obj: " + (int)event_708.Object);

                    winBitmap = (int)event_708.Object;
                    for (int i = 0; i < MLMediaPlayer.Cea708CaptionWindowsMax; i++)
                    {
                        if ((winBitmap & (1 << i)) != 0)
                        {
                            //Debug.LogFormat("HideWindow: bmp: {0}, winID: {1}", winBitmap, i);
                        }
                    }
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.TGW:
                    //Debug.LogFormat("Got event: {0}, obj: {1} ", event_708.Type, (int)event_708.Object);
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.DLW:
                    //Debug.LogFormat("Got event: " + event_708.Type + " obj: " + (int)event_708.Object);

                    winBitmap = (int)event_708.Object;
                    for (int i = 0; i < MLMediaPlayer.Cea708CaptionWindowsMax; i++)
                    {
                        if (((winBitmap & (1 << i)) == 0) || (i == 0 || i == 1))
                        {
                            _cea708Strings[i] = string.Empty;
                        }
                    }
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.DLY:
                    //Debug.LogFormat("Got event: {0}, obj: {1} ", event_708.Type, (int)event_708.Object);
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.DLC:
                    //Debug.LogFormat("Got event: {0}", event_708.Type);
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.RST:
                    //Debug.LogFormat("Got event: {0}", event_708.Type);
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.SPA:
                    MLMediaPlayer.Cea708CaptionPenAttr pen = (MLMediaPlayer.Cea708CaptionPenAttr)event_708.Object;

                    //Debug.LogFormat("Got event: {0},\n" +
                    //    " CaptionCommand SPA penSize: {1} penOffset: {2}, textTag: {3}," +
                    //    " fontTag: {4}, edgeType: {5}, underline: {6}, italic: {7}",
                    //    event_708.Type, pen.PenSize, pen.PenOffset, pen.TextTag, pen.FontTag, pen.EdgeType, pen.Underline, pen.Italic);
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.SPC:
                    MLMediaPlayer.Cea708CaptionPenColor penColor = (MLMediaPlayer.Cea708CaptionPenColor)event_708.Object;

                    //Debug.LogFormat("Got event: {0},\n" +
                    //    " CaptionCommand SPC foregroundColor: {1} backgroundColor: {2} edgeColor: {3}",
                    //    event_708.Type, penColor.ForegroundColor, penColor.BackgroundColor, penColor.EdgeColor);
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.SPL:
                    MLMediaPlayer.Cea708CaptionPenLocation penLoc = (MLMediaPlayer.Cea708CaptionPenLocation)event_708.Object;

                    //Debug.LogFormat("Got event: {0},\n" +
                    //    "CaptionCommand SPL row:{1}, column: {2}",
                    //     event_708.Type, penLoc.Row, penLoc.Column);
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.SWA:
                    MLMediaPlayer.Cea708CaptionWindowAttr winAttr = (MLMediaPlayer.Cea708CaptionWindowAttr)event_708.Object;

                    //Debug.LogFormat("Got event: {0} \n" +
                    //    "CaptionCommand SWA fillColor: {1}, borderColor: {2}, borderType: {3},\n" +
                    //    "wordWrap: {4}, printDirection: {5}, scrollDirection: {6}, justify: {7},\n" +
                    //    "effectDirection: {8}, effectSpeed: {9}, displayEffect: {10}",
                    //    event_708.Type, winAttr.FillColor, winAttr.BorderColor, winAttr.BorderType,
                    //    winAttr.WordWrap, winAttr.PrintDirection, winAttr.ScrollDirection, winAttr.Justify,
                    //    winAttr.EffectDirection, winAttr.EffectSpeed, winAttr.DisplayEffect);
                    break;
                case MLMediaPlayer.Cea708CaptionEmitCommand.DFX:
                    MLMediaPlayer.Cea708CaptionWindow win = (MLMediaPlayer.Cea708CaptionWindow)event_708.Object;

                    //Debug.LogFormat("Got event: {0},\n" +
                    //    "CaptionCommand DFx windowId: {1}, priority: {2}, columnLock: {3}, rowLock: {4},\n" +
                    //    "visible: {5}, anchorVertical: {6}, relativePositioning: {7},  anchorHorizontal: {8},\n" +
                    //    "rowCount:{9}, anchorId: {10}, ColumnCount: {11}, penStyle: {12}, windowStyle: {13}",
                    //    event_708.Type,
                    //    win.ID, win.Priority, win.ColumnLock, win.RowLock, win.Visible, win.AnchorVertical,
                    //    win.RelativePositioning, win.AnchorHorizontal, win.RowCount, win.AnchorID,
                    //    win.ColumnCount, win.PenStyle, win.WindowStyle);
                    break;
                default:
                    Debug.LogError("Error: MLMediaPlayerCEA708Parser.Subtitle708Event unknown event type: " + event_708.Type);
                    break;
            }
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full);
        }

        #endif
    }
}

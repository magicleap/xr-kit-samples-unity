// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
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
    /// Component used to hook into the Hand Tracking script and attach
    /// primitive game objects to it's detected keypoint positions for
    /// each hand.
    /// </summary>
    public class HandTrackingVisualizer : MonoBehaviour
    {
        #region Private Variables
        [SerializeField, Tooltip("The hand to visualize.")]
        private MLHandType _handType = MLHandType.Left;

        [SerializeField, Tooltip("The GameObject to use for the Hand Center.")]
        private Transform _center = null;

        [Header("Hand Keypoint Colors")]

        [SerializeField, Tooltip("The color assigned to the pinky finger keypoints.")]
        private Color _pinkyColor = Color.cyan;

        [SerializeField, Tooltip("The color assigned to the ring finger keypoints.")]
        private Color _ringColor = Color.red;

        [SerializeField, Tooltip("The color assigned to the middle finger keypoints.")]
        private Color _middleColor = Color.blue;

        [SerializeField, Tooltip("The color assigned to the index finger keypoints.")]
        private Color _indexColor = Color.magenta;

        [SerializeField, Tooltip("The color assigned to the thumb keypoints.")]
        private Color _thumbColor = Color.yellow;

        [SerializeField, Tooltip("The color assigned to the wrist keypoints.")]
        private Color _wristColor = Color.white;

        private List<Transform> _pinkyFinger = null;
        private List<Transform> _ringFinger = null;
        private List<Transform> _middleFinger = null;
        private List<Transform> _indexFinger = null;
        private List<Transform> _thumb = null;
        private List<Transform> _wrist = null;
        #endregion

        #region Private Properties
        /// <summary>
        /// Returns the hand based on the hand type.
        /// </summary>
        private MLHand Hand
        {
            get
            {
                if (_handType == MLHandType.Left)
                {
                    return MLHands.Left;
                }
                else
                {
                    return MLHands.Right;
                }
            }
        }
        #endregion

        #region Unity Methods
        /// <summary>
        /// Initializes MLHands API.
        /// </summary>
        void Start()
        {
            MLResult result = MLHands.Start();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: HandTrackingVisualizer failed starting MLHands, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }

            Initialize();
        }

        /// <summary>
        /// Stops the communication to the MLHands API.
        /// </summary>
        void OnDestroy()
        {
            if (MLHands.IsStarted)
            {
                MLHands.Stop();
            }
        }

        /// <summary>
        /// Update the keypoint positions.
        /// </summary>
        void Update()
        {
            if (MLHands.IsStarted)
            {
                // Pinky
                for (int i = 0; i < Hand.Pinky.KeyPoints.Count; ++i)
                {
                    _pinkyFinger[i].localPosition = Hand.Pinky.KeyPoints[i].Position;
                    _pinkyFinger[i].gameObject.SetActive(Hand.IsVisible);
                }

                // Ring
                for (int i = 0; i < Hand.Ring.KeyPoints.Count; ++i)
                {
                    _ringFinger[i].localPosition = Hand.Ring.KeyPoints[i].Position;
                    _ringFinger[i].gameObject.SetActive(Hand.IsVisible);
                }

                // Middle
                for (int i = 0; i < Hand.Middle.KeyPoints.Count; ++i)
                {
                    _middleFinger[i].localPosition = Hand.Middle.KeyPoints[i].Position;
                    _middleFinger[i].gameObject.SetActive(Hand.IsVisible);
                }

                // Index
                for (int i = 0; i < Hand.Index.KeyPoints.Count; ++i)
                {
                    _indexFinger[i].localPosition = Hand.Index.KeyPoints[i].Position;
                    _indexFinger[i].gameObject.SetActive(Hand.IsVisible);
                }

                // Thumb
                for (int i = 0; i < Hand.Thumb.KeyPoints.Count; ++i)
                {
                    _thumb[i].localPosition = Hand.Thumb.KeyPoints[i].Position;
                    _thumb[i].gameObject.SetActive(Hand.IsVisible);
                }

                // Wrist
                for (int i = 0; i < Hand.Wrist.KeyPoints.Count; ++i)
                {
                    _wrist[i].localPosition = Hand.Wrist.KeyPoints[i].Position;
                    _wrist[i].gameObject.SetActive(Hand.IsVisible);
                }

                // Hand Center
                if (_center != null)
                {
                    _center.localPosition = Hand.Center;
                    _center.gameObject.SetActive(Hand.IsVisible);
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initialize the available KeyPoints.
        /// </summary>
        private void Initialize()
        {
            // Pinky
            _pinkyFinger = new List<Transform>();
            for (int i = 0; i < Hand.Pinky.KeyPoints.Count; ++i)
            {
                _pinkyFinger.Add(CreateKeyPoint(Hand.Pinky.KeyPoints[i], _pinkyColor).transform);
            }

            // Ring
            _ringFinger = new List<Transform>();
            for (int i = 0; i < Hand.Ring.KeyPoints.Count; ++i)
            {
                _ringFinger.Add(CreateKeyPoint(Hand.Ring.KeyPoints[i], _ringColor).transform);
            }

            // Middle
            _middleFinger = new List<Transform>();
            for (int i = 0; i < Hand.Middle.KeyPoints.Count; ++i)
            {
                _middleFinger.Add(CreateKeyPoint(Hand.Middle.KeyPoints[i], _middleColor).transform);
            }

            // Index
            _indexFinger = new List<Transform>();
            for (int i = 0; i < Hand.Index.KeyPoints.Count; ++i)
            {
                _indexFinger.Add(CreateKeyPoint(Hand.Index.KeyPoints[i], _indexColor).transform);
            }

            // Thumb
            _thumb = new List<Transform>();
            for (int i = 0; i < Hand.Thumb.KeyPoints.Count; ++i)
            {
                _thumb.Add(CreateKeyPoint(Hand.Thumb.KeyPoints[i], _thumbColor).transform);
            }

            // Wrist
            _wrist = new List<Transform>();
            for (int i = 0; i < Hand.Wrist.KeyPoints.Count; ++i)
            {
                _wrist.Add(CreateKeyPoint(Hand.Wrist.KeyPoints[i], _wristColor).transform);
            }
        }

        /// <summary>
        /// Create a GameObject for the desired KeyPoint.
        /// </summary>
        /// <param name="keyPoint"></param>
        /// <returns></returns>
        private GameObject CreateKeyPoint(MLKeyPoint keyPoint, Color color)
        {
            GameObject newObject;

            newObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            newObject.SetActive(false);
            newObject.transform.SetParent(transform);
            newObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            newObject.name = keyPoint.ToString();
            newObject.GetComponent<Renderer>().material.color = color;

            return newObject;
        }
        #endregion
    }
}

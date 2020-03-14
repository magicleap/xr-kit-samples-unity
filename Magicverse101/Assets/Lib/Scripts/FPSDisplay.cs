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
using UnityEngine;
using UnityEngine.UI;

namespace MagicLeap.XR.XRKit.Sample
{
    public class FPSDisplay : MonoBehaviour
    {
        [SerializeField]
        Text display = null;

        [SerializeField]
        private float DelayCollectionSeconds = 2.0f;

        private float enableTime = 0.0f;
        private bool collectionEnabled = false;

        void Start()
        {
            fps = new RollingAverageFPSCounter(5);
            // Delay collection of frames until app has started & ran for
            // a while.
            enableTime = Time.time + DelayCollectionSeconds;
        }

        void Update()
        {
            // Lazy init, after DelayCollectionSeconds have passed
            if (!collectionEnabled && Time.time >= enableTime)
            {
                fps.StartCollecting(Time.time);
                LogTime = Time.time + 5.0f;
                collectionEnabled = true;
            }

            // Ensure collection was started
            if (collectionEnabled)
            {
                fps.AddFrame(Time.time);

                if (Time.time >= LogTime)
                {
                    LogTime = Time.time + LogInterval;
                    float avgFPS = fps.AvgFPS();
                    Debug.Log("Avg FPS: " + avgFPS);
                    if (display)
                    {
                        display.text = "Avg FPS: " + avgFPS.ToString("N2");
                    }
                }
            }
        }

        private RollingAverageFPSCounter fps;
        private float LogTime = 0.0f;
        private float LogInterval = 1.0f;
    }
}

// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "Timer.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap
{
    public class Timer
    {
        public bool LimitPassed
        {
            get
            {
                return Elapsed() > _timeLimit;
            }
        }

        // All time properties calculated in seconds.
        private float _startTime = 0f;
        private float _timeSinceStart = 0f;
        private float _timeLimit = 0f;


        public Timer(float _timeLimitInSeconds)
        {
            Initialize(_timeLimitInSeconds);
        }

        public void Initialize(float _timeLimitInSeconds)
        {
            _timeLimit = _timeLimitInSeconds;
            Reset();
        }

        public void Reset()
        {
            _startTime = Time.realtimeSinceStartup;
        }

        public float Elapsed()
        {
            _timeSinceStart = Time.realtimeSinceStartup - _startTime;
            return _timeSinceStart;
        }
    }
}

#endif

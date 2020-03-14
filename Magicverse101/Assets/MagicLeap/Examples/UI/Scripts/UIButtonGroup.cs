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

namespace MagicLeap
{
    /// <summary>
    /// This class should be applied to the parent of several UIButtons.
    /// This will enforce a single button active policy, that allow switching between multiple sections.
    /// </summary>
    public class UIButtonGroup : MonoBehaviour
    {
        private UIButton[] _buttons = null;

        private void Awake()
        {
            _buttons = GetComponentsInChildren<UIButton>(true);
        }

        public void Clear()
        {
            if(_buttons == null)
            {
                return;
            }

            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].Default(true);
            }
        }
    }
}

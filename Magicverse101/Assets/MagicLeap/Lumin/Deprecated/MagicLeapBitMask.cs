// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MagicLeapBitMask.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Custom attribute to make it easy to turn enum fields into bit masks in
    /// the inspector. The enum type must be defined in order for the inspector
    /// to be able to know what the bits should be set to.
    /// </summary>
    [Obsolete("Please use MLBitMask instead.", true)]
    [AttributeUsage(AttributeTargets.Field)]
    public class MagicLeapBitMask : PropertyAttribute
    {
        /// <summary>
        /// The Type of the Enum that is being turned into a bit mask.
        /// </summary>
        public Type PropertyType;

        /// <summary>
        /// Creates a new instance of BitMask with the passed in
        /// enum Type. This constructor call is automatic when
        /// decorating a field with this Attribute.
        /// </summary>
        /// <param name="propertyType">The Type value of the enum</param>
        public MagicLeapBitMask(Type propertyType)
        {
            this.PropertyType = propertyType;
        }
    }
}

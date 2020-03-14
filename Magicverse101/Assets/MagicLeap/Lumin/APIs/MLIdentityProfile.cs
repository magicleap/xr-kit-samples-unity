// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLIdentityProfile.cs" company="Magic Leap, Inc">
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
    using System.Runtime.InteropServices;

    /// <summary>
    /// Functionality to query for user profiles.
    /// </summary>
    public partial class MLIdentity
    {
        /// <summary>
        /// Public representation of MLIdentity.Profile.
        /// Represents a set of attribute of a user's profile.
        /// </summary>
        [Serializable]
        public partial class Profile
        {
            #if PLATFORM_LUMIN
            /// <summary>
            /// Array of MLIdentity.Profile.Attributes associated with this profile.
            /// </summary>
            private MLIdentity.Profile.Attribute[] attributes;

            /// <summary>
            /// The current request for attributes attached to this profile.
            /// </summary>
            private Request request = null;

            /// <summary>
            /// The specific attributes to request for this profile.
            /// </summary>
            [SerializeField, Tooltip("List of attributes you want to retrieve, if this is not set then all attributes will be requested")]
            private MLIdentity.Profile.Attribute.Type[] requestAttributes = null;

            /// <summary>
            /// Gets the attributes array of associated with this profile.
            /// </summary>
            public MLIdentity.Profile.Attribute[] Attributes
            {
                get
                {
                    return this.attributes;
                }
            }

            /// <summary>
            /// Gets the current request for attributes attached to this profile.
            /// A profile may only handle one request at a time.
            /// </summary>
            public Request CurrentRequest
            {
                get
                {
                    return this.request;
                }

                private set
                {
                    if (this.request == null)
                    {
                        this.request = value;
                    }
                    else
                    {
                        MLPluginLog.Warning("A request is active and currently fetching attributes.");
                    }
                }
            }

            /// <summary>
            /// Gets the specific attributes to request for this profile.
            /// </summary>
            public MLIdentity.Profile.Attribute.Type[] RequestAttributes
            {
                get
                {
                    return this.requestAttributes;
                }
            }

            /// <summary>
            /// Fetch the specified attributes and callback when result is known.
            /// </summary>
            /// <param name="callback">The callback to notify when the CurrentRequest is complete.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if the operation completed successfully.
            /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if there was an internal error.
            /// </returns>
            public MLResult Fetch(Request.RequestAttibutesDelegate callback)
            {
                if (this.CurrentRequest != null)
                {
                    MLResult result = MLResult.Create(MLResult.Code.UnspecifiedFailure, "Already fetching attributes");
                    MLPluginLog.ErrorFormat("MLIdentity.Profile.Fetch failed. Reason: {0}", result);
                    return result;
                }

                this.CurrentRequest = new Request
                {
                    Callback = callback,
                    ResultCode = MLResult.Code.Pending,
                    RequestState = Request.State.REQUEST_ATTRIB_NAMES,
                };

                MLIdentity.AddProfile(this);

                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Handles when a request queries the attribute names.
            /// </summary>
            public void ProcessRequest()
            {
                if (this.request == null)
                {
                    return;
                }

                switch (this.request.RequestState)
                {
                    case Request.State.REQUEST_ATTRIB_NAMES:
                        this.RequestAttributeNamesAsync();
                        break;

                    case Request.State.LISTENING_ATTRIB_NAMES:
                        this.ListenAttributeNamesResponse();
                        break;

                    case Request.State.REQUEST_ATTRIB_VALUES:
                        this.RequestAttributeValuesAsync();
                        break;

                    case Request.State.LISTENING_ATTRIB_VALUES:
                        this.ListenAttributeValuesResponse();
                        break;

                    case Request.State.DONE:
                        this.request?.Callback?.Invoke(MLResult.Create(this.request.ResultCode));

                        //// Removes the profile if something when wrong with the request.
                        if (!MLResult.IsOK(this.request.ResultCode))
                        {
                            this.request = null;
                            MLIdentity.RemoveProfile(this);
                        }

                        this.request = null;
                        break;

                    default:
                        break;
                }
            }

            /// <summary>
            /// Queries a profile for a list of attribute names asynchronously.
            /// </summary>
            private void RequestAttributeNamesAsync()
            {
                MLResult.Code resultCode = MLIdentity.NativeBindings.RequestAttributeNamesAsync(this, this.requestAttributes, ref this.attributes);

                if (MLResult.IsOK(resultCode))
                {
                    this.request.ResultCode = MLResult.Code.Pending;
                    this.request.RequestState = (this.requestAttributes != null && this.requestAttributes.Length > 0) ?
                           MLIdentity.Profile.Request.State.REQUEST_ATTRIB_VALUES :
                           MLIdentity.Profile.Request.State.LISTENING_ATTRIB_NAMES;
                }
                else
                {
                    MLPluginLog.WarningFormat("MLIdentity.Profile.RequestAttributeNamesAsync failed request for attribute names async. Reason: {0}", MLResult.CodeToString(resultCode));
                    this.request.ResultCode = resultCode;
                    this.request.RequestState = MLIdentity.Profile.Request.State.DONE;
                }
            }

            /// <summary>
            /// Queries a profile for a list of attribute values asynchronously.
            /// </summary>
            private void RequestAttributeValuesAsync()
            {
                MLResult.Code resultCode = MLIdentity.NativeBindings.RequestAttributeValuesAsync(this, ref this.attributes);

                if (MLResult.IsOK(resultCode))
                {
                    this.request.RequestState = Request.State.LISTENING_ATTRIB_VALUES;
                }
                else
                {
                    MLPluginLog.WarningFormat("MLIdentity.Profile.RequestAttributeValuesAsync failed request for attribute values async. Reason: {0}", MLResult.CodeToString(resultCode));
                    this.request.RequestState = Request.State.DONE;
                }
            }

            /// <summary>
            /// Listens for a list of attribute names previously requested by RequestAttributeNamesAsync.
            /// </summary>
            private void ListenAttributeNamesResponse()
            {
                MLResult.Code resultCode = MLIdentity.NativeBindings.ListenAttributeNamesResponse(this, ref this.attributes);

                if (MLResult.IsOK(resultCode))
                {
                    this.request.RequestState = Request.State.REQUEST_ATTRIB_VALUES;
                }
                else if (!MLResult.IsPending(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLIdentity.Profile.ListenAttributeNamesResponse failed to retrieve attribute names. Reason: {0}", resultCode);
                    this.request.RequestState = Request.State.DONE;
                }

                this.request.ResultCode = resultCode;
            }

            /// <summary>
            /// Listens for a list of attribute values previously requested by RequestAttributeValuesAsync.
            /// </summary>
            private void ListenAttributeValuesResponse()
            {
                MLResult.Code resultCode = MLIdentity.NativeBindings.ListenAttributeValuesResponse(this, ref this.attributes);
                if (MLResult.IsOK(resultCode))
                {
                    this.request.RequestState = Request.State.DONE;
                }
                else if (!MLResult.IsPending(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLIdentity.Profile.ListenAttributeValuesResponse failed to retrieve attribute values. Reason: {0}", resultCode);
                    this.request.RequestState = Request.State.DONE;
                }

                this.request.ResultCode = resultCode;
            }

            #endif

            /// <summary>
            /// Represents an attribute of a user's profile.
            /// Each attribute has a name (represented by key and value).
            /// </summary>
            [Serializable]
            public struct Attribute
            {
                #if PLATFORM_LUMIN
                /// <summary>
                /// The attribute key.
                /// </summary>
                public Type Key;

                /// <summary>
                /// The attribute Name.
                /// </summary>
                public string Name;

                /// <summary>
                /// The attribute's string value.
                /// </summary>
                public string Value;

                /// <summary>
                /// The attribute is requested.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool IsRequested;

                /// <summary>
                /// The attribute is granted.
                /// </summary>
                [MarshalAs(UnmanagedType.I1)]
                public bool IsGranted;
                #endif

                /// <summary>
                /// The different types of attributes in a user profile. Attributes that were not known at the time the library was built, are marked as Unknown.
                /// Note: Attribute values that are marked 'deprecated' will silently be ignored when requesting attribute values.
                /// </summary>
                public enum Type
                {
                    /// <summary>
                    /// An unknown attribute.
                    /// </summary>
                    Unknown,

                    /// <summary>
                    /// The given name listed on a profile.
                    /// </summary>
                    [Obsolete("This value is deprecated and will be silently ignored.")]
                    GivenName,

                    /// <summary>
                    /// The family name listed on a profile.
                    /// </summary>
                    [Obsolete("This value is deprecated and will be silently ignored.")]
                    FamilyName,

                    /// <summary>
                    /// The email address listed on a profile.
                    /// </summary>
                    [Obsolete("This value is deprecated and will be silently ignored.")]
                    Email,

                    /// <summary>
                    /// The bio listed on a profile.
                    /// </summary>
                    [Obsolete("This value is deprecated and will be silently ignored.")]
                    Bio,

                    /// <summary>
                    /// The main phone number listed on a profile.
                    /// </summary>
                    [Obsolete("This value is deprecated and will be silently ignored.")]
                    PhoneNumber,

                    /// <summary>
                    /// The Avatar2D representation listed on a profile.
                    /// </summary>
                    [Obsolete("This value is deprecated and will be silently ignored.")]
                    Avatar2D,

                    /// <summary>
                    /// The Avatar3D representation listed on a profile.
                    /// </summary>
                    Avatar3D,

                    /// <summary>
                    /// The Nickname listed on a profile.
                    /// </summary>
                    Nickname,

                    /// <summary>
                    /// The number of attributes listed on a profile.
                    /// </summary>
                    AttributeCount
                }
            }
        }
    }
}

// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLContacts.cs" company="Magic Leap, Inc">
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

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// Defines possible status values for an operation performed on a MLContacts.Contact.
    /// </summary>
    [Obsolete("Please use MLContacts.OperationStatus instead.", true)]
    public enum MLContactsOperationStatus
    {
    }

    /// <summary>
    /// Selection bit field for choosing which fields a search should be executed in
    /// </summary>
    [Flags]
    [Obsolete("Please use MLContacts.SearchField instead.", true)]
    public enum MLContactsSearchField
    {
    }

    /// <summary>
    /// Selection bit field for choosing which fields a search should be executed in
    /// </summary>
    [Flags]
    [Obsolete("Please use MLContacts.SelectionField instead.", true)]
    public enum MLContactsSelectionField
    {
    }

    #if PLATFORM_LUMIN
    /// <summary>
    /// Stores the result of an operation performed on a MLContacts.Contact.
    /// </summary>
    [Obsolete("Please use MLContacts.OperationResult instead.", true)]
    public struct MLContactsOperationResult
    {
    }

    /// <summary>
    /// Stores the result of a list operation
    /// </summary>
    [Obsolete("Please use MLContacts.ListResult instead.", true)]
    public struct MLContactsListResult
    {
    }

    /// <summary>
    /// Used to store a list of contacts
    /// </summary>
    [Obsolete("Please use MLContacts.ContactList instead.", true)]
    public struct MLContactsContactList
    {
    }

    /// <summary>
    /// Stores a tagged value, such as phone number or email address.
    /// Optional tag indicates what type of value is stored, e.g. "home", "work", etc.
    /// </summary>
    [Obsolete("Please use MLContacts.TaggedAttribute instead.", true)]
    public class MLContactsTaggedAttribute
    {
    }

    /// <summary>
    /// Stores a list of MLContactsTaggedAttributes
    /// </summary>
    [Obsolete("Please use MLContacts.TaggedAttributeList instead.", true)]
    public class MLContactsTaggedAttributeList
    {
    }

    /// <summary>
    /// Representation of available information for a single contact in address book.
    /// </summary>
    [Obsolete("Please use MLContacts.Contact instead.", true)]
    public class MLContactsContact
    {
    }

    /// <summary>
    /// MLContacts provides address book style functionality for storing, editing, deleting and retrieving contacts from on device storage.
    /// This API does not expose access to user's connections with other magic leap users (e.g.social graph).
    /// </summary>
    public sealed partial class MLContacts : MLAPISingleton<MLContacts>
    {
        /// <summary>
        /// Default limit for retrieval based operations in number of contacts
        /// </summary>
        [Obsolete("Please use MLContacts.DefaultFetchLimit instead")]
        public const int DEFAULT_FETCH_LIMIT = 250;

        /// <summary>
        /// Gets a readable version of the result code as an ASCII string.
        /// </summary>
        /// <param name="result">The MLResult that should be converted.</param>
        /// <returns>ASCII string containing a readable version of the result code.</returns>
        [Obsolete("Please use MLResult.CodeToString(MLResult.Code) instead.", true)]
        public static string GetResultString(MLResultCode result)
        {
            return "This function is deprecated. Use MLResult.CodeToString(MLResult.Code) instead.";
        }
    }
    #endif
}

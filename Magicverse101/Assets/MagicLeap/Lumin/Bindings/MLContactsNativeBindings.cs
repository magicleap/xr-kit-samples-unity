// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLContactsNativeBindings.cs" company="Magic Leap, Inc">
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
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// MLContacts provides address book style functionality for storing, editing, deleting and retrieving contacts from on device storage.
    /// This API does not expose access to user's connections with other magic leap users (e.g.social graph).
    /// </summary>
    public sealed partial class MLContacts
    {
        /// <summary>
        /// See ml_contacts.h for additional comments.
        /// </summary>
        private class NativeBindings : Native.MagicLeapNativeBindings
        {
            /// <summary>
            /// MLContacts library name
            /// </summary>
            private const string MLContactsDLL = "ml_contacts";

            /// <summary>
            /// Initialize all necessary resources for using MLContacts API.
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if connected to MLContacts successfully.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsStartup();

            /// <summary>
            /// De-initialize all resources for this API.
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if disconnected from MLContacts successfully.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsShutdown();

            /// <summary>
            /// Insert a MLContactsContact.
            /// Request add operation for new contact.To get result of this operation, see MLContactsTryGetOperationResult().
            /// Operation will fail for a contact that does not contain a name and at least one email address or phone numbers
            /// or for a contact that is a duplicate of existing one.Contact id is assigned upon successful completion of add
            /// operation; any id specified for input parameter here will be ignored.
            /// </summary>
            /// <param name="contact">Contact info.</param>
            /// <param name="outRequestHandle">Handle which will contain the handle to the request.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully submitted
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if either of the parameters are invalid.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsRequestInsert(ref ContactNative contact, out ulong outRequestHandle);

            /// <summary>
            /// Update an existing MLContactsContact.
            /// Request edit operation for existing contact. To get result of this operation, see MLContactsTryGetOperationResult().
            /// Contacts are matched by id and input contact must contain a valid id.Operation will fail for a contact that does not
            /// contain a name and at least one email address or phone numbers or for a contact that is a duplicate of existing one.
            /// </summary>
            /// <param name="contact">Contact info.</param>
            /// <param name="outRequestHandle">Handle which will contain the handle to the request.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully submitted
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if either of the parameters are invalid.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsRequestUpdate(ref ContactNative contact, out ulong outRequestHandle);

            /// <summary>
            /// Delete an existing MLContactsContact.
            /// Request delete operation for existing contact. To get result of this operation, see MLContactsTryGetOperationResult().
            /// Contacts are matched by id and input contact must contain a valid id.
            /// </summary>
            /// <param name="contact">Contact info.</param>
            /// <param name="outRequestHandle">Handle which will contain the handle to the request.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully submitted
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if either of the parameters are invalid.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsRequestDelete(ref ContactNative contact, out ulong outRequestHandle);

            /// <summary>
            /// Try to get result of an operation that was previously requested on a single MLContactsContact
            /// (i.e.Insert, Update, or Delete).
            /// This API call may be repeated if it returns Pending for a request handle.
            /// </summary>
            /// <param name="requestHandle">Handle to the request.</param>
            /// <param name="result">Result. See OperationResultNative.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the request is still pending.
            /// MLResult.Result will be <c>MLResult.Code.ContactsCompleted</c> if the request is completed.
            /// MLResult.Result will be <c>MLResult.Code.ContactsHandleNotFound</c> if the request corresponding to the handle was not found.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsTryGetOperationResult(ulong requestHandle, out IntPtr result);

            /// <summary>
            /// List available contacts.
            /// Request list operation. To get result of this operation, see MLContactsTryGetListResult. Contacts
            /// are listed in lexicographical order based on serialization of name, [tag, email] list, and[tag, phone_number] list.
            /// </summary>
            /// <param name="args">See ListArgsNative.</param>
            /// <param name="outRequestHandle">Handle which will contain the handle to the request.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if valid.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsRequestList(ref ListArgsNative args, out ulong outRequestHandle);

            /// <summary>
            /// Search for contacts with a given query across specified fields.
            /// Request search operation. To get result of this operation, see MLContactsTryGetListResult.
            /// Search results will be listed in lexicographical order based on serialization of name,
            /// [tag, email] list, and[tag, phone_number] list.Partial matching of search keywords is supported.
            /// </summary>
            /// <param name="args">See ListArgsNative.</param>
            /// <param name="outRequestHandle">Handle which will contain the handle to the request.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if valid.
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsRequestSearch(ref SearchArgsNative args, out ulong outRequestHandle);

            /// <summary>
            /// Try to get result of a request which is expected to return MLContactsListResult (i.e .List or Search).
            /// </summary>
            /// <param name="requestHandle">Handle to the request.</param>
            /// <param name="result">Result. See ListResultNative.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
            /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the request is still pending.
            /// MLResult.Result will be <c>MLResult.Code.ContactsCompleted</c> if the request is completed.
            /// MLResult.Result will be <c>MLResult.Code.ContactsHandleNotFound</c> if the request corresponding to the handle was not found.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsTryGetListResult(ulong requestHandle, out IntPtr result);

            /// <summary>
            /// Cancel a request corresponding to the handle. Request cancellation will also release
            /// resources associated with request handle.
            /// </summary>
            /// <param name="requestHandle">Handle to the request.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.ContactsCancelled</c> if the request is completed.
            /// MLResult.Result will be <c>MLResult.Code.ContactsHandleNotFound</c> if the request corresponding to the handle was not found.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsCancelRequest(ulong requestHandle);

            /// <summary>
            /// Free all resources for a request corresponding to the handle. MLContacts API users are
            /// expected to free resources for all handles.
            /// </summary>
            /// <param name="request_handle">Handle to the request.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if all resources for the request were successfully freed.
            /// MLResult.Result will be <c>MLResult.Code.ContactsHandleNotFound</c> if the request corresponding to the handle was not found.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsReleaseRequestResources(ulong request_handle);

            /// <summary>
            /// Request a subset of contacts as manually selected by the user via separate system application.
            /// Request selection operation. To get result of this operation, MLContactsTryGetListResult().
            /// </summary>
            /// <param name="args">See SelectionArgsNative.</param>
            /// <param name="outRequestHandle">Handle which will contain the handle to the request.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if valid.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
            /// </returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern MLResult.Code MLContactsRequestSelection(in SelectionArgsNative args, out ulong outRequestHandle);

            /// <summary>
            /// Gets a readable version of the result code as an ASCII string.
            /// </summary>
            /// <param name="result">The MLResult.Code that should be converted.</param>
            /// <returns>ASCII string containing a readable version of the result code.</returns>
            [DllImport(MLContactsDLL, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr MLContactsGetResultString(MLResult.Code result);

            /// <summary>
            /// Stores a tagged value, such as phone number or email address.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TaggedAttributeNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Name of the Tag.
                /// </summary>
                public IntPtr Tag;

                /// <summary>
                /// Value of this attribute.
                /// </summary>
                public IntPtr Value;

                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                public MLContacts.TaggedAttribute Data
                {
                    get
                    {
                        return new MLContacts.TaggedAttribute()
                        {
                            Tag = Native.MLConvert.DecodeUTF8(this.Tag),
                            Value = Native.MLConvert.DecodeUTF8(this.Value),
                        };
                    }

                    set
                    {
                        this.Tag = Native.MLConvert.EncodeToUnmanagedUTF8(value.Tag);
                        this.Value = Native.MLConvert.EncodeToUnmanagedUTF8(value.Value);
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>A new instance of this struct.</returns>
                public static TaggedAttributeNative Create()
                {
                    return new TaggedAttributeNative()
                    {
                        Version = 1u,
                        Tag = IntPtr.Zero,
                        Value = IntPtr.Zero
                    };
                }

                /// <summary>
                /// Free allocated unmanaged memory.
                /// </summary>
                public void Clean()
                {
                    Marshal.FreeHGlobal(this.Tag);
                    Marshal.FreeHGlobal(this.Value);
                }
            }

            /// <summary>
            /// Stores a list of TaggedAttributeNative instances.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct TaggedAttributeListNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Number of tagged attributes in this list.
                /// </summary>
                public ulong Count;

                /// <summary>
                /// Pointer referring to the array of tagged attributes.
                /// </summary>
                public IntPtr Items;

                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                public MLContacts.TaggedAttributeList Data
                {
                    get
                    {
                        MLContacts.TaggedAttributeList contactsList = new MLContacts.TaggedAttributeList();
                        int listSize = (int)this.Count;

                        IntPtr walkPtr = this.Items;
                        for (int i = 0; i < listSize; ++i)
                        {
                            NativeBindings.TaggedAttributeNative attribute = (NativeBindings.TaggedAttributeNative)Marshal.PtrToStructure(Marshal.ReadIntPtr(walkPtr), typeof(NativeBindings.TaggedAttributeNative));
                            contactsList.Items.Add(attribute.Data);
                            walkPtr = new IntPtr(walkPtr.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));
                        }

                        return contactsList;
                    }

                    set
                    {
                        if (value.Count == 0)
                        {
                            this.Count = 0;
                            this.Items = IntPtr.Zero;
                        }
                        else
                        {
                            this.Count = (ulong)value.Count;

                            int attributeSize = Marshal.SizeOf(typeof(NativeBindings.TaggedAttributeNative));
                            IntPtr arrayPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * value.Count);
                            IntPtr walkPtr = arrayPtr;
                            for (int i = 0; i < value.Count; ++i)
                            {
                                IntPtr structurePtr = Marshal.AllocHGlobal(attributeSize);
                                NativeBindings.TaggedAttributeNative internalAttribute = NativeBindings.TaggedAttributeNative.Create();
                                internalAttribute.Data = value.Items[i];
                                Marshal.StructureToPtr(internalAttribute, structurePtr, true);
                                Marshal.WriteIntPtr(walkPtr, structurePtr);
                                walkPtr = new IntPtr(walkPtr.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));
                            }

                            this.Items = arrayPtr;
                        }
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>A new instance of this struct.</returns>
                public static TaggedAttributeListNative Create()
                {
                    return new TaggedAttributeListNative()
                    {
                        Version = 1u,
                        Count = 0,
                        Items = IntPtr.Zero
                    };
                }

                /// <summary>
                /// Free allocated unmanaged memory.
                /// </summary>
                public void Clean()
                {
                    IntPtr startPtr = this.Items;
                    IntPtr walkPtr = startPtr;
                    int listSize = (int)this.Count;
                    for (int i = 0; i < listSize; ++i)
                    {
                        IntPtr structurePtr = Marshal.ReadIntPtr(walkPtr);
                        NativeBindings.TaggedAttributeNative attribute = (NativeBindings.TaggedAttributeNative)Marshal.PtrToStructure(structurePtr, typeof(NativeBindings.TaggedAttributeNative));
                        attribute.Clean();
                        Marshal.FreeHGlobal(structurePtr);
                        walkPtr = new IntPtr(walkPtr.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));
                    }

                    Marshal.FreeHGlobal(startPtr);
                }
            }

            /// <summary>
            /// Representation of available information for a single contact in address book.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ContactNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Locally-unique contact identifier.
                /// </summary>
                public IntPtr ID;

                /// <summary>
                /// Contacts name.
                /// </summary>
                public IntPtr Name;

                /// <summary>
                /// List of tagged phone numbers.
                /// </summary>
                public TaggedAttributeListNative PhoneNumberList;

                /// <summary>
                /// List of tagged email addresses.
                /// </summary>
                public TaggedAttributeListNative EmailAddressList;

                /// <summary>
                /// Gets or sets the native structures from the user facing properties.
                /// </summary>
                public MLContacts.Contact Data
                {
                    get
                    {
                        return new MLContacts.Contact()
                        {
                            Name = Native.MLConvert.DecodeUTF8(this.Name),
                            ID = this.ID != IntPtr.Zero ? Native.MLConvert.DecodeUTF8(this.ID) : null,
                            PhoneNumberList = this.PhoneNumberList.Data,
                            EmailAddressList = this.EmailAddressList.Data,
                        };
                    }

                    set
                    {
                        this.ID = value.ID != null ? Native.MLConvert.EncodeToUnmanagedUTF8(value.ID) : IntPtr.Zero;
                        this.Name = value.Name != null ? Native.MLConvert.EncodeToUnmanagedUTF8(value.Name) : IntPtr.Zero;
                        this.PhoneNumberList.Data = value.PhoneNumberList;
                        this.EmailAddressList.Data = value.EmailAddressList;
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>A new instance of this struct.</returns>
                public static ContactNative Create()
                {
                    return new ContactNative()
                    {
                        Version = 1u,
                        ID = IntPtr.Zero,
                        Name = IntPtr.Zero,
                        PhoneNumberList = TaggedAttributeListNative.Create(),
                        EmailAddressList = TaggedAttributeListNative.Create()
                    };
                }

                /// <summary>
                /// Free all unmanaged memory.
                /// </summary>
                public void Clean()
                {
                    Marshal.FreeHGlobal(this.ID);
                    Marshal.FreeHGlobal(this.Name);
                    this.PhoneNumberList.Clean();
                    this.EmailAddressList.Clean();
                }
            }

            /// <summary>
            /// Return values for Contacts API calls.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct OperationResultNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Status of the operation
                /// </summary>
                public MLContacts.OperationStatus Status;

                /// <summary>
                /// Resultant contact with updated fields.
                /// </summary>
                public IntPtr Contact;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>A new instance of this struct.</returns>
                public static OperationResultNative Create()
                {
                    return new OperationResultNative
                    {
                        Version = 1u,
                        Status = MLContacts.OperationStatus.Fail,
                        Contact = IntPtr.Zero
                    };
                }

                /// <summary>
                /// Gets a managed operation result for a specific operation handle.
                /// </summary>
                /// <param name="handle">Handle to a specific operation.</param>
                /// <param name="result">Managed operation result.</param>
                /// <returns>
                /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
                /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the request is still pending.
                /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
                /// MLResult.Result will be <c>MLResult.Code.ContactsCompleted</c> if the request is completed.
                /// MLResult.Result will be <c>MLResult.Code.ContactsHandleNotFound</c> if the request corresponding to the handle was not found.
                /// </returns>
                public static MLResult.Code GetManagedOperationResult(ulong handle, out MLContacts.OperationResult result)
                {
                    MLResult.Code resultCode = NativeBindings.MLContactsTryGetOperationResult(handle, out IntPtr operationResultPtr);
                    if (resultCode != MLResult.Code.Pending)
                    {
                        if (resultCode != MLResult.Code.ContactsCompleted)
                        {
                            MLPluginLog.ErrorFormat("NativeBindings.GetManagedOperationResult failed to get operation result. Reason: {0}", MLResult.CodeToString(resultCode));
                        }

                        NativeBindings.OperationResultNative internalResult = (NativeBindings.OperationResultNative)Marshal.PtrToStructure(operationResultPtr, typeof(NativeBindings.OperationResultNative));
                        NativeBindings.ContactNative internalContact = (NativeBindings.ContactNative)Marshal.PtrToStructure(internalResult.Contact, typeof(NativeBindings.ContactNative));

                        result = new MLContacts.OperationResult()
                        {
                            Status = internalResult.Status,
                            Contact = internalContact.Data,
                        };
                    }
                    else
                    {
                        result = new MLContacts.OperationResult();
                    }

                    return resultCode;
                }
            }

            /// <summary>
            /// Stores arguments for a List request (MLContactsRequestList()).
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ListArgsNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Request results offset by this amount assuming contacts are ordered in a consistent way.
                /// </summary>
                public IntPtr Offset;

                /// <summary>
                /// Limit the number of results returned by list operation.
                /// </summary>
                public uint Limit;

                /// <summary>
                /// Initializes a new instance of the <see cref="ListArgsNative"/> struct.
                /// </summary>
                /// <param name="pageLength">Limit the number of results returned by list operation.</param>
                /// <param name="pageOffset">Request results offset by this amount assuming contacts are ordered in a consistent way.</param>
                public ListArgsNative(uint pageLength, string pageOffset)
                {
                    this.Version = 1u;
                    this.Offset = pageOffset == string.Empty ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(pageOffset);
                    this.Limit = pageLength;
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>A new instance of this struct.</returns>
                public static ListArgsNative Create()
                {
                    return new ListArgsNative()
                    {
                        Version = 1u,
                        Offset = IntPtr.Zero,
                        Limit = MLContacts.DefaultFetchLimit
                    };
                }

                /// <summary>
                /// Free all unmanaged memory.
                /// </summary>
                public void Clean()
                {
                    Marshal.FreeHGlobal(this.Offset);
                }
            }

            /// <summary>
            /// Stores result of an operation that returns a list of contacts.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ListResultNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Status of operation.
                /// </summary>
                public MLContacts.OperationStatus Status;

                /// <summary>
                /// Resultant list of contacts.
                /// </summary>
                public ContactListNative List;

                /// <summary>
                /// Offset to be used to get the next batch of results for this operation.
                /// </summary>
                public IntPtr Offset;

                /// <summary>
                /// Total number of hits for this request.
                /// </summary>
                public ulong TotalHits;

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>A new instance of this struct.</returns>
                public static ListResultNative Create()
                {
                    return new ListResultNative()
                    {
                        Version = 1u,
                        Status = MLContacts.OperationStatus.Fail,
                        List = ContactListNative.Create(),
                        Offset = IntPtr.Zero,
                        TotalHits = 0
                    };
                }

                /// <summary>
                /// Gets a managed operation result for a specific operation handle.
                /// </summary>
                /// <param name="handle">Handle to a specific operation.</param>
                /// <param name="listResult">Managed operation result.</param>
                /// <returns>
                /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
                /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the request is still pending.
                /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
                /// MLResult.Result will be <c>MLResult.Code.ContactsCompleted</c> if the request is completed.
                /// MLResult.Result will be <c>MLResult.Code.ContactsHandleNotFound</c> if the request corresponding to the handle was not found.
                /// </returns>
                public static MLResult.Code GetManagedListResult(ulong handle, out MLContacts.ListResult listResult)
                {
                    MLResult.Code resultCode = NativeBindings.MLContactsTryGetListResult(handle, out IntPtr operationResultPtr);
                    if (resultCode != MLResult.Code.Pending)
                    {
                        if (resultCode != MLResult.Code.ContactsCompleted)
                        {
                            MLPluginLog.ErrorFormat("NativeBindings.GetManagedListResult failed to get list result. Reason: {0}", MLResult.CodeToString(resultCode));
                        }

                        NativeBindings.ListResultNative internalListResult = (NativeBindings.ListResultNative)Marshal.PtrToStructure(operationResultPtr, typeof(NativeBindings.ListResultNative));

                        listResult = new MLContacts.ListResult()
                        {
                            Status = internalListResult.Status,
                            List = internalListResult.List.Data,
                            Offset = Marshal.PtrToStringAnsi(internalListResult.Offset),
                            TotalHits = internalListResult.TotalHits,
                        };
                    }
                    else
                    {
                        listResult = new MLContacts.ListResult();
                    }

                    return resultCode;
                }
            }

            /// <summary>
            /// Stores a list of MLContactsContact.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct ContactListNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Number of contacts.
                /// </summary>
                public ulong Count;

                /// <summary>
                /// Pointer referring to array of contacts.
                /// </summary>
                public IntPtr Contacts;

                /// <summary>
                /// Gets the public facing struct from the native one.
                /// </summary>
                public MLContacts.ContactList Data
                {
                    get
                    {
                        MLContacts.ContactList contactsList = new MLContacts.ContactList();

                        int listSize = (int)this.Count;
                        contactsList.Contacts = new List<MLContacts.Contact>(listSize);
                        IntPtr walkPtr = this.Contacts;
                        for (int i = 0; i < listSize; ++i)
                        {
                            NativeBindings.ContactNative contact = (NativeBindings.ContactNative)Marshal.PtrToStructure(Marshal.ReadIntPtr(walkPtr), typeof(NativeBindings.ContactNative));
                            contactsList.Contacts.Add(contact.Data);
                            walkPtr = new IntPtr(walkPtr.ToInt64() + Marshal.SizeOf(typeof(IntPtr)));
                        }

                        return contactsList;
                    }
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>A new instance of this struct.</returns>
                public static ContactListNative Create()
                {
                    return new ContactListNative()
                    {
                        Version = 1u,
                        Count = 0,
                        Contacts = IntPtr.Zero
                    };
                }
            }

            /// <summary>
            /// Stores arguments for a search request. See MLContactsRequestSearch().
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SearchArgsNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Request results offset by this amount assuming contacts are ordered in a consistent way.
                /// </summary>
                public IntPtr Offset;

                /// <summary>
                /// Limit the number of results returned by search operation.
                /// </summary>
                public uint Limit;

                /// <summary>
                /// Query text/keywords.
                /// </summary>
                public IntPtr Query;

                /// <summary>
                /// Bitwise mask of fields where to search. See MLContacts.SearchField.
                /// </summary>
                public MLContacts.SearchField Fields;

                /// <summary>
                /// Initializes a new instance of the <see cref="SearchArgsNative"/> struct.
                /// </summary>
                /// <param name="searchQuery">Query text/keywords.</param>
                /// <param name="searchFields"> Bitwise mask of fields where to search.</param>
                /// <param name="pageLength">Limit the number of results returned by search operation.</param>
                /// <param name="pageOffset">Request results offset by this amount assuming contacts are ordered in a consistent way.</param>
                public SearchArgsNative(string searchQuery, MLContacts.SearchField searchFields, uint pageLength, string pageOffset)
                {
                    this.Version = 1u;
                    this.Offset = pageOffset == string.Empty ? IntPtr.Zero : Marshal.StringToHGlobalAnsi(pageOffset);
                    this.Limit = pageLength;
                    this.Query = Native.MLConvert.EncodeToUnmanagedUTF8(searchQuery);
                    this.Fields = searchFields;
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>A new instance of this struct.</returns>
                public static SearchArgsNative Create()
                {
                    return new SearchArgsNative()
                    {
                        Version = 1u,
                        Offset = IntPtr.Zero,
                        Limit = MLContacts.DefaultFetchLimit,
                        Query = IntPtr.Zero,
                        Fields = MLContacts.SearchField.All,
                    };
                }

                /// <summary>
                /// Free all allocated unmanaged memory.
                /// </summary>
                public void Clean()
                {
                    Marshal.FreeHGlobal(this.Offset);
                    Marshal.FreeHGlobal(this.Query);
                }
            }

            /// <summary>
            /// Stores arguments for a selection request (MLContactsRequestSelection()).
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct SelectionArgsNative
            {
                /// <summary>
                /// Version of this structure.
                /// </summary>
                public uint Version;

                /// <summary>
                /// Limit the number of selections in the operation.
                /// </summary>
                public uint Limit;

                /// <summary>
                /// Bitwise mask of fields to be fetched for contacts. See MLContacts.SelectionField.
                /// </summary>
                public MLContacts.SelectionField Fields;

                /// <summary>
                /// Initializes a new instance of the <see cref="SelectionArgsNative"/> struct.
                /// </summary>
                /// <param name="selectionFields">Bitwise mask of fields to be fetched for contacts.</param>
                /// <param name="pageLength">Limit the number of selections in the operation.</param>
                public SelectionArgsNative(MLContacts.SelectionField selectionFields, uint pageLength)
                {
                    this.Version = 1u;
                    this.Limit = pageLength;
                    this.Fields = selectionFields;
                }

                /// <summary>
                /// Create and return an initialized version of this struct.
                /// </summary>
                /// <returns>A new instance of this struct.</returns>
                public static SelectionArgsNative Create()
                {
                    return new SelectionArgsNative
                    {
                        Version = 1u,
                        Limit = 1u,
                        Fields = MLContacts.SelectionField.All
                    };
                }
            }
        }
    }
}

#endif

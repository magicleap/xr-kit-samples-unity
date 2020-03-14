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
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    /// <summary>
    /// MLContacts provides address book style functionality for storing, editing, deleting and retrieving contacts from on device storage.
    /// This API does not expose access to user's connections with other magic leap users (e.g.social graph).
    /// </summary>
    public sealed partial class MLContacts : MLAPISingleton<MLContacts>
    {
        #if PLATFORM_LUMIN
        /// <summary>
        /// Default limit for retrieval based operations in number of contacts
        /// </summary>
        public const int DefaultFetchLimit = 250;

        /// <summary>
        /// Maximum contacts to fetch.
        /// </summary>
        private const int MaxFetchSize = 2500;

        /// <summary>
        /// Maximum contacts query size.
        /// </summary>
        private const int MaxSearchQuerySize = 100;

        /// <summary>
        /// Maximum length permitted for a contact name.
        /// </summary>
        private const int MaxNameLength = 128;

        /// <summary>
        /// Maximum length permitted for a contact phone number.
        /// </summary>
        private const int MaxPhoneLength = 20;

        /// <summary>
        /// Maximum length permitted for a contact email.
        /// </summary>
        private const int MaxEmailLength = 128;

        /// <summary>
        /// Maximum length permitted for a contact tag.
        /// </summary>
        private const int MaxTagLength = 128;

        /// <summary>
        /// Maximum permitted phone number count.
        /// </summary>
        private const int MaxPhoneCount = 16;

        /// <summary>
        /// Maximum permitted email addresses count.
        /// </summary>
        private const int MaxEmailCount = 32;

        /// <summary>
        /// REGEX phone validation.
        /// </summary>
        private const string PhoneValidation = @"(^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$)";

        /// <summary>
        /// REGEX email validation.
        /// </summary>
        private const string EmailValidation = @"((?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\]))";

        /// <summary>
        /// Stores pending contact addition operations.
        /// </summary>
        private List<ulong> addOperations = new List<ulong>();

        /// <summary>
        /// Stores pending contact update operations.
        /// </summary>
        private List<ulong> updateOperations = new List<ulong>();

        /// <summary>
        /// Stores pending contact deletion operations.
        /// </summary>
        private List<ulong> deleteOperations = new List<ulong>();

        /// <summary>
        /// Prevents a default instance of the <see cref="MLContacts"/> class from being created.
        /// </summary>
        private MLContacts()
        {
            this.DllNotFoundError = "MLContacts API is currently available only on device.";
        }

        /// <summary>
        /// Delegate for API operation results.
        /// </summary>
        /// <param name="operationResult">Result of the operation.</param>
        /// <param name="requestHandle">Handle for the request.</param>
        public delegate void OperationResultDelegate(OperationResult operationResult, ulong requestHandle);

        /// <summary>
        /// Raised when a contact has been successfully added by AddContact.
        /// </summary>
        public static event OperationResultDelegate OnContactAdded = delegate { };

        /// <summary>
        /// Raised when a contact has been successfully updated by UpdateContact.
        /// </summary>
        public static event OperationResultDelegate OnContactUpdated = delegate { };

        /// <summary>
        /// Raised when a contact has been successfully deleted by DeleteContact.
        /// </summary>
        public static event OperationResultDelegate OnContactDeleted = delegate { };

        /// <summary>
        /// Raised when an operation (Add, Update, Delete) failed.
        /// </summary>
        public static event OperationResultDelegate OnOperationFailed = delegate { };
        #endif

        /// <summary>
        /// Defines possible status values for an operation performed on a MLContacts.Contact.
        /// </summary>
        public enum OperationStatus
        {
            /// <summary>
            /// Operation succeeded.
            /// </summary>
            Success,

            /// <summary>
            /// Operation failed.
            /// </summary>
            Fail,

            /// <summary>
            /// MLContacts.Contact with the details specified for an insert already exists.
            /// </summary>
            Duplicate,

            /// <summary>
            /// MLContacts.Contact to be deleted/updated does not exist.
            /// </summary>
            NotFound,
        }

        /// <summary>
        /// Selection bit field for choosing which fields a search should be executed in
        /// </summary>
        [Flags]
        public enum SearchField
        {
            /// <summary>
            /// Search for name
            /// </summary>
            Name = 1 << 0,

            /// <summary>
            /// Search for phone number
            /// </summary>
            Phone = 1 << 1,

            /// <summary>
            /// Search for email address
            /// </summary>
            Email = 1 << 2,

            /// <summary>
            /// Search all fields
            /// </summary>
            All = Name | Phone | Email,
        }

        /// <summary>
        /// Selection bit field for choosing which fields a search should be executed in
        /// </summary>
        [Flags]
        public enum SelectionField
        {
            /// <summary>
            /// Search for name
            /// </summary>
            Name = 1 << 0,

            /// <summary>
            /// Search for phone number
            /// </summary>
            Phone = 1 << 1,

            /// <summary>
            /// Search for email address
            /// </summary>
            Email = 1 << 2,

            /// <summary>
            /// Search all fields
            /// </summary>
            All = Name | Phone | Email,
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Starts the Contacts API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if connected to MLContacts successfully.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        public static MLResult Start()
        {
            CreateInstance();
            return MLContacts.BaseStart();
        }

        /// <summary>
        /// Add a new contact to the address book.
        /// Contact must contain a name, at least one email address or one phone number, and
        /// it must not be a duplicate or the operation will fail.
        /// Contact ID is assigned upon successful completion, any ID specified in the input will be ignored
        /// OnContactAdded will be raised if the operation completes successfully
        /// OnOperationFailed will be raised if the operation fails to complete
        /// </summary>
        /// <param name="newContact">The contact to add</param>
        /// <param name="requestHandle">Unique identification handle for this operation that can be used as a reference</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully submitted
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if either of the parameters are invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        public static MLResult AddContact(Contact newContact, out ulong requestHandle)
        {
            try
            {
                if (MLContacts.IsValidInstance())
                {
                    MLResult.Code resultCode = MLResult.Code.UnspecifiedFailure;
                    MLResult result;

                    result = ValidateContact(newContact);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLContacts.AddContact failed to update a contact. Reason: {0}", result);
                        requestHandle = 0;
                        return result;
                    }

                    NativeBindings.ContactNative internalContact = NativeBindings.ContactNative.Create();
                    internalContact.Data = newContact;

                    resultCode = NativeBindings.MLContactsRequestInsert(ref internalContact, out requestHandle);
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLContacts.AddContact failed to add a contact. Reason: {0}", result);
                        return result;
                    }

                    _instance.addOperations.Add(requestHandle);

                    internalContact.Clean();

                    return result;
                }
                else
                {
                    requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                    MLPluginLog.ErrorFormat("MLContacts.AddContact failed. Reason: No Instance for MLContacts");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLContacts.AddContact failed. Reason: No Instance for MLContacts");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                MLPluginLog.Error("MLContacts.AddContact failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLContacts.AddContact failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Update an existing contact in the address book.
        /// Contacts are matched by ID and updatedContact must contain a valid ID.
        /// Contact must contain a name and at least one email address or one phone number.
        /// OnContactUpdated will be raised if the operation completes successfully
        /// OnOperationFailed will be raised if the operation fails to complete
        /// </summary>
        /// <param name="updatedContact">The contact to update, with the updated information</param>
        /// <param name="requestHandle">Unique identification handle for this operation that can be used as a reference</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully submitted
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if either of the parameters are invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        public static MLResult UpdateContact(Contact updatedContact, out ulong requestHandle)
        {
            try
            {
                if (MLContacts.IsValidInstance())
                {
                    MLResult result;

                    if (updatedContact.ID == null)
                    {
                        result = MLResult.Create(MLResult.Code.InvalidParam, "Contact to update must have a valid ID.");
                        MLPluginLog.ErrorFormat("MLContacts.UpdateContact failed to update a contact. Reason: {0}", result);
                        requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                        return result;
                    }

                    result = ValidateContact(updatedContact);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLContacts.UpdateContact failed to update a contact. Reason: {0}", result);
                        requestHandle = 0;
                        return result;
                    }

                    NativeBindings.ContactNative internalContact = NativeBindings.ContactNative.Create();
                    internalContact.Data = updatedContact;

                    MLResult.Code resultCode = NativeBindings.MLContactsRequestUpdate(ref internalContact, out requestHandle);
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLContacts.UpdateContact failed to update a contact. Reason: {0}", result);
                        return result;
                    }

                    _instance.updateOperations.Add(requestHandle);

                    internalContact.Clean();

                    return result;
                }
                else
                {
                    requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                    MLPluginLog.ErrorFormat("MLContacts.UpdateContact failed. Reason: No Instance for MLContacts");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLContacts.UpdateContact failed. Reason: No Instance for MLContacts");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                MLPluginLog.Error("MLContacts.UpdateContact failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLContacts.UpdateContact failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Delete an existing contact in the address book.
        /// Contacts are matched by ID and removedContact must contain a valid ID.
        /// OnContactDeleted will be raised if the operation completes successfully
        /// OnOperationFailed will be raised if the operation fails to complete
        /// </summary>
        /// <param name="removedContact">The contact to delete</param>
        /// <param name="requestHandle">Unique identification handle for this operation that can be used as a reference</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully submitted
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if either of the parameters are invalid.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        public static MLResult DeleteContact(Contact removedContact, out ulong requestHandle)
        {
            try
            {
                if (MLContacts.IsValidInstance())
                {
                    MLResult result;

                    if (removedContact.ID == null)
                    {
                        result = MLResult.Create(MLResult.Code.InvalidParam, "Contact to delete must have a valid ID.");
                        MLPluginLog.ErrorFormat("MLContacts.DeleteContact failed to delete a contact. Reason: {0}", result);
                        requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                        return result;
                    }

                    NativeBindings.ContactNative internalContact = NativeBindings.ContactNative.Create();
                    internalContact.Data = removedContact;

                    MLResult.Code resultCode = NativeBindings.MLContactsRequestDelete(ref internalContact, out requestHandle);
                    result = MLResult.Create(resultCode);
                    if (!result.IsOk)
                    {
                        MLPluginLog.ErrorFormat("MLContacts.DeleteContact failed to delete a contact. Reason: {0}", result);
                        return result;
                    }

                    _instance.deleteOperations.Add(requestHandle);

                    internalContact.Clean();

                    return result;
                }
                else
                {
                    requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                    MLPluginLog.ErrorFormat("MLContacts.DeleteContact failed. Reason: No Instance for MLContacts");
                    return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLContacts.DeleteContact failed. Reason: No Instance for MLContacts");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                MLPluginLog.Error("MLContacts.DeleteContact failed. Reason: API symbols not found");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLContacts.DeleteContact failed. Reason: API symbols not found");
            }
        }

        /// <summary>
        /// Check if a contact is valid before submitting it to AddContact or UpdateContact.
        /// </summary>
        /// <param name="checkContact">The contact to check</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if valid.
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        public static MLResult ValidateContact(Contact checkContact)
        {
            if (MLContacts.IsValidInstance())
            {
                string errorResult = string.Empty;

                if (string.IsNullOrEmpty(checkContact.Name))
                {
                    errorResult += Environment.NewLine + "Name is required.";
                }
                else if (checkContact.Name.Length > MaxNameLength)
                {
                    errorResult += Environment.NewLine + "Name is too long. Max length is: " + MaxNameLength;
                }

                if (checkContact.PhoneNumberList.Count == 0 && checkContact.EmailAddressList.Count == 0)
                {
                    errorResult += Environment.NewLine + "Either a phone number or e-mail address is required";
                }

                if (checkContact.PhoneNumberList.Count > MaxPhoneCount)
                {
                    errorResult += Environment.NewLine + "Too many phone numbers have been added. Max count is: " + MaxPhoneCount;
                }
                else
                {
                    foreach (TaggedAttribute phone in checkContact.PhoneNumberList.Items)
                    {
                        if (!string.IsNullOrEmpty(phone.Tag))
                        {
                            if (phone.Tag.Length > MaxTagLength)
                            {
                                errorResult += Environment.NewLine + "Phone Tag is too long. Max length is: " + MaxTagLength;
                            }
                        }

                        if (string.IsNullOrEmpty(phone.Value) || !Regex.IsMatch(phone.Value, PhoneValidation))
                        {
                            errorResult += Environment.NewLine + "Phone number is invalid.";
                        }
                        else if (phone.Value.Length > MaxPhoneLength)
                        {
                            errorResult += Environment.NewLine + "Phone number is too long. Max length is: " + MaxPhoneLength;
                        }
                    }
                }

                if (checkContact.EmailAddressList.Count > MaxEmailCount)
                {
                    errorResult += Environment.NewLine + "Too many email addresses have been added. Limit is: " + MaxEmailCount;
                }
                else
                {
                    foreach (TaggedAttribute email in checkContact.EmailAddressList.Items)
                    {
                        if (!string.IsNullOrEmpty(email.Tag))
                        {
                            if (email.Tag.Length > MaxTagLength)
                            {
                                errorResult += Environment.NewLine + "Email Tag is too long. Max length is: " + MaxTagLength;
                            }
                        }

                        if (string.IsNullOrEmpty(email.Value) || !Regex.IsMatch(email.Value, EmailValidation))
                        {
                            errorResult += Environment.NewLine + "Invalid email address.";
                        }
                        else if (email.Value.Length > MaxEmailLength)
                        {
                            errorResult += Environment.NewLine + "Email is too long. Max length is: " + MaxEmailLength;
                        }
                    }
                }

                if (errorResult != string.Empty)
                {
                    return MLResult.Create(MLResult.Code.InvalidParam, errorResult.Trim());
                }

                return MLResult.Create(MLResult.Code.Ok);
            }
            else
            {
                MLPluginLog.ErrorFormat("MLContacts.ValidateContact failed. Reason: No Instance for MLContacts");
                return MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLContacts.ValidateContact failed. Reason: No Instance for MLContacts");
            }
        }

        /// <summary>
        /// Create a new page. Contacts are obtained asynchronously and pageReady will be raised when available.
        /// </summary>
        /// <param name="pageLength">How many contacts should be in each page. Max: 2500</param>
        /// <param name="outResult">out parameter that will contain the MLResult for the request.
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully created
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the API has not been started.</param>
        /// <param name="pageReady">Raised when the page is collated and ready for viewing.</param>
        /// <param name="pageFailed">Raised when the page failed to be retrieved.</param>
        /// <returns>The created page for searching, null if unable to create the page.</returns>
        public static ListPage CreateListPage(uint pageLength, out MLResult outResult, ListPage.OnPageReadyDelegate pageReady = null, ListPage.OnPageFailedDelegate pageFailed = null)
        {
            if (MLContacts.IsValidInstance())
            {
                outResult = MLResult.Create(MLResult.Code.Ok);
                return new ListPage(pageLength, pageReady, pageFailed);
            }
            else
            {
                MLPluginLog.ErrorFormat("MLContacts.CreateListPage failed. Reason: No Instance for MLContacts");
                outResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLContacts.CreateListPage failed. Reason: No Instance for MLContacts");
                return null;
            }
        }

        /// <summary>
        /// Create a new search page. Contacts are obtained asynchronously and pageReady will be raised when available.
        /// </summary>
        /// <param name="searchQuery">The string to search for. Max Length: 100</param>
        /// <param name="searchFields">Which field(s) should be searched</param>
        /// <param name="pageLength">How many contacts should be in each page. MAX: 2500</param>
        /// <param name="outResult">out parameter that will contain the MLResult for the request.
        /// MLResult.Result will be MLResult.Code.Ok if successfully created
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if the API has not been started.</param>
        /// <param name="pageReady">Raised when the page is collated and ready for viewing.</param>
        /// <param name="pageFailed">Raised when the page failed to be retrieved.</param>
        /// <returns>The created page for searching, null if unable to create the page.</returns>
        public static SearchPage CreateSearchPage(string searchQuery, SearchField searchFields, uint pageLength, out MLResult outResult, ListPage.OnPageReadyDelegate pageReady = null, ListPage.OnPageFailedDelegate pageFailed = null)
        {
            if (MLContacts.IsValidInstance())
            {
                outResult = MLResult.Create(MLResult.Code.Ok);
                return new SearchPage(searchQuery, searchFields, pageLength, pageReady, pageFailed);
            }
            else
            {
                MLPluginLog.ErrorFormat("MLContacts.CreateSearchPage failed. Reason: No Instance for MLContacts");
                outResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLContacts.CreateSearchPage failed. Reason: No Instance for MLContacts");
                return null;
            }
        }

        /// <summary>
        /// Create a new selection page. Contacts are obtained asynchronously and pageReady will be raised when available.
        /// </summary>
        /// <param name="selectionFields">Which field(s) should be selected</param>
        /// <param name="pageLength">How many contacts should be in each page. MAX: 2500</param>
        /// <param name="outResult">out parameter that will contain the MLResult for the request.
        /// MLResult.Result will be MLResult.Code.Ok if successfully created
        /// MLResult.Result will be MLResult.Code.UnspecifiedFailure if the API has not been started.</param>
        /// <param name="pageReady">Raised when the page is collated and ready for viewing.</param>
        /// <param name="pageFailed">Raised when the page failed to be retrieved.</param>
        /// <returns>The created page for searching, null if unable to create the page.</returns>
        public static SelectionPage CreateSelectionPage(SelectionField selectionFields, uint pageLength, out MLResult outResult, ListPage.OnPageReadyDelegate pageReady = null, ListPage.OnPageFailedDelegate pageFailed = null)
        {
            if (MLContacts.IsValidInstance())
            {
                outResult = MLResult.Create(MLResult.Code.Ok);
                return new SelectionPage(selectionFields, pageLength, pageReady, pageFailed);
            }
            else
            {
                MLPluginLog.ErrorFormat("MLContacts.CreateSelectionPage failed. Reason: No Instance for MLContacts");
                outResult = MLResult.Create(MLResult.Code.UnspecifiedFailure, "MLContacts.CreateSelectionPage failed. Reason: No Instance for MLContacts");
                return null;
            }
        }

        /// <summary>
        /// Gets the result string for a MLResult.Code.
        /// </summary>
        /// <param name="result">The MLResult.Code to be requested.</param>
        /// <returns>A pointer to the result string.</returns>
        internal static IntPtr GetResultString(MLResult.Code result)
        {
            try
            {
                return NativeBindings.MLContactsGetResultString(result);
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLContacts.GetResultString failed. Reason: API symbols not found");
            }

            return IntPtr.Zero;
        }

#if !DOXYGEN_SHOULD_SKIP_THIS
        /// <summary>
        /// Starts the Contacts API.
        /// </summary>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.Ok</c> if connected to MLContacts successfully.
        /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        protected override MLResult StartAPI()
        {
            MLResult result = MLResult.Create(NativeBindings.MLContactsStartup());
            if (!result.IsOk)
            {
                MLPluginLog.ErrorFormat("MLContacts.StartAPI failed to initialize MLContacts. Reason: {0}", result);
            }

            return result;
        }
        #endif // DOXYGEN_SHOULD_SKIP_THIS

        /// <summary>
        /// Cleans up API and unmanaged memory.
        /// </summary>
        /// <param name="isSafeToAccessManagedObject">Allow complete cleanup of the API.</param>
        protected override void CleanupAPI(bool isSafeToAccessManagedObject)
        {
            MLResult.Code resultCode = MLResult.Code.UnspecifiedFailure;

            try
            {
                if (isSafeToAccessManagedObject)
                {
                    for (int i = 0; i < _instance.addOperations.Count; ++i)
                    {
                        resultCode = NativeBindings.MLContactsCancelRequest(_instance.addOperations[i]);

                        if (resultCode != MLResult.Code.ContactsCancelled)
                        {
                            MLPluginLog.ErrorFormat("MLContacts.CleanupAPI failed to cancel add request {0}. Reason: {1}", _instance.addOperations[i], MLResult.CodeToString(resultCode));
                        }
                    }

                    for (int i = 0; i < this.updateOperations.Count; ++i)
                    {
                        resultCode = NativeBindings.MLContactsCancelRequest(_instance.updateOperations[i]);
                        if (resultCode != MLResult.Code.ContactsCancelled)
                        {
                            MLPluginLog.ErrorFormat("MLContacts.CleanupAPI failed to cancel update request {0}. Reason: {1}", _instance.updateOperations[i], MLResult.CodeToString(resultCode));
                        }
                    }

                    for (int i = 0; i < _instance.deleteOperations.Count; ++i)
                    {
                        resultCode = NativeBindings.MLContactsCancelRequest(_instance.deleteOperations[i]);
                        if (resultCode != MLResult.Code.ContactsCancelled)
                        {
                            MLPluginLog.ErrorFormat("MLContacts.CleanupAPI failed to cancel delete request {0}. Reason: {1}", _instance.deleteOperations[i], MLResult.CodeToString(resultCode));
                        }
                    }
                }

                resultCode = NativeBindings.MLContactsShutdown();

                if (!MLResult.IsOK(resultCode))
                {
                    MLPluginLog.ErrorFormat("MLContacts.CleanupAPI failed to shutdown MLContacts. Reason: {0}", MLResult.CodeToString(resultCode));
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLContacts.CleanupAPI failed. Reason: API symbols not found");
            }

            if (isSafeToAccessManagedObject)
            {
                _instance.addOperations.Clear();
                _instance.updateOperations.Clear();
                _instance.deleteOperations.Clear();
            }
        }

        /// <summary>
        /// Process all pending operations.
        /// </summary>
        protected override void Update()
        {
            MLContacts.ProcessOperations(ref _instance.addOperations, OnContactAdded, OnOperationFailed);
            MLContacts.ProcessOperations(ref _instance.updateOperations, OnContactUpdated, OnOperationFailed);
            MLContacts.ProcessOperations(ref _instance.deleteOperations, OnContactDeleted, OnOperationFailed);
        }

        /// <summary>
        /// static instance of the MLContacts class
        /// </summary>
        private static void CreateInstance()
        {
            if (!MLContacts.IsValidInstance())
            {
                MLContacts._instance = new MLContacts();
            }
        }

        /// <summary>
        /// Processes updates on requested operations.
        /// </summary>
        /// <param name="handleList">List of operations to get an update for.</param>
        /// <param name="successAction">Callback to trigger in case of success.</param>
        /// <param name="failureAction">Callback to trigger in case of failure.</param>
        private static void ProcessOperations(ref List<ulong> handleList, OperationResultDelegate successAction, OperationResultDelegate failureAction)
        {
            try
            {
                if (MLContacts.IsValidInstance())
                {
                    MLResult.Code resultCode = MLResult.Code.UnspecifiedFailure;
                    for (var listIndex = handleList.Count - 1; listIndex >= 0; listIndex--)
                    {
                        resultCode = NativeBindings.OperationResultNative.GetManagedOperationResult(handleList[listIndex], out OperationResult operationResult);
                        switch (resultCode)
                        {
                            case MLResult.Code.ContactsHandleNotFound:
                            case MLResult.Code.InvalidParam:
                                {
                                    failureAction?.Invoke(operationResult, handleList[listIndex]);

                                    resultCode = NativeBindings.MLContactsReleaseRequestResources(handleList[listIndex]);
                                    if (resultCode != MLResult.Code.Ok)
                                    {
                                        MLPluginLog.ErrorFormat("MLContacts.ProcessOperations failed to release request resources for failed handle {0}. Reason: {1}", handleList[listIndex], MLResult.CodeToString(resultCode));
                                    }

                                    handleList.RemoveAt(listIndex);
                                }

                                break;
                            case MLResult.Code.ContactsCompleted:
                                {
                                    if (operationResult.Status == MLContacts.OperationStatus.Success)
                                    {
                                        successAction?.Invoke(operationResult, handleList[listIndex]);
                                    }
                                    else
                                    {
                                        failureAction?.Invoke(operationResult, handleList[listIndex]);
                                    }

                                    resultCode = NativeBindings.MLContactsReleaseRequestResources(handleList[listIndex]);
                                    if (resultCode != MLResult.Code.Ok)
                                    {
                                        MLPluginLog.ErrorFormat("MLContacts.ProcessOperations failed to release request resources for completed handle {0}. Reason: {1}", handleList[listIndex], MLResult.CodeToString(resultCode));
                                    }

                                    handleList.RemoveAt(listIndex);
                                }

                                break;
                            case MLResult.Code.Pending:
                            default:
                                continue;
                        }
                    }
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLContacts.ProcessOperations failed. Reason: No Instance for MLContacts");
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLContacts.ProcessOperations failed. Reason: API symbols not found");
                return;
            }
        }

        /// <summary>
        /// Gets the contacts list for a specific request.
        /// </summary>
        /// <param name="pageLength">Length of the contacts page.</param>
        /// <param name="pageOffset">Request results offset by this amount assuming contacts are ordered in a consistent way.</param>
        /// <param name="requestHandle">Handle to the request.</param>
        /// <returns>
        /// MLResult.Code will be <c>MLResult.Code.Ok</c> if valid.
        /// MLResult.Code will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
        /// MLResult.Code will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        private static MLResult.Code GetList(uint pageLength, string pageOffset, out ulong requestHandle)
        {
            try
            {
                if (MLContacts.IsValidInstance())
                {
                    if (pageLength > MaxFetchSize)
                    {
                        MLPluginLog.WarningFormat("MLContacts.GetList pageLength is greater than the Max Fetch Limit, {0}. Truncating pageLength to the Max.", MaxFetchSize);
                        pageLength = MaxFetchSize;
                    }

                    NativeBindings.ListArgsNative listArgs = new NativeBindings.ListArgsNative(pageLength, pageOffset);
                    MLResult.Code resultCode = NativeBindings.MLContactsRequestList(ref listArgs, out requestHandle);
                    listArgs.Clean();

                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLContacts.GetList failed to request a list. Reason: {0}", MLResult.CodeToString(resultCode));
                        return resultCode;
                    }

                    if (!Native.MagicLeapNativeBindings.MLHandleIsValid(requestHandle))
                    {
                        MLPluginLog.ErrorFormat("MLContacts.GetList failed to request a search. Reason: Reason: Failed to obtain a valid handle");
                        return MLResult.Code.InvalidParam;
                    }

                    return MLResult.Code.Ok;
                }
                else
                {
                    requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                    MLPluginLog.ErrorFormat("MLContacts.GetList failed. Reason: No Instance for MLContacts");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                MLPluginLog.Error("MLContacts.GetList failed. Reason: API symbols not found");
                return MLResult.Code.UnspecifiedFailure;
            }
        }

        /// <summary>
        /// Search for contacts with a given query across specified fields.
        /// </summary>
        /// <param name="searchQuery">Query text/keywords.</param>
        /// <param name="searchFields">Bitwise mask of fields where to search.</param>
        /// <param name="pageLength">Length of the contacts page.</param>
        /// <param name="pageOffset">Request results offset by this amount assuming contacts are ordered in a consistent way.</param>
        /// <param name="requestHandle">Handle to the request.</param>
        /// <returns>
        /// MLResult.Code will be <c>MLResult.Code.Ok</c> if valid.
        /// MLResult.Code will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
        /// MLResult.Code will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        private static MLResult.Code GetSearch(string searchQuery, SearchField searchFields, uint pageLength, string pageOffset, out ulong requestHandle)
        {
            try
            {
                if (MLContacts.IsValidInstance())
                {
                    if (pageLength > MaxFetchSize)
                    {
                        MLPluginLog.WarningFormat("MLContacts.GetSearch pageLength is greater than the Max Fetch Limit, {0}. Truncating pageLength to the Max.", MaxFetchSize);
                        pageLength = MaxFetchSize;
                    }
                    else if (searchQuery.Length > MaxSearchQuerySize)
                    {
                        requestHandle = 0;
                        MLPluginLog.ErrorFormat("MLContacts.GetSearch failed. Reason: Search query has exceeded the max query size: {0}", MaxSearchQuerySize);
                        return MLResult.Code.InvalidParam;
                    }

                    NativeBindings.SearchArgsNative searchArgs = new NativeBindings.SearchArgsNative(searchQuery, searchFields, pageLength, pageOffset);
                    MLResult.Code resultCode = NativeBindings.MLContactsRequestSearch(ref searchArgs, out requestHandle);
                    searchArgs.Clean();

                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLContacts.GetSearch failed to request a search. Reason: {0}", MLResult.CodeToString(resultCode));
                        return resultCode;
                    }

                    if (!Native.MagicLeapNativeBindings.MLHandleIsValid(requestHandle))
                    {
                        MLPluginLog.ErrorFormat("MLContacts.GetSearch failed to request a search. Reason: Reason: Failed to obtain a valid handle");
                        return MLResult.Code.InvalidParam;
                    }

                    return MLResult.Code.Ok;
                }
                else
                {
                    requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                    MLPluginLog.ErrorFormat("MLContacts.GetSearch failed. Reason: No Instance for MLContacts");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                MLPluginLog.Error("MLContacts.GetSearch failed. Reason: API symbols not found");
                return MLResult.Code.UnspecifiedFailure;
            }
        }

        /// <summary>
        /// Request a subset of contacts as manually selected by the user via separate system application.
        /// Request selection operation.To get result of this operation, MLContactsTryGetListResult().
        /// </summary>
        /// <param name="selectionFields">Bitwise mask of fields to be fetched for contacts.</param>
        /// <param name="pageLength">Length of the contacts page.</param>
        /// <param name="requestHandle">Handle to the request.</param>
        /// <returns>
        /// MLResult.Code will be <c>MLResult.Code.Ok</c> if valid.
        /// MLResult.Code will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
        /// MLResult.Code will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        private static MLResult.Code GetSelection(SelectionField selectionFields, uint pageLength, out ulong requestHandle)
        {
            try
            {
                if (MLContacts.IsValidInstance())
                {
                    if (pageLength > MaxFetchSize)
                    {
                        MLPluginLog.WarningFormat("MLContacts.GetSelection pageLength is greater than the Max Fetch Limit, {0}. Truncating pageLength to the Max.", MaxFetchSize);
                        pageLength = MaxFetchSize;
                    }

                    NativeBindings.SelectionArgsNative selectionArgs = new NativeBindings.SelectionArgsNative(selectionFields, pageLength);

                    MLResult.Code resultCode = NativeBindings.MLContactsRequestSelection(in selectionArgs, out requestHandle);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLContacts.GetSelection failed to request a selection. Reason: {0}", MLResult.CodeToString(resultCode));
                        return resultCode;
                    }

                    if (!Native.MagicLeapNativeBindings.MLHandleIsValid(requestHandle))
                    {
                        MLPluginLog.ErrorFormat("MLContacts.GetSelection failed to request a selection. Reason: Failed to obtain a valid handle");
                        return MLResult.Code.InvalidParam;
                    }

                    return MLResult.Code.Ok;
                }
                else
                {
                    requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                    MLPluginLog.ErrorFormat("MLContacts.GetSelection failed. Reason: No Instance for MLContacts");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                requestHandle = Native.MagicLeapNativeBindings.InvalidHandle;
                MLPluginLog.Error("MLContacts.GetSelection failed. Reason: API symbols not found");
                return MLResult.Code.UnspecifiedFailure;
            }
        }

        /// <summary>
        /// Try to get the list result for a specific request.
        /// </summary>
        /// <param name="requestHandle">Handle to the request.</param>
        /// <param name="listResult">The result.</param>
        /// <returns>
        /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
        /// MLResult.Result will be <c>MLResult.Code.Pending</c> if the request is still pending.
        /// MLResult.Result will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// MLResult.Result will be <c>MLResult.Code.ContactsCompleted</c> if the request is completed.
        /// MLResult.Result will be <c>MLResult.Code.ContactsHandleNotFound</c> if the request corresponding to the handle was not found.
        /// </returns>
        private static MLResult.Code GetListResult(ulong requestHandle, out ListResult listResult)
        {
            try
            {
                if (MLContacts.IsValidInstance())
                {
                    return NativeBindings.ListResultNative.GetManagedListResult(requestHandle, out listResult);
                }
                else
                {
                    listResult = new ListResult();
                    MLPluginLog.ErrorFormat("MLContacts.GetListResult failed. Reason: No Instance for MLContacts");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                listResult = new ListResult();
                MLPluginLog.Error("MLContacts.GetListResult failed. Reason: API symbols not found");
                return MLResult.Code.UnspecifiedFailure;
            }
        }

        /// <summary>
        /// Free all resources for a request corresponding to the handle. MLContacts API users are
        /// expected to free resources for all handles.
        /// </summary>
        /// <param name="requestHandle">A handle to the request.</param>
        /// <returns>
        /// MLResult.Code will be <c>MLResult.Code.Ok</c> if valid.
        /// MLResult.Code will be <c>MLResult.Code.InvalidParam</c> if any of the parameters are invalid.
        /// MLResult.Code will be <c>MLResult.Code.UnspecifiedFailure</c> if the operation failed with an unspecified error.
        /// </returns>
        private static MLResult.Code ReleaseRequest(ulong requestHandle)
        {
            try
            {
                if (MLContacts.IsValidInstance())
                {
                    MLResult.Code resultCode = NativeBindings.MLContactsReleaseRequestResources(requestHandle);
                    if (resultCode != MLResult.Code.Ok)
                    {
                        MLPluginLog.ErrorFormat("MLContacts.ReleaseRequest failed to release the request resources. Reason: {0}", MLResult.CodeToString(resultCode));
                    }

                    return resultCode;
                }
                else
                {
                    MLPluginLog.ErrorFormat("MLContacts.ReleaseRequest failed. Reason: No Instance for MLContacts");
                    return MLResult.Code.UnspecifiedFailure;
                }
            }
            catch (System.EntryPointNotFoundException)
            {
                MLPluginLog.Error("MLContacts.ReleaseRequest failed. Reason: API symbols not found");
                return MLResult.Code.UnspecifiedFailure;
            }
        }

        /// <summary>
        /// Stores the result of an operation performed on a MLContacts.Contact.
        /// </summary>
        public struct OperationResult
        {
            /// <summary>
            /// The status of the operation
            /// </summary>
            public OperationStatus Status;

            /// <summary>
            /// Resultant contact with updated fields, for e.g., the 'id' of the contact would be available
            /// in this resultant contact for AddContact
            /// </summary>
            public Contact Contact;
        }

        /// <summary>
        /// Stores the result of a list operation
        /// </summary>
        public struct ListResult
        {
            /// <summary>
            /// The status of the operation
            /// </summary>
            public OperationStatus Status;

            /// <summary>
            /// The list of contacts
            /// </summary>
            public ContactList List;

            /// <summary>
            /// Total number of contacts found for this request
            /// </summary>
            public ulong TotalHits;

            /// <summary>
            /// Offset used for the next list to continue after this one
            /// </summary>
            internal string Offset;
        }

        /// <summary>
        /// Used to store a list of contacts
        /// </summary>
        public struct ContactList
        {
            /// <summary>
            /// The list of contacts
            /// </summary>
            public List<Contact> Contacts;

            /// <summary>
            /// Gets the number of contacts in the list
            /// </summary>
            public int Count
            {
                get
                {
                    if (this.Contacts == null)
                    {
                        return 0;
                    }

                    return this.Contacts.Count;
                }
            }
        }

        /// <summary>
        /// Stores a tagged value, such as phone number or email address.
        /// Optional tag indicates what type of value is stored, e.g. "home", "work", etc.
        /// </summary>
        public class TaggedAttribute
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TaggedAttribute"/> class.
            /// </summary>
            public TaggedAttribute()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TaggedAttribute"/> class.
            /// </summary>
            /// <param name="src">Class to copy.</param>
            public TaggedAttribute(TaggedAttribute src)
            {
                this.Tag = src.Tag;
                this.Value = src.Value;
            }

            /// <summary>
            /// Gets or sets the name of the tag.
            /// </summary>
            public string Tag { get; set; }

            /// <summary>
            /// Gets or sets the value of the attribute.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Stores a list of TaggedAttributes.
        /// </summary>
        public class TaggedAttributeList
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TaggedAttributeList"/> class.
            /// </summary>
            public TaggedAttributeList()
            {
                this.Items = new List<TaggedAttribute>();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TaggedAttributeList"/> class.
            /// </summary>
            /// <param name="src">Class to copy.</param>
            public TaggedAttributeList(TaggedAttributeList src)
            {
                this.Items = new List<TaggedAttribute>();
                foreach (var item in src.Items)
                {
                    this.Items.Add(new TaggedAttribute(item));
                }
            }

            /// <summary>
            /// Gets the number of tagged attributes in the list.
            /// </summary>
            public int Count
            {
                get
                {
                    if (this.Items == null)
                    {
                        return 0;
                    }

                    return this.Items.Count;
                }
            }

            /// <summary>
            /// Gets or sets a list of tagged attributes.
            /// </summary>
            public List<TaggedAttribute> Items { get; set; }
        }

        /// <summary>
        /// Representation of available information for a single contact in address book.
        /// </summary>
        public class Contact
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Contact"/> class.
            /// </summary>
            public Contact()
            {
                this.PhoneNumberList = new TaggedAttributeList();
                this.EmailAddressList = new TaggedAttributeList();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Contact"/> class.
            /// </summary>
            /// <param name="src">Class to copy.</param>
            public Contact(Contact src)
            {
                this.ID = src.ID;
                this.Name = src.Name;
                this.PhoneNumberList = new TaggedAttributeList(src.PhoneNumberList);
                this.EmailAddressList = new TaggedAttributeList(src.EmailAddressList);
            }

            /// <summary>
            /// Gets the locally-unique contact identifier generated by the system. May change across reboots.
            /// </summary>
            public string ID { get; internal set; }

            /// <summary>
            /// Gets or sets the contact's name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the list of tagged phone numbers. Tag-value pairs are not required to be unique. Tags are
            /// optional(empty strings allowed).
            /// </summary>
            public TaggedAttributeList PhoneNumberList { get; set; }

            /// <summary>
            /// Gets or sets the list of tagged email addresses. Tag-value pairs are not required to be unique. Tags are
            /// optional(empty strings allowed).
            /// </summary>
            public TaggedAttributeList EmailAddressList { get; set; }
        }
        #endif
    }
}

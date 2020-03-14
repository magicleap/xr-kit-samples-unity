// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLContactsListPage.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace UnityEngine.XR.MagicLeap
{
    using System.Collections.Generic;

    #if PLATFORM_LUMIN
    using UnityEngine.XR.MagicLeap.Native;
    #endif

    /// <summary>
    /// MLContacts provides address book style functionality for storing, editing, deleting and retrieving contacts from on device storage.
    /// This API does not expose access to user's connections with other magic leap users (e.g.social graph).
    /// </summary>
    public partial class MLContacts : MLAPISingleton<MLContacts>
    {
        /// <summary>
        /// Helper to step through and manage pages in the MLContacts address book.
        /// </summary>
        public class ListPage
        {
            #if PLATFORM_LUMIN
            /// <summary>
            /// Length of the page being requested.
            /// </summary>
            private readonly uint pageLength = 0;

            /// <summary>
            /// Result of the request.
            /// </summary>
            private ListResult listResult;

            /// <summary>
            /// Offset into the page.
            /// </summary>
            private string nextPageOffset = string.Empty;

            /// <summary>
            /// Initializes a new instance of the <see cref="ListPage"/> class.
            /// </summary>
            /// <param name="pageLength">Length of the contacts page.</param>
            /// <param name="pageReady">Callback to trigger if request succeeds.</param>
            /// <param name="pageFailed">Callback to trigger if request fails.</param>
            public ListPage(uint pageLength, OnPageReadyDelegate pageReady = null, OnPageFailedDelegate pageFailed = null)
            {
                this.PageReadyAction = pageReady;
                this.PageFailedAction = pageFailed;
                this.pageLength = pageLength;
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="ListPage"/> class.
            /// </summary>
            ~ListPage()
            {
                if (this.UnregisterUpdate)
                {
                    MLDevice.Unregister(this.Update);
                }
            }

            /// <summary>
            /// Delegate to handle when a page is ready.
            /// </summary>
            /// <param name="contacts">The contacts list.</param>
            public delegate void OnPageReadyDelegate(MLContacts.ListPage contacts);

            /// <summary>
            /// Delegate to handle when a page failed.
            /// </summary>
            /// <param name="contacts">The contacts list.</param>
            /// <param name="result">The result.</param>
            public delegate void OnPageFailedDelegate(MLContacts.ListPage contacts, MLResult result);

            /// <summary>
            /// Event to trigger when a page is ready.
            /// </summary>
            private event OnPageReadyDelegate PageReadyAction = delegate { };

            /// <summary>
            /// Event to trigger when a page failed.
            /// </summary>
            private event OnPageFailedDelegate PageFailedAction = delegate { };
            #endif

            /// <summary>
            /// Status of a page request.
            /// </summary>
            public enum PageStatus
            {
                /// <summary>
                /// The page request is still pending.
                /// </summary>
                Pending,

                /// <summary>
                /// The page request has succeeded and is ready for viewing.
                /// </summary>
                Ready,

                /// <summary>
                /// The page request failed to complete.
                /// </summary>
                Failed,

                /// <summary>
                /// This is the last page available given the initial parameters. NextPage will return to the beginning
                /// </summary>
                LastPage,
            }

            #if PLATFORM_LUMIN
            /// <summary>
            /// Gets or sets the page status.
            /// </summary>
            public PageStatus Status { get; set; }

            /// <summary>
            /// Gets the list of contacts in this page... will be null if the page is not ready for viewing
            /// </summary>
            public List<Contact> ContactsList
            {
                get
                {
                    if (this.Status != PageStatus.Ready && this.Status != PageStatus.LastPage)
                    {
                        return null;
                    }

                    return this.listResult.List.Contacts;
                }
            }

            /// <summary>
            /// Gets the total number of contacts that will be available by paging through with the given parameters
            /// e.g. If there are 8 total hits with Page length 3, you will be able to call next page 3 times
            /// (3 results), (3 results), (2 results), before looping back to the beginning
            /// </summary>
            public ulong TotalHits { get; private set; }

            /// <summary>
            /// Gets or sets the handle to the request.
            /// </summary>
            protected ulong RequestHandle { get; set; } = 0;

            /// <summary>
            /// Gets or sets a value indicating whether Update should be unregistered.
            /// </summary>
            protected bool UnregisterUpdate { get; set; } = false;

            /// <summary>
            /// Get the next page in the address book, given the initial length.
            /// </summary>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully submitted
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if either of the parameters are invalid.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
            /// </returns>
            public MLResult NextPage()
            {
                return this.InternalNewPage(this.pageLength, this.nextPageOffset);
            }

            /// <summary>
            /// Request a new page
            /// </summary>
            /// <param name="pageLength">The length of the page</param>
            /// <param name="offset">Offset into the page</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully submitted
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if either of the parameters are invalid.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
            /// </returns>
            protected virtual MLResult InternalNewPage(uint pageLength, string offset)
            {
                MLResult.Code resultCode = MLContacts.GetList(pageLength, offset, out ulong handle);
                this.RequestHandle = handle;

                if (resultCode != MLResult.Code.Ok)
                {
                    this.Status = PageStatus.Failed;
                    return MLResult.Create(resultCode);
                }

                this.Status = PageStatus.Pending;
                MLDevice.Register(this.Update);
                this.UnregisterUpdate = true;

                return MLResult.Create(MLResult.Code.Ok);
            }

            /// <summary>
            /// Gets an update for the request.
            /// </summary>
            protected void Update()
            {
                if (this.Status == PageStatus.Pending)
                {
                    MLResult.Code resultCode = MLContacts.GetListResult(this.RequestHandle, out this.listResult);

                    switch (resultCode)
                    {
                        case MLResult.Code.ContactsHandleNotFound:
                        case MLResult.Code.InvalidParam:
                            {
                                this.PageFailed(resultCode);
                            }

                            break;
                        case MLResult.Code.ContactsCompleted:
                            {
                                this.PageReady();
                            }

                            break;
                        case MLResult.Code.Pending:
                        default:
                            break;
                    }
                }
            }

            /// <summary>
            /// Handles a page being ready.
            /// </summary>
            private void PageReady()
            {
                this.Status = PageStatus.Ready;

                if (this.listResult.Offset == null)
                {
                    this.nextPageOffset = string.Empty;
                    this.Status = PageStatus.LastPage;
                }
                else
                {
                    this.nextPageOffset = string.Copy(this.listResult.Offset);
                }

                MLContacts.ReleaseRequest(this.RequestHandle);

                this.RequestHandle = MagicLeapNativeBindings.InvalidHandle;

                this.PageReadyAction?.Invoke(this);

                this.TotalHits = this.listResult.TotalHits;
                this.UnregisterUpdate = false;
                MLDevice.Unregister(this.Update);
            }

            /// <summary>
            /// Handles a page request failure.
            /// </summary>
            /// <param name="resultCode">The result of the request.</param>
            private void PageFailed(MLResult.Code resultCode)
            {
                this.Status = PageStatus.Failed;

                MLContacts.ReleaseRequest(this.RequestHandle);

                this.RequestHandle = MagicLeapNativeBindings.InvalidHandle;

                this.PageFailedAction?.Invoke(this, MLResult.Create(resultCode));

                this.UnregisterUpdate = false;
                MLDevice.Unregister(this.Update);
            }
            #endif
        }
    }
}

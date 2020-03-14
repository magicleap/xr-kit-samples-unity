// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file="MLContactsSelectionPage.cs" company="Magic Leap, Inc">
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
    /// <summary>
    /// MLContacts provides address book style functionality for storing, editing, deleting and retrieving contacts from on device storage.
    /// This API does not expose access to user's connections with other magic leap users (e.g.social graph).
    /// </summary>
    public partial class MLContacts : MLAPISingleton<MLContacts>
    {
        /// <summary>
        /// Helper to step through and manage pages of a selection in the MLContacts address book.
        /// </summary>
        public class SelectionPage : ListPage
        {
            /// <summary>
            /// Bitwise mask of fields to be fetched for contacts.
            /// </summary>
            private readonly SelectionField selectionFields;

            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionPage"/> class.
            /// </summary>
            /// <param name="selectionFields">Bitwise mask of fields to be fetched for contacts.</param>
            /// <param name="pageLength">Length of the contacts page.</param>
            /// <param name="pageReady">Callback to trigger if request succeeds.</param>
            /// <param name="pageFailed">Callback to trigger if request fails.</param>
            public SelectionPage(SelectionField selectionFields, uint pageLength, ListPage.OnPageReadyDelegate pageReady = null, ListPage.OnPageFailedDelegate pageFailed = null)
                : base(pageLength, pageReady, pageFailed)
            {
                this.selectionFields = selectionFields;
            }

            /// <summary>
            /// Request a new page.
            /// </summary>
            /// <param name="pageLength">The length of the page.</param>
            /// <param name="offset">Offset into the page.</param>
            /// <returns>
            /// MLResult.Result will be <c>MLResult.Code.Ok</c> if successfully submitted
            /// MLResult.Result will be <c>MLResult.Code.InvalidParam</c> if either of the parameters are invalid.
            /// MLResult.Result will be <c>MLResult.Code.PrivilegeDenied</c> if necessary privilege is missing.
            /// </returns>
            protected override MLResult InternalNewPage(uint pageLength, string offset)
            {
                MLResult.Code resultCode = MLContacts.GetSelection(this.selectionFields, pageLength, out ulong handle);
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
        }
    }
}

#endif

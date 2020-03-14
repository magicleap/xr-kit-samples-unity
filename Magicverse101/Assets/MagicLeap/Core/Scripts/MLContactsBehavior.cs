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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using MagicLeap.Core.StarterKit;

namespace MagicLeap.Core
{
    /// <summary>
    /// MLContactsBehavior shows the functionality for loading, saving, and deleting contacts.
    /// </summary>
    [AddComponentMenu("XR/MagicLeap/MLContactsBehavior")]
    public class MLContactsBehavior : MonoBehaviour
    {
        #if PLATFORM_LUMIN
        public Dictionary<string, MLContacts.Contact> loadedContacts
        {
            get;
            private set;
        }
        #endif

        public delegate void RefreshListPageResult( MLContacts.ListPage page);
        public delegate void StartupComplete(bool ready);

        #pragma warning disable 067
        public event RefreshListPageResult OnRefreshPageList;
        public event StartupComplete OnStartupComplete;
        #pragma warning restore 067

        /// <summary>
        /// Initialize dicitonary of contacts and start api.
        /// </summary>
        void Start()
        {
            #if PLATFORM_LUMIN
            loadedContacts = new Dictionary<string, MLContacts.Contact>();
            #endif

            StartAPI();
        }

        /// <summary>
        /// Clean Up.
        /// </summary>
        void OnDestroy()
        {
            #if PLATFORM_LUMIN
            if (MLContacts.IsStarted)
            {
                MLContacts.Stop();
            }
            #endif
        }

        /// <summary>
        /// Must check for privileges again after pause.
        /// </summary>
        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                #if PLATFORM_LUMIN
                if (MLContacts.IsStarted)
                {
                    MLContacts.Stop();
                }
                #endif
            }
            else
            {
                #if PLATFORM_LUMIN
                if (MLDevice.IsReady())
                {
                    StartAPI();
                }
                #endif
            }
        }

        /// <summary>
        /// Requests privileges and starts MLContacts.
        /// </summary>
        private void StartAPI()
        {
            #if PLATFORM_LUMIN
            MLResult result = MLPrivilegesStarterKit.Start();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLContactsBehavior failed starting MLPrivileges, disabling script. Reason: {0}", result);
                OnStartupComplete?.Invoke(false);
                enabled = false;
                return;
            }

            result = MLPrivilegesStarterKit.RequestPrivileges(MLPrivileges.Id.AddressBookRead, MLPrivileges.Id.AddressBookWrite);
            if (result.Result != MLResult.Code.PrivilegeGranted)
            {
                Debug.LogErrorFormat("Error: MLContactsBehavior failed requesting privileges, disabling script. Reason: {0}", result);
                OnStartupComplete?.Invoke(false);
                enabled = false;
                return;
            }

            MLPrivilegesStarterKit.Stop();

            result = MLContacts.Start();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLContactsBehavior failed starting MLContacts, disabling script. Reason: {0}", result);
                OnStartupComplete?.Invoke(false);
                enabled = false;
                return;
            }

            OnStartupComplete?.Invoke(true);
            #endif
        }

        /// <summary>
        /// Fetches all contacts matching the query, if any.
        /// </summary>
        /// <param name="query">The query string to use for searching.</param>
        public void LoadContactsFromAPI(string query = "")
        {
            #if PLATFORM_LUMIN
            if(!MLContacts.IsStarted)
            {
                return;
            }

            loadedContacts.Clear();

            MLResult result;
            MLContacts.ListPage fillPage = null;

            if (string.IsNullOrEmpty(query))
            {
                fillPage = MLContacts.CreateListPage(MLContacts.DefaultFetchLimit, out result, RefreshListPageReady, HandlePageFailed);
                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("Error: MLContactsBehavior failed to create the contacts list page, disabling script. Reason: {0}", result);
                    enabled = false;
                    return;
                }
            }
            else
            {
                // Change second parameter to limit searching by name, email address, or phone number only.
                fillPage = MLContacts.CreateSearchPage(query, MLContacts.SearchField.All, MLContacts.DefaultFetchLimit, out result, RefreshListPageReady, HandlePageFailed);
                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("Error: MLContactsBehavior failed to create the contacts search page, disabling script. Reason: {0}", result);
                    enabled = false;
                    return;
                }
            }

            // Begin fetching contacts.
            result = fillPage.NextPage();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLContactsBehavior failed to request the next page of contacts, disabling script. Reason: {0}", result);
                enabled = false;
                return;
            }
            #endif
        }

        #if PLATFORM_LUMIN
        /// <summary>
        /// Adds a new contact or updates an existing contact.
        /// Called by ContactPageVisualizer.
        /// </summary>
        /// <param name="contact">The new contact to save.</param>
        public MLResult SaveContact(MLContacts.Contact contact)
        {
            ulong requestHandle = 0;
            MLResult result;

            if (!string.IsNullOrEmpty(contact.ID))
            {
                Debug.LogFormat("Updating existing contact with id = {0}", contact.ID);
                result = MLContacts.UpdateContact(contact, out requestHandle);
            }
            else
            {
                Debug.LogFormat("Saving new contact with name = {0}", contact.Name);
                result = MLContacts.AddContact(contact, out requestHandle);
            }

            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLContactsBehavior failed to save contact. Reason: {0}", result);
            }

            return result;
        }
        #endif

        /// <summary>
        /// Deletes a contact. Called by ContactListPage.
        /// </summary>
        /// <param name="id">Id of the contact to delete.</param>
        public void DeleteContact(string id)
        {
            #if PLATFORM_LUMIN
            ulong requestHandle = 0;

            if (!string.IsNullOrEmpty(id) && loadedContacts.ContainsKey(id))
            {
                MLResult result = MLContacts.DeleteContact(loadedContacts[id], out requestHandle);
                if (!result.IsOk)
                {
                    Debug.LogErrorFormat("Error: MLContactsBehavior failed to delete contact. Reason: {0}", result);
                }
            }
            else
            {
                Debug.LogErrorFormat("Error: MLContactsBehavior failed to delete contact. Reason: Invalid ID {0}", id);
            }
            #endif
        }

        /// <summary>
        /// Handler when page is retrieved successfully. Store the list of contacts in memory and
        /// fetch the next page. If this is the last page, display them.
        /// </summary>
        /// <param name="page">Page with list of contacts.</param>
        private void RefreshListPageReady(MLContacts.ListPage page)
        {
            #if PLATFORM_LUMIN
            foreach (MLContacts.Contact contact in page.ContactsList)
            {
                loadedContacts[contact.ID] = contact;
            }

            OnRefreshPageList(page);

            if (page.Status == MLContacts.ListPage.PageStatus.LastPage)
            {
                return;
            }

            // Automatically fetches the next page; triggering RefreshListPageReady when done or HandlePageFailed on failure.
            MLResult result = page.NextPage();
            if (!result.IsOk)
            {
                Debug.LogErrorFormat("Error: MLContactsBehavior failed to request the next page while refreshing the list, disabling script. Reason was: {0}", result);
                enabled = false;
                return;
            }
            #endif
        }

        /// <summary>
        /// Handler when page failed to retrieve.
        /// </summary>
        /// <param name="page">Page that failed.</param>
        /// <param name="result">Result of the operation.</param>
        private void HandlePageFailed(MLContacts.ListPage page, MLResult result)
        {
            Debug.LogErrorFormat("Error: MLContactsBehavior failed to retreive a page. Reason was: {0}", result);
        }
    }
}

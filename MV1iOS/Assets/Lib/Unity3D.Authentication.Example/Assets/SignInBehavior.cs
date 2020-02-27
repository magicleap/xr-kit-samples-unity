using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using MagicLeap.XR.XRKit;
using MagicLeap.XR.XRKit.Sample;

namespace Assets
{
    public class SignInBehavior : MonoBehaviour {

        [SerializeField]
        private MLXRSession mlxrSession = null;
        private const string SignInText = "SIGN IN";
        private const string SignOutText = "SIGN OUT";
        private const string PleaseWaitText = "PLEASE WAIT";
        public GameObject SignInButtonText;
        public GameObject SignInButton;
        public GameObject StatusText;
        private bool _replyReceived;
        private bool _authOperationInProgress;
        private bool _signinCancelled;
        private DateTime _watchForReplyStartTime;
        private bool _watchForReply;
        private UnityAuthClient _authClient = null;
        private bool _signedIn;
        private const double MaxSecondsToWaitForAuthReply = 3;
        private const double expirationOffsetMinutes = -30;
        public Button SessionToggleButton;
        public Text SessionToggleButtonText;
        public Text SessionToggleStatus;

        private async void Start()
        {
            _authClient = new MLXROAuthClient();
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Debug.Log("SignInBehavior::Start");

            EnableSessionToggleButton(false);

            if (_authClient.RefreshToken != null)
            {
                EnableSignInButton(false);
                await Refresh();
            }
            else
            {
                EnableSignInButton(true);
            }
        }

        public async void OnSignInClicked()
        {
            EnableSignInButton(false);

            _replyReceived = false;
            _signinCancelled = false;
            _authOperationInProgress = true;
            _watchForReply = false;

            if (_signedIn)
            {
                await SignOut();
            }
            else
            {
                await SignIn();
            }

            EnableSignInButton(true);
        }

        private void StopSession()
        {
            bool result = mlxrSession.StopSession();
            if (SessionToggleStatus)
            {
                SessionToggleStatus.text = result
                    ? "Session stopped successfully."
                    : "Session stopped with error.";
            }
            if (SessionToggleButtonText)
            {
                SessionToggleButtonText.text = "Start Session";
            }
        }

        private void StartSession()
        {
            if (_authClient.AccessToken != null)
            {
                mlxrSession.SessionUpdateToken(_authClient.AccessToken);
            }
            if (SessionToggleStatus)
            {
                SessionToggleStatus.text = "Session started.";
            }
            if (SessionToggleButtonText)
            {
                SessionToggleButtonText.text = "Stop Session";
            }
        }

        public void OnSessionToggleClicked()
        {
            if (mlxrSession.enabled)
            {
                StopSession();
            }
            else
            {
                StartSession();
            }
        }

        private void EnableSessionToggleButton(bool enable)
        {
            if (SessionToggleButton)
            {
                SessionToggleButton.interactable = enable;
            }
        }

        private async Task SignIn()
        {
            Debug.Log("SignInBehavior::Signing in...");
            _signedIn = await _authClient.LoginAsync();

            _authOperationInProgress = false;
            _watchForReply = false;

            if (_signedIn)
            {
                StatusText.GetComponent<Text>().text = "Hello " + _authClient.UserName;
                EnableSessionToggleButton(true);
                StartSession();
            }
            else if (_signinCancelled)
            {
                Debug.Log("SignInBehavior::Sign-in was cancelled by the user.");
                StatusText.GetComponent<Text>().text = "Sign-in cancelled.";
            }
            else
            {
                Debug.Log("SignInBehavior::Failed to perform sign-in.");
                StatusText.GetComponent<Text>().text =
                    "An error occurred during sign in.  Please ensure you have Internet access.";
            }
        }

        private async Task Refresh()
        {
            Debug.Log("SignInBehavior::Refreshing...");
            _signedIn = await _authClient.RefreshAsync();

            if (_signedIn)
            {
                StatusText.GetComponent<Text>().text = "Hello " + _authClient.UserName;
                EnableSessionToggleButton(true);
                StartSession();
            }
            else
            {
                Debug.Log("SignInBehavior::Failed to perform refresh.");
                StatusText.GetComponent<Text>().text =
                    "An error occurred during refresh.  Please ensure you have Internet access.";
                EnableSessionToggleButton(false);
            }
            EnableSignInButton(true);
        }

        private async Task SignOut()
        {
            Debug.Log("SignInBehavior::Signing out...");

            StopSession();
            EnableSessionToggleButton(false);

            _signedIn = !await _authClient.LogoutAsync();

            _authOperationInProgress = false;
            _watchForReply = false;

            if (!_signedIn)
            {
                StatusText.GetComponent<Text>().text = "";
            }
            else if (_signinCancelled)
            {
                Debug.Log("SignInBehavior::Sign-out was cancelled by the user.");
                StatusText.GetComponent<Text>().text = "Sign-out cancelled.";
            }
            else
            {
                Debug.Log("SignInBehavior::Failed to perform sign-out.");
                StatusText.GetComponent<Text>().text =
                    "An error occurred during sign out.  Please ensure you have Internet access.";
            }
        }

        private void EnableSignInButton(bool enabled)
        {
            var text = enabled ? _signedIn ? SignOutText : SignInText : PleaseWaitText;
            SignInButtonText.GetComponent<Text>().text = text;
            SignInButton.GetComponent<Button>().interactable = enabled;
        }

        void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log("SignInBehavior::OnApplicationPause: " + pauseStatus);
            if (pauseStatus)
            {
                StopSession();
            }
            else
            {
                Debug.Log("SignInBehavior::App was resumed.");
                if (_authOperationInProgress)
                {
                    if (!_replyReceived)
                    {
                        Debug.Log("SignInBehavior::Watching for auth reply.");
                        _watchForReply = true;
                        _watchForReplyStartTime = DateTime.Now;
                    }
                }
                else
                {
                    // App has been resumed, but we are not signing in, e.g. user has closed the sign-out browser.
                    StartSession();
                    EnableSignInButton(true);
                }
            }
        }

        async Task Update()
        {
            if (_watchForReply && DateTime.Now - _watchForReplyStartTime > TimeSpan.FromSeconds(MaxSecondsToWaitForAuthReply))
            {
                Debug.Log("SignInBehavior::No auth reply received, assuming the user cancelled or was unable to complete the sign-in.");
                _watchForReply = false;
                _signinCancelled = true;
                _authClient.Browser.OnAuthReply(null);
            }

            if (_signedIn && DateTime.Now >= _authClient.AccessTokenExpiration.AddMinutes(expirationOffsetMinutes))
            {
                _signedIn = false;
                await Refresh();
            }
        }

        public void OnAuthReply(object value)
        {
            if (!_signinCancelled)
            {
                _watchForReply = false;
                _replyReceived = true;
                Debug.Log("SignInBehavior::OnAuthReply: " + value);
                _authClient.Browser.OnAuthReply(value as string);
            }
        }
    }
}

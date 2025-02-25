using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Browser;
using SharpBIM.GitTracker.Core.Auth.BrowseOptions;
using SharpBIM.ServiceContracts.QAQC;

namespace SharpBIM.GitTracker.Core.Auth
{
    internal class SystemBrowser : IBrowser
    {
        #region Private Fields

        private const string ERROR_MESSAGE = "Error occurred.";

        private const string SUCCESSFUL_AUTHENTICATION_MESSAGE =
            "You have been successfully authenticated. You can now continue to use desktop application.";

        private const string SUCCESSFUL_LOGOUT_MESSAGE = "You have been successfully logged out.";
        private HttpListener _httpListener;

        #endregion Private Fields

        #region Private Methods

        private void StartSystemBrowser(GitBrowserOptions gitOp)
        {
            if (!started)
            {
                var startinfo = new ProcessStartInfo();
                startinfo.UseShellExecute = true;
                startinfo.WindowStyle = ProcessWindowStyle.Normal;
                startinfo.CreateNoWindow = gitOp.DisplayMode == DisplayMode.Hidden;
                startinfo.FileName = gitOp.StartUrl;
                Process.Start(startinfo);
                started = true;
            }
        }

        #endregion Private Methods

        #region Public Methods

        private bool started = false;

        public async Task<BrowserResult> InvokeAsync(
            BrowserOptions options,
            CancellationToken cancellationToken = default
        )
        {
            var gitOps = options as GitBrowserOptions;
            var result = new BrowserResult();

            try
            {
                //abort _httpListener if exists

                int trials = 5;

                using (_httpListener = new HttpListener())
                {
                    var listenUrl = gitOps.EndUrl;

                    //HttpListenerContext require uri ends with /
                    if (!listenUrl.EndsWith("/"))
                        listenUrl += "/";

                    _httpListener.Prefixes.Add(listenUrl);

                    _httpListener.Start();
                    StartSystemBrowser(gitOps);

                    cancellationToken.Register(() => _httpListener?.Abort());

                    HttpListenerContext context = null;

                    await Task.Delay(1000);
                    while (trials > 0)
                    {
                        trials--;

                        try
                        {
                            context = await _httpListener.GetContextAsync();
                        }
                        //if _httpListener is aborted while waiting for response it throws HttpListenerException exception
                        catch (System.ObjectDisposedException)
                        {
                            result.ResultType = BrowserResultType.UnknownError;
                            return result;
                        }
                        catch (HttpListenerException)
                        {
                            result.ResultType = BrowserResultType.UnknownError;
                            // return result;
                            continue;
                        }
                        catch (Exception ex)
                        {
                            CQC.Debug();
                        }
                        if (gitOps.Validate(context))
                        {
                            result.ResultType = BrowserResultType.Success;
                            break;
                        }
                    }

                    if (context != null)
                    {
                        context.Response.OutputStream.Close();
                        context.Response.Close();
                    }

                    _httpListener.Stop();
                }
            }
            catch (Exception ex)
            {
                result.ResultType = BrowserResultType.UnknownError;
            }
            finally
            {
                _httpListener?.Abort();
                _httpListener?.Close();
            }

            return result;
        }

        #endregion Public Methods
    }
}
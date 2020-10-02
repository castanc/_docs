 private void renderPWAApp()
        {
            var applicationData = Windows.Storage.ApplicationData.Current;
            var localSettings = applicationData.LocalSettings;
            //localSettings.Values["WNSChannelURI"] = ""; 
            string url = "https://deskhelptest.azurewebsites.net/ui/";
            App.WebView = this.mywebview;
            this.mywebview.ScriptNotify += MyWebView_ScriptNotifyAsync;
            //await WebView.ClearTemporaryWebDataAsync();
            this.mywebview.Navigate(new Uri(@url));
            this.mywebview.DOMContentLoaded += DOMContentLoaded;
        }

        private void DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            App.Appload = true;
        }

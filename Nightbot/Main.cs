using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nightbot
{
    public partial class Main : Form
    {
        bool onPage = false;
        HtmlDocument doc;
        
        public Main()
        {
            InitializeComponent();
            SHDocVw.WebBrowser popupHandle = (SHDocVw.WebBrowser)webBrowser1.ActiveXInstance;
            popupHandle.NewWindow3 += PopupHandle_NewWindow3;

            backgroundWorker1.RunWorkerAsync();
        }

        private void PopupHandle_NewWindow3(ref object ppDisp, ref bool Cancel, uint dwFlags, string bstrUrlContext, string bstrUrl)
        {
            Cancel = true;
            webBrowser1.Navigate(new Uri(bstrUrl));
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.Url.AbsoluteUri.Contains("beta.nightbot.tv/song_requests"))
            {
                onPage = true;
                doc = webBrowser1.Document;
            }
            else
            {
                onPage = false;
                doc = null;
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            while (true)
            {
                if (onPage)
                {
                    try
                    {
                        HtmlElementCollection iframes = doc.GetElementsByTagName("iframe");
                        foreach (HtmlElement player in iframes)
                        {
                            if (player.Id.Equals("yt_player") || player.Id.Equals("sc_player"))
                            {
                                player.SetAttribute("width", "1");
                                player.SetAttribute("height", "1");
                            }
                        }

                        HtmlElement titleElement = doc.GetElementsByTagName("h4")[0].GetElementsByTagName("strong")[0];
                        string title = Regex.Split(titleElement.InnerText, " — ")[0];
                        HtmlElementCollection pElements = doc.GetElementsByTagName("p");
                        string user = "";
                        
                        for (int pI = 0; pI < pElements.Count; pI++)
                        {
                            HtmlElementCollection iElements = pElements[pI].GetElementsByTagName("i");
                            for (int iI = 0; iI < iElements.Count; iI++)
                            {
                                if (iElements[iI].GetAttribute("className").Contains("fa-user"))
                                {
                                    user = pElements[pI].InnerText;
                                    break;
                                }
                            }
                        }

                        File.WriteAllText(@"current_song.txt", title + " - Requested by:" + user);
                    }
                    catch (Exception)
                    {
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}

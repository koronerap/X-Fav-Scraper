using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace TwitterFavFetcher
{
    public partial class Form1 : Form
    {
        Thread scrapeThread;

        public Form1()
        {
            InitializeComponent();
            scrapeThread = new Thread(new ThreadStart(Loop));
            CheckForIllegalCrossThreadCalls = false;
            statusLabel.Text = $"ThreadState: {scrapeThread.ThreadState}";
        }

        void Loop()
        {
            while (true)
            {
                this.BeginInvoke(new MethodInvoker(delegate
                {
                    statusLabel.Text = $"ThreadState: {scrapeThread.ThreadState}";
                    _ = UpdateListAsync();
                }));
                Thread.Sleep(500);
            }
        }

        int currentHeight = 0;

        private async Task UpdateListAsync()
        {
            string dom = await webView21.ExecuteScriptAsync("document.documentElement.outerHTML");

            dom = Regex.Unescape(dom);
            dom = dom.Remove(0, 1);
            dom = dom.Remove(dom.Length - 1, 1);

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(dom);


            HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("//span[contains(@class, 'css-1qaijid') and contains(@class, 'r-bcqeeo')]");

            int count = listBox1.Items.Count;

            foreach (HtmlNode node in nodes)
                if (node.InnerText.StartsWith("@") && !listBox1.Items.Contains(node.InnerText))
                    listBox1.Items.Add(node.InnerText);

            countLabel.Text = "Count: " + listBox1.Items.Count;

            currentHeight += (int)scrollNumericUpDown.Value;
            string s = await webView21.ExecuteScriptAsync("window.scrollTo(0, " + currentHeight + ");");

            int visibleItems = listBox1.ClientSize.Height / listBox1.ItemHeight;
            listBox1.TopIndex = Math.Max(listBox1.Items.Count - visibleItems + 1, 0);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void goButton_Click(object sender, EventArgs e)
        {
            webView21.Source = new Uri(postUrlTextBox.Text);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            List<string> list = new List<string>();

            foreach (string i in listBox1.Items)
                list.Add(i);

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text Files | *.txt";
            sfd.DefaultExt = "txt";
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);


            if(sfd.ShowDialog() == DialogResult.OK)
                File.WriteAllLines(sfd.FileName, list);

        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (scrapeThread.ThreadState == ThreadState.WaitSleepJoin || scrapeThread.ThreadState == ThreadState.Running)
            {
                scrapeThread.Abort();
                startButton.Text = "Start";
                startButton.BackColor = Color.LimeGreen;
            }
            else
            {
                currentHeight = 0;
                GC.SuppressFinalize(scrapeThread);
                scrapeThread = new Thread(new ThreadStart(Loop));
                scrapeThread.Start();
                startButton.Text = "Abort";
                startButton.BackColor = Color.Red;
            }
        }
    }
}
//span.css-1qaijid.r-bcqeeo.r-qvutc0.r-poiln3

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
        private int currentHeight = 0;
        private Thread scrapeThread;
        
        public Form1()
        {
            InitializeComponent();
            scrapeThread = new Thread(new ThreadStart(Loop));
            CheckForIllegalCrossThreadCalls = false;
            statusLabel.Text = $"Thread State: {scrapeThread.ThreadState}";
        }

        void Loop()
        {
            while (true)
            {
                this.BeginInvoke(new MethodInvoker(delegate
                {
                    statusLabel.Text = $"Thread State: {scrapeThread.ThreadState}";
                    _ = UpdateListAsync();
                }));
                Thread.Sleep(500);
            }
        }


        private async Task UpdateListAsync()
        {
            string dom = await webView21.ExecuteScriptAsync("document.documentElement.outerHTML");

            dom = Regex.Unescape(dom);
            dom = dom.Remove(0, 1);
            dom = dom.Remove(dom.Length - 1, 1);

            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(dom);

            HtmlNodeCollection nodes = html.DocumentNode.SelectNodes("//span[contains(@class, 'css-1qaijid') and contains(@class, 'r-bcqeeo')]");

            foreach (HtmlNode node in nodes)
                if (node.InnerText.StartsWith("@") && !listBox1.Items.Contains(node.InnerText))
                    listBox1.Items.Add(node.InnerText);

            countLabel.Text = "Count: " + listBox1.Items.Count;

            currentHeight += (int)scrollNumericUpDown.Value;
            await webView21.ExecuteScriptAsync("window.scrollTo(0, " + currentHeight + ");");

            listBox1.TopIndex = Math.Max(listBox1.Items.Count - (listBox1.ClientSize.Height / listBox1.ItemHeight) + 1, 0);
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            try
            {
                webView21.Source = new Uri(postUrlTextBox.Text);
            }
            catch
            {
                MessageBox.Show("Invalid post URL!","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
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
            currentHeight = 0;
            if (scrapeThread.ThreadState == ThreadState.WaitSleepJoin || scrapeThread.ThreadState == ThreadState.Running)
            {
                scrapeThread.Abort();
                startButton.Text = "Start";
                startButton.BackColor = Color.Green;
            }
            else
            {
                GC.SuppressFinalize(scrapeThread);
                scrapeThread = new Thread(new ThreadStart(Loop));
                scrapeThread.Start();
                startButton.Text = "Abort";
                startButton.BackColor = Color.Red;
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            postUrlTextBox.Select();
        }

        private void postUrlTextBox_TextChanged(object sender, EventArgs e)
        {
            label3.Visible = postUrlTextBox.Text.Length == 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                scrapeThread.Abort();
                GC.SuppressFinalize(scrapeThread);
            }
            catch
            {
                //
            }
        }
    }
}
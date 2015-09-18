using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace screenspin
{
    public partial class Form2 : Form
    {
        String baselink;
        String savePath;
        String workingon;
        public Form2(String baselink, String savePath)
        {
            InitializeComponent();
            this.baselink = baselink;
            this.savePath = savePath;
            //MessageBox.Show(baselink + "/music.mp3");
            //MessageBox.Show(savePath + "/music/LoginScreenLoop.mp3");
            download();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
           
        }
       
        private void download()
        {
            workingon = "Music";
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileAsync(new Uri(baselink + "/music.mp3"), savePath + "/music/LoginScreenLoop.mp3");
        }

        private void download2()
        {
            workingon = "Animation";
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed2);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileAsync(new Uri(baselink + "/login.swf"), savePath + "/cs_bg_champions.png");
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            label1.Text = workingon + " - " + e.BytesReceived +" bytes out of " + e.TotalBytesToReceive + " bytes downloaded.";
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            File.Copy(savePath + "/music/LoginScreenLoop.mp3", savePath + "/music/LoginScreenIntro.mp3", true);
            //MessageBox.Show("Download completed!");
            download2();
        }

        private void Completed2(object sender, AsyncCompletedEventArgs e)
        {
            File.Copy(savePath + "/cs_bg_champions.png", savePath + "/login.swf", true);
            //MessageBox.Show("Download completed!");
            this.Hide();
            MessageBox.Show("Download completed." + Environment.NewLine + Environment.NewLine + "To enable music, you may need to click on \"Disable Menu Animations\".", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

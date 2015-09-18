using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using screenspin.Properties;

namespace screenspin
{
    public partial class Form1 : Form
    {
        static bool didCancel = false;
        static bool Garena = false;

        String json;
        String baseurl = "http://quicklol.net/";

        String current = "loginBraum";
        String fullPath;

        public Form1()
        {

            InitializeComponent();

            Dictionary<string, Assembly> loaded = new Dictionary<string, Assembly>();
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                Assembly resAssembly;
                string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
                dllName = dllName.Replace(".", "_");
                if (!loaded.ContainsKey(dllName))
                {
                    if (dllName.EndsWith("_resources")) return null;
                    System.Resources.ResourceManager rm = new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
                    byte[] bytes = (byte[])rm.GetObject(dllName);
                    resAssembly = System.Reflection.Assembly.Load(bytes);
                    loaded.Add(dllName, resAssembly);
                }
                else
                { resAssembly = loaded[dllName]; }
                return resAssembly;
            };

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);


                if (Settings.Default.LeaguePath == null || Settings.Default.LeaguePath == "")
                {
                    MessageBox.Show("We can't find your League installation.\nYou'll need to show us where it is.", "Initial Setup", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    RequestPath();
                }
                else if (File.Exists(Settings.Default.LeaguePath + @"\lol.launcher.exe"))
                {
                    Garena = false;
                }
                else if (File.Exists(Settings.Default.LeaguePath + @"\lol.exe"))
                {
                    Garena = true;
                }
                else
                {
                    MessageBox.Show("We can't find your League installation.\nYou'll need to show us where it is.", "Initial Setup", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    RequestPath();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong.", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        
           

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            try
            {
                json = new WebClient().DownloadString(baseurl + "loginscreens.json");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't load login screens data from quicklol.", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.Start("http://quicklol.net/contact");
                Application.Exit();
            }

            var jo = (JObject)JsonConvert.DeserializeObject(json);

            if (jo["message"].ToString() != "")
            {
                MessageBox.Show(jo["message"].ToString(), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (jo["url"].ToString() != "")
            {
                Process.Start(jo["url"].ToString());
            }

            foreach(JObject jobject in jo["data"]) {
                comboBox1.Items.Add(jobject["name"]);
            }

            current = jo["latest"].ToString();

            try
            {
                DateTime lastHigh = new DateTime(1900, 1, 1);
                String highDir = null;

                foreach (string subdir in Directory.GetDirectories(Settings.Default.LeaguePath + "/RADS/projects/lol_air_client/releases"))
                {
                    DirectoryInfo fi1 = new DirectoryInfo(subdir);
                    DateTime created = fi1.LastWriteTime;

                    if (created > lastHigh)
                    {
                        highDir = subdir;
                        lastHigh = created;
                    }
                }


                fullPath = highDir + "/deploy/mod/lgn/themes";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to locate League of Legends. Application will exit now.", "Can't Find League", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                Application.Exit();
            }

        }
        private void backup()
        {
            DirectoryCopy(fullPath + "/" + current, fullPath + "/" + current + "_backup", true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {

                if (comboBox1.Text != "")
                {
                    if (Directory.Exists(fullPath + "/" + current + "_backup") == false)
                    {
                        backup();

                        String url = getLsPath(comboBox1.Text);

                        Form2 form2 = new Form2(url, fullPath + "/" + current);
                        form2.Show();
                    }
                    else
                    {
                        String url = getLsPath(comboBox1.Text);

                        Form2 form2 = new Form2(url, fullPath + "/" + current);
                        form2.Show();
                    }
                }
                else
                {
                    MessageBox.Show("You must select login screen.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (radioButton1.Checked == true)
            {
                if (Directory.Exists(fullPath + "/" + current + "_backup") == false)
                {
                    backup();
                    doWork();
                }
                else
                {
                    doWork();
                }
            }

        }

        private void doWork()
        {
            if (textBox1.Text == "Browse for animation or image" && textBox2.Text == "Browse for music")
            {
                MessageBox.Show("You haven't selected animation/image or music.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {

                try
                {
                    String savePath = fullPath + "/" + current;
                    if (textBox1.Text != "Browse for animation or image" && textBox2.Text != "Browse for music")
                    {
                        File.Copy(textBox1.Text, savePath + "/login.swf", true);
                        File.Copy(textBox1.Text, savePath + "/cs_bg_champions.png", true);

                        File.Copy(textBox2.Text, savePath + "/music/LoginScreenLoop.mp3", true);
                        File.Copy(textBox2.Text, savePath + "/music/LoginScreenIntro.mp3", true);
                        MessageBox.Show("Login screen and music changed." + Environment.NewLine + Environment.NewLine + "To enable music, you may need to click on \"Disable Menu Animations\".", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                    else if (textBox1.Text != "Browse for animation or image")
                    {
                        File.Copy(textBox1.Text, savePath + "/login.swf", true);
                        File.Copy(textBox1.Text, savePath + "/cs_bg_champions.png", true);
                        MessageBox.Show("Login screen changed." + Environment.NewLine + Environment.NewLine + "To enable music, you may need to click on \"Disable Menu Animations\".", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                    else if (textBox2.Text != "Browse for music")
                    {

                        File.Copy(textBox2.Text, savePath + "/music/LoginScreenLoop.mp3", true);
                        File.Copy(textBox2.Text, savePath + "/music/LoginScreenIntro.mp3", true);
                        MessageBox.Show("Music changed." + Environment.NewLine + Environment.NewLine + "To enable music, you may need to click on \"Disable Menu Animations\".", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }



                }
                catch (Exception ex)
                {
                    MessageBox.Show("There is something wrong with animation/image or music.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://quicklol.net/");
        }


        private String getLsPath(String name)
        {

            var jo = (JObject)JsonConvert.DeserializeObject(json);

            foreach (JObject jobject in jo["data"])
            {
                if (jobject["name"].ToString() == name)
                {
                    return baseurl + jobject["path"].ToString();
                }
            }

            return baseurl + "data";
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var request = WebRequest.Create(getLsPath(comboBox1.Text) + "/preview.jpg");

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    pictureBox1.BackgroundImage = Bitmap.FromStream(stream);
                    label3.Hide();
                }
            }
            catch (Exception ex)
            {
                label3.Text = "Failed";
                MessageBox.Show("Couldn't load preview for " + comboBox1.Text + ".", "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        static void RequestPath()
        {


            bool loopIt = true;

            // This seems to fix message boxes not showing up on top of things.
            Form form1 = new Form();
            form1.Show();
            form1.Hide();

            while (loopIt)
            {
                FolderBrowserDialog openFileDialog1 = new FolderBrowserDialog();
                openFileDialog1.ShowNewFolderButton = false;
                openFileDialog1.Description = "Please select the path to your League installation. e.g.\n" +
                    @"C:\Riot Games\League of Legends" + "\n" +
                    @"C:\Program Files (x86)\GarenaLoL\GameData\Apps\LoL";
                if (openFileDialog1.ShowDialog(new Form() { TopMost = true, TopLevel = true }) == DialogResult.OK)
                {
                    if (File.Exists(openFileDialog1.SelectedPath + @"\lol.launcher.exe") || File.Exists(openFileDialog1.SelectedPath + @"\lol.exe"))
                    {
                        Settings.Default.LeaguePath = openFileDialog1.SelectedPath;
                        Settings.Default.Save();
                        loopIt = false;
                    }
                    else
                    {
                        MessageBox.Show("Unable to locate League of Legends. Please try again.", "Can't Find League", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
                else
                {
                    // User did cancel the window.
                    didCancel = true;
                    loopIt = false;
                }
            }

        }

        private static void DirectoryCopy(
        string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);

            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
            
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = true;

            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Browse for animation or image";
            fdlg.InitialDirectory = @"c:\";
            fdlg.Filter = "Animation or image(*.swf, .png)|*.swf;*.png";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fdlg.FileName;
            }

            try
            {
                label3.Show();
                pictureBox1.BackgroundImage = null;
                pictureBox1.BackgroundImage = new Bitmap(textBox1.Text);
                label3.Hide();

            }
            catch (Exception ex)
            {
                label3.Text = "Preview not available.";
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Browse for music";
            fdlg.InitialDirectory = @"c:\";
            fdlg.Filter = "MP3 (*.mp3)|*.mp3";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = fdlg.FileName;
            }
        }

        private void comboBox1_Click(object sender, EventArgs e)
        {
            radioButton2.Checked = true;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(fullPath + "/" + current + "_backup") == true)
            {
                Directory.Delete(fullPath + "/" + current, true);
                
                DirectoryCopy(fullPath + "/" + current + "_backup", fullPath + "/" + current, true);
                Directory.Delete(fullPath + "/" + current + "_backup", true);

                MessageBox.Show("All changes made by this application have been reversed.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("There is nothing to revert.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

      
        
    }
}

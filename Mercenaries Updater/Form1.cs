﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;


namespace Mercenaries_Updater
{


    public partial class Form1 : Form
    {

      
        public Form1()
        {
           
            InitializeComponent();


        }

        List<string> links = new List<string>(new string[] {});
        int number = 0;
        string checksum;
        int id = 0;
        string escapeString = @"../";
        string lcl_version = null;
        int result;
        bool versionRdy = true;

     
        

        private void Form1_Load(object sender, EventArgs e)
        {
           if(File.Exists(escapeString + "version.txt"))
            {
                string version = File.ReadAllText(escapeString + "version.txt");
                local_version.Text = version;
                lcl_version = version;



            }

       

            try
            {
                string version = Get(@"https://raw.githubusercontent.com/PitchRE/Mercenaries/master/version.txt");
                version_label.Text = version;
                checksum = Get(@"https://raw.githubusercontent.com/PitchRE/Mercenaries/master/checksum.txt");


         
                if (version == "0x00")
                {
                    MessageBox.Show("Check Mercenaries website for new updater version. You use deprecated updater. \n Program will be terminated.");
                    System.Windows.Forms.Application.Exit();
                }

                if (lcl_version == null) lcl_version = "0.0.0";

                try
                {
                    var version_l = new Version(lcl_version);
                    var version_O = new Version(version);

                    result = version_O.CompareTo(version_l);
                }
                
                catch
                {
                    versionRdy = false;
            }
            

               if(versionRdy == false)
                {
                    richTextBox1.AppendText("Something went wrong... Please verify game files.");
                }

                else if (result > 0)
                {
                  
                    this.Invoke((MethodInvoker)delegate
                    {
                        richTextBox1.AppendText($"Local version {local_version.Text} \n Remote Version: {version} Your game is outdated.");
                    });

                } else
                {
              
                    this.Invoke((MethodInvoker)delegate
                    {
                        richTextBox1.AppendText("Looks like you have newest version of Mercenaries. \n You still can verify game files.");
                    });
                }

                button1.Enabled = true;
            } catch
            {
            

                version_label.Text = "Error. Couldn't connect to server.";
                MessageBox.Show("Error. Couldn't connect to server. Possible solutions If the problem persists: \n Try removing version.txt from Mercenaries directory. \n Check Mercenaries website for new updater version. You might use deprecated updater.");

            }
    
        }

    

        private  void button1_Click(object sender, EventArgs e)
        {
            string dir = System.Reflection.Assembly.GetEntryAssembly().Location;

            button1.Enabled = false;
            Task task1 = Task.Factory.StartNew(() => CheckChecksum());
            Task task2 = task1.ContinueWith(antTask => DownloadPrepare());
       

        }


        private  void DownloadPrepare()
        {
       
            this.Invoke((MethodInvoker)delegate {
                progressBar1.Maximum = links.Count;
                if(links.Count == 0)
                {
                    progressBar1.Maximum = 1;
                    progressBar1.Value = 1;
                    MessageBox.Show("Nothing to download");
                }
                
            });

            foreach (string link in links)
            {
          
                DownloadFile(link, id);
            }
        }


        public string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {

            
            string mynumber = ((System.Net.WebClient)(sender)).QueryString["name"];
      
            number++;
            this.Invoke((MethodInvoker)delegate {
                progressBar1.Value = number;
                richTextBox1.AppendText($"Downloaded {mynumber}... \n");
                if (number == links.Count) richTextBox1.AppendText($"Finished. Your game is ready.");


            });
            Debug.WriteLine($"Finished {number}/{links.Count}");
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {


        }
        private Task DownloadFile(string link, int value)
        {
           
            WebClient client = new WebClient();
            string filename = link;
            link = @"https://raw.githubusercontent.com/PitchRE/Mercenaries/master/" + link;
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadFileCompleted += Client_DownloadFileCompleted;
            client.QueryString.Add("name", filename);
            string newDir = System.IO.Path.GetDirectoryName(filename);


            string dir = new FileInfo(filename).Directory.FullName;
            Debug.WriteLine(dir + ">>>" + Directory.Exists(dir) + "\n");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            link = link.Replace("../", "");
            client.DownloadFileAsync(new Uri(link), filename);



            this.Invoke((MethodInvoker)delegate
            {
                richTextBox1.AppendText($"Downloading {filename}... \n");
            });

            return Task.FromResult<object>(null);
        }

        public Boolean CheckChecksum()
        {
       
            using (StringReader reader = new StringReader(checksum))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] tmp = line.Split(new string[] { ">>>" }, StringSplitOptions.None);
                    string path = tmp[0];
                    string hash = tmp[1].Trim();
                    ValidHash(path, hash);

                }
            }
            return true;
        }

        public Boolean ValidHash(string path, string hash)
        {
            
            using (SHA256 mySHA256 = SHA256.Create())
            {
                string filepath = escapeString + path;
                if (path == "checksum.txt") return false;
            
                if (File.Exists(filepath))
                {
                   
                        var fi1 = new FileInfo(filepath);
                        FileStream fileStream = fi1.Open(FileMode.Open);
                        byte[] hashValue = mySHA256.ComputeHash(fileStream);
                        fileStream.Close();
                        string StringByte = ReadableHash(hashValue);
                
                    if (StringByte == hash)
                    {
                        Console.WriteLine($"True: {filepath}");
                        this.Invoke((MethodInvoker)delegate {
                            richTextBox1.AppendText($"OK: {filepath} \n");

                        });



                        return true;
                    } else
                    {
                      
                        links.Add(filepath);
                        Console.WriteLine($"False: {filepath}");
                        this.Invoke((MethodInvoker)delegate {
                            richTextBox1.AppendText($"Corrupted: {filepath} \n");
         
                        });
                        return false;
                    }
                } else
                {
                    links.Add(filepath);
                    Console.WriteLine($"False No File: {filepath}");
                    this.Invoke((MethodInvoker)delegate {
                        richTextBox1.AppendText($"No File: {filepath} \n");
                    });
                    return false;
                }
                 


            }
        }
        static string ReadableHash(byte[] array)
        {
            StringBuilder myStringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                myStringBuilder.Append($"{array[i]:X2}");
                /*     if ((i % 4) == 3) myStringBuilder.Append(" ");*/
            }
            return myStringBuilder.ToString();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text);
        }
    }

}

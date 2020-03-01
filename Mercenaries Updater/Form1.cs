using System;
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

     
        

        private void Form1_Load(object sender, EventArgs e)
        {
        

            try
            {
                string version = Get(@"https://raw.githubusercontent.com/PitchRE/Mercenaries-Client/master/version.txt");
                version = version.Substring(version.LastIndexOf(':') + 1);
                version_label.Text = version;
                checksum = Get(@"https://raw.githubusercontent.com/PitchRE/Mercenaries-Client/master/checksum.txt");
                button1.Enabled = true;
            } catch
            {
                version_label.Text = "Error. Couldn't connect to server.";
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
            });

            foreach (string link in links)
            {
          
                Debug.WriteLine(link);
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

            string mynumber = ((System.Net.WebClient)(sender)).QueryString["number"];
            number++;
            this.Invoke((MethodInvoker)delegate {
                progressBar1.Value = number;
                if (progressBar1.Value == progressBar1.Maximum) MessageBox.Show("Update Finished.");
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
            link = @"https://raw.githubusercontent.com/PitchRE/Mercenaries-Client/master/" + link;
            Debug.WriteLine(filename);
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadFileCompleted += Client_DownloadFileCompleted;
            client.DownloadFileAsync(new Uri(link.Replace("../", "")), filename);
            Debug.WriteLine($"Downloading {link}");
            this.Invoke((MethodInvoker)delegate {
                richTextBox1.AppendText($"Downloaded {filename} \n");
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

    }

}

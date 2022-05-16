using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using System.Threading;

namespace EncrypThor
{
    public partial class Form1 : Form
    {

        public int processingCounter = 0;
        public string[] fileList;
        System.Drawing.Text.PrivateFontCollection PFC = new System.Drawing.Text.PrivateFontCollection();
        public Form1()
        {
            InitializeComponent();
        }


        

        public static Byte[] encrypt(byte[] s, string key)
        {
            List<Byte> output = new List<byte>();
            Byte[] codeword = Encoding.UTF8.GetBytes(key);

            Byte keybyte = (Byte)(codeword[0] ^ codeword[0]);
            foreach (Byte b in codeword)
            {
                keybyte = (Byte)(b ^ keybyte);
            }

            for (int i = 0; i < s.Length; i++)
            {
                output.Add((Byte)(s[i] ^ codeword[i % codeword.Length] ^ keybyte));
            }

            return output.ToArray();
        }

        public Icon getIcon(string path)
        {
            Icon ico = Icon;
            try
            {
                ico = Icon.ExtractAssociatedIcon(path);
            } catch (Exception e)
            {
                // Do nottin
            }
            return ico;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripTextBox1.TextBox.PasswordChar = '*';

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
            {
                listView1.Items.Add(new ListViewItem(openFileDialog1.FileNames[i]));
            }
        }

        public string generatePassword(string password)
        {
            string passwordBytes = "";

            for (int n = 0; n < 4; n++)
            {
                for (int i = 0; i<password.Length; i++)
                {
                    passwordBytes += (char)password[i]*i ^ (password.Length * i);
                }
                password = passwordBytes;
            }


            string[] s = password.SplitBy(3).ToArray();
            password = "";
            for (int i = 0; i < s.Length; i++)
            {
                int p = 0;
                int.TryParse(s[i], out p);
                password += Convert.ToChar(p);
            }

            return password;
        }




        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 && File.Exists(listView1.SelectedItems[0].Text)) { 
                try
                {
                    string mimeType = MimeMapping.GetMimeMapping(listView1.SelectedItems[0].Text);
                    label1.Text = listView1.SelectedItems[0].Text;
                    label2.Text = "Type: " + mimeType;
                    Icon icon = Icon.ExtractAssociatedIcon(listView1.SelectedItems[0].Text);
                    pictureBox3.BackgroundImage = icon.ToBitmap();
                    if (mimeType.Contains("image"))
                    {
                        pictureBox2.BackgroundImage = Image.FromFile(listView1.SelectedItems[0].Text);
                    }
                    else
                    {
                        pictureBox2.BackgroundImage = null;
                    }
                    label1.Text = listView1.SelectedItems[0].Text;
                } catch (Exception ee)
                {
                    label1.Text = listView1.SelectedItems[0].Text;
                    label2.Text = "unable to determine mimetype.";
                }
                

            } else
            {
                label2.Text = "unable to determine mimetype.";
            }

        }

        private void deleteFromListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in listView1.SelectedItems)
            {
                listView1.Items.Remove(eachItem);
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyValue)
            {
                case (int)Keys.Delete:
                    foreach (ListViewItem eachItem in listView1.SelectedItems)
                    {
                        listView1.Items.Remove(eachItem);
                    }
                    break;

            }
        }

        private void encryptDecryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (toolStripTextBox1.TextLength>2)
            {
                this.encryptFiles();
            } else
            {
                MessageBox.Show("The password has to be at least (!) 3 characters long. Why not try like 16 and up or so?");
            }
        }

        public void encryptFiles()
        {
            this.fileList = new String[listView1.Items.Count];
            for(int i=0; i<listView1.Items.Count; i++)
            {
                this.fileList[i] = listView1.Items[i].Text;
            }
            backgroundWorker1.RunWorkerAsync();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.processingCounter + listView1.Items.Count > 1)
            {
                //panel5.Width = panel4.Width * ((processingCounter) / listView1.Items.Count);
                panel5.Width = (int)((double)panel4.Width * ((double)(this.processingCounter) / (double)listView1.Items.Count));

            } else
            {
                panel5.Width = panel4.Width;
            }
            if (backgroundWorker1.IsBusy)
            {
                label4.Text = "Working...";
            } else
            {
                label4.Text = "Ready!";
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < this.fileList.Length; i++)
            {
                if (File.Exists(this.fileList[i])) { 
                    System.IO.File.WriteAllBytes(this.fileList[i], encrypt(System.IO.File.ReadAllBytes(this.fileList[i]), generatePassword(toolStripTextBox1.Text)));
                }
                this.processingCounter = i;
            }
            backgroundWorker1.Dispose();
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0) { 
                string argument = "/select, \"" + listView1.SelectedItems[0].Text + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
        }
    }
    public static class EnumerableEx
    {
        public static IEnumerable<string> SplitBy(this string str, int chunkLength)
        {
            if (String.IsNullOrEmpty(str)) throw new ArgumentException();
            if (chunkLength < 1) throw new ArgumentException();

            for (int i = 0; i < str.Length; i += chunkLength)
            {
                if (chunkLength + i > str.Length)
                    chunkLength = str.Length - i;

                yield return str.Substring(i, chunkLength);
            }
        }
    }
}

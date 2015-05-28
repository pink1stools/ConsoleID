using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ConsoleID
{
    public partial class Form1 : Form
    {

        public static string sidps = null;
        public static string spsid = null;
        public static byte[] idps = new byte[0x10];
        public static byte[] psid = new byte[0x10];
        public static string dir = null;

        public Form1()
        {
            InitializeComponent();
        }
        public Form1(string file)
        {
            InitializeComponent();
            open_file(file);
            textBox3.Text = file;
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) Console.WriteLine(file);
            textBox3.Text = files[0];
            open_file(files[0]);
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        public void open_file(string file)
        {
            label5.Text = "";
            bool is_lv1 = false;
           

            Stream fin = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            BinaryReader brfin = new BinaryReader(fin);
            if (fin.Length == 0x1000000)
            {
            foreach (string line in File.ReadLines(file))
            {
                if (line.Contains("sys.rom.addr"))
                {
                    is_lv1 = true;
                }
            }
            }

            dir = new FileInfo(file).Directory.FullName;
            if (is_lv1 == false)
            {
                if (file.EndsWith(".bin") && fin.Length == 0x1000000 || file.EndsWith(".BIN") && fin.Length == 0x1000000)
                {
                    fin.Seek(0x303D0, SeekOrigin.Begin);
                    idps = brfin.ReadBytes(0x10);
                    psid = brfin.ReadBytes(0x10);
                    if (idps[0] == 0x00 && idps[1] == 0x00 && idps[2] == 0x00 && idps[3] == 0x01 && idps[4] == 0x00)
                    {
                        fill();
                        label5.Text = "NOR Flash";
                        fin.Close();
                        brfin.Close();
                    }
                    else if (idps[0] == 0x00 && idps[1] == 0x00 && idps[2] == 0x01 && idps[3] == 0x00 && idps[7] == 0x00)
                    {
                        int p1 = 0;
                        int p2 = 1;
                        while (p2 < 16)
                        {
                            swap(idps, p1, p2);
                            swap(psid, p1, p2);
                            p1++; p1++;
                            p2++; p2++;
                        }

                        fill();
                        label5.Text = "NOR Flash Swapped";
                        fin.Close();
                        brfin.Close();
                    }
                    else
                    {
                        fin.Seek(0x80870, SeekOrigin.Begin);
                        idps = brfin.ReadBytes(0x10);
                        psid = brfin.ReadBytes(0x10);
                        if (idps[0] == 0x00 && idps[1] == 0x00 && idps[2] == 0x00 && idps[3] == 0x01 && idps[4] == 0x00)
                        {
                            fill();
                            fin.Close();
                            brfin.Close();
                        }
                        else if (idps[0] == 0x00 && idps[1] == 0x00 && idps[2] == 0x01 && idps[3] == 0x00 && idps[7] == 0x00)
                        {
                            int p1 = 0;
                            int p2 = 1;
                            while (p2 < 16)
                            {
                                swap(idps, p1, p2);
                                swap(psid, p1, p2);
                                p1++; p1++;
                                p2++; p2++;
                            }

                            fill();
                            fin.Close();
                            brfin.Close();
                        }
                        else
                        {


                            MessageBox.Show("Can Not Find IDPS");
                            fin.Close();
                            brfin.Close();
                        }
                    }
                }


                else if (file.EndsWith(".bin") && fin.Length < 0x1000000 || file.EndsWith(".BIN") && fin.Length < 0x1000000)
                {
                    byte[] fileBytes = new byte[fin.Length];
                    fin.Read(fileBytes, 0, fileBytes.Length);


                    int off =idps_search(fileBytes);
                    int padd = 24;

                    fin.Seek(off, SeekOrigin.Begin);
                    idps = brfin.ReadBytes(0x10);
                    fin.Seek(off + padd, SeekOrigin.Begin);
                    psid = brfin.ReadBytes(0x10);
                    fill();
                    label5.Text = "Lv2 Flash";
                    fin.Close();
                    brfin.Close();
                }
                else if (file.EndsWith(".bin") && fin.Length > 0x1000000 || file.EndsWith(".BIN") && fin.Length > 0x1000000)
                {
                    byte[] fileBytes = new byte[fin.Length];
                    fin.Read(fileBytes, 0, fileBytes.Length);


                    int off = idps_search(fileBytes);
                    //int padd = 24;

                    fin.Seek(off, SeekOrigin.Begin);
                    idps = brfin.ReadBytes(0x10);
                   // fin.Seek(off + padd, SeekOrigin.Begin);
                    psid = null;
                    fill();
                    label5.Text = "NAND Flash";
                    fin.Close();
                    brfin.Close();
                }
                else if (!file.EndsWith(".bin") || !file.EndsWith(".BIN"))
                {
                    fin.Close();
                    brfin.Close();
                    MessageBox.Show("Invalid File Type");
                }
            }
            if (is_lv1 == true)
            {
                if (file.EndsWith(".bin") && fin.Length == 0x1000000 || file.EndsWith(".BIN") && fin.Length == 0x1000000)
                {
                    byte[] fileBytes = new byte[fin.Length];
                    fin.Read(fileBytes, 0, fileBytes.Length);


                    int off = idps_search(fileBytes);
                    int padd = 136;

                    fin.Seek(off, SeekOrigin.Begin);
                    idps = brfin.ReadBytes(0x10);
                    fin.Seek(off + padd, SeekOrigin.Begin);
                    psid = brfin.ReadBytes(0x10);
                    fill();
                    label5.Text = "Lv1 Flash";
                    fin.Close();
                    brfin.Close();
                }
            }
        }

        public static string ByteArrayToHexString(byte[] ByteArray)
        {
            string HexString = "";
            for (int i = 0; i < ByteArray.Length; ++i)
                HexString += ByteArray[i].ToString("X2"); // +" ";
            return HexString;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (sidps != null)
            {
                if (sidps.Contains("-") && checkBox1.Checked == false)
                {
                    sidps = ByteArrayToHexString(idps);

                    textBox1.Text = sidps;
                }
                else if (!sidps.Contains("-") && checkBox1.Checked == true)
                {
                    sidps = sidps.Insert(8, "-");
                    sidps = sidps.Insert(17, "-");
                    sidps = sidps.Insert(26, "-");

                    textBox1.Text = sidps;
                }

            }
            if (spsid != null)
            {
                if (spsid.Contains("-") && checkBox1.Checked == false)
                {
                    spsid = ByteArrayToHexString(psid);

                    textBox2.Text = spsid;
                }
                else if (!spsid.Contains("-") && checkBox1.Checked == true)
                {
                    spsid = spsid.Insert(8, "-");
                    spsid = spsid.Insert(17, "-");
                    spsid = spsid.Insert(26, "-");

                    textBox2.Text = spsid;
                }
            }
            
        }

        public static void swap(byte[] array, int position1, int position2)
        {

            byte temp = array[position1]; 
            array[position1] = array[position2]; 
            array[position2] = temp; 
        }

        public void get_info(byte[] array)
        {
            if (array[5] == 0x80) { label3.Text = "Target ID 80 (TEST)"; }
            if (array[5] == 0x81) { label3.Text = "Target ID 81 (TOOL)"; }
            if (array[5] == 0x82) { label3.Text = "Target ID 82 (DEX)"; }
            if (array[5] == 0x83) { label3.Text = "Target ID 83 (Japan)"; }
            if (array[5] == 0x84) { label3.Text = "Target ID 84 (USA)"; }
            if (array[5] == 0x85) { label3.Text = "Target ID 85 (Europe)"; }
            if (array[5] == 0x86) { label3.Text = "Target ID 86 (Korea)"; }
            if (array[5] == 0x87) { label3.Text = "Target ID 87 (United Kingdom)"; }
            if (array[5] == 0x88) { label3.Text = "Target ID 88 (Mexico)"; }
            if (array[5] == 0x89) { label3.Text = "Target ID 89 (Australia/New Zealand)"; }
            if (array[5] == 0x8A) { label3.Text = "Target ID 8A (South Asia)"; }
            if (array[5] == 0x8B) { label3.Text = "Target ID 8B (Taiwan)"; }
            if (array[5] == 0x8C) { label3.Text = "Target ID 8C (Russia)"; }
            if (array[5] == 0x8D) { label3.Text = "Target ID 8D (China (Never released))"; }
            if (array[5] == 0x8E) { label3.Text = "Target ID 8E (Hong Kong)"; }
            if (array[5] == 0x8F) { label3.Text = "Target ID 8F (Brazil)"; }
            if (array[5] == 0xA0) { label3.Text = "Target ID A0 (ARCADE)"; }

            if (array[7] == 0x01) { label4.Text = "Model CECHAxx"; }
            if (array[7] == 0x02) { label4.Text = "Model CECHBxx"; }
            if (array[7] == 0x03) { label4.Text = "Model CECHCxx"; }
            if (array[7] == 0x04) { label4.Text = "Model CECHExx"; }
            if (array[7] == 0x05) { label4.Text = "Model CECHGxx"; }
            if (array[7] == 0x06) { label4.Text = "Model CECHHxx"; }
            if (array[7] == 0x07) { label4.Text = "Model CECHJxx/CECHKxx"; }
            if (array[7] == 0x08) { label4.Text = "Model CECHLxx/CECHMxx/CECHPxx/CECHQxx"; }
            if (array[7] == 0x09) { label4.Text = "Model CECH-20xx"; }
            if (array[7] == 0x0A) { label4.Text = "Model CECH-21xx"; }
            if (array[7] == 0x0B) { label4.Text = "Model CECH-25xx"; }
            if (array[7] == 0x0C) { label4.Text = "Model CECH-30xx"; }
            if (array[7] == 0x0D) { label4.Text = "Model CECH-40xx"; }
            if (array[7] == 0x0E) { label4.Text = "Model ? CECH-42xx"; }
            if (array[7] == 0x0F) { label4.Text = "Model ? CECH-43xx"; }
            if (array[7] == 0x13) { label4.Text = "Model CECH-43xxC"; }

        }

        public void fill()
        {
            if (idps != null)
            {
                sidps = ByteArrayToHexString(idps);
                if (checkBox1.Checked == true)
                {
                    sidps = sidps.Insert(8, "-");
                    sidps = sidps.Insert(17, "-");
                    sidps = sidps.Insert(26, "-");
                }
                textBox1.Text = sidps;
                get_info(idps);
            }
            else if(idps == null)
            {
                textBox1.Text = "";
            }
            if (psid != null)
            {
                spsid = ByteArrayToHexString(psid);
                if (checkBox1.Checked == true)
                {
                    spsid = spsid.Insert(8, "-");
                    spsid = spsid.Insert(17, "-");
                    spsid = spsid.Insert(26, "-");
                }
                textBox2.Text = spsid;
            }
            else if (psid == null)
            {
                textBox2.Text = "";
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dir != null)
            {
                string outFile = dir + "/idps.bin";
                FileStream o = File.Open(outFile, FileMode.Create);
                o.Write(idps, 0, idps.Length);
                o.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dir != null)
            {
                string outFile = dir + "/psid.bin";
                FileStream o = File.Open(outFile, FileMode.Create);
                o.Write(psid, 0, psid.Length);
                o.Close();
            }
        }

        public int idps_search(byte[] fileBytes)
        {
            byte[] t1 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x80 };
            byte[] t2 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x81 };
            byte[] t3 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x82 };
            byte[] t4 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x83 };
            byte[] t5 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x84 };
            byte[] t6 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x85 };
            byte[] t7 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x86 };
            byte[] t8 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x87 };
            byte[] t9 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x88 };
            byte[] t10 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x89 };
            byte[] t11 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x8A };
            byte[] t12 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x8B };
            byte[] t13 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x8C };
            byte[] t14 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x8D };
            byte[] t15 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x8E };
            byte[] t16 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0x8F };
            byte[] t17 = { 0x00, 0x00, 0x00, 0x01, 0x00, 0xA0 };

            int i = 2;
            int f = -1;
            int f2 = -1;
            
            byte[] t = new byte[0x06];
            while(i < 16 && f == -1)
            {
                
                if(i == 0)  { t = t1;} 
                if(i == 1)  { t = t2;} 
                if(i == 2)  { t = t3;} 
                if(i == 3)  { t = t4;} 
                if(i == 4)  { t = t5;} 
                if(i == 5)  { t = t6;} 
                if(i == 6)  { t = t7;} 
                if(i == 7)  { t = t8;} 
                if(i == 8)  { t = t9;} 
                if(i == 9)  { t = t10;}
                if(i == 10)  { t = t11;}
                if(i == 11)  { t = t12;}
                if(i == 12)  { t = t13;}
                if(i == 13)  { t = t14;}
                if(i == 14)  { t = t15;}
                if(i == 15)  { t = t16;}
                if(i == 16)  { t = t17;}

                f = ByteSearch(fileBytes, t, 0);

                i++;
            }
            if (f != 0)
            {
                f2 = ByteSearch(fileBytes, t, f + 16);
            }
            if (f2 != -1)
            {
                return f2;
            }
            else
            {
                return f;
            }
            
        }


        private static int ByteSearch(byte[] searchIn, byte[] searchBytes, int start = 0)
        {
            int found = -1;
            bool matched = false;
            //only look at this if we have a populated search array and search bytes with a sensible start
            if (searchIn.Length > 0 && searchBytes.Length > 0 && start <= (searchIn.Length - searchBytes.Length) && searchIn.Length >= searchBytes.Length)
            {
                //iterate through the array to be searched
                for (int i = start; i <= searchIn.Length - searchBytes.Length; i++)
                {
                    //if the start bytes match we will start comparing all other bytes
                    if (searchIn[i] == searchBytes[0])
                    {
                        if (searchIn.Length > 1)
                        {
                            //multiple bytes to be searched we have to compare byte by byte
                            matched = true;
                            for (int y = 1; y <= searchBytes.Length - 1; y++)
                            {
                                if (searchIn[i + y] != searchBytes[y])
                                {
                                    matched = false;
                                    break;
                                }
                            }
                            //everything matched up
                            if (matched)
                            {
                                found = i;
                                break;
                            }

                        }
                        else
                        {
                            //search byte is only one bit nothing else to do
                            found = i;
                            break; //stop the loop
                        }

                    }
                }

            }
            return found;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                button1.Enabled = false;
            }
            else if (textBox1.Text != "")
            {
                button1.Enabled = true;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
            {
                button2.Enabled = false;
            }
            else if (textBox2.Text != "")
            {
                button2.Enabled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.Reset();
            openFileDialog1.Filter = "BIN files (*.BIN, *.bin)|*.BIN; *.bin|All files (*.*)|*.*";
          
            DialogResult openFile = this.openFileDialog1.ShowDialog();
            if (openFile == DialogResult.OK)
            {
                open_file(this.openFileDialog1.FileName);
                this.textBox3.Text = this.openFileDialog1.FileName;
            }
        }

       




    }




}

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Dmp_Decoder
{
    public partial class Form1 : Form
    {
        public Form1(string file = null)
        {
            InitializeComponent();

            form = this;
            AllowDrop = true;
            //DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;


            openFileBtn.Click += OpenFileThroughBtn;
            saveFileButton.Click += SaveFile;
            saveFileButton.Visible = false;

            loadFileDialog.Filter = "Dump files (*.dmp)|*.dmp";
            saveFileDialog1.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp";
            
            Resize += Form1_Resize;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            if (file != null) OpenFile(file);
        }

        private void ShowBitmap(Bitmap bitmap)
        {
            pictureBox1.Size = bitmap.Size;
            pictureBox1.Image = bitmap;
            openFileBtn.Visible = false;
            form.Size = pictureBox1.Size;
        }

        private void RescaleElements(Control controller)
        {
            pictureBox1.Size = controller.Size;

            saveFileButton.Location = new Point(controller.Width - 150, controller.Height - 75);
        }

        private void HideShowBtns()
        {
            openFileBtn.Visible = false;
            saveFileButton.Visible = true;
        }

        private void HandleExceptions(Exception t)
        {
            var result = MessageBox.Show(t.Message, "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
            if (result == DialogResult.Abort || result == DialogResult.Retry)
            {
                Application.Exit();
                Environment.Exit(-1);
            }
        }

        private void SaveFile(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel) return;

            string filename = saveFileDialog1.FileName;

            pictureBox1.Image.Save(filename);
            MessageBox.Show("Файл сохранён");
        }

        private void OpenFileThroughBtn(object sender, EventArgs e)
        {
            if (loadFileDialog.ShowDialog() == DialogResult.Cancel) return;
            OpenFile(loadFileDialog.FileName);
        }

        private void OpenFile(string filename)
        {
            try
            {
                EncodeDmp(filename);
                HideShowBtns();
            } catch (Exception exp)
            {
                HandleExceptions(exp);
                openFileBtn.Visible = true;
                Application.DoEvents();
            }
            
            RescaleElements(form);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            RescaleElements(control);
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) Console.WriteLine(file);
        }

        private void EncodeDmp(string dmpFile)
        {
            if (dmpFile.Length > 0)
            {
                if (dmpFile.Substring(dmpFile.LastIndexOf('.')) != ".dmp")
                {
                    throw new ArgumentException("Not a \".dmp\" file given!");
                }
            }

            byte[] bytes;
            using (FileStream fsSource = new FileStream(dmpFile, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[fsSource.Length];
                int bytesLeftToRead = bytes.Length;
                int bytesRead = 0;

                #region File Reading
                while (bytesLeftToRead > 0)
                {
                    int n = fsSource.Read(bytes, bytesRead, bytesLeftToRead);

                    if (n == 0) break;

                    bytesRead += n;
                    bytesLeftToRead -= n;
                }
                #endregion

                int skipBytes = 0;
                byte[] bmfhBytes = new byte[BMFH.StructSize]; Array.Copy(bytes, skipBytes, bmfhBytes, 0, bmfhBytes.Length); skipBytes += bmfhBytes.Length;
                byte[] bmihBytes = new byte[BMIH.StructSize]; Array.Copy(bytes, skipBytes, bmihBytes, 0, bmihBytes.Length); skipBytes += bmihBytes.Length;
                byte[] secBytes = new byte[SecBlock.StructSize]; Array.Copy(bytes, skipBytes, secBytes, 0, secBytes.Length); skipBytes += secBytes.Length;
                BMFH bMFH = new BMFH(bmfhBytes); BMIH bMIH = new BMIH(bmihBytes); SecBlock secBlock = new SecBlock(secBytes);
                //skipBytes = bMFH.OffBits.HexToInt();
                byte[] imgBytes = new byte[bytes.Length - skipBytes]; Array.Copy(bytes, skipBytes, imgBytes, 0, imgBytes.Length);
                ImageBlock imageBlock = new ImageBlock(bytes, bMIH.Width.HexToInt(), bMIH.Height.HexToInt(), bMIH.BitCount.HexToInt());

                Bitmap encodedBitmap = imageBlock.GetBitmap();
                ShowBitmap(encodedBitmap);
            }
        }
    }
}

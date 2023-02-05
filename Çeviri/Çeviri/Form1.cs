using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using HtmlAgilityPack;

namespace Çeviri
{
    public partial class Çeviri : Form
    {
        public Çeviri()
        {
            InitializeComponent();
        }
        
        WebBrowser wb = new WebBrowser();

        private void Form1_Load(object sender, EventArgs e)
        {
            wb.Dock = DockStyle.Bottom;
            wb.Height = 0;
            //wb.Visible = false;
            wb.Navigate("https://www.bing.com/translator");

            this.Controls.Add(wb);
        }

        // Gölge Kodu

        private bool Drag;
        private int MouseX;
        private int MouseY;

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        private bool m_aeroEnabled;

        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]

        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
            );

        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();
                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW; return cp;
            }
        }
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0; DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 0,
                            rightWidth = 0,
                            topHeight = 0
                        }; DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                    }
                    break;
                default: break;
            }
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT) m.Result = (IntPtr)HTCAPTION;
        }
        private void PanelMove_MouseDown(object sender, MouseEventArgs e)
        {
            Drag = true;
            MouseX = Cursor.Position.X - this.Left;
            MouseY = Cursor.Position.Y - this.Top;
        }
        private void PanelMove_MouseMove(object sender, MouseEventArgs e)
        {
            if (Drag)
            {
                this.Top = Cursor.Position.Y - MouseY;
                this.Left = Cursor.Position.X - MouseX;
            }
        }
        private void PanelMove_MouseUp(object sender, MouseEventArgs e) { Drag = false; }

        int x, y;
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            x = e.X;
            y = e.Y;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int ComboBoxIndex = comboBox2.SelectedIndex;
            comboBox2.SelectedIndex = comboBox1.SelectedIndex;
            comboBox1.SelectedIndex = ComboBoxIndex;
        }

        int yazdirma = 0;

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            wb.Document.GetElementById("tta_input_ta").InnerText = richTextBox1.Text;
            yazdirma = 0;

            if (!timer1.Enabled)
            {
                timer1.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            richTextBox2.Text = wb.Document.GetElementById("tta_output_ta").InnerText;
            yazdirma++;

            if (yazdirma >= 10)
            {
                timer1.Enabled = false;
                yazdirma = 0;
            }
        }

        int cb1_onceki = 0;
        int cb2_onceki = 1;

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == comboBox2.SelectedIndex)
            {
                comboBox2.SelectedIndex = cb1_onceki;
            }
            
            cb1_onceki = comboBox1.SelectedIndex;

            int mevcut_ekli_dil = wb.Document.GetElementById("t_srcRecentLang").Children.Count;

            //MessageBox.Show(mevcut_ekli_dil.ToString());

            HtmlElement hField = wb.Document.GetElementById("tta_srcsl");
            hField.SetAttribute("selectedIndex", (comboBox1.SelectedIndex + mevcut_ekli_dil).ToString());
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == comboBox2.SelectedIndex)
            {
                comboBox1.SelectedIndex = cb2_onceki;
            }
            
            cb2_onceki = comboBox2.SelectedIndex;

            int mevcut_ekli_dil1 = wb.Document.GetElementById("t_tgtRecentLang").Children.Count;

            //MessageBox.Show(mevcut_ekli_dil1.ToString());

            HtmlElement hField1 = wb.Document.GetElementById("tta_tgtsl");
            hField1.SetAttribute("selectedIndex", (comboBox2.SelectedIndex + mevcut_ekli_dil1).ToString());
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Process myProcess = new Process();
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = "https://turkhackteam.org";
            myProcess.Start();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void panel8_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            this.Left += e.X - x;
            this.Top += e.Y - y;
        }
    }
}
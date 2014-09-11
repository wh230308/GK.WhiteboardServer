using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GK.WhiteboardServer
{
    public partial class ShortcutsForm : Form
    {
        private readonly int pixelH = Screen.PrimaryScreen.Bounds.Height;
        private readonly int pixelW = Screen.PrimaryScreen.Bounds.Width;

        public Image pic1Image
        {
            get
            {
                return pictureBox1.Image;
            }
            set
            {
                pictureBox1.Image = value;

            }
        }

        public Point pic1Location
        {
            get
            {
                return pictureBox1.Location;
            }

            set
            {
                pictureBox1.Location = value;
            }
        }

        public Image pic2Image
        {
            get
            {
                return pictureBox2.Image;
            }
            set
            {
                pictureBox2.Image = value;

            }
        }

        public Point pic2Location
        {
            get
            {
                return pictureBox2.Location;
            }

            set
            {
                pictureBox2.Location = value;
            }
        }

        public Image pic3Image
        {
            get
            {
                return pictureBox3.Image;
            }
            set
            {
                pictureBox3.Image = value;

            }
        }

        public Point pic3Location
        {
            get
            {
                return pictureBox3.Location;
            }

            set
            {
                pictureBox3.Location = value;
            }
        }

        public Image pic4Image
        {
            get
            {
                return pictureBox4.Image;
            }
            set
            {
                pictureBox4.Image = value;

            }
        }

        public Point pic4Location
        {
            get
            {
                return pictureBox4.Location;
            }

            set
            {
                pictureBox4.Location = value;
            }
        }

        public bool IsComplete
        {
            get;

            set;

        }

        public event EventHandler ExceptionNotifition;

        public ShortcutsForm()
        {
            InitializeComponent();

            IsComplete = false;

            //pictureBox1.Location = new Point((pixelW / 8) * 7, pixelH / 8);
            //pictureBox2.Location = new Point((pixelW / 8) * 7, (pixelH / 8) * 7 - 16);
            //pictureBox3.Location = new Point(pixelW / 8 - 96, pixelH / 8);
            //pictureBox4.Location = new Point(pixelW / 8 - 96, (pixelH / 8) * 7 - 16);
        }

        private void ShortcutsForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!IsComplete)
            {
                if (ExceptionNotifition != null)
                {
                    ExceptionNotifition(this, new EventArgs());
                }
            }

            if (e.KeyChar == 27)
            {
                this.Hide();
            }
        }
    }
}

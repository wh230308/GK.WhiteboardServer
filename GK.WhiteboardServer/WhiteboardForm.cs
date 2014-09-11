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
    public partial class WhiteboardForm : Form
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

        public Image pic5Image
        {
            get
            {
                return pictureBox5.Image;
            }

            set
            {
                pictureBox5.Image = value;
            }
        }

        public Point pic5Location
        {
            get
            {
                return pictureBox5.Location;
            }

            set
            {
                pictureBox5.Location = value;
            }
        }

        public Image pic6Image
        {
            get
            {
                return pictureBox6.Image;
            }

            set
            {
                pictureBox6.Image = value;
            }
        }

        public Point pic6Location
        {
            get
            {
                return pictureBox6.Location;
            }

            set
            {
                pictureBox6.Location = value;
            }
        }

        public Image pic7Image
        {
            get
            {
                return pictureBox7.Image;
            }

            set
            {
                pictureBox7.Image = value;
            }
        }

        public Point pic7Location
        {
            get
            {
                return pictureBox7.Location;
            }

            set
            {
                pictureBox7.Location = value;
            }
        }

        public Image pic8Image
        {
            get
            {
                return pictureBox8.Image;
            }

            set
            {
                pictureBox8.Image = value;
            }
        }

        public Point pic8Location
        {
            get
            {
                return pictureBox8.Location;
            }

            set
            {
                pictureBox8.Location = value;
            }
        }

        public Image pic9Image
        {
            get
            {
                return pictureBox9.Image;
            }

            set
            {
                pictureBox9.Image = value;
            }
        }

        public Point pic9Location
        {
            get
            {
                return pictureBox9.Location;
            }

            set
            {
                pictureBox9.Location = value;
            }
        }

        public Image pic10Image
        {
            get
            {
                return pictureBox10.Image;
            }

            set
            {
                pictureBox10.Image = value;
            }
        }

        public Point pic10Location
        {
            get
            {
                return pictureBox10.Location;
            }

            set
            {
                pictureBox10.Location = value;
            }
        }

        public bool IsComplete
        {
            get;

            set;

        }

        public event EventHandler ExceptionNotifition;

        public WhiteboardForm()
        {
            InitializeComponent();

            IsComplete = false;

            //pictureBox2.Location = new Point((pixelW / 8) * 7 - 50, pixelH / 8 - 50);

            //pictureBox3.Location = new Point((pixelW / 8) * 7 - 50, (pixelH / 8) * 7 - 50);

            //pictureBox4.Location = new Point(pixelW / 8 - 50, (pixelH / 8) * 7 - 50);

            //pictureBox5.Location = new Point(pixelW / 2 - 50, pixelH / 2 - 50);

            //pictureBox6.Location = new Point(pixelW / 2 - 48, pixelH / 8 - 16);

            //pictureBox7.Location = new Point((pixelW / 8) * 7 - 16, pixelH / 2 - 48);

            //pictureBox8.Location = new Point(pixelW / 2 - 48, (pixelH / 8) * 7 + 16);

            //pictureBox9.Location = new Point((pixelW / 16) * 5 - 24, (pixelH / 16) * 11 - 24);

            //pictureBox10.Location = new Point(pixelW / 2 - 250, pixelH / 2 - 200);
        }

        private void WhiteboardForm_KeyPress(object sender, KeyPressEventArgs e)
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

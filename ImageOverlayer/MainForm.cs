using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Utilities;
using System.Diagnostics;
using Sc.Util.Rendering;

namespace ImageOverlayer {
	public partial class MainForm : Form {
        private static globalKeyboardHook gkh;

        private bool overlayed = false;
        private double opacity = 0.5;

        public MainForm() {
            InitializeComponent();
            this.TopMost = true;
            this.KeyPreview = true;
            this.AllowDrop = true;

            gkh = new globalKeyboardHook();
            // Don't let the Garbage Man interfere
            GC.KeepAlive(gkh);

            // These are the keys you want to listen to
            gkh.HookedKeys.Add(Keys.Scroll);
			gkh.HookedKeys.Add(Keys.Left);
			gkh.HookedKeys.Add(Keys.Right);
			gkh.HookedKeys.Add(Keys.Up);
			gkh.HookedKeys.Add(Keys.Down);

			// These are the handlers that will be called when the HookedKeys are pushed
			gkh.KeyDown += new KeyEventHandler(gkh_KeyDown);
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        const int GWL_EXSTYLE = -20;
        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_TRANSPARENT = 0x20;

		protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
        }

        void gkh_KeyDown(object sender, KeyEventArgs e) {
			// Use e.Handled = true to prevent other application recieving the keyPress
			// Use e.handled = false to allow other application to recieve
			e.Handled = true;

			switch(e.KeyCode) {
				case Keys.Scroll:
					if(overlayed) {
						this.Opacity = 1;
						var style = GetWindowLong(this.Handle, GWL_EXSTYLE);
						SetWindowLong(this.Handle, GWL_EXSTYLE, style | 0 | 0);
					} else {
						this.Opacity = opacity;
						var style = GetWindowLong(this.Handle, GWL_EXSTYLE);
						SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_TRANSPARENT);
					}

					overlayed = !overlayed;

					break;
				case Keys.Left:
					this.Location = new Point(this.Location.X - 1, this.Location.Y);

					break;
				case Keys.Right:
					this.Location = new Point(this.Location.X + 1, this.Location.Y);

					break;
				case Keys.Up:
					this.Location = new Point(this.Location.X, this.Location.Y - 1);

					break;
				case Keys.Down:
					this.Location = new Point(this.Location.X, this.Location.Y + 1);

					break;
			}
        }

        private void Form1_Closing(object sender, CancelEventArgs e) {
            // Make sure we unhook once the form closes
            gkh.unhook();
        }

		private void label1_MouseDown(object sender, MouseEventArgs e) {
            if(e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

		private void label1_Click(object sender, EventArgs e) {
            MouseEventArgs me = (MouseEventArgs) e;

            if(me.Button == MouseButtons.Right) {
				using(OpenFileDialog dlg = new OpenFileDialog()) {
					dlg.Title = "Open Image";

					if(dlg.ShowDialog() == DialogResult.OK) {
						using(var fromFile = Image.FromFile(dlg.FileName)) {
							Bitmap b = new Bitmap(fromFile);

							pictureBox1.Image = b;

							this.Size = new Size(b.Width + 10, b.Height + 10);

							this.Controls.RemoveByKey("label1");

							rainbowTimer.Start();
						}
					}
				}
			} else if(me.Button == MouseButtons.Middle) {
                Application.Exit();
            }
        }

		private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            label1_MouseDown(sender, e);
        }

		private void pictureBox1_Click(object sender, EventArgs e) {
            label1_Click(sender, e);
        }

		private void rainbowTimer_Tick(object sender, EventArgs e) {
			var currentHSL = SimpleColorTransforms.RgBtoHsl(this.BackColor);

			this.BackColor = SimpleColorTransforms.HsLtoRgb(currentHSL.H + 2.5, 1, 0.5);
		}
	}
}

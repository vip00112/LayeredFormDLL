using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace LayeredFormDLL {
    public partial class LayeredForm : Form {
        private string _caption; // 윈도우 타이틀(TITLE)
        private Point _pos; // Form Move를 위한 현재 좌표 저장

        public LayeredForm() {
            _caption = "TITLE";
            _pos = new Point();

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
        }

        // 윈도우 타이틀(CAPTION) 지정
        public void setCaption(string caption) {
            _caption = caption;
        }

        #region Override Methods
        // LAYERED 스타일 반영
        protected override CreateParams CreateParams {
            get {
                CreateParams createParams = base.CreateParams;
                if (!DesignMode) {
                    createParams.ExStyle |= WS_EX_LAYERED;
                }
                return createParams;
            }
        }

        // Form Move : 마우스 좌 클릭 후 이동
        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left) {
                Location = new Point(Location.X + (_pos.X + e.X), Location.Y + (_pos.Y + e.Y));
            }
        }

        // Form Move : 마우스 좌 땠을시 위치 확정
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) {
                _pos = new Point(-e.X, -e.Y);
            }
        }

        // 마우스 더블 클릭 : 종료
        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            base.OnMouseDoubleClick(e);
            if (MessageBox.Show("정말 종료 하시겠습니까 ?", _caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                Close();
            }
        }
        #endregion

        #region Set Bitmap
        protected void SelectBitmap(Bitmap bitmap) {
            SelectBitmap(bitmap, 255);
        }

        private void SelectBitmap(Bitmap bitmap, int opacity) {
            // Does this bitmap contain an alpha channel?
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb) {
                throw new ApplicationException("The bitmap must be 32bpp with alpha-channel.");
            }

            // Get device contexts
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr hOldBitmap = IntPtr.Zero;

            try {
                // Get handle to the new bitmap and select it into the current device context.
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                hOldBitmap = SelectObject(memDc, hBitmap);

                // Set parameters for layered window update.
                MyPoint locationInWindow = new MyPoint(Left - (bitmap.Width / 2), Top - (bitmap.Height / 2));
                MySize sizeInWindow = new MySize(bitmap.Width, bitmap.Height);
                MyPoint LocationInDC = new MyPoint(0, 0);
                BLENDFUNCTION blend = new BLENDFUNCTION();
                blend.BlendOp = AC_SRC_OVER;
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = (byte) opacity;
                blend.AlphaFormat = AC_SRC_ALPHA;

                // Update the window.
                UpdateLayeredWindow(
                    this.Handle,     // Handle to the layered window
                    screenDc,        // Handle to the screen DC
                    ref locationInWindow, // New screen position of the layered window
                    ref sizeInWindow,     // New size of the layered window
                    memDc,           // Handle to the layered window surface DC
                    ref LocationInDC, // Location of the layer in the DC
                    0,               // Color key of the layered window
                    ref blend,       // Transparency of the layered window
                    ULW_ALPHA        // Use blend as the blend function
                    );
            } finally {
                // Release device context.
                ReleaseDC(IntPtr.Zero, screenDc);
                if (hBitmap != IntPtr.Zero) {
                    SelectObject(memDc, hOldBitmap);
                    DeleteObject(hBitmap);
                }
                DeleteDC(memDc);
            }
        }
        #endregion

        #region Native Methods and Structures
        private const int WS_EX_LAYERED = 0x80000;
        private const int HTCAPTION = 0x02;
        private const int WM_NCHITTEST = 0x84;
        private const int ULW_ALPHA = 0x02;
        private const byte AC_SRC_OVER = 0x00;
        private const byte AC_SRC_ALPHA = 0x01;

        [StructLayout(LayoutKind.Sequential)]
        private struct MyPoint {
            public int x;
            public int y;

            public MyPoint(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MySize {
            public int cx;
            public int cy;

            public MySize(int cx, int cy) {
                this.cx = cx;
                this.cy = cy;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ARGB {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNCTION {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst,
            ref MyPoint pptDst, ref MySize psize, IntPtr hdcSrc, ref MyPoint pprSrc,
            int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);
        #endregion
    }
}
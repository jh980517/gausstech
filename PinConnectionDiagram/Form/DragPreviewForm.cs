namespace PinConnectionDiagram
{
    /// <summary>
    /// Shell 드래그 이미지를 사용할 수 없을 때 마우스를 따라다니는 예비 미리보기다.
    /// </summary>
    public partial class DragPreviewForm : Form
    {
        private const int CursorOffset = 14;
        private const uint SwpNoSize = 0x0001;
        private const uint SwpNoZOrder = 0x0004;
        private const uint SwpNoActivate = 0x0010;
        private Point lastLocation = new Point(int.MinValue, int.MinValue);
        private readonly Bitmap scaledPreviewImage;

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
            IntPtr windowHandle,
            IntPtr insertAfter,
            int x,
            int y,
            int width,
            int height,
            uint flags);

        public DragPreviewForm(Image previewImage)
        {
            Size previewSize = new Size(
                Math.Max(1, (int)Math.Round(previewImage.Width * 0.7D)),
                Math.Max(1, (int)Math.Round(previewImage.Height * 0.7D)));
            scaledPreviewImage = new Bitmap(previewSize.Width, previewSize.Height);
            using (Graphics graphics = Graphics.FromImage(scaledPreviewImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.DrawImage(previewImage, new Rectangle(Point.Empty, previewSize));
            }

            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            ClientSize = previewSize;
            MinimumSize = previewSize;
            MaximumSize = previewSize;

            Controls.Add(new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = scaledPreviewImage,
                SizeMode = PictureBoxSizeMode.Normal
            });
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                const int wsExTransparent = 0x20;
                const int wsExNoActivate = 0x08000000;
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |= wsExTransparent | wsExNoActivate;
                return parameters;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                scaledPreviewImage.Dispose();

            base.Dispose(disposing);
        }

        public void MoveNearCursor()
        {
            if (!IsHandleCreated)
                return;

            Point location = new Point(
                Cursor.Position.X + CursorOffset,
                Cursor.Position.Y + CursorOffset);
            if (location == lastLocation)
                return;

            lastLocation = location;
            SetWindowPos(
                Handle,
                IntPtr.Zero,
                location.X,
                location.Y,
                0,
                0,
                SwpNoSize | SwpNoZOrder | SwpNoActivate);
        }
    }
}

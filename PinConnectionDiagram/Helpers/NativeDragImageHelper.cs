using System.Runtime.InteropServices;

namespace PinConnectionDiagram.Helpers
{
    /// <summary>
    /// Windows Shell이 직접 합성하는 드래그 이미지를 설정해 마우스 추적 지연을 최소화한다.
    /// </summary>
    public static class NativeDragImageHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct ShellDragImage
        {
            public Size Size;
            public Point Offset;
            public IntPtr BitmapHandle;
            public int ColorKey;
        }

        [ComImport]
        [Guid("4657278B-411B-11D2-839A-00C04FD918D0")]
        private class DragDropHelper
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("DE5BF786-477A-11D2-839D-00C04FD918D0")]
        private interface IDragSourceHelper
        {
            void InitializeFromBitmap(
                ref ShellDragImage dragImage,
                [MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.IDataObject dataObject);

            void InitializeFromWindow(
                IntPtr windowHandle,
                ref Point point,
                [MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.IDataObject dataObject);
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr objectHandle);

        public static bool TryInitialize(
            System.Windows.Forms.IDataObject dataObject,
            Image sourceImage,
            double scale)
        {
            Size previewSize = new Size(
                Math.Max(1, (int)Math.Round(sourceImage.Width * scale)),
                Math.Max(1, (int)Math.Round(sourceImage.Height * scale)));
            using Bitmap preview = new Bitmap(previewSize.Width, previewSize.Height);
            using (Graphics graphics = Graphics.FromImage(preview))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.DrawImage(sourceImage, new Rectangle(Point.Empty, previewSize));
            }

            IntPtr bitmapHandle = preview.GetHbitmap();
            try
            {
                ShellDragImage dragImage = new ShellDragImage
                {
                    Size = previewSize,
                    Offset = new Point(8, 8),
                    BitmapHandle = bitmapHandle,
                    ColorKey = ColorTranslator.ToWin32(Color.Magenta)
                };
                IDragSourceHelper helper = (IDragSourceHelper)new DragDropHelper();
                helper.InitializeFromBitmap(ref dragImage, dataObject);
                return true;
            }
            catch (Exception exception) when (
                exception is COMException ||
                exception is InvalidCastException ||
                exception is PlatformNotSupportedException)
            {
                return false;
            }
            finally
            {
                DeleteObject(bitmapHandle);
            }
        }
    }
}

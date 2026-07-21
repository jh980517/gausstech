using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text;

namespace PinConnectionDiagram.Helpers
{
    /// <summary>연결도와 시험 절차를 미리보기 이미지 및 PDF 문서로 변환한다.</summary>
    public static class PdfExportService
    {
        private const int PageWidth = 1240;
        private const int PageHeight = 1754;
        private const int Margin = 75;
        private const int DiagramMargin = 35;
        // 논리적 페이지 크기는 그대로 두고 내부 픽셀 수만 늘려 미리보기와 저장 화질을 높인다.
        private const float DiagramRenderScale = 2F;
        private const float ProcedureRenderScale = 2F;

        // 연결도 페이지 뒤에 필요한 수만큼 시험 절차 페이지를 생성한다.
        public static List<Bitmap> CreatePages(
            string title,
            Bitmap diagram,
            string procedure,
            Bitmap supplies)
        {
            List<Bitmap> pages = CreateDiagramPages(title, diagram, supplies);
            pages.AddRange(CreateProcedurePages(title, procedure, pages.Count + 1));
            return pages;
        }

        // 연결도가 준비물 때문에 축소되면 준비물과 연결도를 서로 다른 페이지로 분리한다.
        private static List<Bitmap> CreateDiagramPages(
            string title,
            Bitmap diagram,
            Bitmap supplies)
        {
            List<Bitmap> pages = new List<Bitmap>();
            Bitmap page = CreateBlankPage(PageHeight, PageWidth, DiagramRenderScale);
            Graphics graphics = CreatePageGraphics(page, DiagramRenderScale);
            DrawHeader(graphics, title, "시험 연결도");

            int diagramTop = DrawSupplyList(graphics, supplies, 170);
            Rectangle content = new Rectangle(
                DiagramMargin,
                diagramTop,
                PageHeight - DiagramMargin * 2,
                PageWidth - diagramTop - 75);
            float widthScale = content.Width / (float)diagram.Width;
            bool requiresSeparateDiagramPage =
                diagram.Height * widthScale + 30 > content.Height;

            if (requiresSeparateDiagramPage)
            {
                // 준비물 페이지임을 제목에서도 명확히 표시하고 연결도는 다음 장에 원래 크기로 배치한다.
                DrawHeader(graphics, title, "시험 준비물");
                DrawFooter(graphics, 1);
                graphics.Dispose();
                pages.Add(page);

                page = CreateBlankPage(PageHeight, PageWidth, DiagramRenderScale);
                graphics = CreatePageGraphics(page, DiagramRenderScale);
                DrawHeader(graphics, title, "시험 연결도");
                content = new Rectangle(
                    DiagramMargin,
                    170,
                    PageHeight - DiagramMargin * 2,
                    PageWidth - 245);
            }

            DrawDiagram(graphics, diagram, content);
            DrawFooter(graphics, pages.Count + 1);
            graphics.Dispose();
            pages.Add(page);
            return pages;
        }

        private static void DrawDiagram(
            Graphics graphics,
            Bitmap diagram,
            Rectangle content)
        {
            float scale = Math.Min(
                content.Width / (float)diagram.Width,
                content.Height / (float)diagram.Height);
            Size size = new Size((int)(diagram.Width * scale), (int)(diagram.Height * scale));
            Rectangle target = new Rectangle(
                content.X + (content.Width - size.Width) / 2,
                content.Y + 15,
                size.Width,
                size.Height);
            // 화면 좌표로 안전하게 캡처한 연결도를 고해상도 페이지에 고품질로 확대한다.
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(diagram, target);
        }

        // 화면에서 캡처한 준비물 카드를 페이지 너비에 맞춰 축소하고 다음 영역의 시작점을 반환한다.
        private static int DrawSupplyList(
            Graphics graphics,
            Bitmap supplies,
            int top)
        {
            int availableWidth = (int)graphics.VisibleClipBounds.Width - Margin * 2;
            int innerWidth = availableWidth - 60;
            float scale = Math.Min(2F, innerWidth / (float)Math.Max(1, supplies.Width));
            int renderedWidth = Math.Max(1, (int)Math.Round(supplies.Width * scale));
            int renderedHeight = Math.Max(1, (int)Math.Round(supplies.Height * scale));
            Rectangle sectionBounds = new Rectangle(
                Margin,
                top,
                availableWidth,
                renderedHeight + 88);

            SmoothingMode previousSmoothingMode = graphics.SmoothingMode;
            InterpolationMode previousInterpolationMode = graphics.InterpolationMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            // 밝은 문서용 컨테이너와 약한 그림자로 화면 카드가 출력물에서도 또렷하게 보이게 한다.
            using SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(28, 30, 46, 69));
            using SolidBrush sectionBrush = new SolidBrush(Color.FromArgb(246, 248, 252));
            using Pen borderPen = new Pen(Color.FromArgb(210, 218, 230), 2F);
            using SolidBrush accentBrush = new SolidBrush(AppTheme.Accent);
            using SolidBrush headingBrush = new SolidBrush(AppTheme.DarkAccent);
            using Font headingFont = new Font("맑은 고딕", 16F, FontStyle.Bold);

            FillRoundedRectangle(
                graphics,
                shadowBrush,
                new Rectangle(sectionBounds.X + 5, sectionBounds.Y + 6, sectionBounds.Width, sectionBounds.Height),
                16);
            FillRoundedRectangle(graphics, sectionBrush, sectionBounds, 16);
            DrawRoundedRectangle(graphics, borderPen, sectionBounds, 16);

            graphics.FillRectangle(accentBrush, sectionBounds.X + 22, sectionBounds.Y + 18, 7, 28);
            graphics.DrawString(
                "시험 준비물",
                headingFont,
                headingBrush,
                sectionBounds.X + 42,
                sectionBounds.Y + 15);

            int imageX = sectionBounds.X + (sectionBounds.Width - renderedWidth) / 2;
            int imageY = sectionBounds.Y + 60;
            graphics.DrawImage(supplies, new Rectangle(imageX, imageY, renderedWidth, renderedHeight));

            graphics.SmoothingMode = previousSmoothingMode;
            graphics.InterpolationMode = previousInterpolationMode;
            return sectionBounds.Bottom + 24;
        }

        // 모서리가 둥근 출력용 패널의 내부를 채운다.
        private static void FillRoundedRectangle(
            Graphics graphics,
            Brush brush,
            Rectangle bounds,
            int radius)
        {
            using GraphicsPath path = CreateRoundedRectanglePath(bounds, radius);
            graphics.FillPath(brush, path);
        }

        // 모서리가 둥근 출력용 패널의 테두리를 그린다.
        private static void DrawRoundedRectangle(
            Graphics graphics,
            Pen pen,
            Rectangle bounds,
            int radius)
        {
            using GraphicsPath path = CreateRoundedRectanglePath(bounds, radius);
            graphics.DrawPath(pen, path);
        }

        // 동일한 둥근 사각형 경로를 배경, 그림자, 테두리에서 공통으로 사용한다.
        private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        // 영역 제목과 절차 문장을 블록 단위로 나누고 페이지 높이를 넘으면 다음 장으로 넘긴다.
        private static IEnumerable<Bitmap> CreateProcedurePages(
            string title,
            string procedure,
            int firstPageNumber)
        {
            string[] blocks = procedure.Split(
                new[] { Environment.NewLine + Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);
            List<Bitmap> pages = new List<Bitmap>();
            // 연결도와 동일하게 시험 절차도 가로 방향으로 출력한다.
            Bitmap page = CreateBlankPage(PageHeight, PageWidth, ProcedureRenderScale);
            Graphics graphics = CreatePageGraphics(page, ProcedureRenderScale);
            DrawHeader(graphics, title, "시험 절차");
            int y = 175;
            int stepNumber = 0;

            foreach (string rawBlock in blocks)
            {
                string block = rawBlock.Trim();
                bool isSection = block.StartsWith("[") && block.EndsWith("]");
                string text = isSection ? $"◆ {block.Trim('[', ']')}" : block.TrimStart('-', ' ');
                int indent = isSection ? 0 : 100;
                if (!isSection)
                {
                    stepNumber++;
                }

                int textWidth = PageHeight - Margin * 2 - indent;
                int textHeight;
                if (isSection)
                {
                    using Font measurementFont = new Font("맑은 고딕", 22F, FontStyle.Bold);
                    textHeight = (int)Math.Ceiling(
                        graphics.MeasureString(text, measurementFont, textWidth).Height);
                }
                else
                {
                    textHeight = LayoutProcedureText(graphics, text, 0, 0, textWidth, false);
                }
                // 절차 문장 사이에 충분한 여백을 두어 긴 설명도 단계별로 쉽게 구분되게 한다.
                int blockHeight = isSection
                    ? textHeight + 28
                    : Math.Max(52, textHeight) + 24;
                if (y + blockHeight > PageWidth - 70)
                {
                    DrawFooter(graphics, firstPageNumber + pages.Count);
                    graphics.Dispose();
                    pages.Add(page);
                    page = CreateBlankPage(PageHeight, PageWidth, ProcedureRenderScale);
                    graphics = CreatePageGraphics(page, ProcedureRenderScale);
                    DrawHeader(graphics, title, "시험 절차 (계속)");
                    y = 175;
                }

                if (isSection)
                {
                    DrawProcedureSection(graphics, text, y, PageHeight - Margin * 2, blockHeight);
                }
                else
                {
                    DrawProcedureStep(graphics, stepNumber, text, y, textWidth, blockHeight);
                }

                y += blockHeight;
            }

            DrawFooter(graphics, firstPageNumber + pages.Count);
            graphics.Dispose();
            pages.Add(page);

            // 절차가 여러 장일 때만 첫 장부터 순번을 붙여 현재 페이지를 바로 알 수 있게 한다.
            if (pages.Count > 1)
            {
                for (int index = 0; index < pages.Count; index++)
                {
                    using Graphics headerGraphics = CreatePageGraphics(
                        pages[index],
                        ProcedureRenderScale);
                    DrawHeader(headerGraphics, title, $"시험 절차 ({index + 1})");
                }
            }

            return pages;
        }

        // 절차 영역 제목을 옅은 보라색 배경과 강조선이 있는 섹션 바로 표시한다.
        private static void DrawProcedureSection(
            Graphics graphics,
            string text,
            int y,
            int width,
            int height)
        {
            Rectangle bounds = new Rectangle(Margin, y, width, height - 12);
            using SolidBrush backgroundBrush = new SolidBrush(
                AppTheme.IsDefense ? Color.FromArgb(245, 243, 226) : Color.FromArgb(244, 240, 251));
            using SolidBrush accentBrush = new SolidBrush(
                ColorHelper.GetMapHeaderColor("시험 대상 케이블"));
            using SolidBrush textBrush = new SolidBrush(
                AppTheme.IsDefense ? Color.FromArgb(70, 76, 48) : Color.FromArgb(86, 67, 135));
            using Font font = new Font("맑은 고딕", 21F, FontStyle.Bold);

            FillRoundedRectangle(graphics, backgroundBrush, bounds, 12);
            graphics.FillRectangle(accentBrush, bounds.X, bounds.Y, 8, bounds.Height);
            graphics.DrawString(
                text,
                font,
                textBrush,
                new RectangleF(bounds.X + 28, bounds.Y + 10, bounds.Width - 45, bounds.Height - 15));
        }

        // 순번을 고정 배지로 분리하고 본문은 독립된 영역에 그려 번호 겹침을 방지한다.
        private static void DrawProcedureStep(
            Graphics graphics,
            int stepNumber,
            string text,
            int y,
            int textWidth,
            int blockHeight)
        {
            Rectangle badgeBounds = new Rectangle(Margin + 10, y + 3, 58, 42);
            using SolidBrush badgeBrush = new SolidBrush(AppTheme.DarkAccent);
            using SolidBrush badgeTextBrush = new SolidBrush(AppTheme.Accent);
            using Font badgeFont = new Font("맑은 고딕", 16F, FontStyle.Bold);
            using StringFormat centeredFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using Pen separatorPen = new Pen(Color.FromArgb(225, 230, 238), 1.5F);

            FillRoundedRectangle(graphics, badgeBrush, badgeBounds, 10);
            graphics.DrawString(
                stepNumber.ToString("00"),
                badgeFont,
                badgeTextBrush,
                badgeBounds,
                centeredFormat);

            LayoutProcedureText(
                graphics,
                text,
                Margin + 100,
                y + 5,
                textWidth,
                true);

            graphics.DrawLine(
                separatorPen,
                Margin + 100,
                y + blockHeight - 10,
                graphics.VisibleClipBounds.Width - Margin,
                y + blockHeight - 10);
        }

        // 굵게 표시할 토큰의 실제 폭을 반영해 측정과 그리기를 동일한 로직으로 수행한다.
        private static int LayoutProcedureText(
            Graphics graphics,
            string text,
            float startX,
            float startY,
            int availableWidth,
            bool draw)
        {
            using Font regularFont = new Font("맑은 고딕", 17F, FontStyle.Regular);
            using Font boldFont = new Font("맑은 고딕", 17F, FontStyle.Bold);
            using SolidBrush textBrush = new SolidBrush(Color.FromArgb(38, 38, 38));
            using StringFormat format = (StringFormat)StringFormat.GenericTypographic.Clone();

            const string tokenPattern = @"TJ\d+(?:\s*~\s*TJ\d+)?|(?:지그|어댑터|시험 대상) 케이블\s+\S+|“[^”]+”|\s+|[^\s]+";
            const string boldPattern = @"^(?:\d{2}|TJ\d+(?:\s*~\s*TJ\d+)?|(?:지그|어댑터|시험 대상) 케이블\s+\S+|“[^”]+”)$";
            float x = startX;
            float y = startY;
            float lineHeight = Math.Max(regularFont.GetHeight(graphics), boldFont.GetHeight(graphics)) + 5;

            foreach (System.Text.RegularExpressions.Match tokenMatch in
                System.Text.RegularExpressions.Regex.Matches(text, tokenPattern))
            {
                string token = tokenMatch.Value;
                bool isBold = System.Text.RegularExpressions.Regex.IsMatch(token, boldPattern);
                Font font = isBold ? boldFont : regularFont;
                // GenericTypographic은 공백만 있는 문자열의 폭을 0에 가깝게 반환할 수 있다.
                // 한글 문장 속 일반/굵은 토큰 사이가 붙지 않도록 공백 폭을 별도로 계산한다.
                float tokenWidth = string.IsNullOrWhiteSpace(token)
                    ? MeasureSpaceWidth(graphics, regularFont, format) * token.Length
                    : graphics.MeasureString(token, font, int.MaxValue, format).Width;

                if (!string.IsNullOrWhiteSpace(token) &&
                    x > startX &&
                    x + tokenWidth > startX + availableWidth)
                {
                    x = startX;
                    y += lineHeight;
                }

                if (draw && !(string.IsNullOrWhiteSpace(token) && x == startX))
                    graphics.DrawString(token, font, textBrush, new PointF(x, y), format);

                if (!(string.IsNullOrWhiteSpace(token) && x == startX))
                    x += tokenWidth;
            }

            return (int)Math.Ceiling(y - startY + lineHeight);
        }

        private static float MeasureSpaceWidth(
            Graphics graphics,
            Font font,
            StringFormat format)
        {
            float separatedWidth = graphics.MeasureString("가 가", font, int.MaxValue, format).Width;
            float joinedWidth = graphics.MeasureString("가가", font, int.MaxValue, format).Width;
            return Math.Max(font.Size * 0.35F, separatedWidth - joinedWidth);
        }

        // 지정한 해상도의 흰색 출력 페이지를 만든다.
        private static Bitmap CreateBlankPage(int width, int height, float renderScale)
        {
            Bitmap page = new Bitmap(
                (int)Math.Round(width * renderScale),
                (int)Math.Round(height * renderScale));
            // 좌표계는 Graphics.ScaleTransform으로만 확대한다.
            // DPI까지 함께 높이면 포인트 단위 글꼴이 중복 확대되어 제목과 본문이 잘린다.
            const float logicalResolution = 150F;
            page.SetResolution(logicalResolution, logicalResolution);
            using Graphics graphics = Graphics.FromImage(page);
            graphics.Clear(Color.White);
            return page;
        }

        // 페이지 크기는 유지하면서 안티앨리어싱과 고품질 보간을 공통 적용한다.
        private static Graphics CreatePageGraphics(Bitmap page, float renderScale)
        {
            Graphics graphics = Graphics.FromImage(page);
            graphics.ScaleTransform(renderScale, renderScale);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            return graphics;
        }

        // 모든 페이지에 공통으로 사용하는 제목 띠와 부제목을 그린다.
        private static void DrawHeader(Graphics graphics, string title, string subtitle)
        {
            using SolidBrush headerBrush = new SolidBrush(AppTheme.DarkAccent);
            using SolidBrush titleBrush = new SolidBrush(AppTheme.Accent);
            using Font titleFont = new Font("맑은 고딕", 25F, FontStyle.Bold);
            using Font subtitleFont = new Font("맑은 고딕", 15F, FontStyle.Bold);
            graphics.FillRectangle(headerBrush, 0, 0, graphics.VisibleClipBounds.Width, 145);
            graphics.DrawString(title, titleFont, titleBrush, new RectangleF(Margin, 22, graphics.VisibleClipBounds.Width - Margin * 2, 60));
            graphics.DrawString(subtitle, subtitleFont, Brushes.WhiteSmoke, new PointF(Margin, 91));
        }

        // 기존 프로그램 로고는 좌측에, 프로그램 이름과 페이지 번호는 우측에 표시한다.
        private static void DrawFooter(Graphics graphics, int pageNumber)
        {
            using Bitmap logo = new Bitmap(Properties.Resources.가우스텍_배경제거);
            const int logoHeight = 64;
            int logoWidth = Math.Max(1, (int)Math.Round(logo.Width * logoHeight / (double)logo.Height));
            int logoY = (int)graphics.VisibleClipBounds.Height - 72;
            graphics.DrawImage(
                logo,
                new Rectangle(Margin, logoY, logoWidth, logoHeight));

            using Font font = new Font("맑은 고딕", 10F);
            string text = $"Test Cable Connection Manager  |  {pageNumber}";
            SizeF size = graphics.MeasureString(text, font);
            graphics.DrawString(text, font, Brushes.Gray,
                graphics.VisibleClipBounds.Width - Margin - size.Width,
                graphics.VisibleClipBounds.Height - 55);
        }

        // 페이지 비트맵을 JPEG 스트림으로 넣은 최소 PDF 문서를 직접 작성한다.
        public static void Save(string path, IReadOnlyList<Bitmap> pages)
        {
            List<byte[]> images = new List<byte[]>();
            foreach (Bitmap page in pages)
            {
                using MemoryStream imageStream = new MemoryStream();
                SaveHighQualityJpeg(page, imageStream);
                images.Add(imageStream.ToArray());
            }

            using FileStream stream = File.Create(path);
            using BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true);
            List<long> offsets = new List<long> { 0 };
            WriteText(writer, "%PDF-1.4\n");
            int pageCount = pages.Count;
            int objectCount = 2 + pageCount * 3;

            WriteObject(writer, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");
            string kids = string.Join(" ", Enumerable.Range(0, pageCount).Select(i => $"{3 + i * 3} 0 R"));
            WriteObject(writer, offsets, 2, $"<< /Type /Pages /Count {pageCount} /Kids [{kids}] >>");

            for (int index = 0; index < pageCount; index++)
            {
                int pageObject = 3 + index * 3;
                int imageObject = pageObject + 1;
                int contentObject = pageObject + 2;
                bool landscape = pages[index].Width > pages[index].Height;
                int mediaWidth = landscape ? 842 : 595;
                int mediaHeight = landscape ? 595 : 842;
                WriteObject(writer, offsets, pageObject,
                    $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {mediaWidth} {mediaHeight}] /Resources << /XObject << /Im0 {imageObject} 0 R >> >> /Contents {contentObject} 0 R >>");
                WriteStreamObject(writer, offsets, imageObject,
                    $"<< /Type /XObject /Subtype /Image /Width {pages[index].Width} /Height {pages[index].Height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {images[index].Length} >>",
                    images[index]);
                byte[] content = Encoding.ASCII.GetBytes($"q {mediaWidth} 0 0 {mediaHeight} 0 0 cm /Im0 Do Q");
                WriteStreamObject(writer, offsets, contentObject, $"<< /Length {content.Length} >>", content);
            }

            long xref = stream.Position;
            WriteText(writer, $"xref\n0 {objectCount + 1}\n0000000000 65535 f \n");
            for (int i = 1; i <= objectCount; i++)
                WriteText(writer, $"{offsets[i]:D10} 00000 n \n");
            WriteText(writer, $"trailer\n<< /Size {objectCount + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        }

        // 여러 페이지를 저장할 때 파일명 뒤에 연속 번호를 붙여 각각의 이미지로 저장한다.
        public static IReadOnlyList<string> SaveImages(
            string selectedPath,
            IReadOnlyList<Bitmap> pages,
            ImageFormat format)
        {
            string directory = Path.GetDirectoryName(selectedPath) ?? string.Empty;
            string baseName = Path.GetFileNameWithoutExtension(selectedPath);
            string extension = format.Guid == ImageFormat.Jpeg.Guid ? ".jpg" : ".png";
            List<string> savedPaths = new List<string>();

            for (int index = 0; index < pages.Count; index++)
            {
                string path = pages.Count == 1
                    ? Path.Combine(directory, baseName + extension)
                    : Path.Combine(directory, $"{baseName}-{index + 1}{extension}");
                if (format.Guid == ImageFormat.Jpeg.Guid)
                {
                    using FileStream stream = File.Create(path);
                    SaveHighQualityJpeg(pages[index], stream);
                }
                else
                {
                    pages[index].Save(path, format);
                }
                savedPaths.Add(path);
            }

            return savedPaths;
        }

        // 기본 JPEG 압축보다 높은 품질을 사용해 선과 작은 글자의 압축 노이즈를 줄인다.
        private static void SaveHighQualityJpeg(Image image, Stream stream)
        {
            ImageCodecInfo jpegCodec = ImageCodecInfo.GetImageEncoders()
                .First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
            using EncoderParameters parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
            image.Save(stream, jpegCodec, parameters);
        }

        // 일반 PDF 객체의 위치를 기록하고 본문을 쓴다.
        private static void WriteObject(BinaryWriter writer, List<long> offsets, int number, string body)
        {
            offsets.Add(writer.BaseStream.Position);
            WriteText(writer, $"{number} 0 obj\n{body}\nendobj\n");
        }

        // 이미지 및 콘텐츠처럼 바이트 데이터가 포함된 PDF 스트림 객체를 쓴다.
        private static void WriteStreamObject(BinaryWriter writer, List<long> offsets, int number, string dictionary, byte[] data)
        {
            offsets.Add(writer.BaseStream.Position);
            WriteText(writer, $"{number} 0 obj\n{dictionary}\nstream\n");
            writer.Write(data);
            WriteText(writer, "\nendstream\nendobj\n");
        }

        // PDF 문법 문자열은 규격에 맞춰 ASCII 바이트로 기록한다.
        private static void WriteText(BinaryWriter writer, string text) =>
            writer.Write(Encoding.ASCII.GetBytes(text));
    }
}

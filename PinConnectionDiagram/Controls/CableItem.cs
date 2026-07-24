using PinConnectionDiagram.Models;
using PinConnectionDiagram.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PinConnectionDiagram.Controls
{
    /// <summary>
    /// 등록된 케이블을 드래그 가능한 목록 항목으로 표현한다.
    /// </summary>
    public partial class CableItem : UserControl
    {
        public CableInfo Info { get; }

        private Color _categoryColor;
        public CableItem(CableInfo info)
        {
            InitializeComponent();

            RegisterDrag(this);

            Info = info;

            LblCableName.Text = CableDisplayHelper.GetDisplayName(info);
            Height = CableDisplayHelper.GetItemHeight(info);
            AdjustWidthToCableName();

            ApplyTheme();

            // 카테고리 색상은 커넥터 이미지 영역과 상·하단 강조선에만 사용한다.
            TlpCableItem.BackColor = Color.White;
            LblCableName.ForeColor = Color.Black;
        }

        private void AdjustWidthToCableName()
        {
            // 목록 항목도 배치된 DropItem과 동일한 너비 계산을 사용한다.
            // 시험 대상 케이블만 120px로 고정하면 큰 글꼴에서 명칭이
            // 의도하지 않게 줄바꿈되므로 모든 카테고리를 동적으로 맞춘다.
            Width = CableDisplayHelper.GetItemWidth(Info, LblCableName.Font);
        }

        /// <summary>
        /// 현재 테마의 카테고리 색상을 이미 생성된 항목에도 다시 적용한다.
        /// </summary>
        public void ApplyTheme()
        {
            _categoryColor = ColorHelper.GetMapHeaderColor(Info.Category);
            PbConnector.BackColor = _categoryColor;
            TlpCableItem.Invalidate();
        }

        private void TlpCableItem_Paint(object sender, PaintEventArgs e)
        {
            if (sender is not TableLayoutPanel tlp)
                return;

            DrawHelper.DrawGlowLine(
                e.Graphics,
                tlp.Width,
                0,
                2,
                _categoryColor);

            DrawHelper.DrawGlowLine(
                e.Graphics,
                tlp.Width,
                tlp.Height - 2,
                2,
                _categoryColor);
        }

        private void RegisterDrag(Control control)
        {
            // 자식 컨트롤 위에서 눌러도 항목 전체가 드래그되도록 재귀 등록한다.
            control.MouseDown += CableItem_MouseDown;

            foreach (Control child in control.Controls)
            {
                RegisterDrag(child);
            }
        }

        private void CableItem_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            // 원본 항목은 유지하고 배치 영역에는 복사본을 생성한다.
            using Bitmap previewImage = new Bitmap(Width, Height);
            DrawToBitmap(previewImage, new Rectangle(Point.Empty, Size));
            DataObject dragData = new DataObject();
            dragData.SetData(typeof(CableItem), this);

            // 탐색기와 같은 네이티브 드래그 이미지는 별도 창보다 커서 추적이 훨씬 부드럽다.
            if (NativeDragImageHelper.TryInitialize(dragData, previewImage, 0.7D))
            {
                DoDragDrop(dragData, DragDropEffects.Copy);
                return;
            }

            // Shell 기능을 사용할 수 없는 환경에서도 드래그 이미지가 사라지지 않게 예비 미리보기를 사용한다.
            using DragPreviewForm preview = new DragPreviewForm(previewImage);
            GiveFeedbackEventHandler movePreview = (_, feedbackEvent) =>
            {
                preview.MoveNearCursor();
                feedbackEvent.UseDefaultCursors = true;
            };

            GiveFeedback += movePreview;
            try
            {
                preview.Show();
                preview.MoveNearCursor();
                DoDragDrop(dragData, DragDropEffects.Copy);
            }
            finally
            {
                GiveFeedback -= movePreview;
                preview.Close();
            }
        }
    }
}

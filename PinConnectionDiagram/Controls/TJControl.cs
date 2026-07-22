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
    /// 개별 TJ의 활성 상태를 표시하고 사용자 토글 요청을 관리자에 전달한다.
    /// </summary>
    public partial class TJControl : UserControl
    {
        public int TJNumber { get; }
        public event Action<TJControl, bool>? StateChanged;
        public bool isOn;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsOn
        {
            get => isOn;
            set
            {
                if (isOn == value)
                    return;

                isOn = value;

                UpdateUI();
            }
        }
        public TJControl(int number)
        {
            InitializeComponent();

            TJNumber = number;

            BtnTJ.Text = $"TJ{number}";

            BtnTJ.FlatAppearance.MouseOverBackColor = Color.Transparent;
            BtnTJ.FlatAppearance.MouseDownBackColor = Color.Transparent;

            BtnTJ.UseVisualStyleBackColor = false;

            UpdateUI();
        }


        private void UpdateUI()
        {
            BtnTJ.BackgroundImage = IsOn
                ? AppTheme.GetImage("TJ_on2", "TJ_defense_on")
                : AppTheme.GetImage("TJ_off", "TJ_defense_off");
            BtnTJ.BackgroundImageLayout = ImageLayout.Stretch;

            BtnTJ.ForeColor = IsOn
       ? Color.Black
       : Color.White;   // 원하는 색상으로 변경

        }

        public void ApplyTheme() => UpdateUI();

        private void BtnTJ_Click(object sender, EventArgs e)
        {
            IsOn = !IsOn;

            // 초기화 경고와 패널 활성화 여부는 MapManager에서 결정한다.
            StateChanged?.Invoke(this, IsOn);
        }
    }
}

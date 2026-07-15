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
            BtnTJ.BackgroundImage = IsOn ? Properties.Resources.TJ_on2 : Properties.Resources.TJ_off;
            BtnTJ.BackgroundImageLayout = ImageLayout.Stretch;

            BtnTJ.ForeColor = IsOn
       ? Color.Black
       : Color.White;   // 원하는 색상으로 변경

        }

        private void BtnTJ_Click(object sender, EventArgs e)
        {
            IsOn = !IsOn;

            StateChanged?.Invoke(this, IsOn);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PinConnectionDiagram.Models;

namespace PinConnectionDiagram
{
    public partial class AddCableForm : Form
    {
        public CableInfo CableInfo { get; private set; }

        public AddCableForm()
        {
            InitializeComponent();

            ButtonHelper.ApplyButtonEffect(
                btnOk,
                Properties.Resources.Button,
                Properties.Resources.Button_push);

            ButtonHelper.ApplyButtonEffect(
                btnCancel,
                Properties.Resources.Button,
                Properties.Resources.Button_push);

            ButtonHelper.CancelButtonFunction(btnCancel, this);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if ((cbCategory.Text == "") ||
                (txtName.Text == "") ||
                (numCount.Value <= 0))
            {
                MessageBox.Show("항목을 모두 입력해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            CableInfo = new CableInfo
            {
                Category = cbCategory.Text,
                Name = txtName.Text,
                Count = (int)numCount.Value
            };

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

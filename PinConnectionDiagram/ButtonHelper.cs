using PinConnectionDiagram.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PinConnectionDiagram
{
    public static class ButtonHelper
    {
        public static void ApplyButtonEffect(
            Button btn,
            Image normal_image,
            Image pressed_image)
        {
            btn.FlatAppearance.MouseOverBackColor = btn.BackColor;
            btn.FlatAppearance.MouseDownBackColor = btn.BackColor;

            Padding orgMargin = btn.Margin;

            btn.BackgroundImage = normal_image;

            btn.MouseEnter += (s, e) =>
            {
                btn.BackgroundImage = pressed_image;
            };

            btn.MouseLeave += (s, e) =>
            {
                btn.BackgroundImage = normal_image;
            };
            btn.MouseDown += (s, e) => {
                btn.Width -= 4;
                btn.Height -= 4;
                btn.Margin = new Padding(
                    orgMargin.Left + 2,
                    orgMargin.Top + 2,
                    18,
                    0);
            };
            btn.MouseUp += (s, e) => {
                btn.Width += 4;
                btn.Height += 4;
                btn.Margin = new Padding(
                    orgMargin.Left,
                    orgMargin.Top,
                    15,
                    0);
            };
        }

        public static void CancelButtonFunction(Button btn, Form form)
        {
            btn.Click += (s, e) =>
            {
                form.Close();
            };
        }
    }
}

using PinConnectionDiagram.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PinConnectionDiagram.Helpers
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

            btn.BackgroundImage = normal_image;

            //Padding orgMargin = btn.Margin;

            btn.MouseEnter += (s, e) =>
            {
                btn.BackgroundImage = pressed_image;
            };

            btn.MouseLeave += (s, e) =>
            {
                btn.BackgroundImage = normal_image;
            };
        }

        public static void CancelButtonFunction(Button btn, Form form)
        {
            form.CancelButton = btn;
            btn.Click += (s, e) =>
            {
                form.Close();
            };
        }
    }
}

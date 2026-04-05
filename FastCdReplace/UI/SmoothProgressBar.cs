using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FastCdReplace.UI
{
    public class SmoothProgressBar : Control
    {
        private int _value;

        [Category("Behavior")]
        [Description("Значение прогресса (0-100)")]
        public int Value
        {
            get => _value;
            set
            {
                int newValue = Math.Max(0, Math.Min(100, value));
                if (_value != newValue)
                {
                    _value = newValue;
                    Invalidate();
                }
            }
        }

        public SmoothProgressBar()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;

            BackColor = Color.WhiteSmoke;
            ForeColor = Color.LightGreen;

            Height = 28;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var rect = ClientRectangle;

            using (var backBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(backBrush, rect);
            }

            int fillWidth = (int)(rect.Width * (_value / 100.0));

            if (fillWidth > 0)
            {
                using var fillBrush = new SolidBrush(Color.FromArgb(144, 238, 144));
                g.FillRectangle(fillBrush, 0, 0, fillWidth, rect.Height);
            }

            using (var borderPen = new Pen(Color.Gray))
            {
                g.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
            }
        }
    }
}
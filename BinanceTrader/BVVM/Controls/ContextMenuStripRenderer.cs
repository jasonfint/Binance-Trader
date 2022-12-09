using System.Drawing;
using System.Windows.Forms;

namespace BTNET.BVVM
{
    public class ContextMenuStripRenderer : ToolStripRenderer
    {
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, e.AffectedBounds, Color.DarkSlateGray, ButtonBorderStyle.Solid);
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            e.Graphics.FillRectangle(SystemBrushes.WindowText, e.AffectedBounds);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            ToolStripItem item = e.Item;
            Graphics graphics = e.Graphics;
            Color textColor = e.TextColor;
            Font textFont = e.TextFont;
            string text = e.Text;
            Rectangle textRectangle = e.TextRectangle;
            TextFormatFlags textFormat = e.TextFormat;
            textColor = (item.Enabled ? textColor : SystemColors.GrayText);

            if (item.Selected)
            {
                TextRenderer.DrawText(graphics, text, textFont, textRectangle, Color.SteelBlue, textFormat);
                ControlPaint.DrawBorder(e.Graphics, item.ContentRectangle, Color.DodgerBlue, ButtonBorderStyle.Solid);
            }
            else
            {
                TextRenderer.DrawText(graphics, text, textFont, textRectangle, textColor, textFormat);
            }
        }
    }
}

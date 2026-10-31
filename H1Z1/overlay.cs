namespace Overlay;
public partial class Box : Form
{
    private readonly List<Rectangle> _rectangles = new();
    public Box(int width, int height)
    {
        Size = new Size(width,height);
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;
        SetTopLevel(true);
        TopMost = true;
        //FormBorderStyle = FormBorderStyle.None;
    }

    public void AddRectangle(int x, int y, int w, int h)
    {
        _rectangles.Add(new Rectangle(x, y, w, h));
        Invalidate();
    }

    public void ClearRectangles()
    {
        _rectangles.Clear();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(TransparencyKey);

        foreach (var rect in _rectangles)
        {
            e.Graphics.DrawRectangle(Pens.Red, rect);
        }
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        
    }

}

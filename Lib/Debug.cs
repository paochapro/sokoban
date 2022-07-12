using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Sokoban;
using static Utils;

class DebugLines : Group<DebugLine>
{ }

//Debug
class DebugLine : Entity
{
    public Vector2 p1;
    public Vector2 p2;
    public Color color;
    bool showCounter;
    string? customText;

    public DebugLine(Vector2 p1, Vector2 p2, Color color, bool showCounter = true, string? customText = null)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.color = color;
        this.showCounter = showCounter;
        this.customText = customText;
    }
    public DebugLine(Vector2 p1, Vector2 p2)
        : this(p1, p2, Color.Red)
    { }
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!MyGame.Debug) return;

        Vector2 p1 = this.p1 - MyGame.Camera;
        Vector2 p2 = this.p2 - MyGame.Camera;

        //Line
        spriteBatch.DrawLine(p1, p2, color, 1);

        //Text
        if (!showCounter) return;

        float length = p1.Y - p2.Y;
        float textHeight = UI.Font.MeasureString(length.ToString()).Y;
        Vector2 textPos = new(p1.X + 15, center(p1.Y, p2.Y, textHeight));
        spriteBatch.DrawString(UI.Font, customText ?? length.ToString(), textPos, color);

        //Endings
        Vector2 endingP1 = new(p1.X - 4, p1.Y);
        Vector2 endingP2 = new(p1.X + 4, p1.Y);
        spriteBatch.DrawLine(endingP1, endingP2, color, 1);
        endingP1 = new(p2.X - 4, p2.Y);
        endingP2 = new(p2.X + 4, p2.Y);
        spriteBatch.DrawLine(endingP1, endingP2, color, 1);
    }
    public override void Destroy() => DebugLines.Destroy(groupID);
    public override void Update(GameTime gameTime) { }
}
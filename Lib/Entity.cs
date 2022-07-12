using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Sokoban;

class Entities : Group<Entity>
{
    static new public Entity Add(Entity ent)
    {
        ent.entityID = lastGroupID++;
        group.Add(ent);
        return ent;
    }
    static new public void Destroy(int id)
    {
        if (group.Count == 0) return;

        group.RemoveAt(id);
        lastGroupID--;

        //UpdateIDs
        foreach (Entity ent in group)
        {
            if (ent.entityID > id)
                ent.entityID--;
        }
    }
    static new public void Destroy(Entity entity)
    {
        Destroy(entity.entityID);
    }
}

//Entity
abstract class Entity
{
    public int entityID { get; set; }
    public int groupID { get; set; }

    protected RectangleF rectangle;
    protected Texture2D texture;
    public RectangleF Rect => rectangle;
    public Texture2D Texture => texture;

    public Entity(RectangleF rectangle, Texture2D texture)
    {
        this.rectangle = rectangle;
        this.texture = texture;
    }

    public Entity() : this(new RectangleF(0, 0, 0, 0), null) { }

    public virtual void Destroy() => Entities.Destroy(entityID);
    public virtual void OnTouch() { }

    public abstract void Update(GameTime gameTime);

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        Rectangle final = (Rectangle)rectangle;
        final.Location -= MyGame.Camera.ToPoint();

        spriteBatch.Draw(texture, final, Color.White);
    }
}

class Group<T> where T : Entity
{
    static protected List<T> group = new List<T>();
    static protected int lastGroupID = 0;

    static public int Count => group.Count;

    static public T Get(int i) => group[i];

    //General
    static public void Update(GameTime gameTime)
    {
        for (int i = 0; i < group.Count; ++i)
            group[i].Update(gameTime);
    }
    static public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < group.Count; ++i)
            group[i].Draw(spriteBatch);
    }
    static public void Add(T ent)
    {
        ent.groupID = lastGroupID++;
        group.Add(ent);
        Entities.Add(ent);
    }

    protected Group() { }

    //Destroy
    static public void Destroy(int id)
    {
        if (group.Count == 0) return;
        Entities.Destroy(group[id].entityID);

        group.RemoveAt(id);
        lastGroupID--;

        //UpdateIDs
        foreach (T ent in group)
        {
            if (ent.groupID > id)
                ent.groupID--;
        }
    }
    static public void Destroy(Entity entity)
    {
        Destroy(entity.groupID);
    }
    //Clear
    static public void Clear()
    {
        group.ForEach(e => Entities.Destroy(e));
        group.Clear();
        lastGroupID = 0;
    }
}
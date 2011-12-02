using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.SamplesFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameState
{
    public partial class Level
    {
        /// <summary>
        /// The name of the level.
        /// </summary>
        [XmlAttribute()]
        public String Name;

        [XmlAttribute()]
        public bool Visible;

        /// <summary>
        /// A Level contains several Layers. Each Layer contains several Items.
        /// </summary>
        public List<Layer> Layers;

        /// <summary>
        /// A Dictionary containing any user-defined Properties.
        /// </summary>
        public SerializableDictionary CustomProperties;

        public Level()
        {
            Visible = true;
            Layers = new List<Layer>();
            CustomProperties = new SerializableDictionary();
        }

        public static Level FromFile(string filename, ContentManager cm)
        {
            FileStream stream = File.Open(filename, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(Level));
            Level level = (Level)serializer.Deserialize(stream);
            stream.Close();

            foreach (Layer layer in level.Layers)
            {
                //CollisionCategory 2
                if (layer.Name == "Main")
                {
                    processMainLayer(layer.Items, cm);
                }

                //CollisionCategory 3
                if (layer.Name == "DynamicObjects")
                {
                    processDynamicObjectsLayer(layer.Items, cm);
                }

                //CollisionCategory 1
                if (layer.Name == "Waypoints")
                {
                    processWaypoints(layer.Items, cm);
                }

                //CollisionCategory None
                if (layer.Name == "Background")
                {
                    processBackgroundLayer(layer.Items, cm);
                }
            }

            return level;
        }

        private static void processBackgroundLayer(List<Item> list, ContentManager cm)
        {
            foreach (Item it in list)
            {
                if (it.GetType() == typeof(TextureItem))
                {
                    TextureItem tItem = (TextureItem)it;

                    tItem.load(cm);
                }

            }
        }

        private static void processWaypoints(List<Item> list, ContentManager content)
        {
            //for now use regex matching for death waypoints.  Lets us put in more than 1 death waypoint.
            Regex deathRegex = new Regex("[Dd]eath+");
            
            foreach (Item it in list)
            {
                Match deathMatch = deathRegex.Match(it.Name);
                if (it.Name == "Hero")
                {
                    CircleItem cItem = (CircleItem)it;

                    Vector2 startPosition;
                    startPosition.X = cItem.Position.X + (cItem.Radius / 2);
                    startPosition.Y = cItem.Position.Y + (cItem.Radius / 2);
                    GameplayScreen._hero.setWorldPosition(startPosition);
                }

                else if (deathMatch.Success)
                {
                    RectangleItem rItem = (RectangleItem)it;

                    Vector2 worldPosition;
                    worldPosition.X = rItem.Position.X + (rItem.Width / 2);
                    worldPosition.Y = rItem.Position.Y + (rItem.Height / 2);

                    Body deathBody = BodyFactory.CreateBody(GameplayScreen.getWorld(), ConvertUnits.ToSimUnits(worldPosition));
                    FixtureFactory.AttachRectangle(ConvertUnits.ToSimUnits(rItem.Width), ConvertUnits.ToSimUnits(rItem.Height), 1f, new Vector2(0f), deathBody);
                    deathBody.IgnoreCCD = true;
                    deathBody.CollisionCategories = Category.Cat1;
                    deathBody.OnCollision += new OnCollisionEventHandler(GameplayScreen._hero.die);
                }

                else if (it.Name == "Exit")
                {
                    RectangleItem rItem = (RectangleItem)it;

                    Vector2 worldPosition;
                    worldPosition.X = rItem.Position.X + (rItem.Width / 2);
                    worldPosition.Y = rItem.Position.Y + (rItem.Height / 2);

                    Body exitBody = BodyFactory.CreateBody(GameplayScreen.getWorld(), ConvertUnits.ToSimUnits(worldPosition));
                    exitBody.IgnoreCCD = false;
                    exitBody.CollisionCategories = Category.Cat1;
                    exitBody.IsSensor = true;
                    FixtureFactory.AttachRectangle(ConvertUnits.ToSimUnits(rItem.Width), ConvertUnits.ToSimUnits(rItem.Height), 1f, new Vector2(0f), exitBody);

                    exitBody.OnCollision += new OnCollisionEventHandler(GameplayScreen.getInstance().loadNextLevelHandler);
                }
            }
        }

        // Interactive TextureItems are passed in the Objects layer
        private static void processDynamicObjectsLayer(List<Item> list, ContentManager content)
        {
            foreach (Item it in list)
            {
                if (it.GetType() == typeof(TextureItem))
                {
                    double damagePerformed;
                    double strength;
                    if (it.CustomProperties.ContainsKey("damage"))
                    {
                        damagePerformed = Double.Parse(it.CustomProperties["damage"].description);
                    }
                    if (it.CustomProperties.ContainsKey("strength"))
                    {
                        strength = Double.Parse(it.CustomProperties["strength"].description);
                        //do something with it.
                    }

                    TextureItem tItem = (TextureItem)it;
                    String textureName = tItem.asset_name;
                    //tItem.
                    Vector2 worldPosition = tItem.Position + tItem.Origin;
                    Vector2 textureOrigin;

                    //load texture that will represent the physics body

                    Texture2D polygonTexture = content.Load<Texture2D>(textureName);
   
                    //Create an array to hold the data from the texture
                    uint[] data = new uint[polygonTexture.Width * polygonTexture.Height];
   
                    //Transfer the texture data to the array
                    polygonTexture.GetData(data);
  
                    //Find the vertices that makes up the outline of the shape in the texture
                    Vertices textureVertices = PolygonTools.CreatePolygon(data, polygonTexture.Width, false);

                    //The tool return vertices as they were found in the texture.
                    //We need to find the real center (centroid) of the vertices for 2 reasons:

                    //1. To translate the vertices so the polygon is centered around the centroid.
                    Vector2 centroid = -textureVertices.GetCentroid();
                    textureVertices.Translate(ref centroid);

                    //2. To draw the texture the correct place.
                    textureOrigin = -centroid;

                    //We simplify the vertices found in the texture.
                    textureVertices = SimplifyTools.ReduceByDistance(textureVertices, 4f);

                    //Since it is a concave polygon, we need to partition it into several smaller convex polygons
                    List<Vertices> listOfVertices = BayazitDecomposer.ConvexPartition(textureVertices);

                    //Adjust the scale of the object for WP7's lower resolution
#if WINDOWS_PHONE
            _scale = 0.6f;
#endif

                    //scale the vertices from graphics space to sim space
                    Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * tItem.Scale;
                    foreach (Vertices vertices in listOfVertices)
                    {
                        vertices.Scale(ref vertScale);
                    }

                    //Create a single body with multiple fixtures
                    //BreakableBody _compoundbb = BodyFactory.CreateBreakableBody(GameplayScreen.getWorld(), textureVertices, 1f, ConvertUnits.ToSimUnits(worldPosition));
                    Body _compound = BodyFactory.CreateCompoundPolygon(GameplayScreen.getWorld(), listOfVertices, 1f, ConvertUnits.ToSimUnits(worldPosition));
                    _compound.BodyType = BodyType.Dynamic;
                    _compound.IgnoreCCD = false;
                    _compound.CollisionCategories = Category.Cat3;

                    
                    
                    tItem.addBody(_compound);
                    tItem.load(content);
                }

            }
        }

        // Collision items are passed in the Objects Layer
        private static void processMainLayer(List<Item> list, ContentManager content)
        {
            foreach (Item it in list)
            {
                if (it.GetType() == typeof(RectangleItem))
                {
                    RectangleItem rItem = (RectangleItem)it;
                    float width = rItem.Width;
                    float height = rItem.Height;
                    Vector2 position;
                    position.X = rItem.Position.X + (width / 2);
                    position.Y = rItem.Position.Y + (height / 2);

                    Body collisionBody = BodyFactory.CreateBody(GameplayScreen.getWorld(), ConvertUnits.ToSimUnits(position), rItem);
                    collisionBody.BodyType = BodyType.Static;
                    collisionBody.IgnoreCCD = true;
                    collisionBody.CollisionCategories = Category.Cat2;


                    Fixture rectangleFixture = FixtureFactory.AttachRectangle(ConvertUnits.ToSimUnits(width), ConvertUnits.ToSimUnits(height), 1f, new Vector2(0f, 0f), collisionBody);
                    rectangleFixture.CollisionCategories = Category.Cat2;

                    //Find textureItem to draw into this RectangleItem
                    if (rItem.CustomProperties.ContainsKey("textureItem"))
                    {
                        if (rItem.CustomProperties["textureItem"].description != null)
                        {
                            String textureName = rItem.CustomProperties["textureItem"].description;
                            //Item t = Level.getItemByName(textureName);
                            
                        }
                    }
                }
                else if (it.GetType() == typeof(CircleItem))
                {
                    CircleItem cItem = (CircleItem)it;
                    float radius = cItem.Radius;
                    Vector2 position;
                    position.X = cItem.Position.X + (radius / 2);
                    position.Y = cItem.Position.Y + (radius / 2);

                    Body collisionBody = BodyFactory.CreateBody(GameplayScreen.getWorld(), ConvertUnits.ToSimUnits(position), cItem);
                    collisionBody.BodyType = BodyType.Static;
                    collisionBody.IgnoreCCD = false;

                    Fixture circleFixture = FixtureFactory.AttachCircle(ConvertUnits.ToSimUnits(radius), 1f, collisionBody);
                    circleFixture.CollisionCategories = Category.Cat2;
                }
                else if (it.GetType() == typeof(PathItem))
                {
                    //Implement PathItem
                }
                else if (it.GetType() == typeof(TextureItem))
                {
                    TextureItem tItem = (TextureItem)it;
                    String textureName = tItem.asset_name;
                    Vector2 worldPosition = tItem.Position;
                    Vector2 textureOrigin;

                    //load texture that will represent the physics body

                    Texture2D polygonTexture = content.Load<Texture2D>(textureName);

                    //Create an array to hold the data from the texture
                    uint[] data = new uint[polygonTexture.Width * polygonTexture.Height];

                    //Transfer the texture data to the array
                    polygonTexture.GetData(data);

                    //Find the vertices that makes up the outline of the shape in the texture
                    Vertices textureVertices = PolygonTools.CreatePolygon(data, polygonTexture.Width, false);

                    //The tool return vertices as they were found in the texture.
                    //We need to find the real center (centroid) of the vertices for 2 reasons:

                    //1. To translate the vertices so the polygon is centered around the centroid.
                    Vector2 centroid = -textureVertices.GetCentroid();
                    textureVertices.Translate(ref centroid);

                    //2. To draw the texture the correct place.
                    textureOrigin = -centroid;

                    //We simplify the vertices found in the texture.
                    textureVertices = SimplifyTools.ReduceByDistance(textureVertices, 4f);

                    //Since it is a concave polygon, we need to partition it into several smaller convex polygons
                    List<Vertices> listOfVertices = BayazitDecomposer.ConvexPartition(textureVertices);

                    //Adjust the scale of the object for WP7's lower resolution
#if WINDOWS_PHONE
            _scale = 0.6f;
#endif

                    //scale the vertices from graphics space to sim space
                    Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1)) * tItem.Scale;
                    foreach (Vertices vertices in listOfVertices)
                    {
                        vertices.Scale(ref vertScale);
                    }

                    //Create a single body with multiple fixtures
                    Body _compound = BodyFactory.CreateCompoundPolygon(GameplayScreen.getWorld(), listOfVertices, 1f, ConvertUnits.ToSimUnits(worldPosition));
                    _compound.BodyType = BodyType.Static;
                    _compound.Mass = 100f;
                    _compound.IgnoreCCD = false;
                    _compound.CollisionCategories = Category.Cat2;
                    tItem.addBody(_compound);

                    if (it.CustomProperties.ContainsKey("damage"))
                    {
                        double damagePerformed = Double.Parse(it.CustomProperties["damage"].description);
                        _compound.OnCollision += new OnCollisionEventHandler(GameplayScreen._hero.take10Damage);
                    }
                    if (it.CustomProperties.ContainsKey("strength"))
                    {
                        double strength = Double.Parse(it.CustomProperties["strength"].description);
                        //do something with it.
                    }

                    
                }
                it.load(content);
            }
        }

        public Item getItemByName(string name)
        {
            foreach (Layer layer in Layers)
            {
                foreach (Item item in layer.Items)
                {
                    if (item.Name == name) return item;
                }
            }
            return null;
        }

        public Layer getLayerByName(string name)
        {
            foreach (Layer layer in Layers)
            {
                if (layer.Name == name) return layer;
            }
            return null;
        }

        public void draw(SpriteBatch sb)
        {
            foreach (Layer layer in Layers) layer.draw(sb);
        }
    }


    public partial class Layer
    {
        /// <summary>
        /// The name of the layer.
        /// </summary>
        [XmlAttribute()]
        public String Name;

        /// <summary>
        /// Should this layer be visible?
        /// </summary>
        [XmlAttribute()]
        public bool Visible;

        /// <summary>
        /// The list of the items in this layer.
        /// </summary>
        public List<Item> Items;

        /// <summary>
        /// The Scroll Speed relative to the main camera. The X and Y components are 
        /// interpreted as factors, so (1;1) means the same scrolling speed as the main camera.
        /// Enables parallax scrolling.
        /// </summary>
        public Vector2 ScrollSpeed;

        public Matrix transform;

        public Layer()
        {
            Items = new List<Item>();
            ScrollSpeed = Vector2.One;
        }

        public void draw(SpriteBatch sb)
        {
            if (!Visible) return;
            // enable parallax if layer scrollspeed is different than Vector2.One
            if (!this.ScrollSpeed.Equals(Vector2.One))
                this.updateTransform();
            foreach (Item item in Items) item.draw(sb);
        }

        public void updateTransform()
        {
            
        }

    }


    [XmlInclude(typeof(TextureItem))]
    [XmlInclude(typeof(RectangleItem))]
    [XmlInclude(typeof(CircleItem))]
    [XmlInclude(typeof(PathItem))]
    public partial class Item
    {
        /// <summary>
        /// The name of this item.
        /// </summary>
        [XmlAttribute()]
        public String Name;

        /// <summary>
        /// Should this item be visible?
        /// </summary>
        [XmlAttribute()]
        public bool Visible;

        /// <summary>
        /// The item's position in world space.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// A Dictionary containing any user-defined Properties.
        /// </summary>
        public SerializableDictionary CustomProperties;


        public Item()
        {
            CustomProperties = new SerializableDictionary();
        }

        /// <summary>
        /// Called by Level.FromFile(filename) on each Item after the deserialization process.
        /// Should be overriden and can be used to load anything needed by the Item (e.g. a texture).
        /// </summary>
        public virtual void load(ContentManager cm)
        {
        }

        public virtual void draw(SpriteBatch sb)
        {
        }
    }


    public partial class TextureItem : Item
    {
        /// <summary>
        /// The item's rotation in radians.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The item's scale vector.
        /// </summary>
        public Vector2 Scale;

        /// <summary>
        /// The color to tint the item's texture with (use white for no tint).
        /// </summary>
        public Color TintColor;

        /// <summary>
        /// If true, the texture is flipped horizontally when drawn.
        /// </summary>
        public bool FlipHorizontally;

        /// <summary>
        /// If true, the texture is flipped vertically when drawn.
        /// </summary>
        public bool FlipVertically;

        /// <summary>
        /// The path to the texture's filename (including the extension) relative to ContentRootFolder.
        /// </summary>
        public String texture_filename;

        /// <summary>
        /// The texture_filename without extension. For using in Content.Load<Texture2D>().
        /// </summary>
        public String asset_name;

        /// <summary>
        /// The XNA texture to be drawn. Can be loaded either from file (using "texture_filename") 
        /// or via the Content Pipeline (using "asset_name") - then you must ensure that the texture
        /// exists as an asset in your project.
        /// Loading is done in the Item's load() method.
        /// </summary>
        public Texture2D texture;

        Body body;

        /// <summary>
        /// The item's origin relative to the upper left corner of the texture. Usually the middle of the texture.
        /// Used for placing and rotating the texture when drawn.
        /// </summary>
        public Vector2 Origin;

        /// <summary>
        /// The RectangleItem the texture should draw itself on.  This creates a tiling effect.
        /// </summary>
        public Rectangle[] destRectangles;


        public TextureItem()
        {
        }

        public void addBody(Body b)
        {
            this.body = b;
        }

        /// <summary>
        /// Called by Level.FromFile(filename) on each Item after the deserialization process.
        /// Loads all assets needed by the TextureItem, especially the Texture2D.
        /// You must provide your own implementation. However, you can rely on all public fields being
        /// filled by the level deserialization process.
        /// </summary>
        public override void load(ContentManager cm)
        {
            //throw new NotImplementedException();
            
            //TODO: provide your own implementation of how a TextureItem loads its assets
            //for example:
            //this.texture = Texture2D.FromFile(<GraphicsDevice>, texture_filename);
            //or by using the Content Pipeline:
            //this.texture = cm.Load<Texture2D>(asset_name);
            this.texture = cm.Load<Texture2D>(asset_name);
            Origin = new Vector2(texture.Width / 2, texture.Height / 2);
        }


        public override void draw(SpriteBatch sb)
        {
            if (!Visible) return;
            SpriteEffects effects = SpriteEffects.None;
            if (FlipHorizontally) effects |= SpriteEffects.FlipHorizontally;
            if (FlipVertically) effects |= SpriteEffects.FlipVertically;

            if (body != null)
            {
                this.Position = ConvertUnits.ToDisplayUnits(body.Position);
                this.Rotation = body.Rotation;
            }
            sb.Draw(texture, Position, null, TintColor, Rotation, Origin, Scale, effects, 0);
        }
    }


    public partial class RectangleItem : Item
    {
        public float Width;
        public float Height;
        public Color FillColor;
        Body body;
        Fixture fixture;

        public RectangleItem()
        {
        }

        public override void load(ContentManager cm)
        {
            base.load(cm);
            Console.WriteLine("Creating Rectangle Item");
            Vector2 wspace = new Vector2(this.Position.X + (this.Width/2), this.Position.Y + (this.Height/2));
            Console.WriteLine("Body worldspace pos= " + wspace.X + "," + wspace.Y);
            body = BodyFactory.CreateBody(GameplayScreen._world, ConvertUnits.ToSimUnits(wspace), this);
            fixture = FixtureFactory.AttachRectangle(ConvertUnits.ToSimUnits(this.Width), ConvertUnits.ToSimUnits(this.Height), 1f, new Vector2(0, 0), body);
            fixture.CollisionGroup = 1;
            body.IsStatic = true;
            body.BodyType = BodyType.Static;
            body.Friction = 1f;
            body.IgnoreCCD = true;
            Console.WriteLine("created body: " + body.ToString());

            Console.WriteLine(CustomProperties.ToString());
            if (CustomProperties.ContainsKey("breakable"))
            {
                if (CustomProperties["breakable"].description == "true")
                {
                    body.BodyType = BodyType.Dynamic;
                }
            }
        }
    }


    public partial class CircleItem : Item
    {
        public float Radius;
        public Color FillColor;

        public CircleItem()
        {
        }
    }


    public partial class PathItem : Item
    {
        public Vector2[] LocalPoints;
        public Vector2[] WorldPoints;
        public bool IsPolygon;
        public int LineWidth;
        public Color LineColor;

        public PathItem()
        {
        }
    }


    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////
    //
    //    NEEDED FOR SERIALIZATION. YOU SHOULDN'T CHANGE ANYTHING BELOW!
    //
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////


    public class CustomProperty
    {
        public string name;
        public object value;
        public Type type;
        public string description;

        public CustomProperty()
        {
        }

        public CustomProperty(string n, object v, Type t, string d)
        {
            name = n;
            value = v;
            type = t;
            description = d;
        }

        public CustomProperty clone()
        {
            CustomProperty result = new CustomProperty(name, value, type, description);
            return result;
        }
    }


    public class SerializableDictionary : Dictionary<String, CustomProperty>, IXmlSerializable
    {

        public SerializableDictionary()
            : base()
        {

        }

        public SerializableDictionary(SerializableDictionary copyfrom)
            : base(copyfrom)
        {
            string[] keyscopy = new string[Keys.Count];
            Keys.CopyTo(keyscopy, 0);
            foreach (string key in keyscopy)
            {
                this[key] = this[key].clone();
            }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty) return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                CustomProperty cp = new CustomProperty();
                cp.name = reader.GetAttribute("Name");
                cp.description = reader.GetAttribute("Description");

                string type = reader.GetAttribute("Type");
                if (type == "string") cp.type = typeof(string);
                if (type == "bool") cp.type = typeof(bool);
                if (type == "Vector2") cp.type = typeof(Vector2);
                if (type == "Color") cp.type = typeof(Color);
                if (type == "Item") cp.type = typeof(Item);

                if (cp.type == typeof(Item))
                {
                    cp.value = reader.ReadInnerXml();
                    this.Add(cp.name, cp);
                }
                else
                {
                    reader.ReadStartElement("Property");
                    XmlSerializer valueSerializer = new XmlSerializer(cp.type);
                    object obj = valueSerializer.Deserialize(reader);
                    cp.value = Convert.ChangeType(obj, cp.type);
                    this.Add(cp.name, cp);
                    reader.ReadEndElement();
                }

                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (String key in this.Keys)
            {
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("Name", this[key].name);
                if (this[key].type == typeof(string)) writer.WriteAttributeString("Type", "string");
                if (this[key].type == typeof(bool)) writer.WriteAttributeString("Type", "bool");
                if (this[key].type == typeof(Vector2)) writer.WriteAttributeString("Type", "Vector2");
                if (this[key].type == typeof(Color)) writer.WriteAttributeString("Type", "Color");
                if (this[key].type == typeof(Item)) writer.WriteAttributeString("Type", "Item");
                writer.WriteAttributeString("Description", this[key].description);

                if (this[key].type == typeof(Item))
                {
                    Item item = (Item)this[key].value;
                    if (item != null) writer.WriteString(item.Name);
                    else writer.WriteString("$null$");
                }
                else
                {
                    XmlSerializer valueSerializer = new XmlSerializer(this[key].type);
                    valueSerializer.Serialize(writer, this[key].value);
                }
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Must be called after all Items have been deserialized. 
        /// Restores the Item references in CustomProperties of type Item.
        /// </summary>
        public void RestoreItemAssociations(Level level)
        {
            foreach (CustomProperty cp in Values)
            {
                if (cp.type == typeof(Item)) cp.value = level.getItemByName((string)cp.value);
            }
        }


    }
}
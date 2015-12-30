using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace アルティメットバトルオージー

{
    class MenuManager
    {
        private List<Menu> mMenus;

        public MenuManager()
        {
            mMenus = new List<Menu>();
        }

        public void AddMenu(Menu menu)
        {
            mMenus.Add(menu);
            menu.Parent = this;
        }

        public KeyboardState PrevKeyboadState { get; set; }

        public void Update(GameTime gametime)
        {
            foreach (Menu m in mMenus)
            {
                if (m.UpdateEnabled)
                {
                    m.Update(gametime);
                }
            }

        }

        public void Draw(SpriteBatch sb)
        {
            
            foreach (Menu m in mMenus)
            {
                if (m.DrawEnabled)
                {
                    m.Draw(sb);
                }
            }
        }

        public bool AnyActive()
        {
            foreach (Menu m in mMenus)
            {
                if (m.DrawEnabled)
                    return true;
            }
            return false;
        }
    }


    class Menu
    {
        public delegate void MenuItemSelected();
        public delegate void SetFloatValue(float newVal);
        private SpriteBin MenuSprites;
        public const int MenuCenterX = 615;
        public const int TitleY = 75;
        public const int StartElements = 233;
        public const int ElementDelta = 75;
        public bool UpdateEnabled { get; set; }
        public bool DrawEnabled { get; set; }
        public List<ImageSprite> images;
        public List<TextSprite> texts;


        public bool Enabled
        {
            get
            {
                return DrawEnabled && UpdateEnabled;
            }
            set
            {
                DrawEnabled = value;
                UpdateEnabled = value;
            }
        }

        public MenuManager Parent { get; set; }
        private TextSprite Title;
        private int currentElement;
        private List<MenuItem> elements;
        public Texture2D background;
        public Video video;
        public static VideoPlayer videoPlayer = new VideoPlayer();
        public SoundEffect selectSound;



        public Menu(SpriteFont menufont, string title, bool beginEnabled, Video backgroundVideo, SoundEffect selectSound = null, List<ImageSprite> images = null, List<TextSprite> texts = null)
        {
            MediaPlayer.Play(Game1.contentM.Load<Song>("Music/Battle Without Honor or Humanity"));
            this.texts = texts;
            this.images = images;
            this.selectSound = selectSound;
            this.video = backgroundVideo;
            elements = new List<MenuItem>();
            MenuSprites = new SpriteBin(menufont);
            Title = MenuSprites.AddTextSprite(title);
            Title.Position = new Vector2(MenuCenterX, TitleY);
            Enabled = beginEnabled;
        }

        public Menu(SpriteFont menufont, string title, bool beginEnabled, Texture2D background, SoundEffect selectSound = null, List<ImageSprite> images = null, List<TextSprite> texts = null)
        {
            this.texts = texts;
            this.images = null;
            this.selectSound = selectSound;
            this.background = background;
            elements = new List<MenuItem>();
            MenuSprites = new SpriteBin(menufont);
            Title = MenuSprites.AddTextSprite(title);
            Title.Position = new Vector2(MenuCenterX, TitleY);
            Enabled = beginEnabled;
        }

        public Menu(SpriteFont menufont, string title, bool beginEnabled)
        {
            elements = new List<MenuItem>();
            MenuSprites = new SpriteBin(menufont);
            Title = MenuSprites.AddTextSprite(title);
            Title.Position = new Vector2(MenuCenterX, TitleY);
            Enabled = beginEnabled;
        }

        public void AddMenuItem(string text, MenuItemSelected action)
        {
            TextSprite elemSprite = MenuSprites.AddTextSprite(text);
            
            elemSprite.Position = new Vector2(MenuCenterX, elements.Count * ElementDelta + StartElements);

            SelectMenuItem m;

            m = new SelectMenuItem(elemSprite, action, this);
            
            m.HomePosition = elemSprite.Position;
            elements.Add(m);

            if (elements.Count == 1)
            {
                elements[0].Highlight();
            }
        }

        public void AddPlayerSelectMenuItem(Player p, MenuItemSelected action)
        {
            ImageSprite elemSprite = MenuSprites.AddImageSprite(p.icon);

            float posX = (elements.Count % 2) * elemSprite.Size().X + (Game1.gDevice.Viewport.Width / 2) - (elemSprite.Size().X) + 250;
            float posY = (float) Math.Floor((double) elements.Count / 2) * elemSprite.Size().Y + 100;
            elemSprite.Position = new Vector2(posX, posY);

            SelectPlayerMenuItem m;
            m = new SelectPlayerMenuItem(elemSprite, p, action, this);
            m.HomePosition = elemSprite.Position;
            elements.Add(m);

            if (elements.Count == 1)
            {
                elements[0].Highlight();
            }
        }

        public void Update(GameTime gameTime)
        {
            if (video != null)
            {
                if (videoPlayer.State == MediaState.Stopped)
                {
                    videoPlayer.IsLooped = true;
                    videoPlayer.Play(video);
                    videoPlayer.Volume = 0;
                } 
            }

            KeyboardState currentState = Keyboard.GetState();
            if (currentState.IsKeyDown(Keys.Up) && Parent.PrevKeyboadState.IsKeyUp(Keys.Up))
            {
                elements[currentElement].UnHighlight();
                currentElement = currentElement - 1;
                if (currentElement < 0)
                    currentElement = 0;
                elements[currentElement].Highlight();
                
            }

            if (currentState.IsKeyDown(Keys.Down) && Parent.PrevKeyboadState.IsKeyUp(Keys.Down))
            {
                elements[currentElement].UnHighlight();
                currentElement = currentElement + 1;
                if (currentElement >= elements.Count)
                    currentElement = elements.Count - 1;
                elements[currentElement].Highlight();
            }

            if (currentState.IsKeyDown(Keys.N) && Parent.PrevKeyboadState.IsKeyUp(Keys.N))
            {
                if (selectSound != null)
                {
                    selectSound.Play();
                }

                elements[currentElement].Select();
            }
            Parent.PrevKeyboadState = currentState;
        }

        public static void stopMedia()
        {
            Menu.videoPlayer.Stop();
            MediaPlayer.Stop();
        }

        public void Draw(SpriteBatch sb)
        {
            if (background != null)
            {
                sb.Draw(background, Game1.gDevice.Viewport.Bounds, Color.White);
            }
            else if (video != null)
            {
                Texture2D videoTexture = null;
                if (videoPlayer.State != MediaState.Stopped)
                    videoTexture = videoPlayer.GetTexture();
                sb.Draw(videoTexture, Game1.gDevice.Viewport.Bounds, Color.White);
            }


            if (images != null)
            {
                foreach (ImageSprite img in images)
                {
                    img.Draw(sb);
                }
            }

            if (texts != null)
            {
                foreach (TextSprite text in texts)
                {
                    text.Draw(sb);
                }
            }

            foreach (MenuItem m in elements)
            {
                m.update();
                m.draw(sb);
            }


            MenuSprites.Draw(sb);
            
        
        }
    }

    class MenuItem
    {
        public Vector2 HomePosition { get; set; }

        public virtual void Select()
        {

        }

        public virtual void Highlight()
        {

        }

        public virtual void UnHighlight()
        {

        }

        public virtual void Decrease()
        {

        }
        public virtual void Increase()
        {

        }

        public virtual void draw(SpriteBatch sb)
        {

        }

        public virtual void update()
        {

        }
    }

    class SelectMenuItem : MenuItem
    {
        public TextSprite Text { get; set; }
        public Menu.MenuItemSelected Action { get; set; }
        public Menu Parent { get; set; }

        public SelectMenuItem(TextSprite sprite, Menu.MenuItemSelected action, Menu parent)
        {
            this.Text = sprite;
            this.Action = action;
            Text.Color = Color.Red;
            Parent = parent;
            Text.Scale = 1.0f;
        }

        public override void Select()
        {
            if (Action != null)
            {
                Action();
            }
        }

        public override void  Highlight()
        {
            Text.Color = Color.Green;
            Text.Scale = 1.5f;
        }
        public override void UnHighlight()
        {
            Text.Color = Color.Red;
            Text.Scale = 1;
        }
    }



    class SelectPlayerMenuItem : MenuItem
    {
        public ImageSprite image { get; set; }
        public Menu.MenuItemSelected Action { get; set; }
        public Menu Parent { get; set; }
        public Player p;

        // currently focused
        public bool focus;

        public TextSprite playerName;

        public SelectPlayerMenuItem(ImageSprite sprite, Player p, Menu.MenuItemSelected action, Menu parent)
        {
            this.focus = false;
            this.image = sprite;
            this.Action = action;
            Parent = parent;
            this.p = p;
            p.position = new Vector2(300, 400);
            playerName = new TextSprite(p.name, Game1.contentM.
Load<SpriteFont>("Fonts/Bloody"));
            playerName.Position = p.position + new Vector2(0, -300);
            playerName.Scale = 1.5f;
        }

        public override void Select()
        {
            if (Action != null)
            {
                Action();
            }
        }

        public override void Highlight()
        {
            this.focus = true;
            image.Color = Color.Red;
        }
        public override void UnHighlight()
        {
            this.focus = false;
            image.Color = Color.White;
        }

        public override void update()
        {
            p.updateAnimation(WorldData.GetInstance().animations["idle"]);    
        }

        public override void draw(SpriteBatch sb)
        {
            if (this.focus)
            {
                playerName.Draw(sb);
                p.drawPlayer(sb);
            }
        }
    }   

}

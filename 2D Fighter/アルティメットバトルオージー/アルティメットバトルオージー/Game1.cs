using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace アルティメットバトルオージー
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public SpriteFont menuFont;
        SpriteBin Sprites;
        MenuManager mMenus;
        public static ContentManager contentM;
        public static GraphicsDevice gDevice;
        public static Vector2 screenSize;
        public bool quit = false;
        Camera2d cam;

        #region
        ParticleEngine blood;
        //MouseState mouseState, lastMouseState;
        Texture2D particleImage;
        #endregion


        // used for setting up game
        Player selectedPlayer;
        Arena selectedMap;

        World mWorld;

        public String pipelineMode;
        Poser poser;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1368;
            graphics.PreferredBackBufferHeight = 768;
            Content.RootDirectory = "Content";
            screenSize = new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            contentM = this.Content;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            pipelineMode = "mainmenu";

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            menuFont = Content.Load<SpriteFont>("Fonts/Bloody");
            Sprites = new SpriteBin(menuFont);

            gDevice = this.GraphicsDevice;
            cam = new Camera2d(gDevice.Viewport);

            #region
            particleImage = Content.Load<Texture2D>("Art/Particles/blood");
            blood = new ParticleEngine(GraphicsDevice, particleImage, 1150);
            #endregion


            foreach (Player p in WorldData.GetInstance().players.Values)
            {
                p.LoadContent(contentM);
            }

            CreateMenus(menuFont);
        }

        protected void CreateMenus(SpriteFont menuFont)
        {
            Texture2D background = this.Content.Load<Texture2D>("Art/yellowpixel");
            Video backgroundVideo = this.Content.Load<Video>("Videos/fire");
            SoundEffect selectSound = this.Content.Load<SoundEffect>("Sounds/SwordHitSelect");

            List<Menu> menus = new List<Menu>();
            
            ImageSprite logo = new ImageSprite(Content.Load<Texture2D>("Art/battleorgylogo"));
            logo.Position = new Vector2((gDevice.Viewport.Width / 2) - (logo.Size().X / 2), (gDevice.Viewport.Height / 2) - (logo.Size().Y / 2));
            List<ImageSprite> imgs = new List<ImageSprite>();
            List<TextSprite> texts = new List<TextSprite>();
            TextSprite start = new TextSprite("Press N to start", Content.Load<SpriteFont>("Fonts/Bloody"));
            start.Centered = true;
            start.Color = Color.Green;
            start.Position = new Vector2(Game1.gDevice.Viewport.Width / 2, gDevice.Viewport.Height - 30);
            start.Scale = 1.2f;
            texts.Add(start);
            imgs.Add(logo);

            Menu startMenu = new Menu(menuFont, "", true, backgroundVideo, selectSound, imgs, texts);
            menus.Add(startMenu);

            Menu mainMenu = new Menu(menuFont, "", true, backgroundVideo, selectSound);
            menus.Add(mainMenu);


            Menu devTools = new Menu(menuFont, "Dev Tools", false, backgroundVideo);
            menus.Add(devTools);
            Menu confirmQuit = new Menu(menuFont, "Quit?", false, backgroundVideo);
            menus.Add(confirmQuit);
            Menu mapSelect = createMapSelectMenu();
            menus.Add(mapSelect);
            Menu playerSelect = createPlayerSelectMenu(mapSelect, menus);
            menus.Add(playerSelect);

            startMenu.AddMenuItem("", () => {
                foreach (Menu m in menus)
                {
                    if (m != mainMenu)
                    {
                        m.Enabled = false;
                    }
                    else
                    {
                        m.Enabled = true;
                    }
                }

            
            });

            mainMenu.AddMenuItem("Dev Tools", () => { devTools.Enabled = true; confirmQuit.Enabled = false; mainMenu.Enabled = false; });
            
            mainMenu.AddMenuItem("Quit", () => { devTools.Enabled = false;  confirmQuit.Enabled = true; mainMenu.Enabled = false; });
            
            mainMenu.AddMenuItem("Play Game", () => { 
                // disable everything but select player
                foreach (Menu m in menus)
                {
                    if (m != playerSelect)
                    {
                        m.Enabled = false;
                    }
                    else
                    {
                        m.Enabled = true;
                    }
                }
            
            });
         
            confirmQuit.AddMenuItem("Yes", () => { this.Exit(); });
            confirmQuit.AddMenuItem("No", () => { devTools.Enabled = false; confirmQuit.Enabled = false; mainMenu.Enabled = true; });

            devTools.AddMenuItem("Poser", () => { pipelineMode = "poser"; poser = new Poser(this); devTools.Enabled = false; confirmQuit.Enabled = false; mainMenu.Enabled = false; });
            devTools.AddMenuItem("Back", () => { devTools.Enabled = false; confirmQuit.Enabled = false; mainMenu.Enabled = true; });
           
            mMenus = new MenuManager();
            foreach (Menu m in menus)
            {
                mMenus.AddMenu(m);
                if (m == startMenu)
                {
                    m.Enabled = true;
                }
                else
                {
                    m.Enabled = false;
                }
            }
        }

        Menu createMapSelectMenu()
        {
            Video backgroundVideo = this.Content.Load<Video>("Videos/fire");

            SoundEffect selectSound = this.Content.Load<SoundEffect>("Sounds/SwordHitSelect");

            Menu m = new Menu(menuFont, "Select Map", false, backgroundVideo, selectSound);

            foreach (String mapName in WorldData.GetInstance().arenas.Keys)
            {
                m.AddMenuItem(mapName, () => {
                    selectedMap = WorldData.GetInstance().arenas[mapName];
                    startGame();   
                });
            }

            return m;
        }

        Menu createPlayerSelectMenu(Menu nextMenu, List<Menu> menus)
        {
            Video backgroundVideo = this.Content.Load<Video>("Videos/fire");
            SoundEffect selectSound = this.Content.Load<SoundEffect>("Sounds/SwordHitSelect");

            Menu m = new Menu(menuFont, "", false, backgroundVideo);

            foreach (Player p in WorldData.GetInstance().players.Values)
            {
                m.AddPlayerSelectMenuItem(p, () => {
                    
                    foreach (Menu oM in menus)
                    {
                        if (oM != nextMenu)
                        {
                            oM.Enabled = false;
                        }
                        else
                        {
                            oM.Enabled = true;
                        }
                    }

                    this.selectedPlayer = p;
                });
            }
            return m;
        }


        public void startGame()
        {
            Menu.stopMedia();
            pipelineMode = "playgame";
            // setup world instance for game
            List<Player> worldPlayers = new List<Player>();

            Player player1 = Player.deepClonePlayer(selectedPlayer);
            Player player2 = Player.deepClonePlayer(player1);
            player1.mode = Player.Modes.PLAYEYCONTROLLED;
            player2.mode = Player.Modes.DUMMY;
            worldPlayers.Add(player1);
            worldPlayers.Add(player2);
            Arena arena = selectedMap;

            player1.position = new Vector2(arena.arenasize.Width / 2 - 100, arena.arenasize.Height / 2);

            mWorld = new World(worldPlayers, arena, cam, blood);
            cam.mode = Camera2d.Modes.PLAY;
            mWorld.LoadContent(Content);   

        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (quit || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();


            switch (pipelineMode) 
            {
                case "playgame":
                    mWorld.Update(gameTime);
                    blood.Update();
                    break;
                case "mainmenu":
                    mMenus.Update(gameTime);
                    break;
                case "poser":
                    poser.Update(gameTime);                    
                    break;
            }

           
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred,
                      BlendState.AlphaBlend,
                      null,
                      null,
                      null,
                      null,
                      cam.getTransformation(gDevice));

            // TODO: Add your drawing code here

            switch (pipelineMode)
            {
                case "playgame":
                    mWorld.Draw(spriteBatch);
                    break;
                case "mainmenu":
                    mMenus.Draw(spriteBatch);
                    break;
                case "poser":
                    poser.draw(spriteBatch);
                    break;
            }


            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

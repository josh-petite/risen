using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Risen.Client.Graphics;
using Risen.Shared.Models;

namespace Risen.Client
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameMain : Game
    {
        //private readonly ISocketClient _socketClient;
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private readonly LoginModel _loginModelJosh = new LoginModel { Username = "Josh", Password = "Test" };
        private readonly LoginModel _loginModelGordon = new LoginModel { Username = "Gordon", Password = "Test" };
        
        public GameMain()
        {
            //_socketClient = new SocketClient();
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _graphics.GraphicsDevice.Viewport = new Viewport(new Rectangle(0, 0, 640, 480));
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Services.AddService(typeof(SpriteBatch), _spriteBatch);
            Services.AddService(typeof(Screen), new Screen(this));
            Services.AddService(typeof(InputManager), new InputManager(this));

            //_socketClient.Connect();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
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
            var mgr = (InputManager)Services.GetService(typeof(InputManager));

            foreach (var key in mgr.GetNewlyPressedKeys())
                Evalutate(key);

            //_socketClient.Update();

            base.Update(gameTime);
        }

        private void Evalutate(Keys key)
        {
            switch (key)
            {
                //case Keys.F1:
                //    _socketClient.Send(MessageType.Login, JsonConvert.SerializeObject(_loginModelJosh));
                //    break;
                //case Keys.F2:
                //    _socketClient.Send(MessageType.Login, JsonConvert.SerializeObject(_loginModelGordon));
                //    break;
                //default:
                //    _socketClient.Send(MessageType.Unknown, key.ToString());
                //    break;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            base.Draw(gameTime);
            _spriteBatch.End();
        }
    }
}

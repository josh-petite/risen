using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Risen.Client.Tcp;
using Risen.Shared.Enums;
using Risen.Shared.Models;

namespace Risen.Client
{
    public interface IGameMain : IDisposable
    {
        void Run();
    }

    public class GameMain : Game, IGameMain
    {
        private readonly ISocketClient _socketClient;
        private readonly IInputManager _inputManager;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;

        private readonly LoginModel _loginModelJosh = new LoginModel {Username = "Josh", Password = "Test"};
        private readonly LoginModel _loginModelKris = new LoginModel { Username = "Kris", Password = "Test" };
        
        public GameMain(ISocketClient socketClient, IInputManager inputManager)
        {
            _socketClient = socketClient;
            _inputManager = inputManager;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public static int KeepAlivesReceived { get; set; }
        public static string MessageReceived { get; set; }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            KeepAlivesReceived = 0;
            _graphics.GraphicsDevice.Viewport = new Viewport(new Rectangle(0, 0, 320, 240));
            _socketClient.Connect();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("Font");
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
            _inputManager.Update(Keyboard.GetState());
            var newlyPressedKeys = _inputManager.GetNewlyPressedKeys();

            foreach (var key in newlyPressedKeys)
                Evalutate(key);

            _socketClient.Update();

            base.Update(gameTime);
        }

        private void Evalutate(Keys key)
        {
            switch (key)
            {
                case Keys.F1:
                    _socketClient.Send(MessageType.Login, JsonConvert.SerializeObject(_loginModelJosh));
                    break;
                case Keys.F2:
                    _socketClient.Send(MessageType.Login, JsonConvert.SerializeObject(_loginModelKris));
                    break;
                default:
                    _socketClient.Send(MessageType.Unknown, key.ToString());
                    break;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_spriteFont, string.Format("Keep Alive Messages Received: {0}", KeepAlivesReceived), new Vector2(6, 6), Color.Black);
            _spriteBatch.DrawString(_spriteFont, string.Format("Keep Alive Messages Received: {0}", KeepAlivesReceived), new Vector2(5, 5), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            _socketClient.Dispose();
            base.Dispose(disposing);
        }
    }
}

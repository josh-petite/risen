#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using Newtonsoft.Json;
using Risen.Client.Tcp;
using Risen.Shared.Enums;
using Risen.Shared.Models;

#endregion

namespace Risen.Client
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameMain : Game
    {
        private readonly ISocketClient _socketClient;
        private readonly IInputManager _inputManager;
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private readonly LoginModel _loginModelJosh = new LoginModel { Username = "Josh", Password = "Test" };
        private readonly LoginModel _loginModelKris = new LoginModel { Username = "Gordon", Password = "Test" };

        public GameMain()
        {
            _socketClient = new SocketClient();
            _inputManager = new InputManager();
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

            // TODO: use this.Content to load your game content here
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

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}

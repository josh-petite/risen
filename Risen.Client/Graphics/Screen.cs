using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Risen.Client.Graphics
{
    public class Screen : DrawableGameComponent
    {
        private const int Width = 640;
        private const int Height = 480;
        private const int Scale = 2;
        
        private int[] _pixels;
        private int[] _zBuffer;
        private SpriteBatch _spriteBatch;
        private Texture2D _background;

        public Screen(Game game) : base(game)
        {
            game.Components.Add(this);
        }


        Random r = new Random();
        public override void Update(GameTime gameTime)
        {
            //var elapsed = gameTime.ElapsedGameTime.Milliseconds;

            
        }
        
        public override void Draw(GameTime gameTime)
        {
            for (int i = 0; i < Height * Width; i++)
            {
                int xo = (int) (Math.Sin(gameTime.TotalGameTime.Milliseconds - i)%2000/2000.0d*Math.PI*2)*100;
                int yo = (int)(Math.Cos(gameTime.TotalGameTime.Milliseconds - i) % 2000 / 2000.0d * Math.PI * 2) * 60;
                //var result = (r.Next(255) >> 16) + (r.Next(255) >> 8) + (r.Next(255));
                //_pixels[i] = result;
            }

            _background.SetData(_pixels);

            _spriteBatch.Draw(_background, Vector2.Zero, Color.White);
            base.Draw(gameTime);
        }

        public override void Initialize()
        {
            _pixels = new int[Width * Height];
            _background = new Texture2D(Game.GraphicsDevice, Width, Height);
            _spriteBatch = (SpriteBatch) Game.Services.GetService(typeof (SpriteBatch));
        }
    }
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Risen.Client.Graphics
{
    public class Screen : DrawableGameComponent
    {
        private const int Width = 320;
        private const int Height = 240;
        private const int Scale = 2;
        
        private int[] _pixels;
        private int[] _zBuffer;
        private SpriteBatch _spriteBatch;
        private Texture2D _background;

        public Screen(Game game) : base(game) {}
        
        public override void Update(GameTime gameTime)
        {
            var elapsed = gameTime.ElapsedGameTime.Milliseconds;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int xx = (int)Math.Sin(elapsed) % 15;
                    int yy = (int)Math.Cos(elapsed) % 15;
                    _pixels[x*y] =  xx + yy;
                }
            }

            _background.SetData(_pixels);
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_background, Vector2.Zero, Color.TransparentBlack);
            _spriteBatch.End();
        }

        public override void Initialize()
        {
            _pixels = new int[Width * Height];
            _background = new Texture2D(Game.GraphicsDevice, Width, Height);
            _spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }
    }
}

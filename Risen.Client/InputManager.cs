using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Risen.Client
{
    public class InputManager : GameComponent
    {
        private KeyboardState _currentState;
        private KeyboardState _previousState;

        public InputManager(Game game) : base(game)
        {
            game.Components.Add(this);
        }

        public bool KeyWasNewlyPressed(Keys key)
        {
            return _currentState.IsKeyDown(key) && _previousState.IsKeyUp(key);
        }

        public void Update()
        {
            _previousState = _currentState;
            _currentState = Keyboard.GetState();
        }

        public Keys[] GetNewlyPressedKeys()
        {
            return _currentState.GetPressedKeys().Where(KeyWasNewlyPressed).ToArray();
        }
    }
}

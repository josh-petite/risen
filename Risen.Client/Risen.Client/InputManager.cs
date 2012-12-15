using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace Risen.Client
{
    public interface IInputManager
    {
        bool KeyWasNewlyPressed(Keys key);
        void Update(KeyboardState currentState);
        Keys[] GetNewlyPressedKeys();
    }

    public class InputManager : IInputManager
    {
        private KeyboardState _currentState;
        private KeyboardState _previousState;

        public bool KeyWasNewlyPressed(Keys key)
        {
            return _currentState.IsKeyDown(key) && _previousState.IsKeyUp(key);
        }

        public void Update(KeyboardState currentState)
        {
            _previousState = _currentState;
            _currentState = currentState;
        }

        public Keys[] GetNewlyPressedKeys()
        {
            return _currentState.GetPressedKeys().Where(KeyWasNewlyPressed).ToArray();
        }
    }
}

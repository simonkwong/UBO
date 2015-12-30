using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace アルティメットバトルオージー
{
    class KeyboardManager
    {

        public static KeyboardManager GetInstance()
        {
            if (mInstance == null)
            {
                mInstance = new KeyboardManager();
            }
            return mInstance;
        }

        public void Update(GameTime time)
        {
            mPrevState = mCurrentState;
            mCurrentState = Keyboard.GetState();
//            mCurrentState.GetPressedKeys
        }

        public bool IsKeyDown(Keys k)
        {
            return mCurrentState.IsKeyDown(k);
        }

        public bool KeyPressedThisFrame(Keys k)
        {
            return mCurrentState.IsKeyDown(k) && mPrevState.IsKeyUp(k);
        }

        private KeyboardState mPrevState;
        private KeyboardState mCurrentState;
        private static KeyboardManager mInstance;

        private KeyboardManager()
        {

        }



    }
}

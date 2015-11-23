//Measure FrameRate
using Microsoft.Xna.Framework;
using System;

namespace namespace_default
{
    class FrameRate
    {
        public int frameRate;
        int frameCounter;
        TimeSpan elapsedTime;

        public void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            } 
        }
        public void Count()
        {
            frameCounter++;
        }
    }
}

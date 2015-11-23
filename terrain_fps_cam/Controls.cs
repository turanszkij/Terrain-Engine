//This class defines all the adjustment controllers (controls mainly elements of the Enviroment class
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace namespace_default
{
    class Controls
    {
        Game1 Game;

        Texture2D controlBG_tex;
        Rectangle[] controlBG_rec = new Rectangle[2];
        Texture2D[] slider = new Texture2D[2];
        Texture2D checkbox;
        Texture2D acceptbg;
        Texture2D[] compass = new Texture2D[2];
        TextWriter writer;
        public bool show = false;
        bool buttonpressed = false;

        CheckboxOption[] checkitem = new CheckboxOption[11];
        SliderOptionFloat[] floatitem = new SliderOptionFloat[18];
        SliderOptionInt[] intitem = new SliderOptionInt[1];
        AcceptItem accept;
        Compass compassitem;

        public Controls(Game1 newGame)
        {
            Game = newGame;

            controlBG_tex = Game.Content.Load<Texture2D>("controls/controlbg");
            controlBG_rec[0] = new Rectangle(0, Game.device.Viewport.Height - controlBG_tex.Height, controlBG_tex.Width, controlBG_tex.Height);
            controlBG_rec[1] = new Rectangle(Game.device.Viewport.Width - controlBG_tex.Width, Game.device.Viewport.Bounds.Bottom - controlBG_tex.Height, controlBG_tex.Width, controlBG_tex.Height);

            slider[0] = Game.Content.Load<Texture2D>("controls/slider_base");
            slider[1] = Game.Content.Load<Texture2D>("controls/slider_slide");

            acceptbg = Game.Content.Load<Texture2D>("controls/acceptbg");

            checkbox = Game.Content.Load<Texture2D>("controls/checkbox");

            compass[0] = Game.Content.Load<Texture2D>("controls/compass");
            compass[1] = Game.Content.Load<Texture2D>("controls/indicator");

            writer=new TextWriter(Game,"controls/controlfont");

            checkitem[0] = new CheckboxOption("Enable Default Light:", writer, checkbox, new Vector2(10, Game.device.Viewport.Height - 16 * 30), Game.enviro.enableDefaultLighting);
            checkitem[1] = new CheckboxOption("Enable Point Light:", writer, checkbox, new Vector2(10, Game.device.Viewport.Height - 15 * 30), Game.enviro.enablePointLight);
            checkitem[2] = new CheckboxOption("Enable Fog:", writer, checkbox, new Vector2(10, Game.device.Viewport.Height - 10 * 30), Game.enviro.fogEnabled);
            checkitem[3] = new CheckboxOption("FlyMode:", writer, checkbox, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 7 * 30), Game.enviro.FlyMode);
            checkitem[4] = new CheckboxOption("Gray Scale:", writer, checkbox, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 3 * 30), Game.enviro.grayScale);
            checkitem[5] = new CheckboxOption("First Person Mode:", writer, checkbox, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 6 * 30), Game.cam.fpsState);
            checkitem[6] = new CheckboxOption("Smooth Camera:", writer, checkbox, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 5 * 30), Game.enviro.smoothCamera);
            checkitem[7] = new CheckboxOption("Enable Snow:", writer, checkbox, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 9 * 30), Game.enviro.enableSnow);
            checkitem[8] = new CheckboxOption("Enable Rain:", writer, checkbox, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 8 * 30), Game.enviro.enableRain);
            checkitem[9] = new CheckboxOption("Inverted Colors:", writer, checkbox, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 4 * 30), Game.enviro.invertColors);

            floatitem[0] = new SliderOptionFloat("Ambient Light", writer, slider, new Vector2(10, Game.device.Viewport.Height - 14 * 30), Game.enviro.ambient, 0, 1);
            floatitem[1] = new SliderOptionFloat("Light Power", writer, slider, new Vector2(10, Game.device.Viewport.Height - 13 * 30), Game.enviro.lightpower, 0, 10);
            floatitem[2] = new SliderOptionFloat("Light Scatter", writer, slider, new Vector2(10, Game.device.Viewport.Height - 12 * 30), Game.enviro.lightscatter, 0, 100);
            floatitem[3] = new SliderOptionFloat("Grass Fade", writer, slider, new Vector2(10, Game.device.Viewport.Height - 11 * 30), Game.enviro.grassFade, 0, 100);
            floatitem[4] = new SliderOptionFloat("Fog Start", writer, slider, new Vector2(10, Game.device.Viewport.Height - 9 * 30), Game.enviro.fogStart, 0, 200);
            floatitem[5] = new SliderOptionFloat("Fog End", writer, slider, new Vector2(10, Game.device.Viewport.Height - 8 * 30), Game.enviro.fogEnd, 0, 500);
            floatitem[6] = new SliderOptionFloat("Fog Color   R:", writer, slider, new Vector2(10, Game.device.Viewport.Height - 7 * 30), Game.enviro.fogColor.X, 0, 1);
            floatitem[7] = new SliderOptionFloat("            G:", writer, slider, new Vector2(10, Game.device.Viewport.Height - 6 * 30), Game.enviro.fogColor.Y, 0, 1);
            floatitem[8] = new SliderOptionFloat("            B:", writer, slider, new Vector2(10, Game.device.Viewport.Height - 5 * 30), Game.enviro.fogColor.Z, 0, 1);
            
            floatitem[9] = new SliderOptionFloat("Wind Wave Size", writer, slider, new Vector2(10, Game.device.Viewport.Height - 4 * 30), Game.enviro.WindWaveSize, 0, 50);
            floatitem[10] = new SliderOptionFloat("Wind Randomness", writer, slider, new Vector2(10, Game.device.Viewport.Height - 3 * 30), Game.enviro.WindRandomness, 0, 5);
            //floatitem[11] = new SliderOptionFloat("Wind Speed", writer, slider, new Vector2(10, Game.device.Viewport.Height - 2 * 30), Game.enviro.WindSpeed, 0, 100);
            floatitem[11] = new SliderOptionFloat("Wind Amount", writer, slider, new Vector2(10, Game.device.Viewport.Height - 2 * 30), Game.enviro.WindAmount, 0, 1);
            floatitem[12] = new SliderOptionFloat("View Distance", writer, slider, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 15 * 30), Game.enviro.viewDistance, 0, 1000);
            floatitem[13] = new SliderOptionFloat("Field Of View", writer, slider, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 14 * 30), Game.enviro.fieldOfView, 40, 150);
            floatitem[14] = new SliderOptionFloat("Grass Size", writer, slider, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 12 * 30), Game.enviro.grassWidth, 0.5f, 3);
            floatitem[15] = new SliderOptionFloat("Weather Intensity", writer, slider, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 11 * 30), Game.enviro.weatherParticles, 0, 10000);
            floatitem[16] = new SliderOptionFloat("Water Level", writer, slider, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 10 * 30), Game.enviro.waterLevel, 0, 30);
            floatitem[17] = new SliderOptionFloat("Water Wave Height:", writer, slider, new Vector2(10, Game.device.Viewport.Height - 1 * 30), Game.enviro.waveHeight, 0, 3);

            intitem[0] = new SliderOptionInt("Grass Intensity", writer, slider, new Vector2(Game.device.Viewport.Width - 300, Game.device.Viewport.Height - 13 * 30), Game.enviro.grassIntensity, 0, 10);

            accept = new AcceptItem("Accept", writer, acceptbg, new Vector2(Game.device.Viewport.Width-100, Game.device.Viewport.Height - 50));

            compassitem = new Compass(Game, new Vector2(Game.device.Viewport.Width/2-compass[0].Width/2,Game.device.Viewport.Height-compass[0].Height), compass);

        }

        public void Update()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !buttonpressed)
            {
                if (show)
                {
                    show = false;
                }
                else
                    show = true;

                buttonpressed = true;
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.Enter) && buttonpressed)
            {
                buttonpressed = false;
            }

            if (show)
            {
                Game.enviro.enableDefaultLighting = checkitem[0].Value();
                Game.enviro.enablePointLight = checkitem[1].Value();
                Game.enviro.fogEnabled = checkitem[2].Value();
                Game.enviro.FlyMode = checkitem[3].Value();
                Game.enviro.grayScale = checkitem[4].Value();
                Game.cam.fpsState = checkitem[5].Value();
                Game.enviro.smoothCamera = checkitem[6].Value();
                Game.enviro.enableSnow = checkitem[7].Value();
                Game.enviro.enableRain = checkitem[8].Value();
                Game.enviro.invertColors = checkitem[9].Value();

                Game.enviro.ambient = floatitem[0].Value();
                Game.enviro.lightpower = floatitem[1].Value();
                Game.enviro.lightscatter = floatitem[2].Value();
                Game.enviro.grassFade = floatitem[3].Value();
                Game.enviro.fogStart = floatitem[4].Value();
                Game.enviro.fogEnd = floatitem[5].Value();
                Game.enviro.fogColor.X = floatitem[6].Value();
                Game.enviro.fogColor.Y = floatitem[7].Value();
                Game.enviro.fogColor.Z = floatitem[8].Value();

                Game.enviro.WindWaveSize = floatitem[9].Value();
                Game.enviro.WindRandomness = floatitem[10].Value();
                //Game.enviro.WindSpeed = floatitem[11].Value();
                Game.enviro.WindAmount = floatitem[11].Value();
                Game.enviro.viewDistance = floatitem[12].Value();
                    if (Game.enviro.grassFade>Game.enviro.viewDistance)
                    {
                        Game.enviro.grassFade = Game.enviro.viewDistance;
                        floatitem[3] = new SliderOptionFloat("Grass Fade", writer, slider, new Vector2(10, Game.device.Viewport.Height - 11 * 30), Game.enviro.grassFade, 0, 500);
                    }
                Game.enviro.fieldOfView = floatitem[13].Value();
                Game.enviro.grassWidth = Game.enviro.grassHeight = floatitem[14].Value();
                Game.enviro.weatherParticles = (int)floatitem[15].Value();
                Game.enviro.waterLevel = floatitem[16].Value();
                Game.enviro.waveHeight = floatitem[17].Value();

                Game.enviro.grassIntensity = intitem[0].Value();

                if (accept.Value())
                {
                    Game.terrain = new TextureTerrain(Game, Game.enviro.map, Game.enviro.texture);
                    if (Game.enviro.billboardedGrass)
                        Game.bbgrass = new BBGrass(Game, Game.terrain, Game.enviro.grassTexture);
                    else
                        Game.grass = new Grass(Game, Game.terrain, Game.enviro.grassTexture);
                    Game.snow = new PointSprites_Multi(Game, Game.cam.cameraPosition, Game.Content.Load<Texture2D>("snow"), Game.enviro.weatherParticles, 0.2f, Game.rand);
                }

                Game.enviro.WindDirection = compassitem.Value();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (show)
            {
                spriteBatch.Draw(controlBG_tex, controlBG_rec[0], Color.White);
                spriteBatch.Draw(controlBG_tex, controlBG_rec[1], null, Color.White, 0, new Vector2(), SpriteEffects.FlipHorizontally, 0);

                foreach (CheckboxOption x in checkitem)
                {
                    if (x != null)
                    {
                        x.Draw(spriteBatch);
                    }
                }

                foreach (SliderOptionFloat x in floatitem)
                {
                    if (x!=null)
                    {
                        x.Draw(spriteBatch);
                    }
                }

                foreach (SliderOptionInt x in intitem)
                {
                    if (x!=null)
                    {
                        x.Draw(spriteBatch);
                    }
                }

                accept.Draw(spriteBatch);

                writer.Write("Wind Direction", new Vector2(Game.device.Viewport.Width / 2 - 50, Game.device.Viewport.Height - 120), Color.White);
                compassitem.Draw(spriteBatch);
            }
        }

    }

    class CheckboxOption
    {
        Texture2D texture;
        Vector2 textposition;
        Vector2 interactive_position;
        bool value;
        bool buttonpressed = false;
        short currentFrame=0;

        TextWriter writer;
        string text;

        public CheckboxOption(string newText, TextWriter newWriter, Texture2D newTexture, Vector2 newTextPosition, bool newValue)
        {
            text = newText;
            writer = newWriter;
            texture = newTexture;
            textposition = newTextPosition;
            interactive_position = new Vector2(textposition.X+200, textposition.Y);
            value = newValue;
        }

        public bool Value()
        {
            MouseState mouse = Mouse.GetState();
            Rectangle mouse_rec=new Rectangle(mouse.X,mouse.Y,1,1),item_rec=new Rectangle((int)interactive_position.X,(int)interactive_position.Y,texture.Width/2,texture.Height);

            if (mouse.LeftButton == ButtonState.Pressed && !buttonpressed)
            {
                if (mouse_rec.Intersects(item_rec))
                {
                    if (value)
                    {
                        value = false;
                    }
                    else
                        value = true;
                }

                buttonpressed = true;
            }
            else if(mouse.LeftButton!=ButtonState.Pressed && buttonpressed)
                buttonpressed = false;

            if (!value)
            {
                currentFrame = 0;
            }
            else
            {
                currentFrame = 1;
            }
            return value;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            writer.Write(text, textposition, Color.White);
            spriteBatch.Draw(texture, interactive_position, new Rectangle(texture.Width / 2 * currentFrame, 0, texture.Width / 2, texture.Height), Color.White);
        }
    }

    class SliderOptionInt
    {
        Texture2D[] texture = new Texture2D[2];
        Vector2 textposition;
        Vector2 interactive_position;
        Vector2 slider_position;
        int int_value;
        string text;
        short currentFrame = 0;
        int min, max;

        TextWriter writer;

        public SliderOptionInt(string newText, TextWriter newWriter, Texture2D[] newTextures, Vector2 newTextPosition, int newIntVal, int newMin, int newMax)
        {
            text = newText;
            writer = newWriter;
            texture = newTextures;
            textposition = newTextPosition;
            interactive_position = new Vector2(textposition.X + 150, textposition.Y);
            int_value = newIntVal;
            min = newMin;
            max = newMax;
            slider_position = new Vector2(interactive_position.X + (texture[0].Width / 2 / (max - min)) * int_value, interactive_position.Y - texture[1].Height / 3);
        }

        public int Value()
        {
            MouseState mouse = Mouse.GetState();
            Rectangle mouse_rec = new Rectangle(mouse.X, mouse.Y, 1, 1), item_rec = new Rectangle((int)interactive_position.X, (int)interactive_position.Y, texture[0].Width / 2, texture[0].Height);

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (mouse_rec.Intersects(item_rec) && mouse.X > interactive_position.X && mouse.X < interactive_position.X + texture[0].Width / 2)
                {
                    slider_position.X = mouse.X;

                    int scale = texture[0].Width / 2 / (max - min);
                    int_value = (int)Math.Round((double)(slider_position.X - interactive_position.X) / scale + min);

                    currentFrame = 1;
                }
            }
            else
                currentFrame = 0;


            return int_value;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            writer.Write(text, textposition, Color.White);
            spriteBatch.Draw(texture[0], interactive_position, new Rectangle(texture[0].Width / 2 * currentFrame, 0, texture[0].Width / 2, texture[0].Height), Color.White);
            spriteBatch.Draw(texture[1], slider_position, new Rectangle(texture[1].Width / 2 * currentFrame, 0, texture[1].Width / 2, texture[1].Height), Color.White);
            writer.Write(min.ToString(), new Vector2(interactive_position.X - 25, interactive_position.Y), Color.White);
            writer.Write(max.ToString(), new Vector2(interactive_position.X + texture[0].Width / 2 + 5, interactive_position.Y), Color.White);
            writer.Write(int_value.ToString(), new Vector2(slider_position.X - 2, slider_position.Y - 18), Color.White);
        }
    }

    class SliderOptionFloat
    {
        Texture2D[] texture = new Texture2D[2];
        Vector2 textposition;
        Vector2 interactive_position;
        Vector2 slider_position;
        float float_value;
        string text;
        short currentFrame = 0;
        float min, max;

        TextWriter writer;

        public SliderOptionFloat(string newText, TextWriter newWriter, Texture2D[] newTextures, Vector2 newTextPosition, float newFloatVal, float newMin, float newMax)
        {
            text = newText;
            writer = newWriter;
            texture = newTextures;
            textposition = newTextPosition;
            interactive_position = new Vector2(textposition.X + 150, textposition.Y);
            float_value = newFloatVal;
            min = newMin;
            max = newMax;
            slider_position = new Vector2(interactive_position.X + (texture[0].Width / 2 / (max - min)) * float_value - min, interactive_position.Y - texture[1].Height / 3);
        }

        public float Value()
        {
            MouseState mouse = Mouse.GetState();
            Rectangle mouse_rec = new Rectangle(mouse.X, mouse.Y, 1, 1), item_rec = new Rectangle((int)interactive_position.X, (int)interactive_position.Y, texture[0].Width / 2, texture[0].Height);

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (mouse_rec.Intersects(item_rec) && mouse.X > interactive_position.X && mouse.X < interactive_position.X + texture[0].Width / 2)
                {
                    slider_position.X = mouse.X;

                    float scale = texture[0].Width / 2 / (max - min);
                    float_value = (slider_position.X - interactive_position.X) / scale + min;

                    currentFrame = 1;
                }
            }
            else
                currentFrame = 0;


            return float_value;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            writer.Write(text, textposition, Color.White);
            spriteBatch.Draw(texture[0], interactive_position, new Rectangle(texture[0].Width / 2 * currentFrame, 0, texture[0].Width / 2, texture[0].Height), Color.White);
            spriteBatch.Draw(texture[1], slider_position, new Rectangle(texture[1].Width / 2 * currentFrame, 0, texture[1].Width / 2, texture[1].Height), Color.White);
            writer.Write(min.ToString(), new Vector2(interactive_position.X - 25, interactive_position.Y), Color.White);
            writer.Write(max.ToString(), new Vector2(interactive_position.X + texture[0].Width / 2 + 5, interactive_position.Y), Color.White);
            writer.Write(float_value.ToString(), new Vector2(slider_position.X - 10, slider_position.Y - 15), Color.White);
        }
    }

    class Compass
    {
        Rectangle rectangle_main, rectangle_sub;
        Vector2 pos_main, pos_sub;
        Texture2D[] texture;

        public Compass(Game1 newGame, Vector2 position, Texture2D[] newTexture)
        {
            texture = newTexture;
            rectangle_main = new Rectangle((int)position.X, (int)position.Y, texture[0].Width, texture[0].Height);
            //rectangle_sub = new Rectangle(rectangle_main.Center.X + (int)newGame.enviro.WindDirection.X - texture[1].Width/2, rectangle_main.Center.Y + (int)newGame.enviro.WindDirection.Z - texture[1].Height/2, texture[1].Width, texture[1].Height);
            rectangle_sub = new Rectangle(rectangle_main.Center.X - texture[1].Width / 2, rectangle_main.Center.Y - texture[1].Height / 2 - 10, texture[1].Width, texture[1].Height);
            pos_main = new Vector2(rectangle_main.Center.X, rectangle_main.Center.Y);
            pos_sub = new Vector2(rectangle_sub.Center.X, rectangle_sub.Center.Y);
        }

        public Vector3 Value()
        {
            MouseState mouse = Mouse.GetState();
            Rectangle mouse_rec = new Rectangle(mouse.X, mouse.Y, 1, 1);

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (mouse_rec.Intersects(rectangle_main))
                {
                    rectangle_sub.X = mouse_rec.X - texture[1].Width / 2;
                    rectangle_sub.Y = mouse_rec.Y - texture[1].Height / 2;
                }
            }

            pos_main = new Vector2(rectangle_main.Center.X, rectangle_main.Center.Y);
            pos_sub = new Vector2(rectangle_sub.Center.X, rectangle_sub.Center.Y);

            Vector2 value2 = pos_main - pos_sub;
            Vector3 value3= new Vector3(value2.X, 0,value2.Y) / 2f;
            //value3.Normalize();
            return value3;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture[0], rectangle_main, Color.White);
            spriteBatch.Draw(texture[1], rectangle_sub, Color.White);
        }
    }

    class AcceptItem
    {
        Texture2D texture;
        Vector2 position;
        Vector2 interactive_position;
        short currentFrame = 0;
        bool buttonpressed = false;
        TextWriter writer;
        string text;

        public AcceptItem(string newText, TextWriter newWriter, Texture2D newTexture, Vector2 newPos)
        {
            text = newText;
            writer = newWriter;
            texture = newTexture;
            position = newPos;
            interactive_position = position - new Vector2(25, 17);
        }

        public bool Value()
        {
            MouseState mouse = Mouse.GetState();
            Rectangle mouse_rec = new Rectangle(mouse.X, mouse.Y, 1, 1), item_rec = new Rectangle((int)interactive_position.X, (int)interactive_position.Y, texture.Width / 2, texture.Height);
            bool value = false;
            currentFrame = 0;

            if (mouse_rec.Intersects(item_rec))
            {
                if (mouse.LeftButton == ButtonState.Pressed && !buttonpressed)
                {
                    buttonpressed = true;
                    value = true;
                }

                currentFrame = 1;
            }
            if (mouse.LeftButton != ButtonState.Pressed && buttonpressed)
            {
                buttonpressed = false;
                value = false;
                currentFrame = 0;
            }

            return value;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, interactive_position, new Rectangle(texture.Width / 2 * currentFrame, 0, texture.Width / 2, texture.Height), Color.White);
            writer.Write(text, position, Color.White);
        }
    }
}

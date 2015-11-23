using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace namespace_default
{
    class Water
    {
        WaterVertex[] vertices;
        int[] indices;
        VertexBuffer v_buffer;
        IndexBuffer i_buffer;
        Game1 Game;
        int width, length;
        Texture2D bumpMap;
        float size;

        RenderTarget2D reflectionRenderTarget;
        RenderTarget2D refractionRenderTarget;


        public Water(Game1 newGame,int newWidth,int newLength, float newSize, Texture2D newTexture, Texture2D newBumpMap)
        {
            Game=newGame;
            width = newWidth;
            length = newLength;
            size = newSize;
            bumpMap = newBumpMap;
            

            SetUpVertices();
            SetUpIndices();
            SetUpBuffers();

            refractionRenderTarget = new RenderTarget2D(Game.device, (int)(Game.device.PresentationParameters.BackBufferWidth * Game.enviro.reflectionQuality), (int)(Game.device.PresentationParameters.BackBufferHeight * Game.enviro.reflectionQuality), true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            reflectionRenderTarget = new RenderTarget2D(Game.device, (int)(Game.device.PresentationParameters.BackBufferWidth * Game.enviro.reflectionQuality), (int)(Game.device.PresentationParameters.BackBufferHeight * Game.enviro.reflectionQuality), true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            Game.waterEffect.CurrentTechnique = Game.waterEffect.Techniques["Water"];
        }

        public void Draw()
        {
            Game.device.RasterizerState = RasterizerState.CullNone;
            Game.device.BlendState = BlendState.Opaque;

            Game.waterEffect.CurrentTechnique = Game.waterEffect.Techniques["Water"];
            Game.waterEffect.Parameters["xView"].SetValue(Game.cam.viewMatrix);
            Game.waterEffect.Parameters["xProjection"].SetValue(Game.cam.projectionMatrix);
            Game.waterEffect.Parameters["xWorld"].SetValue(Matrix.CreateScale(size)*Game.terrain.worldPosition * Matrix.CreateTranslation(new Vector3(0,Game.enviro.waterLevel,0)));
            Game.waterEffect.Parameters["xReflectionView"].SetValue(Game.cam.reflectionViewMatrix);
            Game.waterEffect.Parameters["xReflectionMap"].SetValue(reflectionRenderTarget);
            Game.waterEffect.Parameters["xRefractionMap"].SetValue(refractionRenderTarget);

            Game.waterEffect.Parameters["xShEye"].SetValue(Game.cam.shEye);
            Game.waterEffect.Parameters["xCamPos"].SetValue(Game.cam.cameraPosition);
            Game.waterEffect.Parameters["xEnableLight"].SetValue(Game.enviro.enableDefaultLighting);
            Game.waterEffect.Parameters["xLightPower"].SetValue(Game.enviro.lightpower);
            Game.waterEffect.Parameters["xLightDirection"].SetValue(Game.enviro.lightDirection);
            Game.waterEffect.Parameters["xAmbient"].SetValue(Game.enviro.ambient);

            Game.waterEffect.Parameters["WindDirection"].SetValue(Vector3.Normalize(Game.enviro.WindDirection));
            Game.waterEffect.Parameters["WindWaveSize"].SetValue(Game.enviro.WindWaveSize);
            Game.waterEffect.Parameters["WindRandomness"].SetValue(Game.enviro.WindRandomness);
            Game.waterEffect.Parameters["WindSpeed"].SetValue(Game.enviro.WindDirection.Length());
            Game.waterEffect.Parameters["WindAmount"].SetValue(Game.enviro.WindAmount);
            Game.waterEffect.Parameters["WindTime"].SetValue(Game.enviro.WindTime);

            Game.waterEffect.Parameters["xWaveHeight"].SetValue(Game.enviro.waveHeight);
            Game.waterEffect.Parameters["xWaveLength"].SetValue(0.3f);
            Game.waterEffect.Parameters["xWaterBumpMap"].SetValue(bumpMap);

            Game.waterEffect.Parameters["xEnableFog"].SetValue(Game.enviro.fogEnabled);
            Game.waterEffect.Parameters["xFogStart"].SetValue(Game.enviro.fogStart);
            Game.waterEffect.Parameters["xFogEnd"].SetValue(Game.enviro.fogEnd);
            Game.waterEffect.Parameters["xFogColor"].SetValue(Game.enviro.fogColor);

            Game.waterEffect.Parameters["xEnablePLight"].SetValue(Game.enviro.enablePointLight);
            Game.waterEffect.Parameters["xPLight"].SetValue(Game.enviro.pointLightPos);
            Game.waterEffect.Parameters["xPLightScatter"].SetValue(Game.enviro.lightscatter);


            Game.waterEffect.Parameters["xGrayScale"].SetValue(Game.enviro.grayScale);
            Game.waterEffect.Parameters["xInvertedColors"].SetValue(Game.enviro.invertColors);
            Game.waterEffect.Parameters["xUnderWater"].SetValue(Game.enviro.underWater);
            Game.waterEffect.Parameters["xDetailWater"].SetValue(Game.enviro.detailWater);

            Game.waterEffect.CurrentTechnique.Passes["Pass0"].Apply();

            Game.device.SetVertexBuffer(v_buffer);
            Game.device.Indices = i_buffer;
            Game.device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);

            Game.device.BlendState = BlendState.Opaque;  
        }
        public void DrawRefractionMap(short clipDirection)
        {
            Game.device.SetRenderTarget(refractionRenderTarget);
            Game.device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);


            Game.sky.Draw(Game.cam.cameraPosition + new Vector3(0, -10000, 0), Game.cam.viewMatrix, clipDirection);

            if (Game.enviro.billboardedGrass)
                Game.bbgrass.Draw(clipDirection, Game.cam.viewMatrix);
            else
                Game.grass.Draw(clipDirection, Game.cam.viewMatrix);

            Game.human.Draw(Game.cam.viewMatrix, clipDirection);
            Game.car.Draw(Game.cam.viewMatrix, clipDirection);
            
            Game.terrain.Draw(Game.cam.viewMatrix, clipDirection);

            if (clipDirection == 1)
            {
                Game.device.BlendState = BlendState.NonPremultiplied;
                Game.device.DepthStencilState = DepthStencilState.DepthRead;
                Game.moon.Draw(Game.cam.viewMatrix);
                foreach (PointSprites_Single x in Game.clouds)
                {
                    x.Draw(Game.cam.viewMatrix);
                }
            }


            Game.device.SetRenderTarget(null);


            if (Keyboard.GetState().IsKeyDown(Keys.Back))
            {
                System.IO.Stream s = System.IO.File.OpenWrite("refraction.jpg");
                refractionRenderTarget.SaveAsJpeg(s, refractionRenderTarget.Width, refractionRenderTarget.Height);
                s.Close();
            }
        }
        public void DrawReflectionMap(short clipDirection)
        {
            Game.device.SetRenderTarget(reflectionRenderTarget);
            Game.device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);




            Game.sky.Draw(Game.cam.cameraPosition + new Vector3(0, -10000, 0), Game.cam.reflectionViewMatrix, clipDirection);
            Game.device.BlendState = BlendState.NonPremultiplied;
            Game.device.DepthStencilState = DepthStencilState.DepthRead;
            Game.moon.Draw(Game.cam.reflectionViewMatrix);
            foreach (PointSprites_Single x in Game.clouds)
            {
                x.Draw(Game.cam.reflectionViewMatrix);
            }
            Game.human.Draw(Game.cam.reflectionViewMatrix, clipDirection);
            Game.car.Draw(Game.cam.reflectionViewMatrix, clipDirection);


            Game.terrain.Draw(Game.cam.reflectionViewMatrix, clipDirection);


            Game.device.SetRenderTarget(null);

            if (Keyboard.GetState().IsKeyDown(Keys.Back))
            {
                System.IO.Stream s = System.IO.File.OpenWrite("reflection.jpg");
                reflectionRenderTarget.SaveAsJpeg(s, reflectionRenderTarget.Width, reflectionRenderTarget.Height);
                s.Close();
            }
        }

        private void SetUpVertices()
        {
            vertices = new WaterVertex[width * length];
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < length; z++)
                {
                    vertices[x + z * width] = new WaterVertex(new Vector3(x, 0, z), Vector3.Normalize(new Vector3(0, 1, 0)), new Vector2((float)x / 30.0f * size, (float)z / 30.0f * size), (float)Game.rand.NextDouble()*2-1);
                }
            }
        }
        private void SetUpIndices()
        {
            indices = new int[(width - 1) * (length - 1) * 6];
            int counter = 0;
            for (int y = 0; y < length - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int lowerLeft = x + y * width;
                    int lowerRight = (x + 1) + y * width;
                    int topLeft = x + (y + 1) * width;
                    int topRight = (x + 1) + (y + 1) * width;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }
        private void SetUpBuffers()
        {
            v_buffer = new VertexBuffer(Game.device, WaterVertex.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            i_buffer = new IndexBuffer(Game.device, typeof(int), indices.Length, BufferUsage.WriteOnly);

            v_buffer.SetData(vertices);
            i_buffer.SetData(indices);
        }

        public struct WaterVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TextureCoordinate;
            public float Randomness;

            public WaterVertex(Vector3 newPosition, Vector3 newNormal, Vector2 newTexcoord, float newRandomness)
            {
                Position = newPosition;
                Normal = newNormal;
                TextureCoordinate = newTexcoord;
                Randomness = newRandomness;
            }

            public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
                , new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
                , new VertexElement(sizeof(float) * (3 + 3), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                , new VertexElement(sizeof(float) * (3 + 3 + 2), VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
            );
        }
    }
}

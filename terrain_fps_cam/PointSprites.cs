//With this you can load point Sprites into your program via various methods
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace namespace_default
{
    public class PointSprites_Single
    {
        Game1 Game;
        public Vector3 position;
        Texture2D texture;
        VertexPositionTexture[] vertices = new VertexPositionTexture[4];
        int[] indices = new int[6];
        float size;
        DynamicVertexBuffer v_buffer;
        DynamicIndexBuffer i_buffer;

        public PointSprites_Single(Game1 newGame, Vector3 newPosition, Texture2D newTexture, float newSize)
        {
            position = newPosition;
            texture = newTexture;
            Game=newGame;
            size = newSize;

            vertices[0] = new VertexPositionTexture(position, new Vector2(0, 0));
            vertices[1] = new VertexPositionTexture(position, new Vector2(0, 1));
            vertices[2] = new VertexPositionTexture(position, new Vector2(1, 1));

            //vertices[3] = new VertexPositionTexture(new Vector3(), new Vector2(1, 1));
            vertices[3] = new VertexPositionTexture(position, new Vector2(1, 0));
            //vertices[5] = new VertexPositionTexture(new Vector3(), new Vector2(0, 0));

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 0;
            indices[4] = 2;
            indices[5] = 3;

            SetUpBuffers();
        }

        public void Update(Vector3 amount)
        {
            position += amount;
            vertices[0] = new VertexPositionTexture(position, new Vector2(0, 0));
            vertices[1] = new VertexPositionTexture(position, new Vector2(0, 1));
            vertices[2] = new VertexPositionTexture(position, new Vector2(1, 1));
            vertices[3] = new VertexPositionTexture(position, new Vector2(1, 0));

        }

        public void Draw(Matrix newView)
        {
            Game.device.RasterizerState = RasterizerState.CullClockwise;
            Game.pointSpriteEffect.CurrentTechnique = Game.pointSpriteEffect.Techniques["PointSprites"];
            Game.pointSpriteEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            Game.pointSpriteEffect.Parameters["xProjection"].SetValue(Game.cam.infinite_proj);
            Game.pointSpriteEffect.Parameters["xView"].SetValue(newView);
            Game.pointSpriteEffect.Parameters["xCamPos"].SetValue(Game.cam.cameraPosition);
            Game.pointSpriteEffect.Parameters["xTexture"].SetValue(texture);
            Game.pointSpriteEffect.Parameters["xCamUp"].SetValue(Game.cam.infinite_proj.Up);
            Game.pointSpriteEffect.Parameters["xPointSpriteSize"].SetValue(size);
            Game.pointSpriteEffect.Parameters["xGrayScale"].SetValue(Game.enviro.grayScale);
            Game.pointSpriteEffect.Parameters["xInvertedColors"].SetValue(Game.enviro.invertColors);
            Game.pointSpriteEffect.Parameters["xUnderWater"].SetValue(Game.enviro.underWater);
            Game.pointSpriteEffect.Parameters["xEnablePLight"].SetValue(false);
            Game.pointSpriteEffect.CurrentTechnique.Passes[0].Apply();

            //Game.device.SetVertexBuffer(v_buffer);
            //Game.device.Indices = i_buffer;
            //Game.device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);

            Game.device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
        }

        private void SetUpBuffers()
        {
            v_buffer = new DynamicVertexBuffer(Game.device, VertexPositionTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            i_buffer = new DynamicIndexBuffer(Game.device, typeof(int), indices.Length, BufferUsage.WriteOnly);

            v_buffer.SetData(vertices);
            i_buffer.SetData(indices);
        }
    }


    public class PointSprites_Multi
    {
        Game1 Game;
        public Vector3 position;
        public Vector3 velocity;
        Texture2D texture;
        VertexPositionTexture[] vertices;
        int amount;
        float size;
        Random rand;

        public PointSprites_Multi(Game1 newGame, Vector3 newPosition, Texture2D newTexture, int newAmount, float newSize, Random newRand)
        {
            position = newPosition;
            texture = newTexture;
            Game = newGame;
            amount = newAmount;
            size = newSize;
            rand = newRand;

            vertices = new VertexPositionTexture[amount * 6];

            SetUpVertices();
        }

        private void SetUpVertices()
        {
            for (int i = 0; i < amount*6; i+=6)
            {
                Vector3 POS = position + new Vector3(
                    (float)rand.NextDouble() * rand.Next(-40, 40)
                    , (float)rand.Next(1, 40)
                    , (float)rand.NextDouble() *rand.Next(-40, 40)
                    );
                vertices[i] = new VertexPositionTexture(POS, new Vector2(1, 1));
                vertices[i + 1] = new VertexPositionTexture(POS, new Vector2(0, 0));
                vertices[i + 2] = new VertexPositionTexture(POS, new Vector2(1, 0));

                vertices[i + 3] = new VertexPositionTexture(POS, new Vector2(1, 1));
                vertices[i + 4] = new VertexPositionTexture(POS, new Vector2(0, 1));
                vertices[i + 5] = new VertexPositionTexture(POS, new Vector2(0, 0));
            }
        }

        public void Fall(Vector3 center, float Speed)
        {
            velocity = Game.enviro.WindDirection/100f+new Vector3(0,-Speed,0);

            for (int i = 0; i < amount * 6; i += 6)
            {
                vertices[i].Position += velocity;
                vertices[i + 1].Position += velocity;
                vertices[i + 2].Position += velocity;

                vertices[i + 3].Position += velocity;
                vertices[i + 4].Position += velocity;
                vertices[i + 5].Position += velocity;

                /*if (
                    (
                      (
                      (int)vertices[i].Position.X > Game.terrain.terrainLength || (int)vertices[i].Position.X < 0
                      || (int)vertices[i].Position.Z > Game.terrain.terrainWidth || (int)vertices[i].Position.Z < 0
                      )
                      && vertices[i].Position.Y < 20

                    )


                    || 


                    (
                        (int)vertices[i].Position.X < Game.terrain.terrainLength && (int)vertices[i].Position.X > 0
                        && (int)vertices[i].Position.Z < Game.terrain.terrainWidth && (int)vertices[i].Position.Z > 0
                        && vertices[i].Position.Y < Game.terrain.heightData[(int)vertices[i].Position.X, (int)vertices[i].Position.Z]
                    )


                   )*/
                if(vertices[i].Position.Y<Game.enviro.waterLevel || Vector3.Distance(center,vertices[i].Position)>40)
                {
                    Vector3 POS = center + new Vector3(
                    (float)rand.NextDouble() * rand.Next(-40, 40)
                    , (float)rand.Next(1, 40)
                    , (float)rand.NextDouble() * rand.Next(-40, 40)
                    );
                    vertices[i].Position = POS;
                    vertices[i + 1].Position = POS;
                    vertices[i + 2].Position = POS;

                    vertices[i + 3].Position = POS;
                    vertices[i + 4].Position = POS;
                    vertices[i + 5].Position = POS;
                }
            }

        }

        public void Draw(Matrix newView)
        {
            Game.device.BlendState = BlendState.NonPremultiplied;
            Game.device.DepthStencilState = DepthStencilState.DepthRead;
            Game.device.RasterizerState = RasterizerState.CullCounterClockwise;
            Game.pointSpriteEffect.CurrentTechnique = Game.pointSpriteEffect.Techniques["PointSprites"];
            Game.pointSpriteEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            Game.pointSpriteEffect.Parameters["xProjection"].SetValue(Game.cam.projectionMatrix);
            Game.pointSpriteEffect.Parameters["xView"].SetValue(newView);
            Game.pointSpriteEffect.Parameters["xCamPos"].SetValue(Game.cam.cameraPosition);
            Game.pointSpriteEffect.Parameters["xTexture"].SetValue(texture);
            Game.pointSpriteEffect.Parameters["xCamUp"].SetValue(Vector3.Normalize(-Game.enviro.WindDirection)+new Vector3(0,Game.enviro.WindDirection.Length(),0));
            Game.pointSpriteEffect.Parameters["xPointSpriteSize"].SetValue(size);
            Game.pointSpriteEffect.Parameters["xGrayScale"].SetValue(Game.enviro.grayScale);
            Game.pointSpriteEffect.Parameters["xInvertedColors"].SetValue(Game.enviro.invertColors);

            Game.pointSpriteEffect.Parameters["xEnablePLight"].SetValue(Game.enviro.enablePointLight);
            Game.pointSpriteEffect.Parameters["xPLight"].SetValue(Game.enviro.pointLightPos);
            Game.pointSpriteEffect.Parameters["xPLightScatter"].SetValue(Game.enviro.lightscatter);
            Game.pointSpriteEffect.Parameters["xLightPower"].SetValue(Game.enviro.lightpower);

            Game.pointSpriteEffect.CurrentTechnique.Passes[0].Apply();
            Game.device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length/3);
            Game.device.DepthStencilState = DepthStencilState.Default;
            Game.device.BlendState = BlendState.Opaque;
        }
    }
}

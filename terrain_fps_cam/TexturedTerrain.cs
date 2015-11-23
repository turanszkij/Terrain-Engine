//You can load a single textured terrain with this class
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace namespace_default
{
    public class TextureTerrain
    {
        Game1 Game;
        public VertexPositionNormalTexture[] vertices;
        public int[] indices;
        VertexBuffer vertexbuff;
        IndexBuffer indexbuff;

        Texture2D texture;

        public int terrainWidth;
        public int terrainLength;
        public float[,] heightData=new float[100,100];

        public Matrix worldPosition;


        public TextureTerrain(Game1 newGame, string newMapName, string newTexture)
        {
            Game = newGame;
            texture = Game.Content.Load<Texture2D>(newTexture);
            LoadHeightData(Game.Content.Load<Texture2D>(newMapName));
            SetUpVertices();
            SetUpIndices();
            CalculateNormals();
            CopytoBuffers();
        }
        private void LoadHeightData(Texture2D heightMap)
        {
            terrainWidth = heightMap.Width;
            terrainLength = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainLength];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainLength];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainLength; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R / 5f;
                }

            worldPosition = Matrix.CreateTranslation(0, 0, 0);
        }
        private void SetUpVertices()
        {
            vertices = new VertexPositionNormalTexture[terrainWidth * terrainLength];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainLength; y++)
                {
                    vertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], y);

                    vertices[x + y * terrainWidth].TextureCoordinate.X = (float)x / 4.0f;
                    vertices[x + y * terrainWidth].TextureCoordinate.Y = (float)y / 4.0f;
                }
            }
        }
        private void SetUpIndices()
        {
            indices = new int[(terrainWidth - 1) * (terrainLength - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainLength - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }
        }
        private void CalculateNormals()
        {
            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Normalize(Vector3.Cross(side2, side1));
                //Vector3 normal = Vector3.Subtract(side1, side2);

                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            //for (int i = 0; i < vertices.Length; i++)
            //{
            //    vertices[i].Normal.Normalize();
            //}
        }
        private void CopytoBuffers()
        {
            vertexbuff = new VertexBuffer(Game.device,
                VertexPositionNormalTexture.VertexDeclaration
                , vertices.Length
                , BufferUsage.WriteOnly);
            vertexbuff.SetData(vertices);

            if (indices.Length > 0)
            {
                indexbuff = new IndexBuffer(Game.device
                    , typeof(int)
                    , indices.Length
                    , BufferUsage.WriteOnly);
                indexbuff.SetData(indices);
            }
        }

        public void Draw(Matrix newView, short clip)
        {
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.CullClockwiseFace;
            Game.device.RasterizerState = rs;

            Game.device.DepthStencilState = DepthStencilState.Default;
            Game.device.BlendState = BlendState.Opaque;


            Game.texturedEffect.CurrentTechnique = Game.texturedEffect.Techniques["Textured"];
            Game.texturedEffect.Parameters["xTexture0"].SetValue(texture);
            Game.texturedEffect.Parameters["xTextureSh"].SetValue(Game.shadowMap);

            Game.texturedEffect.Parameters["xViewProjection"].SetValue(newView * Game.cam.projectionMatrix);
            Game.texturedEffect.Parameters["xWorld"].SetValue(worldPosition);
            Game.texturedEffect.Parameters["xShViewProjection"].SetValue(Game.cam.shadowViewMatrix * Game.cam.shadowProjectionMatrix);
            Game.texturedEffect.Parameters["xShEye"].SetValue(Game.cam.shEye);
            Game.texturedEffect.Parameters["xEnableLight"].SetValue(Game.enviro.enableDefaultLighting);
            Game.texturedEffect.Parameters["xLightDirection"].SetValue(Game.enviro.lightDirection);
            Game.texturedEffect.Parameters["xLightPower"].SetValue(Game.enviro.lightpower);
            Game.texturedEffect.Parameters["xAmbient"].SetValue(Game.enviro.ambient);
            Game.texturedEffect.Parameters["xCamPos"].SetValue(Game.cam.cameraPosition);

            Game.texturedEffect.Parameters["xEnableFog"].SetValue(Game.enviro.fogEnabled);
            Game.texturedEffect.Parameters["xFogStart"].SetValue(Game.enviro.fogStart);
            Game.texturedEffect.Parameters["xFogEnd"].SetValue(Game.enviro.fogEnd);
            Game.texturedEffect.Parameters["xFogColor"].SetValue(Game.enviro.fogColor);

            Game.texturedEffect.Parameters["xEnablePLight"].SetValue(Game.enviro.enablePointLight);
            Game.texturedEffect.Parameters["xPLight"].SetValue(Game.enviro.pointLightPos);
            Game.texturedEffect.Parameters["xPLightScatter"].SetValue(Game.enviro.lightscatter);


            Game.texturedEffect.Parameters["xGrayScale"].SetValue(Game.enviro.grayScale);
            Game.texturedEffect.Parameters["xInvertedColors"].SetValue(Game.enviro.invertColors);
            Game.texturedEffect.Parameters["xUnderWater"].SetValue(Game.enviro.underWater);

            Game.texturedEffect.Parameters["xClipping"].SetValue(clip);
            Game.texturedEffect.Parameters["xClipHeight"].SetValue(Game.enviro.waterLevel);
            Game.texturedEffect.Parameters["xDepthBias"].SetValue(Game.enviro.depthbias);

            Game.texturedEffect.CurrentTechnique.Passes["Pass0"].Apply();

            Game.device.SetVertexBuffer(vertexbuff);
            Game.device.Indices = indexbuff;

            Game.device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);

            
        }
        public void DrawForShadowMap()
        {
            Game.device.SetVertexBuffer(vertexbuff);
            Game.device.Indices = indexbuff;

            Game.device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);
        }


        struct VertexPosition
        {
            public Vector3 Position;

            public VertexPosition(Vector3 newPosition)
            {
                Position = newPosition;
            }

            public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
            );
        }
    }
}

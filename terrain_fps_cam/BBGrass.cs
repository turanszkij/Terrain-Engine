using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace namespace_default
{
    public class BBGrass
    {
        Game1 Game;
        BillBoardVertex[] bbvertices;
        TextureTerrain terrain;
        VertexBuffer[] bbvertexbuff = new VertexBuffer[2];
        Texture2D bbtex;


        public BBGrass(Game1 newGame, TextureTerrain newTerrain, string newTexture)
        {
            Game = newGame;
            terrain = newTerrain;
            bbtex = Game.Content.Load<Texture2D>(newTexture);
            SetUpBillboards();
            CopytoBuffers();

            Game.billboardGrassEffect.Parameters["xTexture"].SetValue(bbtex);
            Game.billboardGrassEffect.CurrentTechnique = Game.billboardGrassEffect.Techniques["GrassBB"];
        }

        public void Draw(short clip, Matrix newView)
        {
            Game.device.RasterizerState = RasterizerState.CullNone;
            Game.billboardGrassEffect.Parameters["xWorld"].SetValue(terrain.worldPosition);
            Game.billboardGrassEffect.Parameters["xView"].SetValue(newView);
            Game.billboardGrassEffect.Parameters["xProjection"].SetValue(Game.cam.projectionMatrix);
            Game.billboardGrassEffect.Parameters["xEnableLight"].SetValue(Game.enviro.enableDefaultLighting);
            Game.billboardGrassEffect.Parameters["xLightPower"].SetValue(Game.enviro.lightpower);
            Game.billboardGrassEffect.Parameters["xLightDirection"].SetValue(Game.enviro.lightDirection);
            Game.billboardGrassEffect.Parameters["xAmbient"].SetValue(Game.enviro.ambient);
            Game.billboardGrassEffect.Parameters["AlphaTestDirection"].SetValue(1f);

            Game.billboardGrassEffect.Parameters["WindDirection"].SetValue(Vector3.Normalize(Game.enviro.WindDirection));
            Game.billboardGrassEffect.Parameters["WindWaveSize"].SetValue(Game.enviro.WindWaveSize);
            Game.billboardGrassEffect.Parameters["WindRandomness"].SetValue(Game.enviro.WindRandomness);
            Game.billboardGrassEffect.Parameters["WindSpeed"].SetValue(Game.enviro.WindDirection.Length());
            Game.billboardGrassEffect.Parameters["WindAmount"].SetValue(Game.enviro.WindAmount);
            Game.billboardGrassEffect.Parameters["WindTime"].SetValue(Game.enviro.WindTime);

            Game.billboardGrassEffect.Parameters["BillboardWidth"].SetValue(Game.enviro.grassWidth);
            Game.billboardGrassEffect.Parameters["BillboardHeight"].SetValue(Game.enviro.grassHeight);

            Game.billboardGrassEffect.Parameters["xEnableFog"].SetValue(Game.enviro.fogEnabled);
            Game.billboardGrassEffect.Parameters["xFogStart"].SetValue(Game.enviro.fogStart);
            Game.billboardGrassEffect.Parameters["xFogEnd"].SetValue(Game.enviro.fogEnd);
            Game.billboardGrassEffect.Parameters["xFogColor"].SetValue(Game.enviro.fogColor);

            Game.billboardGrassEffect.Parameters["xEnablePLight"].SetValue(Game.enviro.enablePointLight);
            Game.billboardGrassEffect.Parameters["xPLight"].SetValue(Game.enviro.pointLightPos);
            Game.billboardGrassEffect.Parameters["xPLightScatter"].SetValue(Game.enviro.lightscatter);

            Game.billboardGrassEffect.Parameters["xGrassFade"].SetValue(Game.enviro.grassFade);

            Game.billboardGrassEffect.Parameters["xGrayScale"].SetValue(Game.enviro.grayScale);
            Game.billboardGrassEffect.Parameters["xInvertedColors"].SetValue(Game.enviro.invertColors);
            Game.billboardGrassEffect.Parameters["xUnderWater"].SetValue(Game.enviro.underWater);

            Game.billboardGrassEffect.Parameters["xClipping"].SetValue(clip);
            Game.billboardGrassEffect.Parameters["xClipHeight"].SetValue(Game.enviro.waterLevel);

            Game.billboardGrassEffect.CurrentTechnique.Passes["Pass0"].Apply();

            if (bbvertexbuff[0] != null)
            {
                Game.device.SetVertexBuffer(bbvertexbuff[0]);
                Game.device.DrawPrimitives(PrimitiveType.TriangleList, 0, bbvertices.Length / 3 / 2);
            }
            if (bbvertexbuff[1] != null)
            {
                Game.device.SetVertexBuffer(bbvertexbuff[1]);
                Game.device.DrawPrimitives(PrimitiveType.TriangleList, 0, bbvertices.Length / 3 / 2);
            }


            //SECOND PASS
            Game.device.BlendState = BlendState.NonPremultiplied;
            Game.device.DepthStencilState = DepthStencilState.DepthRead;
            Game.billboardGrassEffect.CurrentTechnique = Game.billboardGrassEffect.Techniques["GrassBB"];

            Game.billboardGrassEffect.Parameters["AlphaTestDirection"].SetValue(-1f);

            Game.billboardGrassEffect.CurrentTechnique.Passes["Pass0"].Apply();

            if (bbvertexbuff[0] != null)
            {
                Game.device.SetVertexBuffer(bbvertexbuff[0]);
                Game.device.DrawPrimitives(PrimitiveType.TriangleList, 0, bbvertices.Length / 3 / 2);
            }
            if (bbvertexbuff[1] != null)
            {
                Game.device.SetVertexBuffer(bbvertexbuff[1]);
                Game.device.DrawPrimitives(PrimitiveType.TriangleList, 0, bbvertices.Length / 3 / 2);
            }

            Game.device.BlendState = BlendState.Opaque;
        }

        private void SetUpBillboards()
        {
            Random rand = new Random();
            List<BillBoardVertex> bbverticesList = new List<BillBoardVertex>();

            for (int triangle = 0; triangle < terrain.indices.Length; triangle += 3)
            {
                for (int j = 0; j < Game.enviro.grassIntensity; j++)
                {
                    //A háromszög indexeinek kikeresése
                    int i1 = terrain.indices[triangle];
                    int i2 = terrain.indices[triangle + 1];
                    int i3 = terrain.indices[triangle + 2];
                    //

                    Vector3 v1 = terrain.vertices[i1].Position, v2 = terrain.vertices[i2].Position, v3 = terrain.vertices[i3].Position;
                    Vector3 n1 = terrain.vertices[i1].Normal, n2 = terrain.vertices[i2].Normal, n3 = terrain.vertices[i3].Normal;

                    if (v1.Y > Game.enviro.waterLevel && v2.Y > Game.enviro.waterLevel && v3.Y > Game.enviro.waterLevel)
                    {

                        float a = (float)rand.NextDouble();
                        float b = (float)rand.NextDouble();

                        if (a + b > 1)
                        {
                            a = 1 - a;
                            b = 1 - b;
                        }

                        Vector3 randomPosition = Vector3.Barycentric(v1, v2, v3, a, b);
                        Vector3 randomNormal = Vector3.Barycentric(n1, n2, n3, a, b);
                        randomNormal.Normalize();
                        //float randomness = rand.Next(500, 1200) / 1000f;
                        float randomness = (float)rand.NextDouble() * 2 - 1;

                        bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(0, 0), randomness));
                        bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(0, 1), randomness));
                        bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(1, 1), randomness));

                        bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(0, 0), randomness));
                        bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(1, 1), randomness));
                        bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(1, 0), randomness));
                    }
                }
            }

            bbvertices = new BillBoardVertex[bbverticesList.Count];
            bbvertices = bbverticesList.ToArray();
        }
        private void CopytoBuffers()
        {
            if (bbvertices.Length > 0)
            {
                bbvertexbuff[0] = new VertexBuffer(Game.device,
                    BillBoardVertex.VertexDeclaration
                    , bbvertices.Length / 2
                    , BufferUsage.WriteOnly);
                bbvertexbuff[0].SetData(bbvertices, 0, bbvertices.Length / 2);

                bbvertexbuff[1] = new VertexBuffer(Game.device,
                    BillBoardVertex.VertexDeclaration
                    , bbvertices.Length / 2
                    , BufferUsage.WriteOnly);
                bbvertexbuff[1].SetData(bbvertices, bbvertices.Length / 2, bbvertices.Length / 2);
            }
        }


        public struct BillBoardVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TextureCoordinate;
            public float Randomness;

            public BillBoardVertex(Vector3 newPosition, Vector3 newNormal, Vector2 newTexcoord, float newRandomness)
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

//Load fake grass randomly on each triangle of the already set up terrain
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace namespace_default
{
    public class Grass
    {
        Game1 Game;
        BillBoardVertex[] bbvertices;
        TextureTerrain terrain;
        VertexBuffer[] bbvertexbuff = new VertexBuffer[2];
        Texture2D bbtex;


        public Grass(Game1 newGame, TextureTerrain newTerrain, string newTexture)
        {
            Game = newGame;
            terrain = newTerrain;
            bbtex = Game.Content.Load<Texture2D>(newTexture);
            SetUpBillboards();
            CopytoBuffers();


            Game.grassEffect.Parameters["xTexture"].SetValue(bbtex);
            Game.grassEffect.CurrentTechnique = Game.grassEffect.Techniques["GrassBB"];
        }

        public void Draw(short clip, Matrix newView)
        {
            Game.device.BlendState = BlendState.Opaque;
            Game.device.RasterizerState = RasterizerState.CullNone;
            Game.device.SamplerStates[0] = SamplerState.LinearClamp;

            Game.grassEffect.Parameters["xWorld"].SetValue(terrain.worldPosition);
            Game.grassEffect.Parameters["xView"].SetValue(newView);
            Game.grassEffect.Parameters["xProjection"].SetValue(Game.cam.projectionMatrix);
            Game.grassEffect.Parameters["xEnableLight"].SetValue(Game.enviro.enableDefaultLighting);
            Game.grassEffect.Parameters["xLightPower"].SetValue(Game.enviro.lightpower);
            Game.grassEffect.Parameters["xLightDirection"].SetValue(Game.enviro.lightDirection);
            Game.grassEffect.Parameters["xAmbient"].SetValue(Game.enviro.ambient);
            Game.grassEffect.Parameters["AlphaTestDirection"].SetValue(1f);

            Game.grassEffect.Parameters["WindDirection"].SetValue(Vector3.Normalize(Game.enviro.WindDirection));
            Game.grassEffect.Parameters["WindWaveSize"].SetValue(Game.enviro.WindWaveSize);
            Game.grassEffect.Parameters["WindRandomness"].SetValue(Game.enviro.WindRandomness);
            Game.grassEffect.Parameters["WindSpeed"].SetValue(Game.enviro.WindDirection.Length());
            Game.grassEffect.Parameters["WindAmount"].SetValue(Game.enviro.WindAmount);
            Game.grassEffect.Parameters["WindTime"].SetValue(Game.enviro.WindTime);

            Game.grassEffect.Parameters["BillboardWidth"].SetValue(Game.enviro.grassWidth);
            Game.grassEffect.Parameters["BillboardHeight"].SetValue(Game.enviro.grassHeight);

            Game.grassEffect.Parameters["xEnableFog"].SetValue(Game.enviro.fogEnabled);
            Game.grassEffect.Parameters["xFogStart"].SetValue(Game.enviro.fogStart);
            Game.grassEffect.Parameters["xFogEnd"].SetValue(Game.enviro.fogEnd);
            Game.grassEffect.Parameters["xFogColor"].SetValue(Game.enviro.fogColor);

            Game.grassEffect.Parameters["xEnablePLight"].SetValue(Game.enviro.enablePointLight);
            Game.grassEffect.Parameters["xPLight"].SetValue(Game.enviro.pointLightPos);
            Game.grassEffect.Parameters["xPLightScatter"].SetValue(Game.enviro.lightscatter);

            Game.grassEffect.Parameters["xGrassFade"].SetValue(Game.enviro.grassFade);

            Game.grassEffect.Parameters["xGrayScale"].SetValue(Game.enviro.grayScale);
            Game.grassEffect.Parameters["xInvertedColors"].SetValue(Game.enviro.invertColors);
            Game.grassEffect.Parameters["xUnderWater"].SetValue(Game.enviro.underWater);

            Game.grassEffect.Parameters["xClipping"].SetValue(clip);
            Game.grassEffect.Parameters["xClipHeight"].SetValue(Game.enviro.waterLevel);

            Game.grassEffect.CurrentTechnique.Passes["Pass0"].Apply();

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
            Game.grassEffect.CurrentTechnique = Game.grassEffect.Techniques["GrassBB"];

            Game.grassEffect.Parameters["AlphaTestDirection"].SetValue(-1f);

            Game.grassEffect.CurrentTechnique.Passes["Pass0"].Apply();

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
            Game.device.DepthStencilState = DepthStencilState.Default;
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
                    if (rand.Next(0, 10-Game.enviro.grassIntensity) == 0)
                    {
                        Vector3 v1 = terrain.vertices[i1].Position, v2 = terrain.vertices[i2].Position, v3 = terrain.vertices[i3].Position;
                        if (v1.Y > Game.enviro.waterLevel && v2.Y > Game.enviro.waterLevel && v3.Y > Game.enviro.waterLevel)
                        {
                            Vector3 n1 = terrain.vertices[i1].Normal, n2 = terrain.vertices[i2].Normal, n3 = terrain.vertices[i3].Normal;

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
                            float randomness = (float)rand.NextDouble() * 2 - 1;

                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(0, 0), randomness, new Vector3(1, 0, 0)));
                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(0, 1), randomness, new Vector3(1, 0, 0)));
                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(1, 1), randomness, new Vector3(1, 0, 0)));

                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(0, 0), randomness, new Vector3(1, 0, 0)));
                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(1, 1), randomness, new Vector3(1, 0, 0)));
                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(1, 0), randomness, new Vector3(1, 0, 0)));


                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(0, 0), randomness, new Vector3(0, 0, 1)));
                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(0, 1), randomness, new Vector3(0, 0, 1)));
                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(1, 1), randomness, new Vector3(0, 0, 1)));

                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(0, 0), randomness, new Vector3(0, 0, 1)));
                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(1, 1), randomness, new Vector3(0, 0, 1)));
                            bbverticesList.Add(new BillBoardVertex(randomPosition, randomNormal, new Vector2(1, 0), randomness, new Vector3(0, 0, 1)));
                        }
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
                    , bbvertices.Length/2
                    , BufferUsage.WriteOnly);
                bbvertexbuff[0].SetData(bbvertices, 0, bbvertices.Length / 2);

                bbvertexbuff[1] = new VertexBuffer(Game.device,
                    BillBoardVertex.VertexDeclaration
                    , bbvertices.Length / 2
                    , BufferUsage.WriteOnly);
                bbvertexbuff[1].SetData(bbvertices, bbvertices.Length/2, bbvertices.Length / 2);
            }
        }


        public struct BillBoardVertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TextureCoordinate;
            public float Randomness;
            public Vector3 Direction;

            public BillBoardVertex(Vector3 newPosition, Vector3 newNormal, Vector2 newTexcoord, float newRandomness, Vector3 newDirection)
            {
                Position = newPosition;
                Normal = newNormal;
                TextureCoordinate = newTexcoord;
                Randomness = newRandomness;
                Direction = newDirection;
            }

            public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
                , new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
                , new VertexElement(sizeof(float) * (3 + 3), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                , new VertexElement(sizeof(float) * (3 + 3 + 2), VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
                , new VertexElement(sizeof(float) * (3 + 3 + 2 + 1), VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 2)
            );
        }
    }
}

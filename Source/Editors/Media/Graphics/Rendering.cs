﻿using System.Windows.Forms;
using CryBits.Editors.Entities;
using CryBits.Editors.Entities.Tools;
using CryBits.Editors.Forms;
using CryBits.Editors.Library;
using CryBits.Entities;
using SFML.Graphics;
using SFML.System;
using static CryBits.Editors.Logic.Utils;
using Button = CryBits.Editors.Entities.Tools.Button;
using CheckBox = CryBits.Editors.Entities.Tools.CheckBox;
using Panel = CryBits.Editors.Entities.Tools.Panel;
using TextBox = CryBits.Editors.Entities.Tools.TextBox;

namespace CryBits.Editors.Media
{
    internal partial class Graphics
    {
        // Locais de renderização
        public static RenderWindow Win_Interface;
        public static RenderWindow Win_Tile;
        public static RenderWindow Win_Map;
        public static RenderWindow Win_Map_Tile;
        public static RenderTexture Win_Map_Lighting;
        public static RenderWindow Win_Item;
        public static RenderWindow Win_Class;
        public static RenderWindow Win_NPC;

        private static void Transparent(RenderWindow window)
        {
            Vector2u textureSize = Tex_Transparent.Size;

            // Desenha uma textura transparente na janela inteira
            for (uint x = 0; x <= window.Size.X / textureSize.X; x++)
                for (uint y = 0; y <= window.Size.Y / textureSize.Y; y++)
                    Render(window, Tex_Transparent, new Vector2u(textureSize.X * x, textureSize.Y * y));
        }

        public static void Present()
        {
            // Desenha 
            Editor_Maps_Tile();
            Editor_Maps_Map();
            Editor_Tile();
            Editor_Class();
            Editor_Item();
            Editor_NPC();
            Interface();
        }

        #region Map Editor
        private static void Editor_Maps_Tile()
        {
            EditorMaps form = EditorMaps.Form;

            // Somente se necessário
            if (Win_Map == null || !form.butMNormal.Checked) return;

            // Reinicia o dispositivo caso haja alguma alteração no tamanho da tela
            if (new Size((int)Win_Map_Tile.Size.X, (int)Win_Map_Tile.Size.Y) != form.picTile.Size)
            {
                Win_Map_Tile.Dispose();
                Win_Map_Tile = new RenderWindow(EditorMaps.Form.picTile.Handle);
            }

            // Limpa a área com um fundo preto
            Win_Map_Tile.Clear(Color.Black);

            // Dados
            Texture texture = Tex_Tile[form.cmbTiles.SelectedIndex + 1];
            Point position = new Point(form.scrlTileX.Value, form.scrlTileY.Value);

            // Desenha o azulejo e as grades
            Transparent(Win_Map_Tile);
            Render(Win_Map_Tile, texture, new Rectangle(position, Size(texture)), new Rectangle(new Point(0), Size(texture)));
            RenderRectangle(Win_Map_Tile, new Rectangle(new Point(form.Tile_Source.X - position.X, form.Tile_Source.Y - position.Y), form.Tile_Source.Size), CColor(165, 42, 42, 250));
            RenderRectangle(Win_Map_Tile, form.Tile_Mouse.X, form.Tile_Mouse.Y, Grid, Grid, CColor(65, 105, 225, 250));

            // Exibe o que foi renderizado
            Win_Map_Tile.Display();
        }

        private static void Editor_Maps_Map()
        {
            // Previne erros
            if (EditorMaps.Form == null || EditorMaps.Form.IsDisposed || EditorMaps.Form.Selected == null) return;

            // Limpa a área com um fundo preto
            Win_Map.Clear(Color.Black);

            // Desenha o mapa
            Map selected = EditorMaps.Form.Selected;
            Editor_Maps_Map_Panorama(selected);
            Editor_Maps_Map_Tiles(selected);
            Editor_Maps_Map_Weather(selected);
            Editor_Maps_Map_Light(selected);
            Editor_Maps_Map_Fog(selected);
            Editor_Maps_Map_Grids(selected);
            Editor_Maps_Map_NPCs(selected);

            // Exibe o que foi renderizado
            Win_Map.Display();
        }

        private static void Editor_Maps_Map_Panorama(Map map)
        {
            EditorMaps form = EditorMaps.Form;

            // Desenha o panorama
            if (form.butVisualization.Checked && map.Panorama > 0)
            {
                Rectangle destiny = new Rectangle
                {
                    X = form.scrlMapX.Value * -form.Grid_Zoom,
                    Y = form.scrlMapY.Value * -form.Grid_Zoom,
                    Size = Size(Tex_Panorama[map.Panorama])
                };
                Render(Win_Map, Tex_Panorama[map.Panorama], EditorMaps.Form.Zoom(destiny));
            }
        }

        private static void Editor_Maps_Map_Tiles(Map map)
        {
            EditorMaps form = EditorMaps.Form;
            MapTileData data;
            int beginX = form.scrlMapX.Value, beginY = form.scrlMapY.Value;
            Color color;

            // Desenha todos os azulejos
            for (byte c = 0; c < map.Layer.Count; c++)
            {
                // Somente se necessário
                if (!form.lstLayers.Items[c].Checked) continue;

                // Transparência da camada
                color = CColor();
                if (form.butEdition.Checked && form.butMNormal.Checked)
                {
                    if (EditorMaps.Form.lstLayers.SelectedIndices.Count > 0)
                        if (c != EditorMaps.Form.lstLayers.SelectedItems[0].Index)
                            color = CColor(255, 255, 255, 150);
                }
                else
                    color = CColor(map.Color.R, map.Color.G, map.Color.B);

                // Continua
                for (int x = beginX; x < Map.Width; x++)
                    for (int y = beginY; y < Map.Height; y++)
                        if (map.Layer[c].Tile[x, y].Texture > 0)
                        {
                            // Dados
                            data = map.Layer[c].Tile[x, y];
                            Rectangle source = new Rectangle(new Point(data.X * Grid, data.Y * Grid), Grid_Size);
                            Rectangle destiny = new Rectangle(new Point((x - beginX) * Grid, (y - beginY) * Grid), Grid_Size);

                            // Desenha o azulejo
                            if (!data.IsAutotile)
                                Render(Win_Map, Tex_Tile[data.Texture], source, form.Zoom(destiny), color);
                            else
                                Editor_Maps_AutoTile(destiny.Location, data, color);
                        }
            }
        }

        private static void Editor_Maps_AutoTile(Point position, MapTileData data, Color color)
        {
            // Desenha todas as partes do azulejo
            for (byte i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 1: position.X += 16; break;
                    case 2: position.Y += 16; break;
                    case 3: position.X += 16; position.Y += 16; break;
                }
                Render(Win_Map, Tex_Tile[data.Texture], new Rectangle(data.Mini[i].X, data.Mini[i].Y, 16, 16), EditorMaps.Form.Zoom(new Rectangle(position, new Size(16, 16))), color);
            }
        }

        private static void Editor_Maps_Map_Fog(Map map)
        {
            // Somente se necessário
            if (map.Fog.Texture <= 0) return;
            if (!EditorMaps.Form.butVisualization.Checked) return;

            // Desenha a fumaça
            Size textureSize = Size(Tex_Fog[map.Fog.Texture]);
            for (int x = -1; x <= Map.Width * Grid / textureSize.Width; x++)
                for (int y = -1; y <= Map.Height * Grid / textureSize.Height; y++)
                {
                    Point position = new Point(x * textureSize.Width + TempMap.Fog_X, y * textureSize.Height + TempMap.Fog_Y);
                    Render(Win_Map, Tex_Fog[map.Fog.Texture], EditorMaps.Form.Zoom(new Rectangle(position, textureSize)), CColor(255, 255, 255, map.Fog.Alpha));
                }
        }

        private static void Editor_Maps_Map_Weather(Map map)
        {
            // Somente se necessário
            if (!EditorMaps.Form.butVisualization.Checked || map.Weather.Type == Weathers.Normal) return;

            // Dados
            byte x = 0;
            if (map.Weather.Type == Weathers.Snowing) x = 32;

            // Desenha as partículas
            for (int i = 0; i < Lists.Weather.Length; i++)
                if (Lists.Weather[i].Visible)
                    Render(Win_Map, Tex_Weather, new Rectangle(x, 0, 32, 32), EditorMaps.Form.Zoom(new Rectangle(Lists.Weather[i].X, Lists.Weather[i].Y, 32, 32)), CColor(255, 255, 255, 150));
        }

        private static void Editor_Maps_Map_Light(Map map)
        {
            EditorMaps form = EditorMaps.Form;
            byte light = (byte)((255 * ((decimal)map.Lighting / 100) - 255) * -1);

            // Somente se necessário
            if (!form.butVisualization.Checked) return;

            // Escuridão
            Win_Map_Lighting.Clear(CColor(0, 0, 0, light));

            // Desenha o ponto iluminado
            if (map.Light.Count > 0)
                for (byte i = 0; i < map.Light.Count; i++)
                {
                    var destiny = new Rectangle
                    {
                        X = map.Light[i].Rec.X - form.scrlMapX.Value,
                        Y = map.Light[i].Rec.Y - form.scrlMapY.Value,
                        Width = map.Light[i].Width,
                        Height = map.Light[i].Height
                    };
                    Render(Win_Map_Lighting, Tex_Lighting, form.Zoom_Grid(destiny), null, new RenderStates(BlendMode.Multiply));
                }

            // Pré visualização
            if (form.butMLighting.Checked)
                Render(Win_Map_Lighting, Tex_Lighting, form.Zoom_Grid(form.Map_Selection), null, new RenderStates(BlendMode.Multiply));

            // Apresenta o que foi renderizado
            Win_Map_Lighting.Display();
            Win_Map.Draw(new Sprite(Win_Map_Lighting.Texture));

            // Ponto de remoção da luz
            if (form.butMLighting.Checked)
                if (map.Light.Count > 0)
                    for (byte i = 0; i < map.Light.Count; i++)
                        RenderRectangle(Win_Map, form.Zoom_Grid(new Rectangle(map.Light[i].Rec.X - form.scrlMapX.Value, map.Light[i].Rec.Y - form.scrlMapY.Value, 1, 1)), CColor(175, 42, 42, 175));

            // Trovoadas
            Render(Win_Map, Tex_Blank, 0, 0, 0, 0, form.picMap.Width, form.picMap.Height, CColor(255, 255, 255, TempMap.Lightning));
        }

        private static void Editor_Maps_Map_Grids(Map map)
        {
            EditorMaps form = EditorMaps.Form;
            Rectangle source = form.Tile_Source, destiny = new Rectangle();
            Point begin = new Point(form.Map_Selection.X - form.scrlMapX.Value, form.Map_Selection.Y - form.scrlMapY.Value);

            // Dados
            destiny.Location = form.Zoom_Grid(begin.X, begin.Y);
            destiny.Size = new Size(source.Width / form.Zoom(), source.Height / form.Zoom());

            // Desenha as grades
            if (form.butGrid.Checked || !form.butGrid.Enabled)
                for (byte x = 0; x < Map.Width; x++)
                    for (byte y = 0; y < Map.Height; y++)
                    {
                        RenderRectangle(Win_Map, x * form.Grid_Zoom, y * form.Grid_Zoom, form.Grid_Zoom, form.Grid_Zoom, CColor(25, 25, 25, 70));
                        Editor_Maps_Map_Zones(map, x, y);
                        Editor_Maps_Map_Attributes(map, x, y);
                        Editor_Maps_Map_DirBlock(map, x, y);
                    }

            if (!form.chkAuto.Checked && form.butMNormal.Checked)
                // Normal
                if (form.butPencil.Checked)
                    Render(Win_Map, Tex_Tile[form.cmbTiles.SelectedIndex + 1], source, destiny);
                // Retângulo
                else if (form.butRectangle.Checked)
                    for (int x = begin.X; x < begin.X + form.Map_Selection.Width; x++)
                        for (int y = begin.Y; y < begin.Y + form.Map_Selection.Height; y++)
                            Render(Win_Map, Tex_Tile[form.cmbTiles.SelectedIndex + 1], source, new Rectangle(form.Zoom_Grid(x, y), destiny.Size));

            // Desenha a grade
            if (!form.butMAttributes.Checked || !form.optA_DirBlock.Checked)
                RenderRectangle(Win_Map, destiny.X, destiny.Y, form.Map_Selection.Width * form.Grid_Zoom, form.Map_Selection.Height * form.Grid_Zoom);
        }

        private static void Editor_Maps_Map_Zones(Map map, byte x, byte y)
        {
            EditorMaps form = EditorMaps.Form;
            Point position = new Point((x - form.scrlMapX.Value) * form.Grid_Zoom, (y - form.scrlMapY.Value) * form.Grid_Zoom);
            byte zoneNum = map.Attribute[x, y].Zone;
            Color color;

            // Apenas se necessário
            if (!EditorMaps.Form.butMZones.Checked) return;
            if (zoneNum == 0) return;

            // Define a cor
            if (zoneNum % 2 == 0)
                color = CColor((byte)((zoneNum * 42) ^ 3), (byte)(zoneNum * 22), (byte)(zoneNum * 33), 150);
            else
                color = CColor((byte)(zoneNum * 33), (byte)(zoneNum * 22), (byte)(zoneNum * 42), 150 ^ 3);

            // Desenha as zonas
            Render(Win_Map, Tex_Blank, new Rectangle(position, new Size(form.Grid_Zoom, form.Grid_Zoom)), color);
            DrawText(Win_Map, zoneNum.ToString(), position.X, position.Y, Color.White);
        }

        private static void Editor_Maps_Map_Attributes(Map map, byte x, byte y)
        {
            EditorMaps form = EditorMaps.Form;
            Point position = new Point((x - form.scrlMapX.Value) * form.Grid_Zoom, (y - EditorMaps.Form.scrlMapY.Value) * form.Grid_Zoom);
            TileAttributes attribute = (TileAttributes)map.Attribute[x, y].Type;
            Color color;
            string letter;

            // Apenas se necessário
            if (!EditorMaps.Form.butMAttributes.Checked) return;
            if (EditorMaps.Form.optA_DirBlock.Checked) return;
            if (attribute == TileAttributes.None) return;

            // Define a cor e a letra
            switch (attribute)
            {
                case TileAttributes.Block: letter = "B"; color = Color.Red; break;
                case TileAttributes.Warp: letter = "T"; color = Color.Blue; break;
                case TileAttributes.Item: letter = "I"; color = Color.Green; break;
                default: return;
            }
            color = new Color(color.R, color.G, color.B, 100);

            // Desenha as Atributos
            Render(Win_Map, Tex_Blank, new Rectangle(position, new Size(form.Grid_Zoom, form.Grid_Zoom)), color);
            DrawText(Win_Map, letter, position.X, position.Y, Color.White);
        }

        private static void Editor_Maps_Map_DirBlock(Map map, byte x, byte y)
        {
            Point tile = new Point(EditorMaps.Form.scrlMapX.Value + x, EditorMaps.Form.scrlMapY.Value + y);
            byte sourceY;

            // Apenas se necessário
            if (!EditorMaps.Form.butMAttributes.Checked) return;
            if (!EditorMaps.Form.optA_DirBlock.Checked) return;

            // Previne erros
            if (tile.X > map.Attribute.GetUpperBound(0)) return;
            if (tile.Y > map.Attribute.GetUpperBound(1)) return;

            for (byte i = 0; i < (byte)Directions.Count; i++)
            {
                // Estado do bloqueio
                if (map.Attribute[tile.X, tile.Y].Block[i])
                    sourceY = 8;
                else
                    sourceY = 0;

                // Renderiza
                Render(Win_Map, Tex_Directions, x * Grid + Block_Position(i).X, y * Grid + Block_Position(i).Y, i * 8, sourceY, 6, 6);
            }
        }

        private static void Editor_Maps_Map_NPCs(Map map)
        {
            EditorMaps form = EditorMaps.Form;

            if (EditorMaps.Form.butMNPCs.Checked)
                for (byte i = 0; i < map.NPC.Count; i++)
                    if (map.NPC[i].Spawn)
                    {
                        Point position = new Point((map.NPC[i].X - form.scrlMapX.Value) * form.Grid_Zoom, (map.NPC[i].Y - form.scrlMapY.Value) * form.Grid_Zoom);

                        // Desenha uma sinalização de onde os NPCBehaviour estão
                        Render(Win_Map, Tex_Blank, new Rectangle(position, new Size(form.Grid_Zoom, form.Grid_Zoom)), CColor(0, 220, 0, 150));
                        DrawText(Win_Map, (i + 1).ToString(), position.X + 10, position.Y + 10, Color.White);
                    }
        }
        #endregion

        #region Tile Editor
        public static void Editor_Tile()
        {
            EditorTiles form = EditorTiles.Form;

            // Somente se necessário
            if (Win_Tile == null) return;

            // Limpa a tela e desenha um fundo transparente
            Win_Tile.Clear();
            Transparent(Win_Tile);

            // Desenha o azulejo e as grades
            Texture texture = Tex_Tile[form.scrlTile.Value];
            Point position = new Point(form.scrlTileX.Value * Grid, form.scrlTileY.Value * Grid);
            Render(Win_Tile, texture, new Rectangle(position, Size(texture)), new Rectangle(new Point(0), Size(texture)));

            for (byte x = 0; x <= form.picTile.Width / Grid; x++)
                for (byte y = 0; y <= form.picTile.Height / Grid; y++)
                {
                    // Desenha os atributos
                    if (form.optAttributes.Checked)
                        Editor_TileAttributes(x, y);
                    // Bloqueios direcionais
                    else if (form.optDirBlock.Checked)
                        Editor_Tile_DirBlock(x, y);

                    // Grades
                    RenderRectangle(Win_Tile, x * Grid, y * Grid, Grid, Grid, CColor(25, 25, 25, 70));
                }

            // Exibe o que foi renderizado
            Win_Tile.Display();
        }

        private static void Editor_TileAttributes(byte x, byte y)
        {
            EditorTiles form = EditorTiles.Form;
            Point tile = new Point(form.scrlTileX.Value + x, form.scrlTileY.Value + y);
            Point point = new Point(x * Grid + Grid / 2 - 5, y * Grid + Grid / 2 - 6);

            // Previne erros
            if (tile.X > Lists.Tile[form.scrlTile.Value].Data.GetUpperBound(0)) return;
            if (tile.Y > Lists.Tile[form.scrlTile.Value].Data.GetUpperBound(1)) return;

            // Desenha uma letra e colore o azulejo referente ao atributo
            switch ((TileAttributes)Lists.Tile[form.scrlTile.Value].Data[tile.X, tile.Y].Attribute)
            {
                case TileAttributes.Block:
                    Render(Win_Tile, Tex_Blank, x * Grid, y * Grid, 0, 0, Grid, Grid, CColor(225, 0, 0, 75));
                    DrawText(Win_Tile, "B", point.X, point.Y, Color.Red);
                    break;
            }
        }

        private static void Editor_Tile_DirBlock(byte x, byte y)
        {
            EditorTiles form = EditorTiles.Form;
            Point tile = new Point(form.scrlTileX.Value + x, form.scrlTileY.Value + y);
            byte sourceY;

            // Previne erros
            if (tile.X > Lists.Tile[form.scrlTile.Value].Data.GetUpperBound(0)) return;
            if (tile.Y > Lists.Tile[form.scrlTile.Value].Data.GetUpperBound(1)) return;

            // Bloqueio total
            if (Lists.Tile[form.scrlTile.Value].Data[x, y].Attribute == (byte)TileAttributes.Block)
            {
                Editor_TileAttributes(x, y);
                return;
            }

            for (byte i = 0; i < (byte)Directions.Count; i++)
            {
                // Estado do bloqueio
                if (Lists.Tile[form.scrlTile.Value].Data[tile.X, tile.Y].Block[i])
                    sourceY = 8;
                else
                    sourceY = 0;

                // Renderiza
                Render(Win_Tile, Tex_Directions, x * Grid + Block_Position(i).X, y * Grid + Block_Position(i).Y, i * 8, sourceY, 6, 6);
            }
        }
        #endregion

        #region Item Editor
        private static void Editor_Item()
        {
            // Somente se necessário
            if (Win_Item == null) return;

            // Desenha o item
            short textureNum = (short)EditorItems.Form.numTexture.Value;
            Win_Item.Clear();
            Transparent(Win_Item);
            if (textureNum > 0 && textureNum < Tex_Item.Length) Render(Win_Item, Tex_Item[textureNum], new Point(0));
            Win_Item.Display();
        }
        #endregion

        #region NPC Editor
        private static void Editor_NPC()
        {
            // Somente se necessário
            if (Win_NPC == null) return;

            // Desenha o NPC
            Character(Win_NPC, (short)EditorNPCs.Form.numTexture.Value);
        }
        #endregion

        #region Class Editors
        private static void Editor_Class()
        {
            // Somente se necessário
            if (Win_Class == null) return;

            // Desenha o NPC
            Character(Win_Class, (short)EditorClasses.Form.numTexture.Value);
        }
        #endregion

        #region Character
        private static void Character(RenderWindow window, short textureNum)
        {
            Texture texture = Tex_Character[textureNum];
            Size size = new Size(Size(texture).Width / 4, Size(texture).Height / 4);

            // Desenha o item
            window.Clear();
            Transparent(window);
            if (textureNum > 0 && textureNum < Tex_Character.Length) Render(window, texture, (int)(window.Size.X - size.Width) / 2, (int)(window.Size.Y - size.Height) / 2, 0, 0, size.Width, size.Height);
            window.Display();
        }
        #endregion

        #region Interface Editor
        public static void Interface()
        {
            // Apenas se necessário
            if (Win_Interface == null) return;

            // Desenha as ferramentas
            Win_Interface.Clear();
            Interface_Order(Lists.Tool.Nodes[(byte)EditorInterface.Form.cmbWindows.SelectedIndex]);
            Win_Interface.Display();
        }

        private static void Interface_Order(TreeNode node)
        {
            for (byte i = 0; i < node.Nodes.Count; i++)
            {
                // Desenha a ferramenta
                Tool tool = (Tool)node.Nodes[i].Tag;
                if (tool.Visible)
                {
                    if (tool is Panel) Panel((Panel)tool);
                    else if (tool is TextBox) TextBox((TextBox)tool);
                    else if (tool is Button) Button((Button)tool);
                    else if (tool is CheckBox) CheckBox((CheckBox)tool);

                    // Pula pra próxima
                    Interface_Order(node.Nodes[i]);
                }
            }
        }

        private static void Button(Button tool)
        {
            // Desenha o botão
            if (tool.Texture_Num < Tex_Button.Length)
                Render(Win_Interface, Tex_Button[tool.Texture_Num], tool.Position, new Color(255, 255, 225, 225));
        }

        private static void Panel(Panel tool)
        {
            // Desenha o painel
            if (tool.Texture_Num < Tex_Panel.Length)
                Render(Win_Interface, Tex_Panel[tool.Texture_Num], tool.Position);
        }

        private static void CheckBox(CheckBox tool)
        {
            // Define as propriedades dos retângulos
            Rectangle recSource = new Rectangle(new Point(), new Size(Tex_CheckBox.Size.X / 2, Size(Tex_CheckBox).Height));
            Rectangle recDestiny = new Rectangle(tool.Position, recSource.Size);

            // Desenha a textura do marcador pelo seu estado 
            if (tool.Checked)
                recSource.Location = new Point(Size(Tex_CheckBox).Width / 2, 0);

            // Desenha o marcador 
            byte margin = 4;
            Render(Win_Interface, Tex_CheckBox, recSource, recDestiny);
            DrawText(Win_Interface, tool.Text, recDestiny.Location.X + Size(Tex_CheckBox).Width / 2 + margin, recDestiny.Location.Y + 1, Color.White);
        }

        private static void TextBox(TextBox tool)
        {
            // Desenha a ferramenta
            Render_Box(Win_Interface, Tex_TextBox, 3, tool.Position, new Size(tool.Width, Size(Tex_TextBox).Height));
        }
        #endregion
    }
}
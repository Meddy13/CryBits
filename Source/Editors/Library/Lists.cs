﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Objects;

class Lists
{
    // Armazenamento de dados
    public static Structures.Options Options = new Structures.Options();
    public static Structures.Server_Data Server_Data = new Structures.Server_Data();
    public static Dictionary<Guid, Class> Class = new Dictionary<Guid, Class>();
    public static Dictionary<Guid, Map> Map = new Dictionary<Guid, Map>();
    public static Structures.Weather[] Weather;
    public static Structures.Tile[] Tile;
    public static Dictionary<Guid, NPC> NPC = new Dictionary<Guid, NPC>();
    public static Dictionary<Guid, Item> Item = new Dictionary<Guid, Item>();
    public static Dictionary<Guid, Shop> Shop = new Dictionary<Guid, Shop>();
    public static TreeNode Tool;

    public static string GetID(object Object) => Object == null ? Guid.Empty.ToString() : ((Structures.Data)Object).ID.ToString();

    public static object GetData<T>(Dictionary<Guid, T> Dictionary, Guid ID)
    {
        if (Dictionary.ContainsKey(ID))
            return Dictionary[ID];
        else
            return null;
    }


    // Estrutura dos itens em gerais
    public class Structures
    {
        public class Data
        {
            public Guid ID;

            public Data(Guid ID)
            {
                this.ID = ID;
            }
        }

        [Serializable]
        public struct Options
        {
            public string Directory_Client;
            public bool Pre_Map_Grid;
            public bool Pre_Map_View;
            public bool Pre_Map_Audio;
            public string Username;
        }

        public struct Server_Data
        {
            public string Game_Name;
            public string Welcome;
            public short Port;
            public byte Max_Players;
            public byte Max_Characters;
            public byte Max_Party_Members;
            public byte Max_Map_Items;
            public byte Num_Points;
            public byte Max_Name_Length;
            public byte Min_Name_Length;
            public byte Max_Password_Length;
            public byte Min_Password_Length;
        }

        public class Inventory
        {
            public Item Item;
            public short Amount;

            public Inventory(Item Item, short Amount)
            {
                this.Item = Item;
                this.Amount = Amount;
            }
            public override string ToString() => Item.Name + " - " + Amount + "x";
        }

        [Serializable]
        public class Tile
        {
            public byte Width;
            public byte Height;
            public Tile_Data[,] Data;
        }

        [Serializable]
        public class Tile_Data
        {
            public byte Attribute;
            public bool[] Block = new bool[(byte)Globals.Directions.Count];
        }

        public class MapMusicProperty : StringConverter
        {

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                //true means show a combobox
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                //true will limit to list. false will show the list, 
                //but allow free-form entry
                return false;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var musicList = new List<string> { "None", "ssss", "dddd", "a" };

                return new StandardValuesCollection(musicList.ToArray());
            }

        }


        public class MapProp
        {
            // Dados gerais
            public short Revision;
            public Map_Tile[,] Tile;
            public List<Map_Layer> Layer = new List<Map_Layer>();
            public List<Map_Light> Light = new List<Map_Light>();
            public BindingList<Map_NPC> NPC = new BindingList<Map_NPC>();
            public Map_Weather Weather = new Map_Weather();

            /*
                     // Limites
        numPanorama.Maximum = Graphics.Tex_Panorama.GetUpperBound(0);
        numFog_Texture.Maximum = Graphics.Tex_Fog.GetUpperBound(0);
        numWidth.Minimum = Globals.Min_Map_Width;
        numHeight.Minimum = Globals.Min_Map_Height;
        numWeather_Intensity.Maximum = Globals.Max_Weather_Intensity;
             */
            // Propriedades
            [Category("General")]
            public string Name { get; set; }
            [Category("General")]
            public Globals.Map_Morals Moral { get; set; }

            [Category("Size")]
            public byte Width { get; set; }
            [Category("Size")]
            public byte Height { get; set; }

            [Category("Misc")]
            public byte Lighting { get; set; }
            [Category("Misc")]
            public Audio.Musics Music { get; set; }

            [Category("Fog"), DisplayName("Fog Alpha")]
            public byte Fog_Alpha
            {
                get => Fog.Alpha;
                set => Fog.Alpha = value;
            }

            [Category("Fog"), DisplayName("Fog X Speed")]
            public sbyte Fog_SpeedX
            {
                get => Fog.Speed_X;
                set => Fog.Speed_X = value;
            }

            [Category("Fog"), DisplayName("Fog Y Speed")]
            public sbyte Fog_SpeedY
            {
                get => Fog.Speed_Y;
                set => Fog.Speed_Y = value;
            }

            [Category("Weather"), DisplayName("Weather Intensity")]
            public byte Weather_SpeedY
            {
                get => Weather.Intensity;
                set => Weather.Intensity = value;
            }

            [Category("Weather"), DisplayName("Weather Type")]
            public Globals.Weathers Weather_Type
            {
                get => Weather.Type;
                set
                {
                    Weather.Type = value;
                    Globals.Weather_Update();
                }
            }
            //Weather
            [Category("weather"), Description("weatherdesc"), DisplayName("weather"),
             DefaultValue("None"), TypeConverter(typeof(MapMusicProperty)), Browsable(true)]
            public string Weathers { get; set; }
            public byte Panorama;
            public Color Color;


            public Map_Fog Fog = new Map_Fog();
            public Map[] Link = new Map[(byte)Globals.Directions.Count];

            // Construtor
      
            public override string ToString() => Name;

            // Verifica se as coordenas estão no limite do mapa
            public bool OutLimit(short x, short y) => x >= Width || y >= Height || x < 0 || y < 0;
        }

        public struct Weather
        {
            public bool Visible;
            public int x;
            public int y;
            public int Speed;
            public int Start;
            public bool Back;
        }




    }
}
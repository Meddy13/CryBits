﻿using Lidgren.Network;
using System;
using System.Drawing;
using System.Windows.Forms;

partial class Receive
{
    // Pacotes do servidor
    private enum Packets
    {
        Alert,
        Connect,
        CreateCharacter,
        Join,
        Classes,
        Characters,
        JoinGame,
        HigherIndex,
        Player_Data,
        Player_Position,
        Player_Vitals,
        Player_Leave,
        Player_Attack,
        Player_Move,
        Player_Direction,
        Player_Experience,
        Player_Inventory,
        Player_Equipments,
        Player_Hotbar,
        JoinMap,
        Map_Revision,
        Map,
        Latency,
        Message,
        NPCs,
        Map_NPCs,
        Map_NPC,
        Map_NPC_Movement,
        Map_NPC_Direction,
        Map_NPC_Vitals,
        Map_NPC_Attack,
        Map_NPC_Died,
        Items,
        Map_Items,
        Party,
        Party_Invitation,
        Trade,
        Trade_Invitation
    }

    public static void Handle(NetIncomingMessage Data)
    {
        // Manuseia os dados recebidos
        switch ((Packets)Data.ReadByte())
        {
            case Packets.Alert: Alert(Data); break;
            case Packets.Connect: Connect(); break;
            case Packets.Join: Join(Data); break;
            case Packets.CreateCharacter: CreateCharacter(); break;
            case Packets.JoinGame: JoinGame(); break;
            case Packets.Classes: Classes(Data); break;
            case Packets.Characters: Characters(Data); break;
            case Packets.HigherIndex: HigherIndex(Data); break;
            case Packets.Player_Data: Player_Data(Data); break;
            case Packets.Player_Position: Player_Position(Data); break;
            case Packets.Player_Vitals: Player_Vitals(Data); break;
            case Packets.Player_Move: Player_Move(Data); break;
            case Packets.Player_Leave: Player_Leave(Data); break;
            case Packets.Player_Direction: Player_Direction(Data); break;
            case Packets.Player_Attack: Player_Attack(Data); break;
            case Packets.Player_Experience: Player_Experience(Data); break;
            case Packets.Player_Inventory: Player_Inventory(Data); break;
            case Packets.Player_Equipments: Player_Equipments(Data); break;
            case Packets.Player_Hotbar: Player_Hotbar(Data); break;
            case Packets.Map_Revision: Map_Revision(Data); break;
            case Packets.Map: Map(Data); break;
            case Packets.JoinMap: JoinMap(); break;
            case Packets.Latency: Latency(); break;
            case Packets.Message: Message(Data); break;
            case Packets.NPCs: NPCs(Data); break;
            case Packets.Map_NPCs: Map_NPCs(Data); break;
            case Packets.Map_NPC: Map_NPC(Data); break;
            case Packets.Map_NPC_Movement: Map_NPC_Movement(Data); break;
            case Packets.Map_NPC_Direction: Map_NPC_Direction(Data); break;
            case Packets.Map_NPC_Vitals: Map_NPC_Vitals(Data); break;
            case Packets.Map_NPC_Attack: Map_NPC_Attack(Data); break;
            case Packets.Map_NPC_Died: Map_NPC_Died(Data); break;
            case Packets.Items: Items(Data); break;
            case Packets.Map_Items: Map_Items(Data); break;
            case Packets.Party: Party(Data); break;
            case Packets.Party_Invitation: Party_Invitation(Data); break;
            case Packets.Trade: Trade(Data); break;
            case Packets.Trade_Invitation: Trade_Invitation(Data); break;
        }
    }

    private static void Alert(NetIncomingMessage Data)
    {
        // Mostra a mensagem
        MessageBox.Show(Data.ReadString());
    }

    private static void Connect()
    {
        // Reseta os valores
        Game.SelectCharacter = 1;

        // Abre o painel de seleção de personagens
        Panels.Menu_Close();
        Panels.Get("SelectCharacter").Visible = true;
    }

    private static void Join(NetIncomingMessage Data)
    {
        // Definir os valores que são enviados do servidor
        Player.MyIndex = Data.ReadByte();
        Player.HigherIndex = Data.ReadByte();

        // Limpa a estrutura dos jogadores
        Lists.Player = new Lists.Structures.Player[Data.ReadByte() + 1];

        for (byte i = 1; i < Lists.Player.Length; i++)
            Clear.Player(i);
    }

    private static void CreateCharacter()
    {
        // Reseta os valores
        TextBoxes.Get("CreateCharacter_Name").Text = string.Empty;
        CheckBoxes.Get("GenderMale").Checked = true;
        CheckBoxes.Get("GenderFemale").Checked = false;
        Game.CreateCharacter_Class = 1;
        Game.CreateCharacter_Tex = 0;

        // Abre o painel de criação de personagem
        Panels.Menu_Close();
        Panels.Get("CreateCharacter").Visible = true;
    }

    private static void Classes(NetIncomingMessage Data)
    {
        int Amount = Data.ReadByte();

        // Recebe os dados das classes
        Lists.Class = new Lists.Structures.Class[Amount + 1];

        for (byte i = 1; i <= Amount; i++)
        {
            // Recebe os dados do personagem
            Lists.Class[i] = new Lists.Structures.Class();
            Lists.Class[i].Name = Data.ReadString();
            Lists.Class[i].Description = Data.ReadString();
            Lists.Class[i].Tex_Male = new short[Data.ReadByte()];
            for (byte n = 0; n < Lists.Class[i].Tex_Male.Length; n++) Lists.Class[i].Tex_Male[n] = Data.ReadInt16();
            Lists.Class[i].Tex_Female = new short[Data.ReadByte()];
            for (byte n = 0; n < Lists.Class[i].Tex_Female.Length; n++) Lists.Class[i].Tex_Female[n] = Data.ReadInt16();
        }
    }

    private static void Characters(NetIncomingMessage Data)
    {
        byte Amount = Data.ReadByte();

        // Redimensiona a lista
        Lists.Server_Data.Max_Characters = Amount;
        Lists.Characters = new Lists.Structures.Characters[Amount + 1];

        for (byte i = 1; i <= Amount; i++)
        {
            // Recebe os dados do personagem
            Lists.Characters[i] = new Lists.Structures.Characters
            {
                Name = Data.ReadString(),
                Class = Data.ReadByte(),
                Texture_Num = Data.ReadInt16(),
                Genre = Data.ReadBoolean(),
                Level = Data.ReadInt16()
            };
        }
    }

    private static void JoinGame()
    {
        // Reseta os valores
        Chat.Order = new System.Collections.Generic.List<Chat.Structure>();
        Chat.Lines_First = 0;
        TextBoxes.Get("Chat").Text = string.Empty;
        CheckBoxes.Get("Options_Sounds").Checked = Lists.Options.Sounds;
        CheckBoxes.Get("Options_Musics").Checked = Lists.Options.Musics;
        CheckBoxes.Get("Options_Chat").Checked = Chat.Text_Visible = Lists.Options.Chat;
        Game.Need_Information = 0;
        Loop.Chat_Timer = Loop.Chat_Timer = Environment.TickCount + 10000;
        Player.Me.Party = new byte[0];

        // Fecha os paineis 
        Panels.Get("Menu_Character").Visible = false;
        Panels.Get("Menu_Inventory").Visible = false;
        Panels.Get("Menu_Options").Visible = false;
        Panels.Get("Chat").Visible = false;
        Panels.Get("Drop").Visible = false;
        Panels.Get("Party_Invitation").Visible = false;

        // Abre o jogo
        Audio.Music.Stop();
        Tools.CurrentWindow = Tools.Windows.Game;
    }

    private static void HigherIndex(NetIncomingMessage Data)
    {
        // Define o número maior de índice
        Player.HigherIndex = Data.ReadByte();
    }

    private static void Map_Revision(NetIncomingMessage Data)
    {
        bool Needed = false;
        short Map_Num = Data.ReadInt16();

        // Limpa todos os outros jogadores
        for (byte i = 1; i <= Player.HigherIndex; i++)
            if (i != Player.MyIndex)
                Clear.Player(i);

        // Verifica se é necessário baixar os dados do mapa
        if (System.IO.File.Exists(Directories.Maps_Data.FullName + Map_Num + Directories.Format))
        {
            Read.Map(Map_Num);
            if (Lists.Map.Revision != Data.ReadInt16())
                Needed = true;
        }
        else
            Needed = true;

        // Solicita os dados do mapa
        Send.RequestMap(Needed);

        // Reseta os sangues do mapa
        Lists.Temp_Map.Blood = new System.Collections.Generic.List<Lists.Structures.Map_Blood>();
    }

    private static void Map(NetIncomingMessage Data)
    {
        // Define os dados
        short Map_Num = Data.ReadInt16();

        // Lê os dados
        Lists.Map.Revision = Data.ReadInt16();
        Lists.Map.Name = Data.ReadString();
        Lists.Map.Width = Data.ReadByte();
        Lists.Map.Height = Data.ReadByte();
        Lists.Map.Moral = Data.ReadByte();
        Lists.Map.Panorama = Data.ReadByte();
        Lists.Map.Music = Data.ReadByte();
        Lists.Map.Color = Data.ReadInt32();
        Lists.Map.Weather.Type = Data.ReadByte();
        Lists.Map.Weather.Intensity = Data.ReadByte();
        Lists.Map.Fog.Texture = Data.ReadByte();
        Lists.Map.Fog.Speed_X = Data.ReadSByte();
        Lists.Map.Fog.Speed_Y = Data.ReadSByte();
        Lists.Map.Fog.Alpha = Data.ReadByte();
        Data.ReadByte(); // Luz global
        Data.ReadByte(); // Iluminação

        // Ligações
        Lists.Map.Link = new short[(byte)Game.Directions.Count];
        for (short i = 0; i < (short)Game.Directions.Count; i++)
            Lists.Map.Link[i] = Data.ReadInt16();

        // Azulejos
        byte Num_Layers = Data.ReadByte();

        // Redimensiona os dados
        Lists.Map.Tile = new Lists.Structures.Map_Tile[Lists.Map.Width + 1, Lists.Map.Height + 1];
        for (byte x = 0; x <= Lists.Map.Width; x++)
            for (byte y = 0; y <= Lists.Map.Height; y++)
                Lists.Map.Tile[x, y].Data = new Lists.Structures.Map_Tile_Data[(byte)global::Map.Layers.Amount, Num_Layers + 1];

        // Lê os azulejos
        for (byte i = 0; i <= Num_Layers; i++)
        {
            // Dados básicos
            Data.ReadString(); // Name
            byte t = Data.ReadByte(); // Tipo

            // Azulejos
            for (byte x = 0; x <= Lists.Map.Width; x++)
                for (byte y = 0; y <= Lists.Map.Height; y++)
                {
                    Lists.Map.Tile[x, y].Data[t, i].X = Data.ReadByte();
                    Lists.Map.Tile[x, y].Data[t, i].Y = Data.ReadByte();
                    Lists.Map.Tile[x, y].Data[t, i].Tile = Data.ReadByte();
                    Lists.Map.Tile[x, y].Data[t, i].Automatic = Data.ReadBoolean();
                    Lists.Map.Tile[x, y].Data[t, i].Mini = new Point[4];
                }
        }

        // Dados específicos dos azulejos
        for (byte x = 0; x <= Lists.Map.Width; x++)
            for (byte y = 0; y <= Lists.Map.Height; y++)
            {
                Lists.Map.Tile[x, y].Attribute = Data.ReadByte();
                Data.ReadInt16(); // Dado 1
                Data.ReadInt16(); // Dado 2
                Data.ReadInt16(); // Dado 3
                Data.ReadInt16(); // Dado 4
                Data.ReadByte(); // Zona

                // Bloqueio direcional
                Lists.Map.Tile[x, y].Block = new bool[(byte)Game.Directions.Count];
                for (byte i = 0; i < (byte)Game.Directions.Count; i++)
                    Lists.Map.Tile[x, y].Block[i] = Data.ReadBoolean();
            }

        // Luzes
        Lists.Map.Light = new Lists.Structures.Map_Light[Data.ReadByte()];
        if (Lists.Map.Light.GetUpperBound(0) > 0)
            for (byte i = 0; i < Lists.Map.Light.Length; i++)
            {
                Lists.Map.Light[i].X = Data.ReadByte();
                Lists.Map.Light[i].Y = Data.ReadByte();
                Lists.Map.Light[i].Width = Data.ReadByte();
                Lists.Map.Light[i].Height = Data.ReadByte();
            }

        // NPCs
        Lists.Map.NPC = new short[Data.ReadByte() + 1];
        for (byte i = 1; i < Lists.Map.NPC.Length; i++)
        {
            Lists.Map.NPC[i] = Data.ReadInt16();
            Data.ReadByte(); // Zone
            Data.ReadBoolean(); // Spawn
            Data.ReadByte(); // X
            Data.ReadByte(); // Y
        }

        // Salva o mapa
        Write.Map(Map_Num);

        // Redimensiona as partículas do clima
        global::Map.Weather_Update();
        global::Map.Autotile.Update();
    }

    private static void JoinMap()
    {
        // Se tiver, reproduz a música de fundo do mapa
        if (Lists.Map.Music > 0)
            Audio.Music.Play((Audio.Musics)Lists.Map.Music);
        else
            Audio.Music.Stop();
    }

    private static void Latency()
    {
        // Define a latência
        Game.Latency = Environment.TickCount - Game.Latency_Send;
    }

    private static void Message(NetIncomingMessage Data)
    {
        // Adiciona a mensagem
        string Text = Data.ReadString();
        Color Color = Color.FromArgb(Data.ReadInt32());
        Chat.AddText(Text, new SFML.Graphics.Color(Color.R, Color.G, Color.B));
    }

    private static void Items(NetIncomingMessage Data)
    {
        // Quantidade de itens
        Lists.Item = new Lists.Structures.Items[Data.ReadInt16()];

        for (short i = 1; i < Lists.Item.Length; i++)
        {
            // Redimensiona os valores necessários 
            Lists.Item[i].Potion_Vital = new short[(byte)Game.Vitals.Count];
            Lists.Item[i].Equip_Attribute = new short[(byte)Game.Attributes.Count];

            // Lê os dados
            Lists.Item[i].Name = Data.ReadString();
            Lists.Item[i].Description = Data.ReadString();
            Lists.Item[i].Texture = Data.ReadInt16();
            Lists.Item[i].Type = Data.ReadByte();
            Data.ReadInt16(); // Price
            Data.ReadBoolean(); // Stackable
            Lists.Item[i].Bind = (Game.BindOn)Data.ReadByte();
            Lists.Item[i].Rarity = Data.ReadByte();
            Lists.Item[i].Req_Level = Data.ReadInt16();
            Lists.Item[i].Req_Class = Data.ReadByte();
            Lists.Item[i].Potion_Experience = Data.ReadInt32();
            for (byte v = 0; v < (byte)Game.Vitals.Count; v++) Lists.Item[i].Potion_Vital[v] = Data.ReadInt16();
            Lists.Item[i].Equip_Type = Data.ReadByte();
            for (byte a = 0; a < (byte)Game.Attributes.Count; a++) Lists.Item[i].Equip_Attribute[a] = Data.ReadInt16();
            Lists.Item[i].Weapon_Damage = Data.ReadInt16();
        }
    }

    private static void Map_Items(NetIncomingMessage Data)
    {
        // Quantidade
        Lists.Temp_Map.Item = new Lists.Structures.Map_Items[Data.ReadInt16() + 1];

        // Lê os dados de todos
        for (byte i = 1; i < Lists.Temp_Map.Item.Length; i++)
        {
            // Geral
            Lists.Temp_Map.Item[i].Index = Data.ReadInt16();
            Lists.Temp_Map.Item[i].X = Data.ReadByte();
            Lists.Temp_Map.Item[i].Y = Data.ReadByte();
        }
    }

    private static void Party(NetIncomingMessage Data)
    {
        // Lê os dados do grupo
        Player.Me.Party = new byte[Data.ReadByte()];
        for (byte i = 0; i < Player.Me.Party.Length; i++) Player.Me.Party[i] = Data.ReadByte();
    }
    
    private static void Party_Invitation(NetIncomingMessage Data)
    {
        // Abre a janela de convite para o grupo
        Game.Party_Invitation = Data.ReadString();
        Panels.Get("Party_Invitation").Visible = true;
    }

    private static void Trade(NetIncomingMessage Data)
    {
        // Abre ou fecha a troca
        Player.Me.Trade = Data.ReadByte();
        Panels.Get("Trade").Visible = Player.Me.Trade != 0;
    }

    private static void Trade_Invitation(NetIncomingMessage Data)
    {
        // Abre a janela de convite para o grupo
        Game.Trade_Invitation = Data.ReadString();
        Panels.Get("Trade_Invitation").Visible = true;
    }
}
﻿using System;
using System.Drawing;

class Game
{
    // Números aleatórios
    public static Random Random = new Random();

    // Dimensão das grades 
    public const byte Grid = 32;

    // Medida de calculo do atraso do jogo
    public static short FPS;

    // Ataque
    public const short Attack_Speed = 750;

    // Animação
    public const byte Animation_Amount = 4;
    public const byte Animation_Stopped = 1;
    public const byte Animation_Right = 0;
    public const byte Animation_Left = 2;
    public const byte Animation_Attack = 2;

    // Movimentação
    public const byte Movement_Up = 3;
    public const byte Movement_Down = 0;
    public const byte Movement_Left = 1;
    public const byte Movement_Right = 2;

    // Visão do jogador
    private static Point Sight_Offset;
    public static Rectangle Sight;

    // Bloqueio direcional
    public const byte Max_DirBlock = 3;

    // Tamanho da tela
    public const short Screen_Width = Map_Width * Grid;
    public const short Screen_Height = Map_Height  * Grid;

    // Limites em geral
    public const byte Max_Inventory = 30;
    public const byte Max_Hotbar = 10;

    // Limitações dos mapas
    public const byte Map_Width = 25;
    public const byte Map_Height = 19;

    public enum Attributes
    {
        Strength,
        Resistance,
        Intelligence,
        Agility,
        Vitality,
        Count
    }

    public enum Directions
    {
        Up,
        Down,
        Left,
        Right,
        Count
    }

    public enum Movements
    {
        Stopped,
        Walking,
        Moving
    }

    public enum Messages
    {
        Game,
        Map,
        Global,
        Private
    }

    public enum Vitals
    {
        HP,
        MP,
        Count
    }

    public enum NPCs
    {
        Friendly,
        AttackOnSight,
        AttackWhenAttacked,
        ShopKeeper
    }

    public enum Target
    {
        Player = 1,
        NPC
    }

    public enum Items
    {
        None,
        Equipment,
        Potion
    }

    public enum Equipments
    {
        Weapon,
        Armor,
        Helmet,
        Shield,
        Amulet,
        Count
    }

    public enum Hotbar
    {
        None,
        Item
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Count
    }

    public enum BindOn
    {
        None,
        Pickup,
        Equip,
        Count
    }

    public enum Trade_Status
    {
        Waiting,
        Confirmed,
        Accepted,
        Declined
    }

    // Converte o valor em uma posição adequada à camera
    public static int ConvertX(int x) => x - (Sight.X * Grid) - Sight_Offset.X;
    public static int ConvertY(int y) => y - (Sight.Y * Grid) - Sight_Offset.Y;

    public static Directions ReverseDirection(Directions Direction)
    {
        // Retorna a direção inversa
        switch (Direction)
        {
            case Directions.Up: return Directions.Down;
            case Directions.Down: return Directions.Up;
            case Directions.Left: return Directions.Right;
            case Directions.Right: return Directions.Left;
            default: return Directions.Count;
        }
    }

    public static short Find_Shop_Bought(Lists.Structures.Item Item)
    {
        for (byte i = 0; i < Utilities.Shop_Open.Bought.Length; i++)
            if (Utilities.Shop_Open.Bought[i].Item == Item)
                return i;

        return -1;
    }

    public static void UpdateCamera()
    {
        // Offset da tela
        Sight_Offset.X = Player.Me.X2 + Grid;
        Sight_Offset.Y = Player.Me.Y2 + Grid;

        // Início da tela
        Sight.X = Player.Me.X - Map_Width / 2 ;
        Sight.Y = Player.Me.Y - Map_Height / 2 ;

        // Reajusta a posição horizontal da tela
        if (Sight.X < 0)
        {
            Sight_Offset.X = 0;
            if (Sight.X == -1 && Player.Me.X2 > 0) Sight_Offset.X = Player.Me.X2;
            Sight.X = 0;
        }

        // Reajusta a posição vertical da tela
        if (Sight.Y < 0)
        {
            Sight_Offset.Y = 0;
            if (Sight.Y == -1 && Player.Me.Y2 > 0) Sight_Offset.Y = Player.Me.Y2;
            Sight.Y = 0;
        }

        // Final da tela
        Sight.Width = Sight.X + Map_Width;
        Sight.Height = Sight.Y + Map_Height;

        // Reajusta a posição horizontal da tela
        if (Sight.Width >= Map_Width)
        {
            Sight_Offset.X = Grid;
            if (Sight.Width == Map_Width && Player.Me.X2 < 0) Sight_Offset.X = Player.Me.X2 + Grid;
            Sight.Width = Map_Width -1;
            Sight.X = Sight.Width - Map_Width -1;
        }

        // Reajusta a posição vertical da tela
        if (Sight.Height >= Map_Height)
        {
            Sight_Offset.Y = Grid;
            if (Sight.Height == Map_Height  && Player.Me.Y2 < 0) Sight_Offset.Y = Player.Me.Y2 + Grid;
            Sight.Height = Map_Height -1;
            Sight.Y = Sight.Height - Map_Height;
        }
    }
}
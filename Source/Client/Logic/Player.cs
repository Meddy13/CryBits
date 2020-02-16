﻿using Lidgren.Network;
using SFML.Graphics;
using System;
using System.Drawing;
using SFML.Window;

class Player
{
    // O maior índice dos jogadores conectados
    public static byte HigherIndex;

    // Inventário
    public static Lists.Structures.Inventory[] Inventory = new Lists.Structures.Inventory[Game.Max_Inventory + 1];
    public static byte Inventory_Change;

    // Hotbar
    public static Lists.Structures.Hotbar[] Hotbar = new Lists.Structures.Hotbar[Game.Max_Hotbar + 1];
    public static byte Hotbar_Change;

    // O próprio jogador
    public static byte MyIndex;
    public static Lists.Structures.Player Me
    {
        get
        {
            return Lists.Player[MyIndex];
        }
        set
        {
            Lists.Player[MyIndex] = value;
        }
    }

    public static bool IsPlaying(byte Index)
    {
        // Verifica se o jogador está dentro do jogo
        if (MyIndex > 0 && !string.IsNullOrEmpty(Lists.Player[Index].Name))
            return true;
        else
            return false;
    }

    public static void Logic()
    {
        // Verificações
        CheckMovement();
        CheckAttack();

        // Lógica dos jogadores
        for (byte i = 1; i <= Player.HigherIndex; i++)
        {
            // Dano
            if (Lists.Player[i].Hurt + 325 < Environment.TickCount) Lists.Player[i].Hurt = 0;

            // Movimentaçãp
            ProcessMovement(i);
        }
    }

    private static bool CanMove()
    {
        // Não mover se já estiver tentando movimentar-se
        return Lists.Player[MyIndex].Movement == Game.Movements.Stopped;
    }

    private static void CheckMovement()
    {
        if (Me.Movement > 0) return;

        // Move o personagem
        if (Keyboard.IsKeyPressed(Keyboard.Key.Up)) Move(Game.Directions.Up);
        else if (Keyboard.IsKeyPressed(Keyboard.Key.Down)) Move(Game.Directions.Down);
        else if (Keyboard.IsKeyPressed(Keyboard.Key.Left)) Move(Game.Directions.Left);
        else if (Keyboard.IsKeyPressed(Keyboard.Key.Right)) Move(Game.Directions.Right);
    }

    public static void Move(Game.Directions Direction)
    {
        // Verifica se o jogador pode se mover
        if (!CanMove()) return;

        // Define a direção do jogador
        if (Lists.Player[MyIndex].Direction != Direction)
        {
            Lists.Player[MyIndex].Direction = Direction;
            Send.Player_Direction();
        }

        // Verifica se o azulejo seguinte está livre
        if (Map.Tile_Blocked(Lists.Player[MyIndex].Map, Lists.Player[MyIndex].X, Lists.Player[MyIndex].Y, Direction)) return;
        
        // Define a velocidade que o jogador se move
        if (Keyboard.IsKeyPressed(Keyboard.Key.LShift))
            Lists.Player[MyIndex].Movement = Game.Movements.Running;
        else
            Lists.Player[MyIndex].Movement = Game.Movements.Walking;

        // Movimento o jogador
        Send.Player_Move();

        // Define a Posição exata do jogador
        switch (Direction)
        {
            case Game.Directions.Up: Lists.Player[MyIndex].Y2 = Game.Grid; Lists.Player[MyIndex].Y -= 1; break;
            case Game.Directions.Down: Lists.Player[MyIndex].Y2 = Game.Grid * -1; Lists.Player[MyIndex].Y += 1; break;
            case Game.Directions.Right: Lists.Player[MyIndex].X2 = Game.Grid * -1; Lists.Player[MyIndex].X += 1; break;
            case Game.Directions.Left: Lists.Player[MyIndex].X2 = Game.Grid; Lists.Player[MyIndex].X -= 1; break;
        }
    }

    private static void ProcessMovement(byte Index)
    {
        // VOLTAR AQUI
        byte Speed = 0;
        short x = Lists.Player[Index].X2, y = Lists.Player[Index].Y2;

        // Reseta a animação se necessário
        //if (Lists.Player[Index].Animation == Game.Animation_Stopped) Lists.Player[Index].Animation = Game.Animation_Right;

        // Define a velocidade que o jogador se move
        switch (Lists.Player[Index].Movement)
        {
            case Game.Movements.Walking: Speed = 2; break;
            case Game.Movements.Running: Speed = 3; break;
            case Game.Movements.Stopped:
                // Reseta os dados
                Lists.Player[Index].X2 = 0;
                Lists.Player[Index].Y2 = 0;
                return;
        }

        // Define a Posição exata do jogador
        switch (Lists.Player[Index].Direction)
        {
            case Game.Directions.Up: Lists.Player[Index].Y2 -= Speed; break;
            case Game.Directions.Down: Lists.Player[Index].Y2 += Speed; break;
            case Game.Directions.Right: Lists.Player[Index].X2 += Speed; break;
            case Game.Directions.Left: Lists.Player[Index].X2 -= Speed; break;
        }

        // Verifica se não passou do limite
        if (x > 0 && Lists.Player[Index].X2 < 0) Lists.Player[Index].X2 = 0;
        if (x < 0 && Lists.Player[Index].X2 > 0) Lists.Player[Index].X2 = 0;
        if (y > 0 && Lists.Player[Index].Y2 < 0) Lists.Player[Index].Y2 = 0;
        if (y < 0 && Lists.Player[Index].Y2 > 0) Lists.Player[Index].Y2 = 0;

        // Alterar as animações somente quando necessário
        if (Lists.Player[Index].Direction == Game.Directions.Right || Lists.Player[Index].Direction == Game.Directions.Down)
        {
            if (Lists.Player[Index].X2 < 0 || Lists.Player[Index].Y2 < 0)
                return;
        }
        else if (Lists.Player[Index].X2 > 0 || Lists.Player[Index].Y2 > 0)
            return;

        //// Define as animações
        Lists.Player[Index].Movement = Game.Movements.Stopped;
        //if (Lists.Player[Index].Animation == Game.Animation_Left)
        //    Lists.Player[Index].Animation = Game.Animation_Right;
        //else
        //    Lists.Player[Index].Animation = Game.Animation_Left;
    }

    private static void CheckAttack()
    {
        // Reseta o ataque
        if (Me.Attack_Timer + Game.Attack_Speed < Environment.TickCount)
        {
            Me.Attack_Timer = 0;
            Me.Attacking = false;
        }

        // Somente se estiver pressionando a tecla de ataque e não estiver atacando
        if (!Keyboard.IsKeyPressed(Keyboard.Key.LControl)) return;
        if (Me.Attack_Timer > 0) return;

        // Envia os dados para o servidor
        Me.Attack_Timer = Environment.TickCount;
        Send.Player_Attack();
    }

    public static void CollectItem()
    {
        bool HasItem = false, HasSlot = false;

        // Previne erros
        if (TextBoxes.Focused != null) return;

        // Verifica se tem algum item nas coordenadas 
        for (byte i = 1; i < Lists.Temp_Map.Item.Length; i++)
            if (Lists.Temp_Map.Item[i].X == Me.X && Lists.Temp_Map.Item[i].Y == Me.Y)
                HasItem = true;

        // Verifica se tem algum espaço vazio no inventário
        for (byte i = 1; i <= Game.Max_Inventory; i++)
            if (Inventory[i].Item_Num == 0)
                HasSlot = true;

        // Somente se necessário
        if (!HasItem) return;
        if (!HasSlot) return;
        if (Environment.TickCount <= Me.Collect_Timer + 250) return;

        // Coleta o item
        Send.CollectItem();
        Me.Collect_Timer = Environment.TickCount;
    }
}

partial class Graphics
{
    private static void Player_Character(byte Index)
    {
        // Desenha o jogador
        Player_Texture(Index);
        Player_Name(Index);
        Player_Bars(Index);
    }

    private static void Player_Texture(byte Index)
    {
        // VOLTAR AQUI
        //byte Column = Game.Animation_Stopped;
        int x = Lists.Player[Index].X * Game.Grid + Lists.Player[Index].X2, y = Lists.Player[Index].Y * Game.Grid + Lists.Player[Index].Y2;
        //short x2 = Lists.Player[Index].X2, y2 = Lists.Player[Index].Y2;
        //bool Hurt = false;
        short Texture_Num = Lists.Player[Index].Texture_Num;

        //// Previne sobrecargas
        //if (Texture <= 0 || Texture > Tex_Character.GetUpperBound(0)) return;

        //// Define a animação
        //if (Lists.Player[Index].Attacking && Lists.Player[Index].Attack_Timer + Game.Attack_Speed / 2 > Environment.TickCount)
        //    Column = Game.Animation_Attack;
        //else
        //{
        //    if (x2 > 8 && x2 < Game.Grid) Column = Lists.Player[Index].Animation;
        //    if (x2 < -8 && x2 > Game.Grid * -1) Column = Lists.Player[Index].Animation;
        //    if (y2 > 8 && y2 < Game.Grid) Column = Lists.Player[Index].Animation;
        //    if (y2 < -8 && y2 > Game.Grid * -1) Column = Lists.Player[Index].Animation;
        //}

        //// Demonstra que o personagem está sofrendo dano
        //if (Lists.Player[Index].Hurt > 0) Hurt = true;

        //// Desenha o jogador
        Character(Texture_Num, new Point(Game.ConvertX(x), Game.ConvertY(y)), Game.Movements.Stopped, Lists.Player[Index].Direction);
    }

    private static void Player_Bars(byte Index)
    {
        short Texture_Num = Lists.Player[Index].Texture_Num;
        int x = Lists.Player[Index].X * Game.Grid + Lists.Player[Index].X2, y = Lists.Player[Index].Y * Game.Grid + Lists.Player[Index].Y2;
        Point Position = new Point(Game.ConvertX(x), Game.ConvertY(y) + Lists.Sprite[Texture_Num].Frame_Height + 4);
        int FullWidth = Lists.Sprite[Texture_Num].Frame_Width;
        short Value = Lists.Player[Index].Vital[(byte)Game.Vitals.HP];

        // Apenas se necessário
        if (Value <= 0 || Value >= Lists.Player[Index].Max_Vital[(byte)Game.Vitals.HP]) return;

        // Cálcula a largura da barra
        int Width = (Value * FullWidth) / Lists.Player[Index].Max_Vital[(byte)Game.Vitals.HP];

        // Desenha as barras 
        Render(Tex_Bars, Position.X, Position.Y, 0, 4, FullWidth, 4);
        Render(Tex_Bars, Position.X, Position.Y, 0, 0, Width, 4);
    }

    private static void Player_Name(byte Index)
    {
        short Texture_Num = Lists.Player[Index].Texture_Num;
        int x = Lists.Player[Index].X * Game.Grid + Lists.Player[Index].X2, y = Lists.Player[Index].Y * Game.Grid + Lists.Player[Index].Y2;

        // Posição do texto
        Point Position = new Point();
        Position.X = x + (Lists.Sprite[Texture_Num].Frame_Width - Tools.MeasureString(Lists.Player[Index].Name)) / 2;
        Position.Y = y - Lists.Sprite[Texture_Num].Frame_Height / 2;

        // Cor do texto
        SFML.Graphics.Color Color;
        if (Index == Player.MyIndex) Color = SFML.Graphics.Color.Yellow;
        else Color = SFML.Graphics.Color.White;

        // Desenha o texto
        DrawText(Lists.Player[Index].Name, Game.ConvertX(Position.X), Game.ConvertY(Position.Y), Color);
    }
}
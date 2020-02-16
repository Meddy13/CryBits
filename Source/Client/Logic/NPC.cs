﻿using Lidgren.Network;
using SFML.Graphics;
using System;
using System.Drawing;

class NPC
{
    public static void Logic()
    {
        // Lógica dos NPCs
        for (byte i = 1; i < Lists.Temp_Map.NPC.Length; i++)
            if (Lists.Temp_Map.NPC[i].Index > 0)
            {
                // Dano
                if (Lists.Temp_Map.NPC[i].Hurt + 325 < Environment.TickCount) Lists.Temp_Map.NPC[i].Hurt = 0;

                // Movimento
                ProcessMovement(i);
            }
    }

    private static void ProcessMovement(byte Index)
    {
        // VOLTAR AQUI
        //byte Speed = 0;
        //short x = Lists.Temp_Map.NPC[Index].X2, y = Lists.Temp_Map.NPC[Index].Y2;

        //// Reseta a animação se necessário
        //if (Lists.Temp_Map.NPC[Index].Animation == Game.Animation_Stopped) Lists.Temp_Map.NPC[Index].Animation = Game.Animation_Right;

        //// Define a velocidade que o jogador se move
        //switch (Lists.Temp_Map.NPC[Index].Movement)
        //{
        //    case Game.Movements.Walking: Speed = 2; break;
        //    case Game.Movements.Moving: Speed = 3; break;
        //    case Game.Movements.Stopped:
        //        // Reseta os dados
        //        Lists.Temp_Map.NPC[Index].X2 = 0;
        //        Lists.Temp_Map.NPC[Index].Y2 = 0;
        //        return;
        //}

        //// Define a Posição exata do jogador
        //switch (Lists.Temp_Map.NPC[Index].Direction)
        //{
        //    case Game.Directions.Up: Lists.Temp_Map.NPC[Index].Y2 -= Speed; break;
        //    case Game.Directions.Down: Lists.Temp_Map.NPC[Index].Y2 += Speed; break;
        //    case Game.Directions.Right: Lists.Temp_Map.NPC[Index].X2 += Speed; break;
        //    case Game.Directions.Left: Lists.Temp_Map.NPC[Index].X2 -= Speed; break;
        //}

        //// Verifica se não passou do limite
        //if (x > 0 && Lists.Temp_Map.NPC[Index].X2 < 0) Lists.Temp_Map.NPC[Index].X2 = 0;
        //if (x < 0 && Lists.Temp_Map.NPC[Index].X2 > 0) Lists.Temp_Map.NPC[Index].X2 = 0;
        //if (y > 0 && Lists.Temp_Map.NPC[Index].Y2 < 0) Lists.Temp_Map.NPC[Index].Y2 = 0;
        //if (y < 0 && Lists.Temp_Map.NPC[Index].Y2 > 0) Lists.Temp_Map.NPC[Index].Y2 = 0;

        //// Alterar as animações somente quando necessário
        //if (Lists.Temp_Map.NPC[Index].Direction == Game.Directions.Right || Lists.Temp_Map.NPC[Index].Direction == Game.Directions.Down)
        //{
        //    if (Lists.Temp_Map.NPC[Index].X2 < 0 || Lists.Temp_Map.NPC[Index].Y2 < 0)
        //        return;
        //}
        //else if (Lists.Temp_Map.NPC[Index].X2 > 0 || Lists.Temp_Map.NPC[Index].Y2 > 0)
        //    return;

        //// Define as animações
        //Lists.Temp_Map.NPC[Index].Movement = Game.Movements.Stopped;
        //if (Lists.Temp_Map.NPC[Index].Animation == Game.Animation_Left)
        //    Lists.Temp_Map.NPC[Index].Animation = Game.Animation_Right;
        //else
        //    Lists.Temp_Map.NPC[Index].Animation = Game.Animation_Left;
    }
}

partial class Graphics
{
    private static void NPC(byte Index)
    {
        int x2 = Lists.Temp_Map.NPC[Index].X2, y2 = Lists.Temp_Map.NPC[Index].Y2;
        byte Column = 0;
        bool Hurt = false;
        short Texture = Lists.NPC[Lists.Temp_Map.NPC[Index].Index].Texture;

        // Previne sobrecargas
        if (Texture <= 0 || Texture > Tex_Character.GetUpperBound(0)) return;

        // VOLTAR AQUI
        // Define a animação
        //if (Lists.Temp_Map.NPC[Index].Attacking && Lists.Temp_Map.NPC[Index].Attack_Timer + Game.Attack_Speed / 2 > Environment.TickCount)
        //    Column = Game.Animation_Attack;
        //else
        //{
        //    if (x2 > 8 && x2 < Game.Grid) Column = Lists.Temp_Map.NPC[Index].Animation;
        //    else if (x2 < -8 && x2 > Game.Grid * -1) Column = Lists.Temp_Map.NPC[Index].Animation;
        //    else if (y2 > 8 && y2 < Game.Grid) Column = Lists.Temp_Map.NPC[Index].Animation;
        //    else if (y2 < -8 && y2 > Game.Grid * -1) Column = Lists.Temp_Map.NPC[Index].Animation;
        //}

        //// Demonstra que o personagem está sofrendo dano
        //if (Lists.Temp_Map.NPC[Index].Hurt > 0) Hurt = true;

        //// Desenha o jogador
        //int x = Lists.Temp_Map.NPC[Index].X * Game.Grid + x2;
        //int y = Lists.Temp_Map.NPC[Index].Y * Game.Grid + y2;
        //Character(Texture, new Point(Game.ConvertX(x), Game.ConvertY(y)), Lists.Temp_Map.NPC[Index].Direction, Column, Hurt);
        //NPC_Name(Index, x, y);
        //NPC_Bars(Index, x, y);
    }

    private static void NPC_Name(byte Index, int x, int y)
    {
        Point Position = new Point(); SFML.Graphics.Color Color;
        short NPC_Num = Lists.Temp_Map.NPC[Index].Index;
        int Name_Size = Tools.MeasureString(Lists.NPC[NPC_Num].Name);
        short Texture_Num = Lists.NPC[NPC_Num].Texture;

        // Posição do texto
        Position.X = x + (Lists.Sprite[Texture_Num].Frame_Width - Name_Size) / 2;
        Position.Y = y - Lists.Sprite[Texture_Num].Frame_Height / 2;

        // Cor do texto
        switch ((Game.NPCs)Lists.NPC[NPC_Num].Type)
        {
            case Game.NPCs.Friendly: Color = SFML.Graphics.Color.White; break;
            case Game.NPCs.AttackOnSight: Color = SFML.Graphics.Color.Red; break;
            case Game.NPCs.AttackWhenAttacked: Color = new SFML.Graphics.Color(228, 120, 51); break;
            default: Color = SFML.Graphics.Color.White; break;
        }

        // Desenha o texto
        DrawText(Lists.NPC[NPC_Num].Name, Game.ConvertX(Position.X), Game.ConvertY(Position.Y), Color);
    }

    private static void NPC_Bars(byte Index, int x, int y)
    {
        Lists.Structures.Map_NPCs NPC = Lists.Temp_Map.NPC[Index];
        short Texture_Num = Lists.NPC[NPC.Index].Texture;
        short Value = NPC.Vital[(byte)Game.Vitals.HP];

        // Apenas se necessário
        if (Value <= 0 || Value >= Lists.NPC[NPC.Index].Vital[(byte)Game.Vitals.HP]) return;

        // Posição
        Point Position = new Point(Game.ConvertX(x), Game.ConvertY(y) + Lists.Sprite[Texture_Num].Frame_Height + 4);
        int FullWidth = Lists.Sprite[Texture_Num].Frame_Width;
        int Width = (Value * FullWidth) / Lists.NPC[NPC.Index].Vital[(byte)Game.Vitals.HP];

        // Desenha a barra 
        Render(Tex_Bars, Position.X, Position.Y, 0, 4, FullWidth, 4);
        Render(Tex_Bars, Position.X, Position.Y, 0, 0, Width, 4);
    }
}
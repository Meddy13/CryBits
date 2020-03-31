﻿using System;
using System.Collections.Generic;
using System.Drawing;

class Player
{
    public static Character_Structure Character(byte Index)
    {
        // Retorna com os valores do personagem atual
        return Lists.Player[Index].Character[Lists.Temp_Player[Index].Using];
    }

    public class Character_Structure
    {
        // Dados básicos
        public byte Index;
        public string Name = string.Empty;
        public byte Class;
        public short Texture_Num;
        public bool Genre;
        public short Level;
        public int Experience;
        public byte Points;
        public short[] Vital = new short[(byte)Game.Vitals.Count];
        public short[] Attribute = new short[(byte)Game.Attributes.Count];
        public short Map;
        public byte X;
        public byte Y;
        public Game.Directions Direction;
        public Lists.Structures.Inventories[] Inventory;
        public short[] Equipment;
        public Lists.Structures.Hotbar[] Hotbar;

        public void GiveExperience(int Value)
        {
            // Dá a experiência ao jogador, caso ele estiver em um grupo divide a experiência entre os membros
            if (Lists.Temp_Player[Index].Party.Count > 0 && Value > 0) Party_SplitXP(Index, Value);
            else Experience += Value;

            // Verifica se a experiência não ficou negtiva
            if (Character(Index).Experience < 0) Character(Index).Experience = 0;

            // Verifica se passou de level
            CheckLevelUp(Index);
        }

        // Cálcula o dano do jogador
        public short Damage
        {
            get
            {
                short Value = Attribute[(byte)Game.Attributes.Strength];
                if (Lists.Item[Equipment[(byte)Game.Equipments.Weapon]] != null) Value += Lists.Item[Equipment[(byte)Game.Equipments.Weapon]].Weapon_Damage;
                return Value;
            }
        }

        // Cálcula o dano do jogador
        public short Player_Defense
        {
            get
            {
                return Attribute[(byte)Game.Attributes.Resistance];
            }
        }

        public short MaxVital(byte Vital)
        {
            short[] Base = Lists.Class[Class].Vital;

            // Cálcula o máximo de vital que um jogador possui
            switch ((Game.Vitals)Vital)
            {
                case Game.Vitals.HP:
                    return (short)(Base[Vital] + (Attribute[(byte)Game.Attributes.Vitality] * 1.50 * (Level * 0.75)) + 1);
                case Game.Vitals.MP:
                    return (short)(Base[Vital] + (Attribute[(byte)Game.Attributes.Intelligence] * 1.25 * (Level * 0.5)) + 1);
            }

            return 1;
        }

        public short Regeneration(byte Vital)
        {
            // Cálcula o máximo de vital que um jogador possui
            switch ((Game.Vitals)Vital)
            {
                case Game.Vitals.HP:
                    return (short)(MaxVital(Vital) * 0.05 + Attribute[(byte)Game.Attributes.Vitality] * 0.3);
                case Game.Vitals.MP:
                    return (short)(MaxVital(Vital) * 0.05 + Attribute[(byte)Game.Attributes.Intelligence] * 0.1);
            }

            return 0;
        }

        public int ExpNeeded
        {
            get
            {
                short Total = 0;
                // Quantidade de experiência para passar para o próximo level
                for (byte i = 0; i < (byte)Game.Attributes.Count; i++) Total += Attribute[i];
                return (int)((Level + 1) * 2.5 + (Total + Points) / 2);
            }
        }
    }

    public static void Join(byte Index)
    {
        // Previne que alguém que já está online de logar
        if (IsPlaying(Index)) return;

        // Define que o jogador está dentro do jogo
        Lists.Temp_Player[Index].Playing = true;

        // Envia todos os dados necessários
        Send.Join(Index);
        Send.Map_Players(Index);
        Send.Player_Experience(Index);
        Send.Player_Inventory(Index);
        Send.Player_Hotbar(Index);
        Send.Items(Index);
        Send.NPCs(Index);
        Send.Map_Items(Index, Character(Index).Map);
        Send.Shops(Index);

        // Transporta o jogador para a sua determinada Posição
        Warp(Index, Character(Index).Map, Character(Index).X, Character(Index).Y);

        // Entra no jogo
        Send.JoinGame(Index);
        Send.Message(Index, Lists.Server_Data.Welcome, Color.Blue);
    }

    public static void Leave(byte Index)
    {
        if (!Lists.Temp_Player[Index].InEditor)
        {
            // Salva os dados do e envia atualiza os demais jogadores da desconexão
            Write.Player(Index);
            Send.Player_Leave(Index);

            // Sai do grupo
            if (Lists.Temp_Player[Index].Playing)
            {
                Party_Leave(Index);
                Trade_Leave(Index);
            }
        }

        // Limpa os dados do jogador
        Clear.Player(Index);
    }

    public static bool IsPlaying(byte Index)
    {
        // Verifica se o jogador está dentro do jogo
        if (Socket.IsConnected(Index))
            if (Lists.Temp_Player[Index].Playing)
                return true;

        return false;
    }

    public static byte FindUser(string Name)
    {
        // Encontra o usuário
        for (byte i = 1; i <= Game.HigherIndex; i++)
            if (IsPlaying(i))
                if (Lists.Player[i].User == Name)
                    return i;

        return 0;
    }

    public static byte Find(string Name)
    {
        // Encontra o usuário
        for (byte i = 1; i < Lists.Player.Length; i++)
            if (IsPlaying(i))
                if (Character(i).Name == Name)
                    return i;

        return 0;
    }

    public static byte FindCharacter(byte Index, string Name)
    {
        // Encontra o personagem
        for (byte i = 1; i <= Lists.Server_Data.Max_Characters; i++)
            if (Lists.Player[Index].Character[i].Name.Equals(Name))
                return i;

        return 0;
    }

    public static bool HasCharacter(byte Index)
    {
        // Verifica se o jogador tem algum personagem
        for (byte i = 1; i <= Lists.Server_Data.Max_Characters; i++)
            if (!string.IsNullOrEmpty(Lists.Player[Index].Character[i].Name))
                return true;

        return false;
    }

    public static bool MultipleAccounts(string User)
    {
        // Verifica se já há alguém conectado com essa conta
        for (byte i = 1; i <= Game.HigherIndex; i++)
            if (Socket.IsConnected(i))
                if (Lists.Player[i].User.Equals(User))
                    return true;

        return false;
    }

    public static void Move(byte Index, byte Movement)
    {
        byte x = Character(Index).X, y = Character(Index).Y;
        short Map_Num = Character(Index).Map;
        short Next_X = x, Next_Y = y;
        short Link = Lists.Map[Map_Num].Link[(byte)Character(Index).Direction];
        bool SecondMovement = false;

        // Previne erros
        if (Movement < 1 || Movement > 2) return;
        if (Lists.Temp_Player[Index].GettingMap) return;
        if (Lists.Temp_Player[Index].Trade != 0) return;
        if (Lists.Temp_Player[Index].Shop != 0) return;

        // Próximo azulejo
        Map.NextTile(Character(Index).Direction, ref Next_X, ref Next_Y);

        // Ponto de ligação
        if (Map.OutLimit(Map_Num, Next_X, Next_Y))
        {
            if (Link > 0)
                switch (Character(Index).Direction)
                {
                    case Game.Directions.Up: Warp(Index, Link, x, Lists.Map[Link].Height); return;
                    case Game.Directions.Down: Warp(Index, Link, x, 0); return;
                    case Game.Directions.Right: Warp(Index, Link, 0, y); return;
                    case Game.Directions.Left: Warp(Index, Link, Lists.Map[Link].Width, y); return;
                }
            else
            {
                Send.Player_Position(Index);
                return;
            }
        }
        // Bloqueio
        else if (!Map.Tile_Blocked(Map_Num, x, y, Character(Index).Direction))
        {
            Character(Index).X = (byte)Next_X;
            Character(Index).Y = (byte)Next_Y;
        }

        // Atributos
        Lists.Structures.Map_Tile Azulejo = Lists.Map[Map_Num].Tile[Next_X, Next_Y];

        switch ((Map.Attributes)Azulejo.Attribute)
        {
            // Teletransporte
            case Map.Attributes.Warp:
                if (Azulejo.Data_4 > 0) Character(Index).Direction = (Game.Directions)Azulejo.Data_4 - 1;
                Warp(Index, Azulejo.Data_1, (byte)Azulejo.Data_2, (byte)Azulejo.Data_3);
                SecondMovement = true;
                break;
        }

        // Envia os dados
        if (!SecondMovement && (x != Character(Index).X || y != Character(Index).Y))
            Send.Player_Move(Index, Movement);
        else
            Send.Player_Position(Index);
    }

    private static void Warp(byte Index, short Map_Num, byte x, byte y)
    {
        short Map_Old = Character(Index).Map;

        // Evita que o jogador seja transportado para fora do limite
        if (Map_Num < 0 || Map_Num > Lists.Map.GetUpperBound(0)) return;
        if (x > Lists.Map[Map_Num].Width) x = Lists.Map[Map_Num].Width;
        if (y > Lists.Map[Map_Num].Height) y = Lists.Map[Map_Num].Height;
        if (x < 0) x = 0;
        if (y < 0) y = 0;

        // Define a Posição do jogador
        Character(Index).Map = Map_Num;
        Character(Index).X = x;
        Character(Index).Y = y;

        // Envia os dados dos NPCs
        Send.Map_NPCs(Index, Map_Num);

        // Envia os dados para os outros jogadores
        if (Map_Old != Map_Num)
            Send.Player_LeaveMap(Index, Map_Old);
        else
            Send.Player_Position(Index);

        // Atualiza os valores
        Lists.Temp_Player[Index].GettingMap = true;

        // Verifica se será necessário enviar os dados do mapa para o jogador
        Send.Map_Revision(Index, Map_Num);
    }

    public static void Attack(byte Index)
    {
        short Next_X = Character(Index).X, Next_Y = Character(Index).Y;
        byte Victim_Index;

        // Próximo azulejo
        Map.NextTile(Character(Index).Direction, ref Next_X, ref Next_Y);

        // Apenas se necessário
        if (Lists.Temp_Player[Index].Trade != 0) return;
        if (Environment.TickCount < Lists.Temp_Player[Index].Attack_Timer + 750) return;
        if (Map.Tile_Blocked(Character(Index).Map, Character(Index).X, Character(Index).Y, Character(Index).Direction, false)) goto @continue;

        // Ataca um jogador
        Victim_Index = Map.HasPlayer(Character(Index).Map, Next_X, Next_Y);
        if (Victim_Index > 0)
        {
            Attack_Player(Index, Victim_Index);
            return;
        }

        // Ataca um NPC
        Victim_Index = Map.HasNPC(Character(Index).Map, Next_X, Next_Y);
        if (Victim_Index > 0)
        {
            Attack_NPC(Index, Victim_Index);
            return;
        }

    @continue:
        // Demonstra que aos outros jogadores o ataque
        Send.Player_Attack(Index, 0, 0);
        Lists.Temp_Player[Index].Attack_Timer = Environment.TickCount;
    }

    private static void Attack_Player(byte Index, byte Victim)
    {
        short Damage;
        short x = Character(Index).X, y = Character(Index).Y;

        // Define o azujelo a frente do jogador
        Map.NextTile(Character(Index).Direction, ref x, ref y);

        // Verifica se a vítima pode ser atacada
        if (!IsPlaying(Victim)) return;
        if (Lists.Temp_Player[Victim].GettingMap) return;
        if (Character(Index).Map != Character(Victim).Map) return;
        if (Character(Victim).X != x || Character(Victim).Y != y) return;
        if (Lists.Map[Character(Index).Map].Moral == (byte)Map.Morals.Pacific)
        {
            Send.Message(Index, "This is a peaceful area.", Color.White);
            return;
        }

        // Tempo de ataque 
        Lists.Temp_Player[Index].Attack_Timer = Environment.TickCount;

        // Cálculo de dano
        Damage = (short)(Character(Index).Damage - Character(Victim).Player_Defense);

        // Dano não fatal
        if (Damage > 0)
        {
            // Demonstra o ataque aos outros jogadores
            Send.Player_Attack(Index, Victim, (byte)Game.Target.Player);

            if (Damage < Character(Victim).Vital[(byte)Game.Vitals.HP])
            {
                Character(Victim).Vital[(byte)Game.Vitals.HP] -= Damage;
                Send.Player_Vitals(Victim);
            }
            // FATALITY
            else
            {
                // Dá 10% da experiência da vítima ao atacante
                Character(Index).GiveExperience(Character(Victim).Experience / 10);

                // Mata a vítima
                Died(Victim);
            }
        }
        else
            // Demonstra o ataque aos outros jogadores
            Send.Player_Attack(Index);
    }

    private static void Attack_NPC(byte Index, byte Victim)
    {
        short Damage;
        short x = Character(Index).X, y = Character(Index).Y;
        Lists.Structures.Map_NPCs Map_NPC = Lists.Temp_Map[Character(Index).Map].NPC[Victim];

        // Define o azujelo a frente do jogador
        Map.NextTile(Character(Index).Direction, ref x, ref y);

        // Verifica se a vítima pode ser atacada
        if (Map_NPC.X != x || Map_NPC.Y != y) return;

        // Mensagem
        if (Map_NPC.Target_Index != Index && !string.IsNullOrEmpty(Lists.NPC[Map_NPC.Index].SayMsg)) Send.Message(Index, Lists.NPC[Map_NPC.Index].Name + ": " + Lists.NPC[Map_NPC.Index].SayMsg, Color.White);

        // Não executa o combate com um NPC amigavel
        switch ((NPC.Behaviour)Lists.NPC[Map_NPC.Index].Behaviour)
        {
            case NPC.Behaviour.Friendly: return;
            case NPC.Behaviour.ShopKeeper: Shop_Open(Index, Lists.NPC[Map_NPC.Index].Shop); return;
        }

        // Define o alvo do NPC
        Lists.Temp_Map[Character(Index).Map].NPC[Victim].Target_Index = Index;
        Lists.Temp_Map[Character(Index).Map].NPC[Victim].Target_Type = (byte)Game.Target.Player;

        // Tempo de ataque 
        Lists.Temp_Player[Index].Attack_Timer = Environment.TickCount;

        // Cálculo de dano
        Damage = (short)(Character(Index).Damage - Lists.NPC[Map_NPC.Index].Attribute[(byte)Game.Attributes.Resistance]);

        // Dano não fatal
        if (Damage > 0)
        {
            // Demonstra o ataque aos outros jogadores
            Send.Player_Attack(Index, Victim, (byte)Game.Target.NPC);

            if (Damage < Map_NPC.Vital[(byte)Game.Vitals.HP])
            {
                Lists.Temp_Map[Character(Index).Map].NPC[Victim].Vital[(byte)Game.Vitals.HP] -= Damage;
                Send.Map_NPC_Vitals(Character(Index).Map, Victim);
            }
            // FATALITY
            else
            {
                // Experiência ganhada
                Character(Index).GiveExperience(Lists.NPC[Map_NPC.Index].Experience);

                // Reseta os dados do NPC 
                NPC.Died(Character(Index).Map, Victim);
            }
        }
        else
            // Demonstra o ataque aos outros jogadores
            Send.Player_Attack(Index);
    }

    public static void Died(byte Index)
    {
        Lists.Structures.Class Data = Lists.Class[Character(Index).Class];

        // Recupera os vitais
        for (byte n = 0; n < (byte)Game.Vitals.Count; n++)
            Character(Index).Vital[n] = Character(Index).MaxVital(n);

        // Perde 10% da experiência
        Character(Index).Experience /= 10;
        Send.Player_Experience(Index);

        // Retorna para o ínicio
        Character(Index).Direction = (Game.Directions)Data.Spawn_Direction;
        Warp(Index, Data.Spawn_Map, Data.Spawn_X, Data.Spawn_Y);
    }

    public static void Logic()
    {
        // Lógica dos jogadores
        for (byte i = 0; i <= Game.HigherIndex; i++)
        {
            // Não é necessário
            if (!IsPlaying(i)) continue;

            ///////////////
            // Reneração // 
            ///////////////
            if (Environment.TickCount > Loop.Timer_Player_Regen + 5000)
                for (byte v = 0; v < (byte)Game.Vitals.Count; v++)
                    if (Character(i).Vital[v] < Character(i).MaxVital(v))
                    {
                        // Renera a vida do jogador
                        Character(i).Vital[v] += Character(i).Regeneration(v);
                        if (Character(i).Vital[v] > Character(i).MaxVital(v)) Character(i).Vital[v] = Character(i).MaxVital(v);

                        // Envia os dados aos jogadores
                        Send.Player_Vitals(i);
                    }
        }

        // Reseta as contagens
        if (Environment.TickCount > Loop.Timer_Player_Regen + 5000) Loop.Timer_Player_Regen = Environment.TickCount;
    }

    private static void CheckLevelUp(byte Index)
    {
        byte NumLevel = 0; int ExpRest;

        // Previne erros
        if (!IsPlaying(Index)) return;

        while (Character(Index).Experience >= Character(Index).ExpNeeded)
        {
            NumLevel += 1;
            ExpRest = Character(Index).Experience - Character(Index).ExpNeeded;

            // Define os dados
            Character(Index).Level += 1;
            Character(Index).Points += 3;
            Character(Index).Experience = ExpRest;
        }

        // Envia os dados
        Send.Player_Experience(Index);
        if (NumLevel > 0) Send.Map_Players(Index);
    }

    public static bool GiveItem(byte Index, short Item_Num, short Amount)
    {
        byte Slot_Item = FindInventory(Index, Item_Num);
        byte Slot_Empty = FindInventory(Index, 0);

        // Somente se necessário
        if (Item_Num == 0) return false;
        if (Slot_Empty == 0) return false;
        if (Amount == 0) Amount = 1;

        // Empilhável
        if (Slot_Item > 0 && Lists.Item[Item_Num].Stackable)
            Character(Index).Inventory[Slot_Item].Amount += Amount;
        // Não empilhável
        else
        {
            Character(Index).Inventory[Slot_Empty].Item_Num = Item_Num;
            Character(Index).Inventory[Slot_Empty].Amount = Lists.Item[Item_Num].Stackable ? Amount : (byte)1;
        }

        // Envia os dados ao jogador
        Send.Player_Inventory(Index);
        return true;
    }

    public static void TakeItem(byte Index, byte Slot, short Amount)
    {
        // Tira o item do jogaor
        if (Slot > 0)
            if (Amount == Character(Index).Inventory[Slot].Amount)
                Character(Index).Inventory[Slot] = new Lists.Structures.Inventories();
            else
                Character(Index).Inventory[Slot].Amount -= Amount;

        // Atualiza o inventário
        Send.Player_Inventory(Index);
    }

    public static void DropItem(byte Index, byte Slot, short Amount)
    {
        short Map_Num = Character(Index).Map;
        Lists.Structures.Map_Items Map_Item = new Lists.Structures.Map_Items();

        // Somente se necessário
        if (Lists.Temp_Map[Map_Num].Item.Count == Lists.Server_Data.Max_Map_Items) return;
        if (Character(Index).Inventory[Slot].Item_Num == 0) return;
        if (Lists.Item[Character(Index).Inventory[Slot].Item_Num].Bind == (byte)Game.BindOn.Pickup) return;
        if (Lists.Temp_Player[Index].Trade != 0) return;

        // Verifica se não está dropando mais do que tem
        if (Amount > Character(Index).Inventory[Slot].Amount) Amount = Character(Index).Inventory[Slot].Amount;

        // Solta o item no chão
        Map_Item.Index = Character(Index).Inventory[Slot].Item_Num;
        Map_Item.Amount = Amount;
        Map_Item.X = Character(Index).X;
        Map_Item.Y = Character(Index).Y;
        Lists.Temp_Map[Map_Num].Item.Add(Map_Item);
        Send.Map_Items(Map_Num);

        // Retira o item do inventário do jogador 
        if (Amount == Character(Index).Inventory[Slot].Amount) Character(Index).Inventory[Slot].Item_Num = 0;
        Character(Index).Inventory[Slot].Amount = (short)(Character(Index).Inventory[Slot].Amount - Amount);
        Send.Player_Inventory(Index);
    }

    public static void UseItem(byte Index, byte Slot)
    {
        short Item_Num = Character(Index).Inventory[Slot].Item_Num;

        // Somente se necessário
        if (Item_Num == 0) return;
        if (Lists.Temp_Player[Index].Trade != 0) return;

        // Requerimentos
        if (Character(Index).Level < Lists.Item[Item_Num].Req_Level)
        {
            Send.Message(Index, "You do not have the level required to use this item.", Color.White);
            return;
        }
        if (Lists.Item[Item_Num].Req_Class > 0)
            if (Character(Index).Class != Lists.Item[Item_Num].Req_Class)
            {
                Send.Message(Index, "You can not use this item.", Color.White);
                return;
            }

        if (Lists.Item[Item_Num].Type == (byte)Game.Items.Equipment)
        {
            // Retira o item da hotbar
            byte HotbarSlot = FindHotbar(Index, (byte)Game.Hotbar.Item, Slot);
            Character(Index).Hotbar[HotbarSlot].Type = 0;
            Character(Index).Hotbar[HotbarSlot].Slot = 0;

            // Retira o item do inventário
            Character(Index).Inventory[Slot].Item_Num = 0;
            Character(Index).Inventory[Slot].Amount = 0;

            // Caso já estiver com algum equipamento, desequipa ele
            if (Character(Index).Equipment[Lists.Item[Item_Num].Equip_Type] > 0) GiveItem(Index, Item_Num, 1);

            // Equipa o item
            Character(Index).Equipment[Lists.Item[Item_Num].Equip_Type] = Item_Num;
            for (byte i = 0; i < (byte)Game.Attributes.Count; i++) Character(Index).Attribute[i] += Lists.Item[Item_Num].Equip_Attribute[i];

            // Envia os dados
            Send.Player_Inventory(Index);
            Send.Player_Equipments(Index);
            Send.Player_Hotbar(Index);
        }
        else if (Lists.Item[Item_Num].Type == (byte)Game.Items.Potion)
        {
            // Efeitos
            bool HadEffect = false;
            Character(Index).GiveExperience(Lists.Item[Item_Num].Potion_Experience);
            for (byte i = 0; i < (byte)Game.Vitals.Count; i++)
            {
                // Verifica se o item causou algum efeito 
                if (Character(Index).Vital[i] < Character(Index).MaxVital(i) && Lists.Item[Item_Num].Potion_Vital[i] != 0) HadEffect = true;

                // Efeito
                Character(Index).Vital[i] += Lists.Item[Item_Num].Potion_Vital[i];

                // Impede que passe dos limites
                if (Character(Index).Vital[i] < 0) Character(Index).Vital[i] = 0;
                if (Character(Index).Vital[i] > Character(Index).MaxVital(i)) Character(Index).Vital[i] = Character(Index).MaxVital(i);
            }

            // Foi fatal
            if (Character(Index).Vital[(byte)Game.Vitals.HP] == 0) Died(Index);

            // Remove o item caso tenha tido algum efeito
            if (Lists.Item[Item_Num].Potion_Experience > 0 || HadEffect)
            {
                Character(Index).Inventory[Slot].Item_Num = 0;
                Character(Index).Inventory[Slot].Amount = 0;
                Send.Player_Inventory(Index);
                Send.Player_Vitals(Index);
            }
        }
    }

    public static byte FindHotbar(byte Index, byte Type, byte Slot)
    {
        // Encontra algo especifico na hotbar
        for (byte i = 1; i <= Game.Max_Hotbar; i++)
            if (Character(Index).Hotbar[i].Type == Type && Character(Index).Hotbar[i].Slot == Slot)
                return i;

        return 0;
    }

    public static byte FindInventory(byte Index, short Item_Num)
    {
        // Encontra algo especifico na hotbar
        for (byte i = 1; i <= Game.Max_Inventory; i++)
            if (Character(Index).Inventory[i].Item_Num == Item_Num)
                return i;

        return 0;
    }

    public static void Party_Leave(byte Index)
    {
        if (Lists.Temp_Player[Index].Party.Count > 0)
        {
            // Retira o jogador do grupo
            for (byte i = 0; i < Lists.Temp_Player[Index].Party.Count; i++)
                Lists.Temp_Player[Lists.Temp_Player[Index].Party[i]].Party.Remove(Index);

            // Envia o dados para todos os membros do grupo
            for (byte i = 0; i < Lists.Temp_Player[Index].Party.Count; i++) Send.Party(Lists.Temp_Player[Index].Party[i]);
            Lists.Temp_Player[Index].Party.Clear();
            Send.Party(Index);
        }
    }

    private static void Party_SplitXP(byte Index, int Experience)
    {
        // Somatório do level de todos os jogadores do grupo
        int Given_Experience, Experience_Sum = 0, Difference;
        double[] Diff = new double[Lists.Temp_Player[Index].Party.Count];
        double Diff_Sum = 0, k;

        // Cálcula a diferença dos leveis entre os jogadores
        for (byte i = 0; i < Lists.Temp_Player[Index].Party.Count; i++)
        {
            Difference = Math.Abs(Character(Index).Level - Character(Lists.Temp_Player[Index].Party[i]).Level);

            // Constante para a diminuir potêncialmente a experiência que diferenças altas ganhariam
            if (Difference < 3) k = 1.15;
            else if (Difference < 6) k = 1.55;
            else if (Difference < 10) k = 1.85;
            else k = 2.3;

            // Transforma o valor em fração
            Diff[i] = 1 / Math.Pow(k, Math.Min(15, Difference));
            Diff_Sum += Diff[i];
        }

        // Divide a experiência pro grupo com base na diferença dos leveis 
        for (byte i = 0; i < Lists.Temp_Player[Index].Party.Count; i++)
        {
            // Caso a somatório for maior que um (100%) balanceia os valores
            if (Diff_Sum > 1) Diff[i] *= (1 / Diff_Sum);

            // Divide a experiência
            Given_Experience = (int)((Experience / 2) * Diff[i]);
            Experience_Sum += Given_Experience;
            Character(Lists.Temp_Player[Index].Party[i]).Experience += Given_Experience;
            CheckLevelUp(Lists.Temp_Player[Index].Party[i]);
            Send.Player_Experience(Lists.Temp_Player[Index].Party[i]);
        }

        // Dá ao jogador principal o restante da experiência
        Character(Index).Experience += Experience - Experience_Sum;
        CheckLevelUp(Index);
        Send.Player_Experience(Index);
    }

    public static void Trade_Leave(byte Index)
    {
        byte Trade_Player = Lists.Temp_Player[Index].Trade;

        // Cancela a troca
        if (Trade_Player > 0)
        {
            Lists.Temp_Player[Trade_Player].Trade = 0;
            Lists.Temp_Player[Index].Trade = 0;
            Send.Trade(Trade_Player);
            Send.Trade(Index);
        }
    }

    public static byte Total_Trade_Items(byte Index)
    {
        byte Total = 0;

        // Retorna a quantidade de itens oferecidos na troca
        for (byte i = 1; i <= Game.Max_Inventory; i++)
            if (Lists.Temp_Player[Index].Trade_Offer[i].Item_Num > 0)
                Total++;
        return Total;
    }

    public static byte Total_Inventory_Free(byte Index)
    {
        byte Total = 0;

        // Retorna a quantidade de itens oferecidos na troca
        for (byte i = 1; i <= Game.Max_Inventory; i++)
            if (Character(Index).Inventory[i].Item_Num == 0)
                Total++;
        return Total;
    }

    public static void Shop_Open(byte Index, short Shop_Num)
    {
        // Abre a loja
        Lists.Temp_Player[Index].Shop = Shop_Num;
        Send.Shop_Open(Index, Shop_Num);
    }
}
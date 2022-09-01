using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using MySql.Data.MySqlClient;

namespace ConqerAndDevelopIIServer
{

    
    class Tile
    {
        public Dictionary<string, int> Distances = new Dictionary<string, int>();
        public int PosX;
        public int PosY;
        public string Name;
        public int OwnerID = 0;
        public int SoldierHP = 0;
        public int SoldierMaxHP = 0;
        public int SoldierMoves = 0;
        public int SoldierMaxMoves = 0;
        public int SoldierExp = 0;
        public int Capital = 0;
        public int Development = 0;
        public Battle Battle = null;
        public int FortBuildTime = -1;
        


        public string BuildingType = "nothing";
        
        

        public Tile(int px, int py)
        {
            PosX = px;
            PosY = py;
            Development = Program.random.Next(1, 4);         
            if(Program.random.Next(1,12) == 1)
            {
                BuildingType = "gold";
            }
            Name = Program.GenerateName(Program.random.Next(4,9), Program.random);            
        }
        public void MakeSoldier(int HP, int MaxHP, int Moves, int MaxMoves, int Exp)
        {
            SoldierHP = HP;
            SoldierMaxHP = MaxHP;
            SoldierMoves = Moves;
            SoldierMaxMoves = MaxMoves;
            SoldierExp = Exp;
        }
    }
    


    class TileToUpdate
    {
        public Tile t;
        public string username;
        public bool Read = false;
        public TileToUpdate(Tile tile, string usr)
        {
            t = tile;
            username = usr;

        }
    }
    class Army
    {

    }
    class BattleTile
    {
        public int PosX;
        public int PosY;
        public string Type = "nothing";
        public int OwnerID;
        //Terrain types: 0 - Flat
        public int TerrainType = 0;
        public BattleTile(string type, int owner)
        {
            Type = type;
            OwnerID = owner;
        }
    }
    class BattleTileToUpdate
    {
        public BattleTile bt;
        public string username;
        public bool Read = false;
        public BattleTileToUpdate(BattleTile battil, string usr)
        {
            bt = battil;
            username = usr;
        }
    }
    class Deal
    {
        public string Title;
        public List<string> Signers;
        
    }
    class Battle
    {
        public BattleTile[,] BattleMap = new BattleTile[15, 15];
        public List<BattleTileToUpdate> BattleTilesToUpdate = new List<BattleTileToUpdate>();
        public int[] Participants = new int[7] { -1,-1,-1,-1,-1,-1,-1 };
        public Battle()
        {
            for(int x = 0; x < 8; x++)
            {
                for(int y = 0; y < 8; y++)
                {
                    BattleMap[x, y] = new BattleTile("nothing", 0);
                    BattleMap[x, y].PosX = x;
                    BattleMap[x, y].PosY = y;
                }
            }
            
        }
    }
    class Game
    {

        public List<string> users = new List<string>();
        public List<TileToUpdate> TilesToUpdate = new List<TileToUpdate>();
        public List<Tile> Tiles = new List<Tile>();
        public int Slots;
        public string Host;
        public string Name;
        public string Password;
        public int turn = 0;
        public Dictionary<int, float> Gold = new Dictionary<int, float> { { 1, 6 }, { 2, 6 }, { 3, 6 }, { 4, 6 } };
        public Dictionary<int, int> Income = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 } };
        public Dictionary<int, int> Expenses = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 } };
        public Dictionary<int, int> RealIncome = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 } };
        public Dictionary<int, int> PalaceTime = new Dictionary<int, int> { { 1, 5 }, { 2, 5 }, { 3, 5 }, { 4, 5 } };
        public Dictionary<int, bool> PalaceBuilt = new Dictionary<int, bool> { { 1, false }, { 2, false }, { 3, false }, { 4, false } };
        public Dictionary<int, int> ResearchPoints = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 } };
        public Dictionary<int, int> Manpower = new Dictionary<int, int> { { 1, 10 }, { 2, 10 }, { 3, 10 }, { 4, 10 } };

        public List<Deal> Deals = new List<Deal>();



        public int RealManpowerIncomeNow(int id)
        {
            return 0;
        }

        public int RealIncomeNow(int id)
        {
            int inc = 2;
            foreach(Tile t in Tiles)
            {
                if(t.OwnerID == id)
                {
                    inc += t.Development;
                    switch (t.BuildingType)
                    {
                        case "fort":
                            inc -= 6;
                            break;
                        case "market":
                            inc += 2;                           
                            break;
                        case "university":
                            inc -= 5;
                            break;
                        case "barracks":
                            inc -= 6;
                            break;
                        
                        case "training":
                            inc -= 4;
                            break;
                        case "mine":
                            inc += 3;
                            break;
                    }

                    if (t.SoldierHP > 0)
                    {
                        inc -= 5;
                    }
                    if(t.SoldierHP < t.SoldierMaxHP)
                    {
                        inc -= 2;
                    }
                }
            }



            return inc;
        }




        public int TrueSoldierLimit(int id)
        {
            double count = 3;
            foreach (Tile t in Tiles)
            {
                if (t.OwnerID == id)
                {
                    count += 0.25;
                    if (t.BuildingType == "barracks")
                    {
                        count++;
                    }
                }
            }
            return (int)Math.Floor(count);
        }
        public int SoldierLimit(int id)
        {
            double count = 3;

            foreach(Tile t in Tiles)
            {
                if(t.OwnerID == id)
                {
                    count += 0.25;
                    if(t.BuildingType == "barracks")
                    {
                        count++;
                    }
                    if(t.SoldierHP > 0)
                    {
                        count--;
                    }
                }
            }
            return (int)Math.Floor(count);
        }

        public bool Ongoing = false;
        public Dictionary<int, string> IDUsername = new Dictionary<int, string>();
        public Dictionary<string, int> UsernameID = new Dictionary<string, int>();
        public Dictionary<string, bool> ConnectedToGame = new Dictionary<string, bool>();
        
        public void AddTileToUpdate(Tile t)
        {
            bool seen = false;
            foreach (TileToUpdate tu in TilesToUpdate)
            {
                if (tu.t == t)
                {
                    seen = true;
                    break;
                }
            }
            if (!seen)
            {
                foreach (string usr in users)
                    TilesToUpdate.Add(new TileToUpdate(t, usr));
            }
        }

        public Game(int slots, string host, string name, string password = "")
        {
           
            Slots = slots;
            Host = host;
            Name = name;
            Password = password;
            users.Add(host);
            IDUsername.Add(1, host);
            UsernameID.Add(host, 1);
            ConnectedToGame.Add(host, false);
            Program.Games.Add(this);
            Thread t = new Thread(x => Program.HandleGames(this));            
            t.Start();
            
            
           
        }
        public void Delete()
        {
            Program.Games.Remove(this);
        }
        public int GetTileCount(int PlayerID)
        {
            int ret = 0;
            foreach(Tile t in Tiles)
            {
                if(t.OwnerID == PlayerID)
                {
                    ret++;
                }
            }
            return ret;
        }
        public string GetUsers()
        {
            string output = Host + " ";
            foreach (string s in users)
            {
                if (s != Host)
                {
                    output += s + " ";
                }
            }
            output.TrimEnd();
            return output;
        }
        
        public void MakeMap()
        {
            Console.WriteLine("Here it goes!");
            Random rand = new Random();
            for (int a = 0; a < 11; a++)
            { 
                for (int b = 0; b < 12; b++)
                {
                    if (rand.Next(0, 7) > 0)
                    {
                        Random r = new Random(a^2 * b + 1);
                        Tile t = new Tile(b, a);
                        Tiles.Add(t);
                        
                        t.OwnerID = 0;                                                                                                            
                    }
                }              
            }
            for(int a = 1; a < users.Count() + 1; a++)
            {

                
                bool done = false;
                do
                {
                    int random = rand.Next(0, Tiles.Count());
                    if (Tiles[random].OwnerID == 0)
                    {
                        Tiles[random].OwnerID = a;
                        Tiles[random].MakeSoldier(10, 10, 2, 2, 0);            
                        Tiles[random].Capital = 1;
                        Tiles[random].Development = 5;
                        done = true;                       
                       
                    }
                }
                while (!done);
                    
            }
            
            foreach(Tile t in Tiles)
            {
                AddTileToUpdate(t);
                Dictionary<Tile, int> Borl = Program.Bordering(t, 15, Tiles);
                foreach (Tile bor in Borl.Keys)
                {
                    t.Distances.Add($"{bor.PosX}.{bor.PosY}", Borl[bor]);
                }
            }
        }
    }
    class Program
    {
        public static Random random = new Random();
        public static string Alphabet = "abcdefghijklmnopqrstuwvxyz";
        public static string GenerateName(int len, Random r)
        {
            
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
            string Name = "";
            Name += consonants[r.Next(consonants.Length)].ToUpper();
            Name += vowels[r.Next(vowels.Length)];
            int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                Name += consonants[r.Next(consonants.Length)];
                b++;
                Name += vowels[r.Next(vowels.Length)];
                b++;
            }

            return Name;
        }
        public static Dictionary<Tile, int> Bordering(Tile T, int Distance, List<Tile> Tiles)
        {
            Dictionary<Tile, int> Bordering = new Dictionary<Tile, int>();
            Bordering.Add(T, 0);
            int count = 1;
            for (int a = 0; a < Distance; a++)
            {
                Dictionary<Tile, int> AddToBoredering = new Dictionary<Tile, int>();
                foreach (Tile tile in Bordering.Keys)
                {
                    foreach (Tile t in Tiles)
                    {
                        if (t.PosX % 2 == 1 && !AddToBoredering.Keys.Contains(t) && !Bordering.Keys.Contains(t))
                        {
                            if (t.PosX + 1 == tile.PosX && t.PosY == tile.PosY)
                                AddToBoredering.Add(t, a);

                            if (t.PosX == tile.PosX && t.PosY + 1 == tile.PosY)
                                AddToBoredering.Add(t, a);

                            if (t.PosX - 1 == tile.PosX && t.PosY == tile.PosY)
                                AddToBoredering.Add(t, a);

                            if (t.PosX - 1 == tile.PosX && t.PosY - 1 == tile.PosY)
                                AddToBoredering.Add(t, a);

                            if (t.PosX + 1 == tile.PosX && t.PosY - 1 == tile.PosY)
                                AddToBoredering.Add(t, a);

                            if (t.PosX == tile.PosX && t.PosY - 1 == tile.PosY)
                                AddToBoredering.Add(t, a);
                        }
                        else if (!Bordering.Keys.Contains(t) && !AddToBoredering.Keys.Contains(t) && t.PosX % 2 == 0)
                        {
                            if (t.PosX == tile.PosX && t.PosY + 1 == tile.PosY)
                                AddToBoredering.Add(t, a);

                            if (t.PosX - 1 == tile.PosX && t.PosY == tile.PosY)
                                AddToBoredering.Add(t, a);

                            if (t.PosX + 1 == tile.PosX && t.PosY == tile.PosY)
                                AddToBoredering.Add(t, a);

                            if (t.PosX + 1 == tile.PosX && t.PosY + 1 == tile.PosY)
                                AddToBoredering.Add(t, a);

                            if (t.PosX - 1 == tile.PosX && t.PosY + 1 == tile.PosY)
                                AddToBoredering.Add(t, a);

                            if (t.PosX == tile.PosX && t.PosY - 1 == tile.PosY)
                                AddToBoredering.Add(t, a);

                        }

                    }

                }

                foreach (Tile t in AddToBoredering.Keys)
                {
                    if (!Bordering.Keys.Contains(t))
                    {
                        Bordering.Add(t, count);
                    }
                }
                count++;
            }
            return Bordering;
        }
        static void Main(string[] args)
        {            
           
            Console.WriteLine("Server up and ready!");
            Thread t = new Thread(Start);
            t.Start();
            try
            {
                while (true)
                {
                    string command = Console.ReadLine();

                    
                    if (command.Contains("help"))
                    {
                        if (command.Split().Count() > 1)
                        {
                            switch (command.Split()[1])
                            {
                                default:
                                    Console.WriteLine($"No such command as {command.Split()[1]} was found. Type help to learn about the existing commands");
                                    break;
                                case "exit":
                                    Console.WriteLine("exit: Closes the server app");
                                    break;
                                case "gamels":
                                    Console.WriteLine("gamels: Shows info about all running games");
                                    break;
                                case "getgamemaps":
                                    Console.WriteLine("getgamemaps: Shows maps of all games");
                                    break;
                                case "userls":
                                    Console.WriteLine("userls: Shows names of all loged players");
                                    break;
                                case "delgame":
                                    Console.WriteLine("delgame <name of the game>: Deletes a game of given name");
                                    break;
                                case "food":
                                    Console.WriteLine("food <name of the game>: Gives information about current amount of food each player has in given game");
                                    break;

                            }
                        }
                        else
                        {
                            Console.WriteLine("List of possible commands: \nexit gamels getgamemaps userls delgame food");
                        }
                    }
                    else if (command.Contains("exit"))
                    {
                        t.Abort();
                        throw new Exception("Server closed manually");
                      
                    }
                    else if (command.Contains("gamels"))
                    {
                        Console.Clear();
                        Console.WriteLine("Games running:");
                        foreach (Game g in Games)
                        {
                            Console.WriteLine($"     -{g.Name}: Host: {g.Host} | Password: {g.Password} | Connected users: {g.GetUsers()}");
                        }
                    }
                    else if (command.Contains("getgamemaps"))
                    {
                        Console.Clear();
                        Console.WriteLine("Game maps: ");
                       
                        foreach (Game g in Games)
                        {
                            Console.WriteLine("\n  " + g.Name);
                            string s = "";
                            foreach(Tile ti in g.Tiles)
                            {
                                s += " " + ti.PosX + "." + ti.PosY + "." + ti.OwnerID;
                            }
                            Console.WriteLine(s);
                        }
                    }
                    else if (command.Contains("userls"))
                    {
                        Console.Clear();
                        Console.WriteLine("Connected useres:");
                        foreach (string u in Users)
                        {

                            Console.WriteLine($"     -{u}");
                        }
                    }
                    else if (command.Contains("delgame"))
                    {
                        Console.WriteLine(command);
                        string[] commandsplit = command.Split(' ');
                        Game ga = null;
                        foreach (Game g in Games)
                        {
                            if (g.Name == commandsplit[1])
                                ga = g;
                        }
                        if (ga != null)
                            ga.Delete();
                        else
                            Console.WriteLine($"No such game as {commandsplit[1]} exists!");
                    }
                   
                   
                    
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Server closed with error: " + e.Message);
                
            }

            
        }
        public static void Start()
        {
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Task.Run(() => HandleClient(client));
            }
        }
        public static List<Game> Games = new List<Game>();
        public static List<string> Users = new List<string>();
        static TcpListener listener = new TcpListener(IPAddress.Any,2055);
        public static List<Thread> Threads = new List<Thread>();
        public static float NormaliseNumber(float number, int digits)
        {
            float d = number * 100;
            d = (float)Math.Floor(d);
            d = d * 0.01f;
            return d;

        }
       
        public static void HandleGames(Game game)
        {
            Console.WriteLine($"Game {game.Name} has been started by user {game.Host}");
           
            try
            {
                Random r = new Random();
                while (true)
                {
                    int sleep = 10000;
                    Thread.Sleep(500);
                    if (game.Ongoing && game.ConnectedToGame.All(x => x.Value == true))
                    {
                        while (true)
                        {
                            
                            if (game.turn == 5)
                            {
                                sleep += 5000;
                            }
                            if (game.turn == 15)
                            {
                                sleep += 5000;
                            }
                            if (game.turn == 30)
                            {
                                sleep += 5000;
                            }

                            Thread.Sleep(sleep);
                            List<TileToUpdate> Remove = new List<TileToUpdate>();
                            foreach(TileToUpdate t in game.TilesToUpdate)
                            {
                                if (t.Read)
                                    Remove.Add(t);
                            }
                            game.TilesToUpdate.RemoveAll(x => Remove.Contains(x));

                            for (int a = 1; a < 5; a++)
                            {
                                game.Income[a] = 2;
                                game.Expenses[a] = 0;
                            }
                          

                            
                            foreach (Tile t in game.Tiles)
                            {
                                if (t.OwnerID != 0)
                                {
                                    game.Income[t.OwnerID] += t.Development;
                                }
                                switch (t.BuildingType)
                                {
                                    case "fort":
                                        game.Expenses[t.OwnerID] += 6;
                                        break;
                                    case "market":

                                        if (random.Next(1, 3) == 1)
                                            game.Income[t.OwnerID] += 2;
                                        else
                                            game.Income[t.OwnerID] += 3;


                                        break;
                                    case "university":
                                        game.Expenses[t.OwnerID] += 5;
                                        game.ResearchPoints[t.OwnerID]++;
                                        break;
                                    case "barracks":
                                        game.Expenses[t.OwnerID] += 6;
                                        break;
                                    case "palace":
                                        game.PalaceBuilt[t.OwnerID] = true;
                                        game.PalaceTime[t.OwnerID] -= 1;
                                        if(game.PalaceTime[t.OwnerID] <= 0)
                                        {
                                           
                                                bool done = false;
                                                do
                                                {
                                                    
                                                    int random = r.Next(0,  game.Tiles.Count());
                                                    if (game.Tiles[random].OwnerID == t.OwnerID)
                                                    {
                                                        game.Tiles[random].Development++;
                                                       
                                                        done = true;

                                                    }
                                                }
                                                while (!done);
                                            game.PalaceTime[t.OwnerID] = 5;

                                        }
                                        break;
                                    case "training":
                                        game.Expenses[t.OwnerID] += 4;
                                        break;
                                    case "mine":
                                        if (random.Next(1, 3) == 1)
                                            game.Income[t.OwnerID] += 3;
                                        else
                                            game.Income[t.OwnerID] += 4;
                                        break;
                                }
                                
                                if(t.SoldierHP > 0)
                                {
                                    game.Expenses[t.OwnerID] += 5;

                                    if (t.BuildingType == "training")
                                        t.SoldierExp += 1;
                                    if (t.SoldierHP < t.SoldierMaxHP)
                                    {
                                        if (t.BuildingType == "fort")
                                        {
                                            t.SoldierHP += 2;
                                        }
                                        else
                                        {
                                            t.SoldierHP++;
                                            game.Expenses[t.OwnerID] += 2;
                                        }
                                        game.AddTileToUpdate(t);
                                    }
                                    if (t.SoldierMoves != t.SoldierMaxMoves)
                                    {
                                        t.SoldierMoves = t.SoldierMaxMoves;
                                        game.AddTileToUpdate(t);
                                    }
                                }

                                
                            }
                            for(int a = 1; a < 5; a++)
                            {
                                game.RealIncome[a] = game.Income[a] - game.Expenses[a];
                                game.Gold[a] += game.RealIncome[a];
                            }
                                                     

                           if (!Games.Contains(game))
                            {
                                throw new Exception("No users");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Game {game.Name} was terminated because of the following error: {e.Message}");
                game.Delete();
                
            }
        }
        
        public static void HandleClient(object client)
        {
            string username = "";
            TcpClient tcpclient = (TcpClient)client;
            Stream s = tcpclient.GetStream();
            StreamWriter Write = new StreamWriter(s);
            StreamReader Read = new StreamReader(s);
            Write.AutoFlush = true;
            tcpclient.ReceiveTimeout = 180000;
            try
            {
                string msg = Read.ReadLine();
                if (msg == "connect")
                {
                    username = Read.ReadLine();
                    Console.WriteLine($"User { username } connected!");
                    Users.Add(username);
                }
                else
                    throw new Exception();
                while (true)
                {
                    
                    string message = Read.ReadLine();

                    if (message.StartsWith("create game"))
                    {
                        string[] split = message.Split(' ');
                        Game g = new Game(int.Parse(split[2]), username, split[3], split[4]);

                    }
                    else if (message.StartsWith("get lobby"))
                    {
                        Game ga = null;
                        foreach (Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {
                                ga = g;

                            }
                        }

                        if (ga == null)
                        {
                            Write.WriteLine("null");
                            Console.WriteLine("null");
                        }
                        else if (ga.Ongoing == false)
                        {
                            string somestring = ga.GetUsers();
                            string me = $"{ga.Name} {ga.Slots.ToString()} {ga.Host} {somestring}";
                            Write.WriteLine(me);

                        }
                        else if (ga.Ongoing == true)
                        {
                            Write.WriteLine("start");

                        }
                    }
                    else if (message.StartsWith("mov"))
                    {
                        string[] split = message.Split(' ');
                        Tile Home = null;
                        Tile Dest = null;
                        Console.WriteLine(message);

                        foreach (Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {
                                foreach (Tile t in g.Tiles)
                                {
                                    if (t.PosX == int.Parse(split[1].Split('.')[0]) && t.PosY == int.Parse(split[1].Split('.')[1]))
                                        Home = t;
                                    else if (t.PosX == int.Parse(split[2].Split('.')[0]) && t.PosY == int.Parse(split[2].Split('.')[1]))
                                        Dest = t;
                                }



                                if (Dest.OwnerID == 0)
                                {
                                    if (g.ResearchPoints.Values.Max() != g.ResearchPoints[Home.OwnerID] || g.ResearchPoints[Home.OwnerID] == 0)
                                    {
                                        if (g.Gold[Home.OwnerID] >= 3)
                                        {
                                            g.Gold[Home.OwnerID] -= 3;
                                            Dest.OwnerID = Home.OwnerID;
                                            Dest.MakeSoldier(Home.SoldierHP, Home.SoldierMaxHP, 0, Home.SoldierMaxMoves, Home.SoldierExp);
                                            Home.MakeSoldier(0, 0, 0, 0, 0);
                                            Dest.SoldierExp++;
                                            g.AddTileToUpdate(Dest);
                                            g.AddTileToUpdate(Home);

                                        }

                                    }
                                    else
                                    {
                                        Dest.OwnerID = Home.OwnerID;
                                        Dest.MakeSoldier(Home.SoldierHP, Home.SoldierMaxHP, 0, Home.SoldierMaxMoves, Home.SoldierExp);
                                        Home.MakeSoldier(0, 0, 0, 0, 0);
                                        Dest.SoldierExp++;
                                        g.AddTileToUpdate(Dest);
                                        g.AddTileToUpdate(Home);
                                    }

                                }
                                else if (Dest.OwnerID == g.UsernameID[username])
                                {
                                    if (Dest.SoldierHP == 0)
                                    {
                                        Dest.MakeSoldier(Home.SoldierHP, Home.SoldierMaxHP, Home.SoldierMoves - Home.Distances[$"{Dest.PosX}.{Dest.PosY}"], Home.SoldierMaxMoves, Home.SoldierExp);
                                        Home.MakeSoldier(0, 0, 0, 0, 0);
                                        foreach (string u in g.users)
                                        {
                                            g.AddTileToUpdate(Dest);
                                            g.AddTileToUpdate(Home);
                                        }
                                    }
                                }
                                else if (Dest.OwnerID != g.UsernameID[username])
                                {
                                    if (Dest.SoldierHP > 0)
                                    {
                                        int attackerbonus = 0;
                                        int defenderbonus = 0;
                                        if (g.ResearchPoints[Home.OwnerID] > g.ResearchPoints[Dest.OwnerID])
                                        {
                                            attackerbonus++;
                                        }
                                        else if (g.ResearchPoints[Home.OwnerID] < g.ResearchPoints[Dest.OwnerID])
                                        {
                                            defenderbonus++;
                                        }

                                        foreach (Tile tile in Bordering(Home, 1, g.Tiles).Keys)
                                        {
                                            if (tile.OwnerID == Home.OwnerID && tile.BuildingType == "training")
                                            {
                                                attackerbonus++;
                                                break;
                                            }
                                        }
                                        foreach (Tile tile in Bordering(Dest, 1, g.Tiles).Keys)
                                        {
                                            if (tile.OwnerID == Dest.OwnerID && tile.BuildingType == "training")
                                            {
                                                defenderbonus++;
                                                break;
                                            }
                                        }
                                        if (Dest.SoldierExp >= 10)
                                        {
                                            defenderbonus++;
                                            attackerbonus--;
                                        }
                                        if (Dest.SoldierExp >= 20)
                                        {
                                            defenderbonus += 2;
                                        }
                                        if (Dest.SoldierExp == 30)
                                        {
                                            defenderbonus += 2;
                                        }

                                        if (Home.SoldierExp >= 10)
                                        {
                                            attackerbonus++;
                                            defenderbonus--;
                                        }
                                        if (Home.SoldierExp >= 20)
                                        {
                                            attackerbonus++;
                                        }
                                        if (Home.SoldierExp == 30)
                                        {
                                            attackerbonus += 2;
                                        }

                                        if (Dest.BuildingType == "fort")
                                        {
                                            Home.SoldierHP -= new Random().Next(2, 5) + defenderbonus;
                                        }
                                        else
                                        {
                                            Home.SoldierHP -= new Random().Next(1, 4) + defenderbonus;
                                        }
                                        if (Home.SoldierHP > 0)
                                            Dest.SoldierHP -= new Random().Next(1, 4) + attackerbonus;
                                        else
                                            Dest.SoldierHP -= 1;
                                        if (Dest.SoldierMoves != 0)
                                        {
                                            Dest.SoldierMoves -= 1;
                                        }
                                        Home.SoldierMoves = 0;
                                        if (Dest.SoldierHP <= 0)
                                        {
                                            if (Home.SoldierHP > 1)
                                                Dest.MakeSoldier(Home.SoldierHP - 1, Home.SoldierMaxHP, 0, Home.SoldierMaxMoves, Home.SoldierExp);
                                            else
                                                Dest.MakeSoldier(Home.SoldierHP, Home.SoldierMaxHP, 0, Home.SoldierMaxMoves, Home.SoldierExp);
                                            Home.MakeSoldier(0, 0, 0, 0, 0);
                                            Dest.OwnerID = Home.OwnerID;
                                        }
                                        Home.SoldierExp += 2;
                                        Dest.SoldierExp += 2;
                                        g.AddTileToUpdate(Dest);
                                        g.AddTileToUpdate(Home);


                                    }
                                    else
                                    {
                                        if (Dest.BuildingType == "fort")
                                        {

                                            Dest.MakeSoldier(Home.SoldierHP - 1, Home.SoldierMaxHP, 0, Home.SoldierMaxMoves, Home.SoldierExp);
                                            Home.MakeSoldier(0, 0, 0, 0, 0);
                                            Dest.SoldierExp += 1;
                                            Dest.OwnerID = Home.OwnerID;

                                        }
                                        if (Dest.BuildingType != "fort")
                                        {

                                            if (Dest.Capital == 1)
                                            {
                                                g.Gold[Home.OwnerID] += (int)Math.Floor(g.Gold[Dest.OwnerID] * 0.25);
                                                g.Gold[Dest.OwnerID] -= (int)Math.Floor(g.Gold[Dest.OwnerID] * 0.25);
                                                int a = 0;
                                                do
                                                {
                                                    a = random.Next(0, g.Tiles.Count());
                                                    if (g.Tiles[a].OwnerID == Dest.OwnerID)
                                                    {
                                                        g.Tiles[a].Capital = 1;
                                                    }
                                                }
                                                while (g.Tiles[a].Capital == 1);



                                            }
                                            Dest.MakeSoldier(Home.SoldierHP - 1, Home.SoldierMaxHP, 0, Home.SoldierMaxMoves, Home.SoldierExp);
                                            Home.MakeSoldier(0, 0, 0, 0, 0);
                                            Dest.OwnerID = Home.OwnerID;

                                        }











                                        foreach (string u in g.users)
                                        {
                                            g.AddTileToUpdate(Dest);
                                            g.AddTileToUpdate(Home);
                                        }
                                    }


                                }
                            }
                        }


                    }
                    else if (message.StartsWith("getmap"))
                    {
                        foreach (Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {
                                string somestring = "";
                                foreach (Tile t in g.Tiles)
                                {
                                    somestring += t.PosX + "." + t.PosY + "." + t.Name + " ";
                                }
                                somestring = somestring.Trim();
                                Write.WriteLine(somestring);
                                Console.WriteLine(somestring);
                                g.ConnectedToGame[username] = true;
                            }
                        }
                    }
                    else if (message.StartsWith("send "))
                    {
                        string[] split = message.Split(' ');
                        foreach (Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {
                                g.Gold[g.UsernameID[username]] -= int.Parse(split[2]);
                                g.Gold[int.Parse(split[1])] += int.Parse(split[2]);
                            }
                        }
                    }
                    else if (message.StartsWith("build "))
                    {
                        string[] split = message.Split(' ');
                        foreach (Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {
                                Tile bu = null;
                                foreach (Tile t in g.Tiles)
                                {
                                    if (t.PosX == int.Parse(split[1].Split('.')[0]) && t.PosY == int.Parse(split[1].Split('.')[1]))
                                        bu = t;
                                }
                                if (bu != null)
                                {
                                    switch (split[2])
                                    {
                                        case "nothing":
                                            g.Gold[g.UsernameID[username]] += 5;
                                            if (bu.BuildingType == "palace")
                                            {
                                                g.PalaceBuilt[g.UsernameID[username]] = false;
                                            }
                                            bu.BuildingType = "nothing";
                                            break;
                                        case "fort":
                                            if (g.Gold[g.UsernameID[username]] >= 50)
                                            {
                                                g.Gold[g.UsernameID[username]] -= 50;
                                                bu.BuildingType = "fort";
                                            }
                                            break;
                                        case "palace":
                                            if (g.Gold[g.UsernameID[username]] >= 50 && g.PalaceBuilt[g.UsernameID[username]] == false)
                                            {
                                                g.Gold[g.UsernameID[username]] -= 50;
                                                bu.BuildingType = "palace";
                                                g.PalaceBuilt[g.UsernameID[username]] = true;
                                            }
                                            break;
                                        case "university":
                                            if (g.Gold[g.UsernameID[username]] >= 40)
                                            {
                                                g.Gold[g.UsernameID[username]] -= 40;
                                                bu.BuildingType = "university";
                                            }
                                            break;
                                        case "market":
                                            if (g.Gold[g.UsernameID[username]] >= 30)
                                            {
                                                g.Gold[g.UsernameID[username]] -= 30;
                                                bu.BuildingType = "market";
                                            }
                                            break;
                                        case "training":
                                            if (g.Gold[g.UsernameID[username]] >= 45)
                                            {
                                                g.Gold[g.UsernameID[username]] -= 45;
                                                bu.BuildingType = "training";
                                            }
                                            break;
                                        case "barracks":
                                            if (g.Gold[g.UsernameID[username]] >= 30)
                                            {
                                                g.Gold[g.UsernameID[username]] -= 30;
                                                bu.BuildingType = "barracks";
                                            }
                                            break;
                                        case "mine":
                                            if (g.Gold[g.UsernameID[username]] >= 45)
                                            {
                                                g.Gold[g.UsernameID[username]] -= 45;
                                                bu.BuildingType = "mine";
                                            }
                                            break;

                                    }
                                    g.AddTileToUpdate(bu);
                                }
                            }
                        }
                    }
                    else if (message.StartsWith("sold "))
                    {
                        string[] split = message.Split(' ');
                        Tile so = null;
                        foreach (Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {
                                foreach (Tile t in g.Tiles)
                                {
                                    if (t.PosX == int.Parse(split[1].Split('.')[0]) && t.PosY == int.Parse(split[1].Split('.')[1]))
                                        so = t;
                                }
                                if (so != null)
                                {
                                    if (so.SoldierHP > 0)
                                    {
                                        so.SoldierHP = 0;
                                        g.AddTileToUpdate(so);
                                    }
                                    else
                                    {
                                        if (g.SoldierLimit(g.UsernameID[username]) > 0 && (so.BuildingType == "fort" || so.BuildingType == "training" || so.BuildingType == "barracks" || so.Capital == 1))
                                        {
                                            so.MakeSoldier(10, 10, 2, 2, 0);
                                            g.Gold[g.UsernameID[username]] -= 20;
                                            g.AddTileToUpdate(so);
                                        }

                                    }
                                }
                            }

                        }
                    }
                  
                    else if (message.StartsWith("getbat "))
                    {
                        string[] pos = message.Split(' ')[1].Split('.');
                        foreach(Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {
                                string whattosend = "";
                                foreach(Tile t in g.Tiles)
                                {
                                    if(t.PosX == int.Parse(pos[0]) && t.PosY == int.Parse(pos[1]))
                                    {
                                        
                                        if (t.Battle != null)
                                        {
                                            foreach (int id in t.Battle.Participants)
                                            {
                                                whattosend += $"{id}:";
                                            }
                                            whattosend.Remove(whattosend.Length - 1);
                                            whattosend += "|";
                                            foreach(BattleTile bt in t.Battle.BattleMap)
                                            {
                                                whattosend += $"{bt.TerrainType} ";
                                            }
                                            whattosend.TrimEnd();
                                        }
                                        Write.WriteLine(whattosend);
                                        break;
                                        
                                    }
                                }
                            }
                        }
                    }
                    else if(message.StartsWith("checkfreename "))
                    {
                        string[] split = message.Split(' ');
                        
                    }
                    else if (message.StartsWith("crtdeal "))
                    {
                        string[] split = message.Split(' ');

                    }
                    else if (message.StartsWith("getotherplayers"))
                    {
                        foreach(Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {
                                string whattosend = "";
                                foreach(string usr in g.users)
                                {
                                    if (usr != username)
                                        whattosend += usr + "|";
                                }
                                whattosend.Remove(whattosend.Length - 1, 1);
                                Write.WriteLine(whattosend);
                            }
                        }
                    }
                    else if (message.StartsWith("chkbat"))
                    {

                        foreach(Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {
                                string whattosend = "";
                                foreach(Tile t in g.Tiles)
                                {
                                    if (t.Battle.Participants.Contains(g.UsernameID[username]))
                                    {
                                        whattosend += $"{t.PosX}.{t.PosY}|";
                                        foreach (BattleTileToUpdate bt in t.Battle.BattleTilesToUpdate)
                                        {
                                            if(bt.username == username)
                                            {
                                                whattosend += $"{bt.bt.PosX}.{bt.bt.PosY}.{bt.bt.OwnerID}.{bt.bt.Type}:";

                                            }
                                        }
                                        whattosend += " ";
                                    }
                                }
                                whattosend.TrimEnd();
                                Write.WriteLine(whattosend);
                            }
                        }
                    }
                    else if (message.StartsWith("getgameinfo"))
                    {
                        foreach (Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {

                                string somestring = $"{g.Gold[g.UsernameID[username]]} {g.RealIncomeNow(g.UsernameID[username])}|";

                                foreach (TileToUpdate t in g.TilesToUpdate)
                                {
                                    if (t.username == username)
                                    {


                                        if (t.t.SoldierHP > 0)
                                            somestring += $"{t.t.PosX}.{t.t.PosY}.{t.t.OwnerID}.{t.t.Capital}.{t.t.Development}.{t.t.BuildingType}.{t.t.SoldierHP}.{t.t.SoldierMaxHP}.{t.t.SoldierMoves}.{t.t.SoldierMaxMoves}.{t.t.SoldierExp} ";
                                        else
                                            somestring += $"{t.t.PosX}.{t.t.PosY}.{t.t.OwnerID}.{t.t.Capital}.{t.t.Development}.{t.t.BuildingType} ";

                                        t.Read = true;
                                    }
                                }
                                Write.WriteLine(somestring);
                            }
                        }
                    }


                    else if (message.StartsWith("get games"))
                    {
                        string tosend = "";
                        foreach (Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {
                                tosend = "fuckyou";
                                break;
                            }
                            if (g.Ongoing == false && g.users.Count() < g.Slots)
                                tosend += $"#{g.Name}|{g.Slots}|{g.GetUsers()}";
                        }
                        Write.WriteLine(tosend);
                    }
                    else if (message.StartsWith("join lobby"))
                    {
                        bool joined = false;
                        foreach (Game g in Games)
                        {
                            if (!g.Ongoing && g.Name == message.Split(' ')[2])
                            {
                                if (g.Password == message.Split(' ')[3])
                                {
                                    g.users.Add(username);
                                    if (!g.IDUsername.Values.Contains(username))
                                    {
                                        g.IDUsername.Add(g.IDUsername.Count() + 1, username);
                                        Write.WriteLine("joined");
                                        Write.WriteLine(g.IDUsername.Count());
                                        g.UsernameID.Add(username, g.IDUsername.Count());
                                        g.ConnectedToGame.Add(username, false);
                                    }
                                    joined = true;
                                    break;

                                }


                            }

                        }
                        if (joined)
                        {


                        }
                        else
                        {
                            Write.WriteLine("notjoined");
                        }
                    }
                    else if (message.StartsWith("disconnect"))
                    {
                        throw new Exception("User " + username + " disconnected manually");
                    }
                    else if (message.StartsWith("start game"))
                    {

                        foreach (Game g in Games)
                        {
                            if (g.users.Contains(username))
                            {
                                g.Ongoing = true;
                                g.MakeMap();
                                Console.WriteLine($"Game { g.Name} startes!");
                            }
                        }

                    }
                    else if (message.StartsWith("exit lobby"))
                    {
                        Game ga = null;
                        foreach (Game g in Games)
                        {
                            if (g.users.Contains(username))
                                ga = g;
                        }
                        ga.users.Remove(username);
                        if (ga.users.Count() == 0 || ga.Host == username)
                            ga.Delete();
                    }
                    else
                    {
                        throw new Exception("Wrong command");
                    }
                            
                    
                        
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Game ga = null;
                foreach (Game g in Games)
                {
                    if (g.users.Contains(username))
                    {
                        g.users.Remove(username);
                        if (g.users.Count() == 0 || g.Host == username)
                            ga = g;
                     
                    }
                }
                if (ga != null)
                    ga.Delete();
                        tcpclient.Close();
            }
            finally
            {
                Console.WriteLine($"User {username} has disconnected!");
                Users.Remove(username);
            }                       
        }
    }
}

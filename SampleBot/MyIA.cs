using BattleIA;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SampleBot
{

    
    public class GridPoint
    {
       
        public GridPoint Parent;
        public Vector2 Position;
      
        public float DistanceToTarget;
        public float Cost;
        public float Weight;
        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + Cost;
                else
                    return -1;
            }
        }
        public bool Walkable;
        
        public GridPoint(Vector2 pos, bool walkable=true, float weight = 1)
        {
            Parent = null;
            Position = pos;
            DistanceToTarget = -1;
            Cost = 1;
            Weight = weight;
            Walkable = walkable;
        }
        public float get_distance(int x, int y){
            float distance = (float)Math.Sqrt((x - Position.X)*(x - Position.X) + (y - Position.Y)* (y - Position.Y));
            return distance;
        }
        
    }

    public class MyIA
    {

        Random rnd = new Random();
        bool isFirst = true;
        UInt16 currentShieldLevel = 0;
        bool hasBeenHit = false;
        //my relative position in current scan.
        int meY = 0;
        int meX = 0;

        GridPoint mytarget = new GridPoint(new Vector2(0,0));
        List<MoveDirection> route = new List<MoveDirection>();


        public MyIA()

        {
            // setup
        }

        /// <summary>
        /// Mise à jour des informations
        /// </summary>
        /// <param name="turn">Turn.</param>
        /// <param name="energy">Energy.</param>
        /// <param name="shieldLevel">Shield level.</param>
        /// <param name="isCloaked">If set to <c>true</c> is cloacked.</param>
        public void StatusReport(UInt16 turn, UInt16 energy, UInt16 shieldLevel, bool isCloaked)
        {
            // si le niveau de notre bouclier a baissé, c'est que l'on a reçu un coup
            if (currentShieldLevel != shieldLevel)
            {
                currentShieldLevel = shieldLevel;
                hasBeenHit = true;
            }
        }


        /// <summary>
        /// On nous demande la distance de scan que l'on veut effectuer
        /// </summary>
        /// <returns>The scan surface.</returns>
        public byte GetScanSurface()
        {
            if (isFirst)
            {
                isFirst = false;
                return 10;
            }
            if (route.Count == 0)
            {
                return 10;
            }
            return 0;
        }

        public Random rng = new Random();

        GridPoint find_nearest_energy(List<GridPoint> pliste)
        {
            // Let's compute distance & find the nearest Energy point 
            // pour chaque nrjtpoint appelé 'p' de nrj_list : 
            // distance = valeur absolue (meX - GridPoint.posx)+ abs(meY-GridPoint.posy)
            GridPoint target = new GridPoint(new Vector2(0,0));
            float mindist = 9999;
            foreach (GridPoint p in pliste)
            {
        
                float d = p.get_distance(meX,meY);
                if (d < mindist)
                {
                    target = p;
                    mindist = d ;
                    Console.WriteLine($"[TGT] mindist = {mindist}");
                }
            }
            Console.WriteLine($"[TGT] position {target.Position.X} {target.Position.Y} ");
            return (target);
        }

        List<MoveDirection> find_route_astar(GridPoint target,List<List<GridPoint>> map)
        {
            Astar as_Path = new Astar(map);
            Stack<GridPoint> solution = new Stack<GridPoint>();

            try {
                solution = as_Path.FindPath(new Vector2(meX,meY), new Vector2(target.Position.X,target.Position.Y ));
                // Reverse path (from start to end...)
                Stack<GridPoint> rsolution = new Stack<GridPoint>();
                if (solution.Count ==0){
                    return new List<MoveDirection>();
                }
                foreach (GridPoint p in solution){
                    Console.WriteLine($"[ROUTE] {p.Position.X},{p.Position.Y}");
                }
                Console.WriteLine($"[ROUTE] {meX},{meY}");
         
 
                route = build_route(solution);
                return route;                 
            } 
            catch{
                route =new  List<MoveDirection>();
                return route;                 

            }

             
        }

        List<MoveDirection> build_route(Stack<GridPoint> points)
        {
            List<MoveDirection> l = new List<MoveDirection>();
            GridPoint init = new GridPoint(new Vector2(meX,meY));
            float nx = meX;
            float ny = meY;
            foreach (GridPoint p in points) {
                if (nx>p.Position.X) {
                    l.Add(MoveDirection.North);
                    nx = p.Position.X;
                    ny = p.Position.Y;

                    continue;
                }
                if (nx<p.Position.X) {
                    l.Add(MoveDirection.South);
                    nx = p.Position.X;
                    ny = p.Position.Y;
                    continue;
                }
                if (ny>p.Position.Y) {
                    l.Add(MoveDirection.East);
                    nx = p.Position.X;
                    ny = p.Position.Y;
                    continue;
                }
                if (ny<p.Position.Y) {
                    l.Add(MoveDirection.West);
                    nx = p.Position.X;
                    ny = p.Position.Y;
                    continue;
                }
            }
            foreach (MoveDirection m in l){
                Console.WriteLine(m);
            }

            return l;
        }


        List<MoveDirection> find_route(GridPoint target)
        {

            // Est / west  : 
            if (target.Position.X < meX)
            {
                for (int i = 0; i < Math.Abs(target.Position.X); i++)
                {
                    route.Add(MoveDirection.East);
                }
            }
            if (target.Position.X > meX)
            {
                for (int i = 0; i < Math.Abs(target.Position.X); i++)
                {
                    route.Add(MoveDirection.West);
                }
            }
            if (target.Position.Y < meY)
            {
                for (int i = 0; i < Math.Abs(target.Position.Y); i++)
                {
                    route.Add(MoveDirection.North);
                }
            }
            if (target.Position.Y > meY)
            {
                for (int i = 0; i < Math.Abs(target.Position.Y); i++)
                {
                    route.Add(MoveDirection.South);
                }
            }
            return route;
        }

        private bool check_route(List<MoveDirection> r)
        {
            return true;
        }


        /// <summary>
        /// Résultat du scan
        /// </summary>
        /// <param name="distance">Distance.</param>
        /// <param name="informations">Informations.</param>
        public void AreaInformation(byte distance, byte[] informations)
        {
            if (distance <=1) { return; }

            int radar_nrj = 0;
            List<GridPoint> NRJ_list = new List<GridPoint>();
            List<List<GridPoint>> ScanMap = new List<List<GridPoint>>();

            Console.WriteLine($"Area: {distance}");
            int index = 0;
            for (int i = 0; i < distance; i++)
            {
                List<GridPoint> lineCells = new List<GridPoint>();
                for (int j = 0; j < distance; j++)
                {
                    GridPoint p = new GridPoint(new Vector2(i,j));
                    // IF point is energy :
                    if (informations[index] == (byte)CaseState.Energy)
                    {
                        radar_nrj++;
                        NRJ_list.Add(p);
                        p.Walkable=true;
                    }
                    // If point point is Ennemy 
                    if (informations[index] == (byte)CaseState.Ennemy)
                    {
                        meX = j;
                        meY = i;
                        p.Walkable=true;
                    }
                    if (informations[index] == (byte)CaseState.Wall)
                    {
                        p.Walkable=false;
                    }
                    if (informations[index] == (byte)CaseState.Empty)
                    {
                        p.Walkable=true;

                    } 
                    index++;
                    lineCells.Add(p);
                }
                ScanMap.Add(lineCells);
            }
            
            // Find route for each nrgpoint : 
            /*
            List<List<MoveDirection>> routes = new List<List<MoveDirection>>();
            foreach (GridPoint p in NRJ_list)
            {
                //route = find_route(p);
                route = find_route_astar(p,ScanMap);

                if (check_route(route))
                {
                    routes.Add(route);
                }

            }
            */
            mytarget = find_nearest_energy(NRJ_list);
            route = find_route_astar(mytarget,ScanMap);
        }

        //début modif
        public byte[] randommove()
        {
            byte[] ret; // ret = tab d'octet
            ret = new byte[2]; //tableau de taille 2 octet
            ret[0] = (byte)BotAction.Move;
            ret[1] = (byte)rnd.Next(1, 5);

            return ret;
        }
        //fin modif

        /// <summary>
        /// On dot effectuer une action
        /// </summary>
        /// <returns>The action.</returns>
        public byte[] GetAction()
        {
            byte[] ret;
            // nous venons d'être touché
            /*
            if (hasBeenHit)
            {
                // plus de bouclier ?
                if (currentShieldLevel == 0)
                {
                    // on en réactive 1 de suite !
                    currentShieldLevel = (byte)rnd.Next(1, 9);
                    ret = new byte[3];
                    ret[0] = (byte)BotAction.ShieldLevel;
                    ret[1] = (byte)(currentShieldLevel & 0xFF);
                    ret[2] = (byte)(currentShieldLevel >> 8);
                    return ret;
                }

                hasBeenHit = false;
                // puis on se déplace fissa, au hazard
                ret = new byte[2];
                ret[0] = (byte)BotAction.Move;
                ret[1] = (byte)rnd.Next(1, 5);
                return ret;
            }

            // si pas de bouclier, on en met un en route
            if (currentShieldLevel == 0)
            {
                // on en réactive 1 de suite !
                currentShieldLevel = 1;
                ret = new byte[3];
                ret[0] = (byte)BotAction.ShieldLevel;
                ret[1] = (byte)(currentShieldLevel & 0xFF);
                ret[2] = (byte)(currentShieldLevel >> 8);
                return ret;
            }

            */
            ret = new byte[2];
            ret[0] = (byte)BotAction.Move;
            // if we have route points : 
            if (route.Count > 0)
            {
                //move to the next point :
                ret[1] = (byte)route[0];
                // remove it :
                route.RemoveAt(0);
            }
            else
            {
                // on se déplace au hazard
                ret[1] = (byte)rnd.Next(1, 5);
            }

         
            return ret;
        }

    }
}
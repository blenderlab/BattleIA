using System;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using BattleIA;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

 
namespace SampleBot
{
    class NRJPOINT
    {
        public int posx;
        public int posy;
        public int distance;

        public int get_distance(int x, int y){
            distance = Math.Abs(x-posx)+Math.Abs(y-posy);
            return distance;
        }
    }
 
    public class MyIA
    {
 
        Random rnd = new Random();
        bool isFirst = true;
        UInt16 currentShieldLevel = 0;
        bool hasBeenHit = false;
        byte scan_length=5;
        //my relative position in current scan.
        int meY = 0;
        int meX = 0;
        bool found_something=false;
        List<MoveDirection> nrj_route = new List<MoveDirection>();
        NRJPOINT nrj_target = new NRJPOINT();
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
            if (!found_something){
                scan_length+=5;
            }
            if (nrj_route.Count==0){
                return scan_length;
            }
            return 0;
        }
 
        /// <summary>
        /// Find nearest NRJ node
        /// </summary>
        /// <param name="pliste">NRJPONTS list.</param>
        NRJPOINT find_nearest_energy(List<NRJPOINT> pliste){
            // Let's compute distance & find the nearest Energy point 
            // pour chaque nrjtpoint appelé 'p' de nrj_list : 
            // distance = valeur absolue (meX - nrjpoint.posx)+ abs(meY-nrjpoint.posy)
            NRJPOINT target = new NRJPOINT();
            target.distance= 9999;
            foreach (NRJPOINT p in pliste){
                p.get_distance(meX,meY);
                if (p.distance < target.distance){
                    target.posx = p.posx;
                    target.posy = p.posy;
                    target.distance=p.distance;
                }
              }
            if (target.distance==9999){
                found_something=false;
            } else {    
            found_something=true;
            scan_length=5;
            }
            return target;

        }

      
        Stack<Node> find_route(NRJPOINT nrj_target,List<List<Node>> map){
            Stack<Node> r = new Stack<Node>();
            Astar astar = new Astar(map);
            Console.WriteLine($"map used : {map}");

            r = astar.FindPath(new Vector2(meX,meY),new Vector2(nrj_target.posx,nrj_target.posy));
            if (r.Count>0){
                Console.WriteLine("Found a route ! ");
                                found_something=true;

            } else {
                found_something=false;
            }
            return r;
        }
 
 
        /// <summary>
        /// Résultat du scan
        /// </summary>
        /// <param name="distance">Distance.</param>
        /// <param name="informations">Informations.</param>
        public void AreaInformation(byte distance, byte[] informations)
        {
            if (distance <= 1){ return; }
            int radar_nrj = 0;
            List<NRJPOINT> NRJ_list = new List<NRJPOINT>();
            Console.WriteLine($"Scanning : {distance-1}");
            int index = 0;
            bool walkable=false;
            int weight = 0;
            List<List<Node>> scan = new  List<List<Node>>();

            for (int i = 0; i < distance; i++)
            {
                List<Node> l = new List<Node>();
                    
                for (int j = 0; j < distance; j++)
                {
                    
                    if (informations[index] == (byte)CaseState.Energy) { 
                            radar_nrj++;
                            NRJPOINT np = new NRJPOINT();
                            np.posx= i;
                            np.posy= j;
                            NRJ_list.Add(np);
                            walkable=true;
                            weight=-1;        
                    }
                    if (informations[index] == (byte)CaseState.Ennemy)
                    {
                        meX = i;
                        meY = j;
                        walkable=true;
                        weight=0;
                    }
                    if (informations[index] == (byte)CaseState.Wall)
                    {
                        walkable=false;
                        weight=0;
                    }
                    if (informations[index] == (byte)CaseState.Empty)
                    {
                        walkable=true;
                        weight=0;
                    }
                    Node n = new Node(new Vector2(i,j),walkable,weight);
                    l.Add(n);

                    index++;
                    //Consn.Position.X} n.Position.X} n.Position.X} {n.Position.Y{n.Position.Y{n.Position.Yole.Write(informations[index]);
                }
                //Console.WriteLine();
                scan.Add(l);
            }
            nrj_target = find_nearest_energy(NRJ_list);
            if (nrj_target.posx>0 && nrj_target.posy>0){
                Console.WriteLine($"New target : {meX-nrj_target.posx} {meY-nrj_target.posy}");
                Stack<Node> R =  find_route(nrj_target,scan);
                Console.WriteLine($"Route calculated ...");
                if (R.Count>0){
                    nrj_route= build_route(R);           
                    Console.WriteLine($"and found !");

                } else {
                    nrj_route.Clear();
                    Console.WriteLine("No route found....");
                }
            } else {
                nrj_route.Clear();
                Console.WriteLine("No Target found....");
            
            }
        }
 

        public List<MoveDirection> build_route(Stack<Node> R){
                List<MoveDirection> r = new List<MoveDirection>();
                NRJPOINT from = new NRJPOINT();
                from.posx= meX;
                from.posy=meY;
                
                foreach(Node N in R){
                    if ((int)N.Position.X < from.posx){
                        r.Add(MoveDirection.North);
                        continue;
                    }
                    if ((int)N.Position.X > from.posx){
                        r.Add(MoveDirection.South);
                                                continue;

                    }
                    if ((int)N.Position.Y < from.posy){
                        r.Add(MoveDirection.East);
                                                continue;

                    }
                    if ((int)N.Position.Y > from.posy){
                        r.Add(MoveDirection.West);
                                                continue;

                    }
                    from.posx=(int)N.Position.X;
                    from.posy=(int)N.Position.Y;
                    
                }
                foreach (MoveDirection d in r){
                    Console.WriteLine(d);
                }
                return(r);
        }
        //début modif
 
        /// <summary>
        /// On dot effectuer une action
        /// </summary>
        /// <returns>The action.</returns>
        public byte[] GetAction()
        {
            byte[] ret;
            // nous venons d'être touché
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
 
            ret = new byte[2];
            ret[0] = (byte)BotAction.Move;
            if (nrj_route.Count>0) {
                ret[1] = (byte)nrj_route[0];
                nrj_route.RemoveAt(0);        
            } else {                
            // on se déplace au hazard
                ret[1] = (byte)rnd.Next(1,5);      
            }
 
            //var ret = new byte[1];
            //ret[0] = (byte)BotAction.None;
 
            //ret = new byte[2];
            //ret[0] = (byte)BotAction.Move;
            //ret[1] = (byte)MoveDirection.North;
 
            //var ret = new byte[2];
            //ret[0] = (byte)BotAction.ShieldLevel;
            //ret[1] = 10;
 
            //var ret = new byte[2];
            //ret[0] = (byte)BotAction.CloackLevel;
            //ret[1] = 20;
 
            // ret = new byte[2];
            // ret[0] = (byte)BotAction.ShieldLevel;
            // ret[1] = (byte)MoveDirection.North;
 
            return ret;
        }
 
    }
}
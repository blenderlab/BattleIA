using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SampleBot
{

    public class Astar
    {
        List<List<GridPoint>> Grid;
        int GridRows
        {
            get
            {
               return Grid[0].Count;
            }
        }
        int GridCols
        {
            get
            {
                return Grid.Count;
            }
        }

        public Astar(List<List<GridPoint>> grid)
        {
            Grid = grid;
        }

        public Stack<GridPoint> FindPath(Vector2 Start, Vector2 End)
        {
            GridPoint start = new GridPoint(new Vector2((int)(Start.X), (int) (Start.Y)), true);
            GridPoint end = new GridPoint(new Vector2((int)(End.X ), (int)(End.Y )), true);

            Stack<GridPoint> Path = new Stack<GridPoint>();
            List<GridPoint> OpenList = new List<GridPoint>();
            List<GridPoint> ClosedList = new List<GridPoint>();
            List<GridPoint> adjacencies;
            GridPoint current = start;
           
            // add start GridPoint to Open List
            OpenList.Add(start);

            while(OpenList.Count != 0 && !ClosedList.Exists(x => x.Position == end.Position))
            {
                current = OpenList[0];
                OpenList.Remove(current);
                ClosedList.Add(current);
                adjacencies = GetAdjacentGridPoints(current);

 
                foreach(GridPoint n in adjacencies)
                {
                    if (!ClosedList.Contains(n) && n.Walkable)
                    {
                        if (!OpenList.Contains(n))
                        {
                            n.Parent = current;
                            n.DistanceToTarget = Math.Abs(n.Position.X - end.Position.X) + Math.Abs(n.Position.Y - end.Position.Y);
                            n.Cost = n.Weight + n.Parent.Cost;
                            OpenList.Add(n);
                            OpenList = OpenList.OrderBy(GridPoint => GridPoint.F).ToList<GridPoint>();
                        }
                    }
                }
            }
            
            // construct path, if end was not closed return null
            if(!ClosedList.Exists(x => x.Position == end.Position))
            {
                return null;
            }

            // if all good, return path
            GridPoint temp = ClosedList[ClosedList.IndexOf(current)];
            if (temp == null) return null;
            do
            {
                Path.Push(temp);
                temp = temp.Parent;
            } while (temp != start && temp != null) ;
            return Path;
        }
		
        private List<GridPoint> GetAdjacentGridPoints(GridPoint n)
        {
            List<GridPoint> temp = new List<GridPoint>();

            int row = (int)n.Position.Y;
            int col = (int)n.Position.X;

            if(row + 1 < GridRows)
            {
                temp.Add(Grid[col][row + 1]);
            }
            if(row - 1 >= 0)
            {
                temp.Add(Grid[col][row - 1]);
            }
            if(col - 1 >= 0)
            {
                temp.Add(Grid[col - 1][row]);
            }
            if(col + 1 < GridCols)
            {
                temp.Add(Grid[col + 1][row]);
            }

            return temp;
        }
    }
}

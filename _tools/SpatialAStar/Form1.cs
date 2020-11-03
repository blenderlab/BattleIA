using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SpatialAStar
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class MyPathNode : SettlersEngine.IPathNode<Object>
        {
            public Int32 X { get; set; }
            public Int32 Y { get; set; }
            public Boolean IsWall {get; set;}

            public bool IsWalkable(Object unused)
            {
                return !IsWall;
            }
        }

public class MySolver<TPathNode, TUserContext> : SettlersEngine.SpatialAStar<TPathNode, TUserContext> where TPathNode : SettlersEngine.IPathNode<TUserContext>
{
    protected override Double Heuristic(PathNode inStart, PathNode inEnd)
    {
        return Math.Abs(inStart.X - inEnd.X) + Math.Abs(inStart.Y - inEnd.Y);
    }

    protected override Double NeighborDistance(PathNode inStart, PathNode inEnd)
    {
        return Heuristic(inStart, inEnd);
    }

    public MySolver(TPathNode[,] inGrid)
        : base(inGrid)
    {
    }
}

        private unsafe void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Random rnd = new Random();
                Bitmap gridBmp = new Bitmap(512, 512, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                MyPathNode[,] grid = new MyPathNode[gridBmp.Width, gridBmp.Height];
                SettlersEngine.ImagePixelLock locked = new SettlersEngine.ImagePixelLock(gridBmp, false);

                using (locked)
                {
                    int* pixels = locked.Pixels;

                    // setup grid with walls
                    for (int x = 0; x < gridBmp.Width; x++)
                    {
                        for (int y = 0; y < gridBmp.Height; y++)
                        {
                            Boolean isWall = ((y % 2) != 0) && (rnd.Next(0, 10) != 8);

                            if (isWall)
                                *pixels = unchecked((int)0xFF000000);
                            else
                                *pixels = unchecked((int)0xFFFFFFFF);

                            grid[x, y] = new MyPathNode()
                            {
                                IsWall = isWall,
                                X = x,
                                Y = y,
                            };

                            pixels++;
                        }
                    }
                }

                // compute and display path
                MySolver<MyPathNode, Object> aStar = new MySolver<MyPathNode, Object>(grid);
                IEnumerable<MyPathNode> path = aStar.Search(new Point(0, 0), new Point(gridBmp.Width - 2, gridBmp.Height - 2), null);

                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

                watch.Start();
                {
                    aStar.Search(new Point(0, 0), new Point(gridBmp.Width - 2, gridBmp.Height - 2), null);
                }
                watch.Stop();

                MessageBox.Show("Pathfinding took " + watch.ElapsedMilliseconds + "ms to complete.");

                foreach (MyPathNode node in path)
                {
                    gridBmp.SetPixel(node.X, node.Y, Color.Red);
                }

                pictureBox1.Image = gridBmp;

                gridBmp.Save(".\\dump.png");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Unknown error...", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

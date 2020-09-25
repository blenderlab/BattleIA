using System;
using BattleIA;
using System.Collections.Generic;

namespace SampleBot
{
    public class MyIA
    {

        Random rnd = new Random();

        bool isFirst = true;

        UInt16 currentShieldLevel = 0;
        bool hasBeenHit = false;

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
                return 20;
            }
            return 0;
        }

        /// <summary>
        /// Résultat du scan
        /// </summary>
        /// <param name="distance">Distance.</param>
        /// <param name="informations">Informations.</param>
        public void AreaInformation(byte distance, byte[] informations)
        {
            Console.WriteLine($"Area: {distance}");
            int index = 0;
            int radarnrj = 0;
            int[,] listnrj = {{0,0}};
            for (int i = 0; i < distance; i++)
            {
                for (int j = 0; j < distance; j++)
                {
                    if (informations[index]==3){
                        //listnrj = (int[,]) ResizeArray(listnrj, new int[] { radarnrj++, 2} );
                        //listnrj[radarnrj++]=new int[] {i,j};
                    }
                    Console.Write(informations[index++]);
                }
                Console.WriteLine();
            }
            Console.WriteLine($"Energy found : {radarnrj}");
        }

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

            // on se déplace au hazard
            //ret = new byte[2];
            //ret[0] = (byte)BotAction.Move;
            //ret[1] = (byte)rnd.Next(1, 5);
            //return ret;

            ret = new byte[2];
            ret[0] = (byte)BotAction.Move;
            ret[1] = (byte)MoveDirection.North;
            return ret;

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

            //var ret = new byte[2];
            //ret[0] = (byte)BotAction.Fire;
            //ret[1] = (byte)MoveDirection.NorthWest;

            return ret;
        }

    }
}

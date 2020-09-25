namespace BattleIA
{
    public static class BotHelper
    {
        public static byte[] ActionNone()
        {
            byte[] ret = new byte[1];
            ret[0] = (byte)BotAction.None;
            return ret;
        }

        public static byte[] ActionMove(MoveDirection direction)
        {
            byte[] ret = new byte[2];
            ret[0] = (byte)BotAction.Move;
            ret[1] = (byte)direction;
            return ret;
        }

        public static byte[] ActionShoot(MoveDirection direction)
        {
            byte[] ret = new byte[2];
            ret[0] = (byte)BotAction.Shoot;
            ret[1] = (byte)direction;
            return ret;
        }

        public static byte[] ActionShield(ushort puissance)
        {
            byte[] ret = new byte[3];
            ret[0] = (byte)BotAction.ShieldLevel;
            ret[1] = (byte)(puissance & 0xFF);
            ret[2] = (byte)(puissance >> 8);
            return ret;
        }

        public static byte[] ActionCloak(ushort distance)
        {
            byte[] ret = new byte[3];
            ret[0] = (byte)BotAction.CloakLevel;
            ret[1] = (byte)(distance & 0xFF);
            ret[2] = (byte)(distance >> 8);
            return ret;
        }
    }
}

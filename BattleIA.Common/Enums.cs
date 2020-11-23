namespace BattleIA
{

    public enum BotState : byte
    {
        Undefined = 0,
        WaitingGUID = 1,
        ErrorGUID = 2,
        Ready = 3,
        Error = 4,
        Disconnect = 5,
        WaitingAnswerD = 6,
        WaitingAction = 7,
        IsDead = 8,
        IsRespawned = 9
    }

    public enum Message : byte
    {
        m_dead = (byte)'D',
        m_mapInfos = (byte)'I',
        m_OK = (byte)'O',
        m_yourTurn = (byte)'T',
        m_newInfos = (byte)'C',
        m_Map = (byte)'M',
        m_Position = (byte)'P',
        m_Respawn = (byte)'R'


    }

    /*
    Empty =

    */
    public enum CaseState : byte
    {
        Empty = 0,
        OurBot = 1,
        Wall = 2,
        Energy = 3,
        Ennemy = 4,
        Respawn = 5,
        Return_to_Start = 6,
        Race_Finish = 7,
        Spawn_T1 = 8,
        Spawn_T2 = 9,
        Poison = (byte)'P',

    }

    public enum BotAction : byte
    {
        None = 0,
        Move = 1,
        ShieldLevel = 2,
        CloakLevel = 3,
        Shoot = 4,
    }

    public enum MessageSize : byte
    {
        Dead = 1,
        OK = 2,
        Position = 3,
        Turn = 9,
        Change = 11,
        Respawn = 1
    }

    public enum MoveDirection : byte
    {
        North = 1,
        West = 2,
        South = 3,
        East = 4,
        /*NorthWest = 5,
        SouthWest = 6,
        SouthEast = 7,
        NorthEast = 8,
        */
    }

}

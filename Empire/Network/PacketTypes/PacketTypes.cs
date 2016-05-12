namespace Empire.Network
{
    public enum PacketType : int
    {
        Salutation,     // "hello, can I join your game?"
        Acknowledge,
        Ping,
        Entity,
        ShipCommand,
        PlayerData,
    }
}

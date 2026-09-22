namespace PoliceChase.AI
{
    public enum PoliceRole
    {
        Chase,      // direct pursuit
        Intercept,  // try to get ahead
        Block,      // block predicted path
        Pin,        // limit movement
        Support     // support others
    }
}

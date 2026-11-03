namespace HackedDesign
{
    /// <summary>
    /// Barrier edge colours. Each is a piece of security tech that blocks one family of payloads and
    /// reacts in its own way when broken (see the Edge colour key in the quickhack design doc).
    /// </summary>
    public enum EdgeColour
    {
        Red,    // Firewall: blocks control payloads and scans (Crash, DoS, Glacier, Ping, Weak point analysis)
        Green,  // Intrusion detection: blocks takeovers and spoofs (Override, Psychosis, Decoy, Radio spoof, Camera loop)
        White,  // Watchdog: blocks nothing, regrows its neighbours
        Black   // Black ICE: blocks everything
    }
}

namespace HackedDesign
{
    /// <summary>
    /// Barrier edge colours. Each is a piece of security tech that blocks one family of payloads and
    /// reacts in its own way when broken (see the Edge colour key in the quickhack design doc).
    /// </summary>
    public enum EdgeColour
    {
        Red,    // Firewall: blocks control payloads (Crash, DoS, Glacier)
        Amber,  // Authentication: blocks takeover payloads (Override, Psychosis)
        Violet, // Encryption: blocks scans (Ping, Weak point analysis)
        Green,  // Intrusion detection: blocks spoofs (Decoy, Radio spoof, Camera loop)
        White,  // Watchdog: blocks nothing, regrows its neighbours
        Black   // Black ICE: blocks everything
    }
}

namespace k4tool
{
    public class Filter
    {
        public Envelope Envelope;

        public int Cutoff;  // 0~100

        public int Resonance; // 0 ~ 7 / 1 ~ 8

        public bool IsLFO;  // 0/off, 1/on

        public int CutoffModVel;  // 0 ~ 100 (±50)

        public int CutoffModPrs;   // 0 ~ 100 (±50)

        public int CutoffModKS;   // 0 ~ 100 (±50)

        public int EnvelopeDepth; // 0 ~ 100 (±50)

        public int EnvelopeVelocityDepth; // 0 ~ 100 (±50)
    }
}
namespace k4tool
{
    public class Single
    {
        const int NumSources = 4;

        public SingleCommon Common;

        public Source[] Sources;

        public Single()
        {
            Sources = new Source[NumSources];
            
        }

    }
    
}
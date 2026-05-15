using CsvHelper.Configuration.Attributes;

namespace HackedDesign
{
    public class DialogLine
    {
        [Name("sequence")]
        public string Sequence { get; set; }
        [Name("speaker")]
        public string Speaker { get; set; }
        [Name("speakertitle")]
        public string Speakertitle { get; set; }
        [Name("emotion")]
        public string Emotion { get; set; }
        [Name("text")]
        public string Text { get; set; }
    }
}

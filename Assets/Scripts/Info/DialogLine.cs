using CsvHelper.Configuration.Attributes;
using System.ComponentModel;

namespace HackedDesign
{
    public class DialogLine
    {
        [Name("sequence")]
        public string Sequence { get; set; }

        [Name("speaker")]
        [Description("Key to lookup to find the speaker")]
        public string Speaker { get; set; }

        [Name("speakertitle")]
        [Description("What text to display under the speaker's avatar")]
        public string Speakertitle { get; set; }

        [Name("emotion")]
        [Description("What sprite emotion to display")]
        public string Emotion { get; set; }

        [Name("message")]
        [Description("Is this a message")]
        public bool message { get; set; }

        [Name("subject")]
        [Description("If this is a message, what's the subject")]
        public string Subject { get; set; }

        [Name("read")]
        [Description("If this is a message, has it already been read?")]
        public bool? Read { get; set; }

        [Name("setflag")]
        [Description("Sets a game flag when this line is played")]
        public string SetFlag { get; set; }

        [Name("text")]
        [Description("The lines of dialog to display")]
        public string Text { get; set; }
    }
}

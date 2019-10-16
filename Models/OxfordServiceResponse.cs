using System.Collections.Generic;

namespace FlashCards.Models
{
    public class OxfordServiceResponse
    {
        public string id { get; set; }
        public Metadata metadata { get; set; }
        public List<Result> results { get; set; }
        public string word { get; set; }
        public class Metadata
        {
            public string operation { get; set; }
            public string provider { get; set; }
            public string schema { get; set; }
        }
        public class Result
        {
            public string id { get; set; }
            public string language { get; set; }
            public List<LexicalEntry> lexicalEntries { get; set; }
            public string type { get; set; }
            public string word { get; set; }

            public class LexicalEntry
            {
                public List<Entry> entries { get; set; }
                public string language { get; set; }
                public LexicalCategory lexicalCategory { get; set; }
                public List<Pronunciation> pronunciations { get; set; }
                public string text { get; set; }
                public class Pronunciation
                {
                    public string audioFile { get; set; }
                    public List<string> dialects { get; set; }
                    public string phoneticNotation { get; set; }
                    public string phoneticSpelling { get; set; }
                }
                public class LexicalCategory
                {
                    public string id { get; set; }
                    public string text { get; set; }
                }
                public class Entry
                {
                    public List<string> etymologies { get; set; }
                    public List<Sens> senses { get; set; }
                    public List<Note2> notes { get; set; }
                    public class Sens
                    {
                        public List<string> definitions { get; set; }
                        public List<Example> examples { get; set; }
                        public string id { get; set; }
                        public List<string> shortDefinitions { get; set; }
                        public List<Subsens> subsenses { get; set; }
                        public List<ThesaurusLink2> thesaurusLinks { get; set; }
                        public List<Note> notes { get; set; }
                        public List<Register> registers { get; set; }
                        public List<Region> regions { get; set; }
                        public class Subsens
                        {
                            public List<string> definitions { get; set; }
                            public List<Example2> examples { get; set; }
                            public string id { get; set; }
                            public List<string> shortDefinitions { get; set; }
                            public List<ThesaurusLink> thesaurusLinks { get; set; }
                            public class ThesaurusLink
                            {
                                public string entry_id { get; set; }
                                public string sense_id { get; set; }
                            }
                            public class Example2
                            {
                                public string text { get; set; }
                            }
                        }
                        public class Example
                        {
                            public string text { get; set; }
                        }
                        public class Region
                        {
                            public string id { get; set; }
                            public string text { get; set; }
                        }

                        public class ThesaurusLink2
                        {
                            public string entry_id { get; set; }
                            public string sense_id { get; set; }
                        }

                        public class Note
                        {
                            public string text { get; set; }
                            public string type { get; set; }
                        }

                        public class Register
                        {
                            public string id { get; set; }
                            public string text { get; set; }
                        }
                    }
                }
                public class Note2
                {
                    public string text { get; set; }
                    public string type { get; set; }
                }
            }
        }
    }
}
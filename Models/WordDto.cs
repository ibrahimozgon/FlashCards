using System.Collections.Generic;

namespace FlashCards.Models
{
    public class WordDto
    {
        public string Word { get; set; }
    }

    public class WordSenses
    {
        public IList<string> Definitions { get; set; }
        public IList<string> Examples { get; set; }
    }
}
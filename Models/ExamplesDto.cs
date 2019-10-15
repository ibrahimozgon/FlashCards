using System.Collections.Generic;

namespace FlashCards.Models
{
    public class ExamplesDto
    {
        public string Word { get; set; }
        public IList<string> Examples { get; set; }
    }
}
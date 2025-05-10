using System;
using System.Collections.Generic;
using System.Linq;

namespace GorillaShirts.Models
{
    public class Pack<T> : IStandNavigationInfo
    {
        public string Name;

        public string Author;

        public string Description;

        public string Note;

        public List<T> Items;

        public int Selection;

        public void Shuffle()
        {
            Random random = new();
            Selection = random.Next() % Items.Count;
            Items = [.. Items.OrderBy(a => random.Next())];
        }

        public (string name, string author, string description, string type, string source, string note) GetNavigationInfo()
        {
            return
            (
                Name,
                Author,
                Description,
                "Pack",
                string.Empty,
                Note
            );
        }
    }
}

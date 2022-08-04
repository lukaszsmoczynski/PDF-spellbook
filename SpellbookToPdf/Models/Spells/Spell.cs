using System;
using System.Collections.Generic;

namespace SpellbookToPdf.Models.Spells
{
    public class Spell: IDnDElement
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public School School { get; set; }
        public CastTime CastTime { get; set; }
        public Range Range { get; set; }
        public Components Components { get; set; }
        public Duration Duration { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Classes { get; set; }
        public bool Ritual { get; set; }
        public string Upcasting { get; set; }
    }
}

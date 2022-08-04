using System.Collections.Generic;

namespace SpellbookToPdf.Models.Spells
{
    public class Components
    {
        public bool Verbal { get; set; }
        public bool Somatic { get; set; }
        public bool Material { get; set; }

        public IList<MaterialComponent> MaterialComponents { get; set; }
    }
}

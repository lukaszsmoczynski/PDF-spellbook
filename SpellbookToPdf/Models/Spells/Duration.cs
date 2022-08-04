namespace SpellbookToPdf.Models.Spells
{

    public class Duration
    {
        public int Amount { get; set; }
        public DurationUnit Unit { get; set; }
        public bool Concentration { get; set; }
        public bool UpTo { get; set; }
    }
}

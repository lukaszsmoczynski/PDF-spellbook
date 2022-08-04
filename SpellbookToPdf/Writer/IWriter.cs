using SpellbookToPdf.Models;

namespace SpellbookToPdf.Writer
{
    interface IWriter
    {
        public void Print(params IDnDElement[] elements);
    }
}

using System;

namespace SpellbookToPdf.Models
{
    internal interface IDnDElement
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}
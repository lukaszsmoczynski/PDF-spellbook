using iText.Kernel.Colors;
using SpellbookToPdf.Models.Spells;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpellbookToPdf.Writer.PdfWriter
{
    static class Helper
    {
        static public Color GetBackgroundColor(Spell spell)
        {
            if (spell.Classes.ToList().Count > 1)
            {
                return Constants.COMMON_COLOR;
            }

            if (spell.Classes.First() == "Artificer")
            {
                return Constants.ARTIFICER_COLOR;
            }
            if (spell.Classes.First() == "Bard")
            {
                return Constants.BARD_COLOR;
            }
            if (spell.Classes.First() == "Cleric")
            {
                return Constants.CLERIC_COLOR;
            }
            if (spell.Classes.First() == "Druid")
            {
                return Constants.DRUID_COLOR;
            }
            if (spell.Classes.First() == "Paladin")
            {
                return Constants.PALADIN_COLOR;
            }
            if (spell.Classes.First() == "Ranger")
            {
                return Constants.RANGER_COLOR;
            }
            if (spell.Classes.First() == "Sorcerer")
            {
                return Constants.WIZARD_COLOR;
            }
            if (spell.Classes.First() == "Warlock")
            {
                return Constants.WARLOCK_COLOR;
            }
            if (spell.Classes.First() == "Wizard")
            {
                return Constants.WIZARD_COLOR;
            }

            return Constants.COMMON_COLOR;
        }

        public static string TranslateCastingTime(CastTime castTime)
        {
            string result = "";
            switch (castTime.Unit)
            {
                case CastTimeUnit.Action: result = $"{castTime.Amount} action"; break;
                case CastTimeUnit.Minute: result = $"{castTime.Amount} minute"; break;
                case CastTimeUnit.Hour: result = $"{castTime.Amount} hour"; break;
                case CastTimeUnit.BonusAction: result = $"{castTime.Amount} bonus action"; break;
                case CastTimeUnit.Reaction: result = $"{castTime.Amount} reaction"; break;
                case CastTimeUnit.Round: result = $"{castTime.Amount} round"; break;
            }

            if (castTime.Amount > 1)
            {
                result += "s";
            }
            return result;
        }

        internal static string TranslateDuration(Duration duration)
        {
            string result = "";

            if (duration.Concentration)
            {
                result = "Concentration";
                if (duration.UpTo)
                {
                    result += ", up to ";
                }
            }

            switch (duration.Unit)
            {
                case DurationUnit.Instant: result += "Instantenous"; break;
                case DurationUnit.Round: result += $"{duration.Amount} round"; break;
                case DurationUnit.Minute: result += $"{duration.Amount} minute"; break;
                case DurationUnit.Hour: result += $"{duration.Amount} hour"; break;
                case DurationUnit.Day: result += $"{duration.Amount} day"; break;
                case DurationUnit.Special: result += "Special"; break;
                case DurationUnit.UntilDispelled: result += "Until dispelled"; break;
                case DurationUnit.UntilDispelledOrTriggered: result += "Until dispelled or triggered"; break;
            }

            if (duration.Amount > 1)
            {
                result += "s";
            }

            return result;
        }
        public static string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
                default:
                    break;
            }

            return (num % 10) switch
            {
                1 => num + "st",
                2 => num + "nd",
                3 => num + "rd",
                _ => num + "th",
            };
        }

        internal static string TranslateLevelSchool(int level, School school)
        {
            string result;
            if (level == 0)
            {
                result = "Cantrip";
            }
            else
            {
                result = AddOrdinal(level) + " level";
            }

            switch (school)
            {
                case School.Abjuration: result += " Abjuration"; break;
                case School.Conjuration: result += " Conjuration"; break;
                case School.Divination: result += " Divination"; break;
                case School.Enchantment: result += " Enchantment"; break;
                case School.Evocation: result += " Evocation"; break;
                case School.Illusion: result += " Illusion"; break;
                case School.Necromancy: result += " Necromancy"; break;
                case School.Transmutation: result += " Transmutation"; break;
            }

            return result;
        }

        public static string TranslateRange(Models.Spells.Range range)
        {
            string result = "";
            switch (range.Unit)
            {
                case RangeUnit.Self: result = "Self"; break;
                case RangeUnit.Foot: result = $"{range.Amount} {(range.Amount > 1 ? "feet" : "foot")}"; break;
                case RangeUnit.Mile: result = $"{range.Amount} {(range.Amount > 1 ? "miles" : "mile")}"; break;
                case RangeUnit.Sight: result = $"Sight"; break;
                case RangeUnit.Special: result = $"Special"; break;
                case RangeUnit.Touch: result = $"Touch"; break;
                case RangeUnit.Unlimited: result = $"Unlimited"; break;
            }
            return result;
        }

        public static string TranslateComponents(Components components)
        {
            var result = new List<string>();
            if (components.Verbal)
            {
                result.Add("V");
            }
            if (components.Somatic)
            {
                result.Add("S");
            }
            if (components.Material)
            {
                result.Add("M");
            }
            return string.Join(", ", result);
        }

        static public Color GetBetterColor(Color constantColor, Color color, Color alternativeColor = null, bool preferColor = false)
        {
            var L1 = CalcRelativeLuminance(constantColor);
            var L2 = CalcRelativeLuminance(color);
            if (alternativeColor is null)
            {
                alternativeColor = InvertColor(color);
            }
            var L3 = CalcRelativeLuminance(alternativeColor);

            var cr1 = (Math.Max(L1, L2) + 0.05) / (Math.Min(L1, L2) + 0.05);
            var cr2 = (Math.Max(L1, L3) + 0.05) / (Math.Min(L1, L3) + 0.05);

            if (preferColor && cr1 > 7)
            {
                return color;
            }

            return cr1 > cr2 ? color : alternativeColor;
        }

        static private double CalcRelativeLuminance(Color color)
        {
            var cv = color.GetColorValue();
            var R = cv[0] <= 0.03928 ? cv[0] / 12.92 : Math.Pow((cv[0] + 0.055) / 1.055, 2.4);
            var G = cv[1] <= 0.03928 ? cv[1] / 12.92 : Math.Pow((cv[1] + 0.055) / 1.055, 2.4);
            var B = cv[2] <= 0.03928 ? cv[2] / 12.92 : Math.Pow((cv[2] + 0.055) / 1.055, 2.4);

            return 0.2126 * R + 0.7152 * G + 0.0722 * B;
        }

        static private Color InvertColor(Color color)
        {
            return new DeviceRgb(1 - color.GetColorValue()[0], 1 - color.GetColorValue()[1], 1 - color.GetColorValue()[2]);
        }
    }
}

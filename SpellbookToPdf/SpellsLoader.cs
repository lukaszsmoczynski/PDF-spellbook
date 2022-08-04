using SpellbookToPdf.Models.Spells;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SpellbookToPdf
{
    class SpellsLoader
    {
        public static List<Spell> LoadSpells()
        {
            var result = new List<Spell>();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(File.ReadAllText(Environment.CurrentDirectory + "\\Resources\\Data\\Spells\\spells.xml"));
            var root = xmlDoc.ChildNodes[0];
            foreach (XmlNode node in root.SelectNodes("spell"))
            {
                result.Add(ProcessSpell(node));
            }

            return result;
        }

        private static Spell ProcessSpell(XmlNode node)
        {
            var spell = new Spell
            {
                Id = Guid.NewGuid(),
                Name = node.SelectSingleNode("name").InnerText,
                Level = int.Parse(node.SelectSingleNode("level").InnerText),
                School = ProcessSchool(node.SelectSingleNode("school").InnerText),
                CastTime = ProcessCastTime(node.SelectSingleNode("time").InnerText),
                Range = ProcessRange(node.SelectSingleNode("range").InnerText),
                Components = ProcessComponents(node.SelectSingleNode("components")),
                Duration = ProcessDuration(node.SelectSingleNode("duration").InnerText),
                Classes = ProcessClasses(node.SelectSingleNode("classes").InnerText),
                Description = ProcessDescription(node.SelectNodes("text")),
                Ritual = node.SelectSingleNode("text")?.InnerXml.Equals("YES") ?? false,
                Upcasting = ProcessUpcasting(node.SelectNodes("upcasting")),
            };

            return spell;
        }

        private static string ProcessUpcasting(XmlNodeList xmlNodeList)
        {
            var result = "";
            foreach (XmlNode node in xmlNodeList)
            {
                result += node.InnerText + "\n";
            }
            return result.Replace("\n\n", "\n");
        }

        private static string ProcessDescription(XmlNodeList xmlNodeList)
        {
            var result = "";
            foreach (XmlNode node in xmlNodeList)
            {
                result += node.InnerText + "\n";
            }
            return result.Replace("\n\n", "\n");
        }

        private static IEnumerable<string> ProcessClasses(string classes)
        {
            var result = new List<string>();

            var tokens = classes.Split(',');
            foreach (var className in tokens.Select(p => p.Trim()))
            {
                if (className.Contains("("))
                {
                    continue;
                }
                result.Add(className);
            }

            return result;
        }

        private static Duration ProcessDuration(string innerText)
        {
            var result = new Duration();

            switch (innerText)
            {
                case "Instantaneous":
                    result.Unit = DurationUnit.Instant;
                    return result;
                case "Special":
                    result.Unit = DurationUnit.Special;
                    return result;
                case "Until dispelled or triggered":
                    result.Unit = DurationUnit.UntilDispelledOrTriggered;
                    return result;
                case "Until dispelled":
                    result.Unit = DurationUnit.UntilDispelled;
                    return result;
            }
            if (innerText.StartsWith("Concentration"))
            {
                result.Concentration = true;
                var tokens = innerText.Split(',');
                innerText = tokens[1].Trim();
            }
            innerText = innerText.ToLower();

            if (innerText.StartsWith("up to"))
            {
                result.UpTo = true;
                innerText = innerText[6..];
            }

            var x = innerText.Split(" ");
            switch (x[1])
            {
                case "round":
                case "rounds": result.Unit = DurationUnit.Round; break;
                case "minute":
                case "minutes": result.Unit = DurationUnit.Minute; break;
                case "hour":
                case "hours": result.Unit = DurationUnit.Hour; break;
                case "day":
                case "days": result.Unit = DurationUnit.Day; break;
            }
            result.Amount = int.Parse(x[0]);

            return result;
        }

        private static Components ProcessComponents(XmlNode components)
        {
            var result = new Components();
            var _components = components.SelectSingleNode("value").InnerText;
            result.Verbal = _components.Contains("V");
            result.Somatic = _components.Contains("S");
            result.Material = _components.Contains("M");

            result.MaterialComponents = ProcessMaterials(components.SelectSingleNode("materials"))?.ToList();

            return result;
        }

        private static IEnumerable<MaterialComponent> ProcessMaterials(XmlNode materials)
        {
            if (materials is null)
            {
                return null;
            }

            var result = new List<MaterialComponent>();
            foreach (XmlNode node in materials)
            {
                var materialComponent = new MaterialComponent()
                {
                    Name = node.SelectSingleNode("name").InnerText,
                    Consumed = node.SelectSingleNode("consumed")?.InnerText.Equals("1") ?? false,
                    Description = node.SelectSingleNode("description")?.InnerText,
                };

                var valueNode = node.SelectSingleNode("value");
                if (valueNode is not null)
                {
                    var tokens = valueNode.InnerText.Split(' ');
                    materialComponent.Value = float.Parse(tokens[0]);
                }

                result.Add(materialComponent);
            }
            return result;
        }

        private static Models.Spells.Range ProcessRange(string range)
        {
            var result = new Models.Spells.Range();

            if (range.StartsWith("Self"))
            {
                result.Unit = RangeUnit.Self;
                return result;
            }
            else if (range.StartsWith("Touch"))
            {
                result.Unit = RangeUnit.Touch;
                return result;
            }
            else if (range.StartsWith("Sight"))
            {
                result.Unit = RangeUnit.Sight;
                return result;
            }
            else if (range.StartsWith("Special"))
            {
                result.Unit = RangeUnit.Special;
                return result;
            }
            else if (range.StartsWith("Unlimited"))
            {
                result.Unit = RangeUnit.Special;
                return result;
            }

            var tokens = range.Split(' ');
            result.Amount = int.Parse(tokens[0]);
            switch (tokens[1])
            {
                case "feet": result.Unit = RangeUnit.Foot; break;
                case "mile":
                case "miles":
                    result.Unit = RangeUnit.Mile;
                    break;
            }

            return result;
        }

        private static CastTime ProcessCastTime(string castTime)
        {
            var tokens = castTime.Split(' ');
            var result = new CastTime
            {
                Amount = int.Parse(tokens[0])
            };

            switch (tokens[1])
            {
                case "bonus": result.Unit = CastTimeUnit.BonusAction; break;
                case "reaction": result.Unit = CastTimeUnit.Reaction; break;
                case "action": result.Unit = CastTimeUnit.Action; break;
                case "round": result.Unit = CastTimeUnit.Round; break;
                case "minute": result.Unit = CastTimeUnit.Minute; break;
                case "hour": result.Unit = CastTimeUnit.Hour; break;
            }

            return result;
        }

        private static School ProcessSchool(string schoolCode)
        {
            return schoolCode switch
            {
                "A" => School.Abjuration,
                "C" => School.Conjuration,
                "D" => School.Divination,
                "EN" => School.Enchantment,
                "EV" => School.Evocation,
                "I" => School.Illusion,
                "N" => School.Necromancy,
                "T" => School.Transmutation,
                _ => throw new Exception(),
            };
        }
    }
}

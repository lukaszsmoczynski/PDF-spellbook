using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout.Properties;
using SpellbookToPdf.Models.Spells;
using System;
using System.Linq;

namespace SpellbookToPdf.Writer.PdfWriter
{
    static class Constants
    {
        public static float TEXT_MARGIN = 3;

        public const float EPSILON = .1f;
        public const float CARD_WIDTH = 180;
        public const float CARD_HEIGHT = 250;
        public const float MARGIN = 3;
        public const float CARD_BORDER_WIDTH = 7;
        public const float RADIUS = 5;
        public const float LINE_WIDTH = 1;
        public const float TITLE_HEIGHT = 17;
        public const float FULL_INNER_SPACE_WIDTH = CARD_WIDTH - 2 * CARD_BORDER_WIDTH;
        public const float HALF_INNER_SPACE_WIDTH = (FULL_INNER_SPACE_WIDTH - LINE_WIDTH) / 2;

        public const float BASE_DETAILS_SIZE = 9;
        public const float MINIMUM_DETAILS_SIZE = 9;

        public static Color COMMON_COLOR = new DeviceRgb(128, 0, 0);
        public static Color ARTIFICER_COLOR = new DeviceRgb(248, 58, 34);
        public static Color BARD_COLOR = new DeviceRgb(140, 83, 133);
        public static Color CLERIC_COLOR = new DeviceRgb(255, 173, 70);
        public static Color DRUID_COLOR = new DeviceRgb(96, 184, 93);
        public static Color PALADIN_COLOR = new DeviceRgb(59, 175, 177);
        public static Color RANGER_COLOR = new DeviceRgb(133, 112, 86);
        public static Color WARLOCK_COLOR = new DeviceRgb(140, 83, 133);
        public static Color WIZARD_COLOR = new DeviceRgb(128, 0, 0);
        public static Color SORCERER_COLOR = new DeviceRgb(248, 58, 34);

        public static float TITLE_MAX_FONT_SIZE = 20;

        public static Color FOREGROUND_COLOR = ColorConstants.WHITE;

        //public static PdfFont FONT
        //    = PdfFontFactory.CreateFont(
        //        "D:\\Szkola\\Programowanie\\.NET\\SpellbookToPdf\\SpellbookToPdf\\Resources\\Fonts\\Modesto Poster.ttf",
        //        "CP1252",
        //        PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //troszkę źle wylicza wysokość
        public static PdfFont FONT
            = PdfFontFactory.CreateFont(
                "D:\\Szkola\\Programowanie\\.NET\\SpellbookToPdf\\SpellbookToPdf\\Resources\\Fonts\\Modesto Condensed Light.ttf",
                "CP1252",
                PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        public static Paragraph DefaultParagraph() => new Paragraph()
            .SetFont(FONT)
            .SetMargin(0)
            .SetMultipliedLeading(1)
            .SetFontColor(Helper.GetBetterColor(FOREGROUND_COLOR, ColorConstants.BLACK));

        public static Paragraph ConstParagraph() => DefaultParagraph()
            .SetFontSize(4);

        public static Paragraph TitleParagraph() => DefaultParagraph()
            .SetFontSize(TITLE_MAX_FONT_SIZE)
            .SetBold()
            .SetTextAlignment(TextAlignment.CENTER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE);

        public static Paragraph CastingTimeHeaderParagraph() => ConstParagraph()
            .SetTextAlignment(TextAlignment.CENTER)
            .Add("CASTING TIME");

        public static Paragraph CastingTimeParagraph(Spell spell) => DefaultParagraph()
            .SetFontSize(10)
            .SetMargin(0)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(Helper.TranslateCastingTime(spell.CastTime));

        public static Paragraph RangeHeaderParagraph() => ConstParagraph()
            .SetTextAlignment(TextAlignment.CENTER)
            .Add("RANGE");

        public static Paragraph RangeParagraph(Spell spell) => DefaultParagraph()
            .SetFontSize(10)
            .SetMargin(1)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(Helper.TranslateRange(spell.Range));

        public static Paragraph ComponentsHeaderParagraph() => ConstParagraph()
            .SetTextAlignment(TextAlignment.CENTER)
            .Add("COMPONENTS");

        public static Paragraph ComponentsParagraph(Spell spell) => DefaultParagraph()
            .SetFontSize(10)
            .SetMargin(1)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(Helper.TranslateComponents(spell.Components));

        public static Paragraph DurationHeaderParagraph() => ConstParagraph()
            .SetTextAlignment(TextAlignment.CENTER)
            .Add("DURATION");

        public static Paragraph DurationParagraph(Spell spell) => DefaultParagraph()
            .SetFontSize(10)
            .SetMargin(1)
            .SetTextAlignment(TextAlignment.CENTER)
            .Add(Helper.TranslateDuration(spell.Duration));

        public static Paragraph MaterialComponentsParagraph(Spell spell) => DefaultParagraph()
            .SetFontSize(8)
            .SetMargin(1)
            .SetFontColor(Helper.GetBetterColor(Helper.GetBackgroundColor(spell), ColorConstants.WHITE, ColorConstants.BLACK))
            .Add(string.Join(",", spell.Components.MaterialComponents.Select(component => { return component.Name; })));

        public static Paragraph DetailsParagraph() => DefaultParagraph()
            .SetFontSize(8)
            .SetPaddingLeft(6)
            .SetPaddingRight(6)
            .SetFirstLineIndent(5)
            .SetTextAlignment(TextAlignment.JUSTIFIED);

        public static Paragraph AtHigherLevelsHeaderParagraph(Spell spell) => DefaultParagraph()
            .SetFontSize(8)
            .SetPadding(2)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontColor(Helper.GetBetterColor(Helper.GetBackgroundColor(spell), ColorConstants.WHITE, ColorConstants.BLACK))
            .Add("At Higher Levels:");

        public static Paragraph AtHigherLevelsParagraph(Spell spell) => DefaultParagraph()
            .SetFontSize(8)
            .SetPaddingLeft(6)
            .SetPaddingRight(6)
            .SetTextAlignment(TextAlignment.JUSTIFIED)
            .Add(spell.Upcasting);

        public static Paragraph ClassesParagraph(Spell spell) => DefaultParagraph()
            .SetMarginBottom(3)
            .SetFontSize(6)
            .SetVerticalAlignment(VerticalAlignment.BOTTOM)
            .SetFontColor(Helper.GetBetterColor(Helper.GetBackgroundColor(spell), ColorConstants.BLACK))
            .Add(string.Join(", ", spell.Classes));

        public static Paragraph LevelSchoolParagraph(Spell spell) => DefaultParagraph()
            .SetMarginBottom(3)
            .SetFontSize(6)
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetVerticalAlignment(VerticalAlignment.BOTTOM)
            .SetFontColor(Helper.GetBetterColor(Helper.GetBackgroundColor(spell), ColorConstants.WHITE, ColorConstants.BLACK))
            .Add(Helper.TranslateLevelSchool(spell.Level, spell.School));

        public static Paragraph LevelBackParagraph(Spell spell) => DefaultParagraph()
            .SetFontSize(25)
            .SetMargin(3)
            .SetFontColor(Helper.GetBackgroundColor(spell))
            .Add(spell.Level.ToString());

        public static Paragraph LevelBackTopParagraph(Spell spell) => LevelBackParagraph(spell)
            .SetTextAlignment(TextAlignment.RIGHT);

        public static Paragraph LevelBackBottomParagraph(Spell spell) => LevelBackParagraph(spell)
            .SetRotationAngle(Math.PI);
    }
}

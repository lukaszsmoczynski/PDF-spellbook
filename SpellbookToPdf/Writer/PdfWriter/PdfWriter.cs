//#define TEST_DEBUG
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Layout;
using iText.Layout.Renderer;
using SpellbookToPdf.Logger;
using SpellbookToPdf.Models;
using SpellbookToPdf.Models.Spells;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpellbookToPdf.Writer.PdfWriter
{
    internal class PdfWriter : IPdfWriter
    {
        private ILogger logger { get; set; }

        public PdfWriter(ILogger logger)
        {
            this.logger = logger;
        }

        public void Print(params IDnDElement[] elements)
        {
            //create document
            //create fron page
            //create


            Spell[] Spells = elements as Spell[];

            var pdfDocument = new PdfDocument(new iText.Kernel.Pdf.PdfWriter(new FileStream("D:/output.pdf", FileMode.Create, FileAccess.Write)));
            var document = new Document(pdfDocument);
            var pdfCanvas = new PdfCanvas(pdfDocument.AddNewPage(PageSize.A4)).SetLineWidth(0);

            var pageWidth = pdfDocument.GetLastPage().GetPageSizeWithRotation().GetWidth();
            var pageHeight = pdfDocument.GetLastPage().GetPageSizeWithRotation().GetHeight();
            int horizontalCardCount = (int)((pageWidth - 2 * Constants.MARGIN) / Constants.CARD_WIDTH);
            int verticalCardcount = (int)((pageHeight - 2 * Constants.MARGIN) / Constants.CARD_HEIGHT);

            int borderXMargin = (int)((pageWidth - horizontalCardCount * (Constants.CARD_WIDTH + Constants.MARGIN) - Constants.MARGIN) / 2);
            int borderYMargin = (int)((pageHeight - verticalCardcount * (Constants.CARD_HEIGHT + Constants.MARGIN) - Constants.MARGIN) / 2);

            var i = 0;
            var j = 0;

            var spells = new List<Spell>();
            foreach (var spell in Spells)
            {
                var spaceForCastTime = Calc(document, Constants.HALF_INNER_SPACE_WIDTH, Constants.CastingTimeHeaderParagraph(), Constants.CastingTimeParagraph(spell)) + 1;
                var spaceForRange = Calc(document, Constants.HALF_INNER_SPACE_WIDTH, Constants.RangeHeaderParagraph(), Constants.RangeParagraph(spell)) + 1;
                var spaceForComponents = Calc(document, Constants.HALF_INNER_SPACE_WIDTH, Constants.ComponentsHeaderParagraph(), Constants.ComponentsParagraph(spell)) + 1;
                var spaceForDuration = Calc(document, Constants.HALF_INNER_SPACE_WIDTH, Constants.DurationHeaderParagraph(), Constants.DurationParagraph(spell)) + 1;
                var verticalLineHeight = (spaceForCastTime > spaceForRange ? spaceForCastTime : spaceForRange)
                    + (spaceForComponents > spaceForDuration ? spaceForComponents : spaceForDuration)
                    + Constants.LINE_WIDTH;

                var spaceForClassesLevelSchool = Constants.CARD_BORDER_WIDTH;
                //calculate space needed for classes
                var spaceForClasses = Calc(document, Constants.HALF_INNER_SPACE_WIDTH, Constants.ClassesParagraph(spell)) + 1;
                var spaceForLevelSchool = Calc(document, Constants.HALF_INNER_SPACE_WIDTH, Constants.LevelSchoolParagraph(spell)) + 1;
                spaceForClassesLevelSchool = spaceForClasses > spaceForLevelSchool ? spaceForClasses : spaceForLevelSchool;

                var spaceForMaterialComponents = Constants.LINE_WIDTH;
                if (spell.Components?.MaterialComponents?.Count > 0)
                    //calculate space needed for material components
                    spaceForMaterialComponents = Calc(document, Constants.FULL_INNER_SPACE_WIDTH, Constants.MaterialComponentsParagraph(spell));

                var firstCardHeightAvailable = Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + Constants.LINE_WIDTH + verticalLineHeight + spaceForMaterialComponents + spaceForClassesLevelSchool);
                var nextCardHeightAvailable = Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + Constants.LINE_WIDTH + Constants.CARD_BORDER_WIDTH);

                var spellDetails = new List<string>();
                var cardCount = CalculateCardCount(
                    document, spell, firstCardHeightAvailable, nextCardHeightAvailable, Constants.FULL_INNER_SPACE_WIDTH,
                    out spellDetails, out float detailsFontSize);

                var spellsForHigherLevels = Calc(document, Constants.FULL_INNER_SPACE_WIDTH, Constants.AtHigherLevelsHeaderParagraph(spell).SetFontSize(detailsFontSize));

                for (int splittedCardNumber = 1; splittedCardNumber <= cardCount; splittedCardNumber++)
                {
                    spells.Add(spell);
                    Rectangle cardRect = new(
                        borderXMargin + i * (Constants.CARD_WIDTH + Constants.MARGIN),
                        pageHeight - (borderYMargin + Constants.MARGIN + (j + 1) * (Constants.CARD_HEIGHT + Constants.MARGIN)),
                        Constants.CARD_WIDTH,
                        Constants.CARD_HEIGHT);

                    //background
                    pdfCanvas
                        .SaveState()
                        .SetFillColor(Helper.GetBackgroundColor(spell))
                        .SetStrokeColor(Helper.GetBackgroundColor(spell))
                        .Rectangle(cardRect)
                        .Fill()
                        .RestoreState();

                    //rounded white rectangle
                    pdfCanvas
                        .SaveState()
                        .SetFillColor(Constants.FOREGROUND_COLOR)
                        .SetStrokeColor(Constants.FOREGROUND_COLOR)
                        .RoundRectangle(
                            cardRect.GetX() + Constants.CARD_BORDER_WIDTH,
                            cardRect.GetY() + (splittedCardNumber == 1 ? spaceForClassesLevelSchool : Constants.CARD_BORDER_WIDTH),
                            cardRect.GetWidth() - 2 * Constants.CARD_BORDER_WIDTH,
                            cardRect.GetHeight() - ((splittedCardNumber == 1 ? spaceForClassesLevelSchool : Constants.CARD_BORDER_WIDTH) + Constants.CARD_BORDER_WIDTH),
                            Constants.RADIUS)
                        .Fill()
                        .RestoreState();

                    GenerateTitle(document, pdfCanvas, cardRect, spell, splittedCardNumber, cardCount);

                    if (splittedCardNumber <= 1)
                    {
                        //vertical line below title
                        pdfCanvas
                            .SaveState()
                            .SetFillColor(Helper.GetBackgroundColor(spell))
                            .SetStrokeColor(Helper.GetBackgroundColor(spell))
                            .Rectangle(
                                cardRect.GetX() + Constants.CARD_BORDER_WIDTH + Constants.HALF_INNER_SPACE_WIDTH,
                                cardRect.GetY() + Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + Constants.LINE_WIDTH + verticalLineHeight),
                                Constants.LINE_WIDTH,
                                verticalLineHeight)
                            .Fill()
                            .RestoreState();

                        GenerateCastingTimeAndRange(pdfCanvas, cardRect, spell, spaceForCastTime, spaceForRange);
                        GenerateComponentsAndDuration(pdfCanvas, cardRect, spell, spaceForComponents, spaceForDuration, spaceForCastTime, spaceForRange, spaceForMaterialComponents, verticalLineHeight);
                        GenerateClassesAndLevelSchool(pdfCanvas, cardRect, spell, spaceForClasses, spaceForLevelSchool);
                    }

                    GenerateDetails(pdfCanvas, cardRect, spell, verticalLineHeight, spaceForClassesLevelSchool, spellDetails[splittedCardNumber - 1], splittedCardNumber, spaceForMaterialComponents, detailsFontSize);

                    if (splittedCardNumber == cardCount)
                    {
                        GenerateAtHigherLevels(pdfCanvas, cardRect, spell, spellsForHigherLevels, detailsFontSize);
                    }

                    if (++i >= horizontalCardCount)
                    {
                        i = 0;
                        if (++j >= verticalCardcount)
                        {
                            j = 0;
                            pdfCanvas = new PdfCanvas(pdfDocument.AddNewPage(PageSize.A4));
                            GenerateBack(document, pdfCanvas, spells, borderXMargin, borderYMargin, pageWidth, pageHeight, horizontalCardCount);
                            spells.Clear();
                            pdfCanvas = new PdfCanvas(pdfDocument.AddNewPage(PageSize.A4));
                        }
                    }
                }
            }

            if (pdfDocument.GetNumberOfPages() % 2 != 0)
            {
                pdfCanvas = new PdfCanvas(pdfDocument.AddNewPage(PageSize.A4));
                GenerateBack(document, pdfCanvas, spells, borderXMargin, borderYMargin, pageWidth, pageHeight, horizontalCardCount);
            }

            document.Close();
        }

        private static void GenerateAtHigherLevels(PdfCanvas pdfCanvas, Rectangle cardRect, Spell spell, float spaceForHigherLevels, float detailsFontSize)
        {
            //line for higher levels
            var rect = new Rectangle(cardRect.GetX() + Constants.CARD_BORDER_WIDTH - Constants.EPSILON,
                    cardRect.GetY() + 40,
                    Constants.FULL_INNER_SPACE_WIDTH + 2 * Constants.EPSILON,
                    spaceForHigherLevels);
            pdfCanvas
                .SaveState()
                .SetFillColor(Helper.GetBackgroundColor(spell))
                .SetStrokeColor(Helper.GetBackgroundColor(spell))
                .Rectangle(rect)
                .Fill()
                .RestoreState();
            var atHigherLevelsHeaderCanvas = new Canvas(pdfCanvas, rect);
            atHigherLevelsHeaderCanvas.Add(Constants.AtHigherLevelsHeaderParagraph(spell).SetFontSize(detailsFontSize));

            //upcasting info
            var upcastingRect = new Rectangle(cardRect.GetX() + Constants.CARD_BORDER_WIDTH - Constants.EPSILON,
                    cardRect.GetY(),
                    Constants.FULL_INNER_SPACE_WIDTH + 2 * Constants.EPSILON,
                    40);
            CreateDebugRectangle(pdfCanvas, upcastingRect);
            var upcastingcanvas = new Canvas(pdfCanvas, upcastingRect);
            upcastingcanvas.Add(Constants.AtHigherLevelsParagraph(spell));
        }

        private static void GenerateBack(Document document, PdfCanvas pdfCanvas, List<Spell> spells, float borderXMargin, float borderYMargin, float pageWidth, float pageHeight, int wCount)
        {
            var i = 0;
            var j = 0;

            foreach (var spell in spells)
            //for (int splittedCardNumber = 1; splittedCardNumber <= cardCount; splittedCardNumber++)
            {
                Rectangle cardRect = new(
                    pageWidth - (borderXMargin + Constants.CARD_WIDTH + i * (Constants.CARD_WIDTH + Constants.MARGIN)),
                    pageHeight - (borderYMargin + Constants.MARGIN + (j + 1) * (Constants.CARD_HEIGHT + Constants.MARGIN)),
                    Constants.CARD_WIDTH,
                    Constants.CARD_HEIGHT);

                //background image (texture)

                //background border
                pdfCanvas
                    .SaveState()
                    .SetFillColor(Helper.GetBackgroundColor(spell))
                    .SetStrokeColor(Helper.GetBackgroundColor(spell))
                    .Rectangle(cardRect)
                    .Fill()
                    .RestoreState();

                //rounded white rectangle
                pdfCanvas
                    .SaveState()
                    .SetFillColor(Constants.FOREGROUND_COLOR)
                    .SetStrokeColor(Constants.FOREGROUND_COLOR)
                    .RoundRectangle(
                        cardRect.GetX() + Constants.CARD_BORDER_WIDTH,
                        cardRect.GetY() + Constants.CARD_BORDER_WIDTH,
                        cardRect.GetWidth() - 2 * Constants.CARD_BORDER_WIDTH,
                        cardRect.GetHeight() - (Constants.CARD_BORDER_WIDTH + Constants.CARD_BORDER_WIDTH),
                        Constants.RADIUS)
                    .Fill()
                    .RestoreState();

                //inner rounded color rectangle
                if (i % 2 == 0)
                {
                    pdfCanvas
                        .SaveState()
                        .SetFillColor(Helper.GetBackgroundColor(spell))
                        .SetStrokeColor(Helper.GetBackgroundColor(spell))
                        .RoundRectangle(
                            cardRect.GetX() + Constants.CARD_BORDER_WIDTH + 10,
                            cardRect.GetY() + Constants.CARD_BORDER_WIDTH + 10,
                            cardRect.GetWidth() - 2 * Constants.CARD_BORDER_WIDTH - 20,
                            cardRect.GetHeight() - (Constants.CARD_BORDER_WIDTH + Constants.CARD_BORDER_WIDTH) - 20,
                            Constants.RADIUS)
                        .Fill()
                        .RestoreState();

                    pdfCanvas
                        .SaveState()
                        .SetFillColor(Constants.FOREGROUND_COLOR)
                        .SetStrokeColor(Constants.FOREGROUND_COLOR)
                        .RoundRectangle(
                            cardRect.GetX() + Constants.CARD_BORDER_WIDTH + 10 + Constants.LINE_WIDTH,
                            cardRect.GetY() + Constants.CARD_BORDER_WIDTH + 10 + Constants.LINE_WIDTH,
                            cardRect.GetWidth() - 2 * Constants.CARD_BORDER_WIDTH - 2 * (10 + Constants.LINE_WIDTH),
                            cardRect.GetHeight() - (Constants.CARD_BORDER_WIDTH + Constants.CARD_BORDER_WIDTH) - 2 * (10 + Constants.LINE_WIDTH),
                            Constants.RADIUS)
                        .Fill()
                        .RestoreState();
                }
                else
                {
                    pdfCanvas
                        .SaveState()
                        .SetLineWidth(2)
                        .SetFillColor(Helper.GetBackgroundColor(spell))
                        .SetStrokeColor(Helper.GetBackgroundColor(spell))
                        .RoundRectangle(
                            cardRect.GetX() + Constants.CARD_BORDER_WIDTH + 10,
                            cardRect.GetY() + Constants.CARD_BORDER_WIDTH + 10,
                            cardRect.GetWidth() - 2 * Constants.CARD_BORDER_WIDTH - 20,
                            cardRect.GetHeight() - (Constants.CARD_BORDER_WIDTH + Constants.CARD_BORDER_WIDTH) - 20,
                            2)
                        .Stroke()
                        .RestoreState();
                }

                //center image
                var backImageRect = new Rectangle(
                    cardRect.GetX() + Constants.CARD_BORDER_WIDTH + 10 + Constants.LINE_WIDTH,
                    cardRect.GetY() + Constants.CARD_BORDER_WIDTH + 10 + Constants.LINE_WIDTH,
                    cardRect.GetWidth() - 2 * Constants.CARD_BORDER_WIDTH - 2 * (10 + Constants.LINE_WIDTH),
                    cardRect.GetHeight() - (Constants.CARD_BORDER_WIDTH + Constants.CARD_BORDER_WIDTH) - 2 * (10 + Constants.LINE_WIDTH)
                    );
                CreateDebugRectangle(pdfCanvas, backImageRect, ColorConstants.YELLOW);

                //draw top image
                //draw center image
                var percerntFill = .75f;
                ImageData imageData = ImageDataFactory.Create(Environment.CurrentDirectory + $"\\Resources\\Spell Schools\\{spell.School}.png");
                Image image = new Image(imageData)
                    .SetOpacity(0.4f)
                    .ScaleToFit(backImageRect.GetWidth() * percerntFill, backImageRect.GetHeight() * percerntFill);
                var rect2 = new Rectangle(
                    backImageRect.GetX() + (backImageRect.GetWidth() - image.GetImageScaledWidth()) / 2,
                    backImageRect.GetY() + (backImageRect.GetHeight() - image.GetImageScaledHeight()) / 2,
                    image.GetImageScaledWidth(),
                    image.GetImageScaledHeight());
                CreateDebugRectangle(pdfCanvas, rect2, ColorConstants.BLUE);
                var imgCanvas = new Canvas(pdfCanvas, rect2);
                imgCanvas.Add(image);
                //pdfCanvas.AddImageAt(imageData, cardRect.GetX() + (cardRect.GetWidth() - imageData.GetWidth()) / 2, cardRect.GetY() + (cardRect.GetHeight() - imageData.GetHeight()) / 2, false);
                //draw bottom image

                var spaceForLevel = Calc(document, cardRect.GetWidth() - 2 * Constants.CARD_BORDER_WIDTH - 2 * (10 + Constants.LINE_WIDTH), Constants.LevelBackParagraph(spell));

                var levelTopRect = new Rectangle(
                    cardRect.GetX() + Constants.CARD_BORDER_WIDTH + 10 + Constants.LINE_WIDTH,
                    cardRect.GetY() + Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + 10 + Constants.LINE_WIDTH + spaceForLevel),
                    cardRect.GetWidth() - 2 * Constants.CARD_BORDER_WIDTH - 2 * (10 + Constants.LINE_WIDTH),
                    spaceForLevel
                    );
                CreateDebugRectangle(pdfCanvas, levelTopRect);
                var levelTopCanvas = new Canvas(pdfCanvas, levelTopRect);
                levelTopCanvas.Add(Constants.LevelBackTopParagraph(spell));

                var levelBottomRect = new Rectangle(
                    cardRect.GetX() + Constants.CARD_BORDER_WIDTH + 10 + Constants.LINE_WIDTH,
                    cardRect.GetY() + Constants.CARD_BORDER_WIDTH + 10 + Constants.LINE_WIDTH,
                    cardRect.GetWidth() - 2 * Constants.CARD_BORDER_WIDTH - 2 * (10 + Constants.LINE_WIDTH),
                    spaceForLevel
                    );
                CreateDebugRectangle(pdfCanvas, levelBottomRect);
                var levelBottomCanvas = new Canvas(pdfCanvas, levelBottomRect);
                levelBottomCanvas.Add(Constants.LevelBackBottomParagraph(spell));

                if (++i >= wCount)
                {
                    i = 0;
                    j++;
                }
            }
        }

        private static int CalculateCardCount(Document document, Spell spell, float firstCardHeightAvailable, float nextCardsHeightAvailable, float width, out List<string> spellDetails, out float fontSize)
        {
            var i = 0;
            while (true)
            {
                var availableSpace = firstCardHeightAvailable + i * nextCardsHeightAvailable;
                fontSize = Constants.BASE_DETAILS_SIZE;
                while (fontSize >= Constants.MINIMUM_DETAILS_SIZE)
                {
                    var totalNeededSpace = 0f;
                    foreach (var spellDetailsLine in spell.Description.Split("\n"))
                    {
                        var detailsParagraph = Constants.DetailsParagraph()
                                .SetFontSize(fontSize)
                                .Add(spellDetailsLine + "\n");
                        totalNeededSpace += Calc(document, width, detailsParagraph);
                    }
                    if (totalNeededSpace <= availableSpace)
                    {
                        spellDetails = SplitText(document, width, spell.Description, fontSize, firstCardHeightAvailable, nextCardsHeightAvailable);
                        return i + 1;
                    }
                    fontSize--;
                }
                i++;
            }
        }

        private static List<string> SplitText(Document document, float width, string description, float fontSize, float firstCardHeightAvailable, float nextCardsHeightAvailable)
        {
            var result = new List<string>();

            var i = 0;
            while (description.Length > 0)
            {
                var tempDescription = description;

                //paragraph = Constants.DetailsParagraph();
                var totalNeededSpace = 0f;
                foreach (var spellDetailsLine in tempDescription.Split("\n"))
                {
                    var detailsParagraph = Constants.DetailsParagraph()
                            .SetFontSize(fontSize)
                            .Add(spellDetailsLine + "\n");
                    totalNeededSpace += Calc(document, width, detailsParagraph);
                }
                var availableSpace = i++ == 0 ? firstCardHeightAvailable : nextCardsHeightAvailable;
                while (totalNeededSpace > availableSpace)
                {
                    if (tempDescription.Contains(" "))
                        tempDescription = tempDescription.Remove(tempDescription.LastIndexOf(" "));
                    else
                        tempDescription = tempDescription.Remove(tempDescription.LastIndexOf("\n"));
                    //paragraph = Constants.DetailsParagraph();
                    totalNeededSpace = 0;
                    foreach (var spellDetailsLine in tempDescription.Split("\n"))
                    {
                        var detailsParagraph = Constants.DetailsParagraph()
                                .SetFontSize(fontSize)
                                .Add(spellDetailsLine + "\n");
                        totalNeededSpace += Calc(document, width, detailsParagraph);
                    }
                }

                result.Add(tempDescription);
                description = description.Replace(tempDescription, "");
            }

            return result;
        }

        private static void GenerateDetails(PdfCanvas pdfCanvas, Rectangle cardRect, Spell spell, float verticalLineHeight, float spaceForClassesLevelSchool, string spellDetails, int splittedCardNumber, float spaceForMaterialComponents, float detailsFontSize)
        {
            //spell details
            var rect = new Rectangle(
                cardRect.GetX() + Constants.CARD_BORDER_WIDTH,
                cardRect.GetY() + (splittedCardNumber == 1 ? spaceForClassesLevelSchool : Constants.CARD_BORDER_WIDTH),
                Constants.FULL_INNER_SPACE_WIDTH,
                Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + Constants.LINE_WIDTH + (splittedCardNumber == 1 ? verticalLineHeight + spaceForClassesLevelSchool + spaceForMaterialComponents : Constants.CARD_BORDER_WIDTH)));
            CreateDebugRectangle(pdfCanvas, rect, ColorConstants.RED);

            //details background
            var percentFill = .75f;
            ImageData imageData = ImageDataFactory.Create(Environment.CurrentDirectory + $"\\Resources\\Spell Schools\\{spell.School}.png");
            Image image = new Image(imageData)
                .SetOpacity(0.4f)
                .ScaleToFit(rect.GetWidth() * percentFill, rect.GetHeight() * percentFill);
            var rect2 = new Rectangle(
                rect.GetX() + (rect.GetWidth() - image.GetImageScaledWidth()) / 2,
                rect.GetY() + (rect.GetHeight() - image.GetImageScaledHeight()) / 2,
                image.GetImageScaledWidth(),
                image.GetImageScaledHeight());
            CreateDebugRectangle(pdfCanvas, rect2, ColorConstants.BLUE);
            var imgCanvas = new Canvas(pdfCanvas, rect2);
            imgCanvas.Add(image);

            //details text
            var canvas = new Canvas(pdfCanvas, rect);

            var splittedDetails = spellDetails.Split("\n");
            var indent = 0f;
            var ignore = false;
            for (var i = 0; i < splittedDetails.Length; i++)
            {
                if (splittedCardNumber == 1)
                {
                    indent = 5;
                }
                else if (splittedDetails[i].Trim().Length <= 0)
                {
                    indent = 5;
                    ignore = true;
                    continue;
                }
                else if (ignore)
                {
                    ignore = false;
                }
                else
                {
                    indent = 0;
                }
                var detailsParagraph = Constants.DetailsParagraph()
                        .SetFontSize(detailsFontSize)
                        .SetFirstLineIndent(indent)
                        .Add(splittedDetails[i] + "\n");
                canvas.Add(detailsParagraph);
            }
            //foreach (var spellDetailsLine in spellDetails.Split("\n"))
            //{
            //    var detailsParagraph = Constants.DetailsParagraph()
            //            .SetFontSize(detailsFontSize)
            //            .Add(spellDetailsLine + "\n");
            //    canvas.Add(detailsParagraph);
            //}
        }

        private static void GenerateClassesAndLevelSchool(PdfCanvas pdfCanvas, Rectangle cardRect, Spell spell, float spaceForClasses, float spaceForLevelSchool)
        {
            //spell classes
            var classesRect = new Rectangle(
                cardRect.GetX() + Constants.CARD_BORDER_WIDTH,
                cardRect.GetY(),
                Constants.HALF_INNER_SPACE_WIDTH,
                spaceForClasses
                );
            CreateDebugRectangle(pdfCanvas, classesRect);
            var classesCanvas = new Canvas(pdfCanvas, classesRect);
            classesCanvas.Add(Constants.ClassesParagraph(spell));

            //spell level and school
            var schoolLevelRect = new Rectangle(
                cardRect.GetX() + Constants.CARD_BORDER_WIDTH + Constants.HALF_INNER_SPACE_WIDTH + Constants.LINE_WIDTH,
                cardRect.GetY(),
                Constants.HALF_INNER_SPACE_WIDTH,
                spaceForLevelSchool);
            CreateDebugRectangle(pdfCanvas, schoolLevelRect);
            var schoolLevelCanvas = new Canvas(pdfCanvas, schoolLevelRect);
            schoolLevelCanvas.Add(Constants.LevelSchoolParagraph(spell));
        }

        private static void GenerateCastingTimeAndRange(PdfCanvas pdfCanvas, Rectangle cardRect, Spell spell, float spaceForCastTime, float spaceForRange)
        {
            pdfCanvas
                .SaveState()
                .SetFillColor(Helper.GetBackgroundColor(spell))
                .SetStrokeColor(Helper.GetBackgroundColor(spell))
                .Rectangle(
                    cardRect.GetX() + Constants.CARD_BORDER_WIDTH - Constants.EPSILON,
                    cardRect.GetY() + Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + 2 * Constants.LINE_WIDTH + (spaceForCastTime > spaceForRange ? spaceForCastTime : spaceForRange)),
                    Constants.FULL_INNER_SPACE_WIDTH + 2 * Constants.EPSILON,
                    Constants.LINE_WIDTH
                )
                .Fill()
                .RestoreState();

            //casting time
            var castingTimeRect = new Rectangle(
                cardRect.GetX() + Constants.CARD_BORDER_WIDTH,
                cardRect.GetY() + Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + Constants.LINE_WIDTH + spaceForCastTime),
                Constants.HALF_INNER_SPACE_WIDTH,
                spaceForCastTime);
            CreateDebugRectangle(pdfCanvas, castingTimeRect);
            var castingTimeCanvas = new Canvas(pdfCanvas, castingTimeRect);
            castingTimeCanvas.Add(Constants.CastingTimeHeaderParagraph());
            castingTimeCanvas.Add(Constants.CastingTimeParagraph(spell));

            //range
            var rangeRect = new Rectangle(
                cardRect.GetX() + Constants.CARD_BORDER_WIDTH + Constants.HALF_INNER_SPACE_WIDTH + Constants.LINE_WIDTH,
                cardRect.GetY() + Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + Constants.LINE_WIDTH + spaceForRange),
                Constants.HALF_INNER_SPACE_WIDTH,
                spaceForRange);

            CreateDebugRectangle(pdfCanvas, rangeRect);
            var rangeCanvas = new Canvas(pdfCanvas, rangeRect);
            rangeCanvas.Add(Constants.RangeHeaderParagraph());
            rangeCanvas.Add(Constants.RangeParagraph(spell));
        }

        private static void CreateDebugRectangle(PdfCanvas pdfCanvas, Rectangle rect, Color color = null)
        {
#if TEST_DEBUG
            if (color == null)
            {
                color = ColorConstants.PINK;
            }
            pdfCanvas
                .SaveState()
                .SetFillColor(color)
                .SetStrokeColor(color)
                .Rectangle(rect)
                .Fill()
                .RestoreState();
#endif
        }

        private static void GenerateComponentsAndDuration(PdfCanvas pdfCanvas, Rectangle cardRect, Spell spell, float spaceForComponents, float spaceForDuration, float spaceForCastTime, float spaceForRange, float spaceForMaterialComponents, float verticalLineHeight)
        {
            //components
            var componentsRect = new Rectangle(
                cardRect.GetX() + Constants.CARD_BORDER_WIDTH,
                cardRect.GetY() + Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + 2 * Constants.LINE_WIDTH + spaceForComponents + (spaceForCastTime > spaceForRange ? spaceForCastTime : spaceForRange)),
                Constants.HALF_INNER_SPACE_WIDTH,
                spaceForComponents);
            CreateDebugRectangle(pdfCanvas, componentsRect);
            var componentsCanvas = new Canvas(pdfCanvas, componentsRect);
            componentsCanvas.Add(Constants.ComponentsHeaderParagraph());
            componentsCanvas.Add(Constants.ComponentsParagraph(spell));

            //line for material components
            pdfCanvas
                .SaveState()
                .SetFillColor(Helper.GetBackgroundColor(spell))
                .SetStrokeColor(Helper.GetBackgroundColor(spell))
                .Rectangle(
                    cardRect.GetX() + Constants.CARD_BORDER_WIDTH - Constants.EPSILON,
                    cardRect.GetY() + Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + Constants.LINE_WIDTH + verticalLineHeight + spaceForMaterialComponents),
                    Constants.FULL_INNER_SPACE_WIDTH + 2 * Constants.EPSILON,
                    spaceForMaterialComponents)
                .Fill()
                .RestoreState();

            //material components
            if (spell.Components?.MaterialComponents?.Count > 0)
            {
                var materialComponentsRect = new Rectangle(
                    cardRect.GetX() + Constants.CARD_BORDER_WIDTH,
                    cardRect.GetY() + Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + Constants.LINE_WIDTH + verticalLineHeight + spaceForMaterialComponents),
                    Constants.FULL_INNER_SPACE_WIDTH,
                    spaceForMaterialComponents);
                var materialComponentsCanvas = new Canvas(pdfCanvas, materialComponentsRect);
                materialComponentsCanvas.Add(Constants.MaterialComponentsParagraph(spell));
            }

            //duration
            var durationRect = new Rectangle(
                cardRect.GetX() + Constants.CARD_BORDER_WIDTH + Constants.HALF_INNER_SPACE_WIDTH + Constants.LINE_WIDTH,
                cardRect.GetY() + Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + Constants.LINE_WIDTH + (spaceForCastTime > spaceForRange ? spaceForCastTime : spaceForRange) + Constants.LINE_WIDTH + spaceForDuration),
                Constants.HALF_INNER_SPACE_WIDTH,
                spaceForDuration);
            CreateDebugRectangle(pdfCanvas, durationRect);
            var durationCanvas = new Canvas(pdfCanvas, durationRect);
            durationCanvas.Add(Constants.DurationHeaderParagraph());
            durationCanvas.Add(Constants.DurationParagraph(spell));
        }

        private static void GenerateTitle(Document document, PdfCanvas pdfCanvas, Rectangle cardRect, Spell spell, int splittedCardNumber, int cardCount)
        {
            //line below title
            pdfCanvas
                .SaveState()
                .SetFillColor(Helper.GetBackgroundColor(spell))
                .SetStrokeColor(Helper.GetBackgroundColor(spell))
                .Rectangle(
                    cardRect.GetX() + Constants.CARD_BORDER_WIDTH - Constants.EPSILON,
                    cardRect.GetY() + Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT + Constants.LINE_WIDTH),
                    Constants.FULL_INNER_SPACE_WIDTH + 2 * Constants.EPSILON,
                    Constants.LINE_WIDTH)
                .Fill()
                .RestoreState();

            var rect = new Rectangle(
                cardRect.GetX() + Constants.CARD_BORDER_WIDTH,
                cardRect.GetY() + Constants.CARD_HEIGHT - (Constants.CARD_BORDER_WIDTH + Constants.TITLE_HEIGHT),
                Constants.FULL_INNER_SPACE_WIDTH,
                Constants.TITLE_HEIGHT);
            CreateDebugRectangle(pdfCanvas, rect);
            var canvas = new Canvas(pdfCanvas, rect);
            var p = Constants.TitleParagraph();
            p.Add(spell.Name + (cardCount > 1 ? $" [{splittedCardNumber}/{cardCount}]" : ""));

            var fontSize = Constants.TITLE_MAX_FONT_SIZE;
            while (Calc(document, Constants.FULL_INNER_SPACE_WIDTH, p) > Constants.TITLE_HEIGHT && fontSize >= 2)
            {
                p.SetFontSize(--fontSize);
            }

            canvas.Add(p);
        }

        private static float Calc(Document document, float width, params Paragraph[] paragraphs)
        {
            float result = 0;
            foreach (var paragraph in paragraphs)
            {
                paragraph.SetWidth(width);
                IRenderer paragraphRenderer = paragraph.CreateRendererSubTree();
                LayoutResult layoutResult = paragraphRenderer.SetParent(document.GetRenderer()).Layout(new LayoutContext(new LayoutArea(1, new Rectangle(width, 1000000))));
                result += layoutResult.GetOccupiedArea()?.GetBBox()?.GetHeight() ?? 1000000;
            }

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using System.IO;
using AvalonTextBox;
using System.Drawing;

namespace JayDev.Notemaker.Common
{
    public class XslsExporter
    {
        private ExcelWorksheet CreateSheet(ExcelPackage p, string sheetName)
        {
            p.Workbook.Worksheets.Add(sheetName);
            ExcelWorksheet ws = p.Workbook.Worksheets[1];
            ws.Name = sheetName; //Setting Sheet's name
            ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
            ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

            return ws;
        }

        public void CreateSpreadsheet(Stream streamToSaveTo, List<Track> tracks, List<Note> notes)
        {
            ExcelPackage package = new ExcelPackage(streamToSaveTo);
            ExcelWorksheet ws = CreateSheet(package, "Notemaker Export");
            ws.Cells[1, 1].Style.Font.Size = 13;
            ws.Cells[1, 1].Value = "Tracks:";
            ws.Cells[2, 1].Value = "Track Number";
            ws.Cells[2, 2].Value = "Track Path";
            List<Track> orderedTracks = tracks.OrderBy(x => x.OrderNumber).ToList();
            for (int i = 0; i < tracks.Count; i++)
            {
                ws.Cells[i + 3, 1].Value = orderedTracks[i].OrderNumber;
                ws.Cells[i + 3, 2].Value = orderedTracks[i].FilePath;
            }

            //set the note column to be 10x the size, and to wrap.
            ws.Column(3).Width *= 15;
            ws.Column(3).Style.WrapText = true;

            int currentRow = orderedTracks.Count + 4;
            ws.Cells[currentRow, 1].Style.Font.Size = 13;
            ws.Cells[currentRow, 1].Value = "Notes:";
            currentRow++;
            ws.Cells[currentRow, 1].Value = "Track Number";
            ws.Cells[currentRow, 2].Value = "Start Time";
            ws.Cells[currentRow, 3].Value = "Note";
            ws.Cells[currentRow, 4].Value = "Rating";
            currentRow++;
            
            for (int i = 0; i < notes.Count; i++)
            {
                if (null != notes[i].Start)
                {
                    int? trackIndex = notes[i].Start.IndexOfTrackInCourse;
                    ws.Cells[currentRow, 1].Value = null == trackIndex ? "" : trackIndex.Value.ToString();
                    ws.Cells[currentRow, 2].Value = notes[i].Start.Time.ToString();
                }


                ws.Cells[currentRow, 3].IsRichText = true;
                List<Section> sections = new AvalonTextBox.Converters.MarkedupTextToNoteSectionsConverter().Convert(notes[i].Body, typeof(List<Section>), null, null) as List<Section>;
                for (int q = 0; q < sections.Count; q++)
                {
                    Section currentSection = sections[q];

                    /***
                     * BEGIN HACK
                     ***/
                    //OK, we have a bug in the excel library. if a richtext chunk has spaces at the start or end, it will drop them... and
                    //if the chunk is ONLY space, it will corrupt the file. hack: replace all leading/trailing spaces with non-breaking
                    //spaces
                    char[] textCharArray = currentSection.Text.ToCharArray();
                    //replace all spaces at the front...
                    for (int z = 0; z < textCharArray.Length; z++)
                    {
                        if (textCharArray[z] == ' ')
                        {
                            textCharArray[z] = '\u00A0'; //'\u00A0' is the non-breaking-space character
                        }
                        else
                        {
                            break;
                        }
                    }
                    //replace all spaces at the end...
                    for (int z = textCharArray.Length - 1; z >= 0; z--)
                    {
                        if (textCharArray[z] == ' ')
                        {
                            textCharArray[z] = '\u00A0'; //'\u00A0' is the non-breaking-space character
                        }
                        else
                        {
                            break;
                        }
                    }
                    string hackedText = new String(textCharArray);
                    /***
                     * END HACK
                     ***/

                    var excelChunk = ws.Cells[currentRow, 3].RichText.Add(hackedText);
                    excelChunk.Italic = currentSection.Style == NoteStyle.Italic ? true : false;
                    excelChunk.Bold = currentSection.Weight == NoteWeight.Bold ? true : false;
                    System.Drawing.Color drawingcolor = System.Drawing.Color.FromArgb(currentSection.Colour.A, currentSection.Colour.R, currentSection.Colour.G, currentSection.Colour.B);
                    excelChunk.Color = drawingcolor;
                }

                ws.Cells[currentRow, 4].Value = notes[i].Rating;

                currentRow++;
            }

            package.Save();
        }
    }
}

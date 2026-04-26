// <copyright file="PdfExporter.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Utils;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using KarmaBanking.App.Models;

/// <summary>
/// Provides utility methods to generate and export data to PDF documents.
/// </summary>
public class PdfExporter
{
    private const float PageWidth = 595f;
    private const float PageHeight = 842f;
    private const float LeftMargin = 48f;
    private const float TopMargin = 54f;
    private const float RowHeight = 18f;

    /// <summary>
    /// Exports a collection of amortization rows to a byte array representing a PDF document.
    /// </summary>
    /// <param name="rows">The amortization rows to export.</param>
    /// <returns>A byte array containing the generated PDF file.</returns>
    public byte[] ExportAmortization(IEnumerable<AmortizationRow> rows)
    {
        var amortizationRows = rows?.ToList() ?? [];
        var pageContents = BuildAmortizationPages(amortizationRows);

        return BuildPdfDocument(pageContents);
    }

    /// <summary>
    /// Exports a collection of transactions to a byte array representing a PDF document.
    /// </summary>
    /// <param name="rows">The transaction rows to export.</param>
    /// <returns>A byte array containing the generated PDF file.</returns>
    /// <exception cref="NotImplementedException">Thrown because transaction export is not yet implemented.</exception>
    public byte[] ExportTransactions(IEnumerable<object> rows)
    {
        throw new NotImplementedException("Transaction PDF export is not implemented yet.");
    }

    private static List<string> BuildAmortizationPages(IReadOnlyList<AmortizationRow> rows)
    {
        List<string> pages = [];
        var currentPage = new StringBuilder();

        WriteTitle(currentPage, "Amortisation schedule", PageHeight - TopMargin);
        WriteHeader(currentPage, PageHeight - TopMargin - 34f);

        var yPosition = PageHeight - TopMargin - 58f;

        foreach (var row in rows)
        {
            if (yPosition < 68f)
            {
                pages.Add(currentPage.ToString());
                currentPage = new StringBuilder();
                WriteTitle(currentPage, "Amortisation schedule", PageHeight - TopMargin);
                WriteHeader(currentPage, PageHeight - TopMargin - 34f);
                yPosition = PageHeight - TopMargin - 58f;
            }

            WriteRow(currentPage, row, yPosition);
            yPosition -= RowHeight;
        }

        pages.Add(currentPage.ToString());
        return pages;
    }

    private static void WriteTitle(StringBuilder builder, string title, float verticalPosition)
    {
        AppendText(builder, 18, LeftMargin, verticalPosition, title);
    }

    private static void WriteHeader(StringBuilder builder, float verticalPosition)
    {
        AppendText(builder, 11, LeftMargin, verticalPosition, "#");
        AppendText(builder, 11, LeftMargin + 36f, verticalPosition, "Due date");
        AppendText(builder, 11, LeftMargin + 130f, verticalPosition, "Principal");
        AppendText(builder, 11, LeftMargin + 255f, verticalPosition, "Interest");
        AppendText(builder, 11, LeftMargin + 360f, verticalPosition, "Balance");
    }

    private static void WriteRow(StringBuilder builder, AmortizationRow row, float verticalPosition)
    {
        AppendText(builder, 10, LeftMargin, verticalPosition, row.InstallmentNumber.ToString(CultureInfo.InvariantCulture));
        AppendText(builder, 10, LeftMargin + 36f, verticalPosition, row.DueDate.ToString("MMM ''yy", CultureInfo.InvariantCulture));
        AppendText(builder, 10, LeftMargin + 130f, verticalPosition, FormatCurrency(row.PrincipalPortion));
        AppendText(builder, 10, LeftMargin + 255f, verticalPosition, FormatCurrency(row.InterestPortion));
        AppendText(builder, 10, LeftMargin + 360f, verticalPosition, FormatCurrency(row.RemainingBalance));
    }

    private static string FormatCurrency(decimal value)
    {
        return value.ToString("C2", CultureInfo.CurrentCulture);
    }

    private static void AppendText(StringBuilder builder, int fontSize, float horizontalPosition, float verticalPosition, string text)
    {
        builder.AppendLine("BT");
        builder.AppendLine($"/F1 {fontSize} Tf");
        builder.AppendLine(
            $"{horizontalPosition.ToString("0.##", CultureInfo.InvariantCulture)} {verticalPosition.ToString("0.##", CultureInfo.InvariantCulture)} Td");
        builder.AppendLine($"({EscapePdfText(text)}) Tj");
        builder.AppendLine("ET");
    }

    private static string EscapePdfText(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
    }

    private static byte[] BuildPdfDocument(IReadOnlyList<string> pageContents)
    {
        List<byte[]> objects = [];
        var pageCount = pageContents.Count;
        var fontObjectId = 3 + (pageCount * 2);
        var pageReferences = new StringBuilder();

        for (var index = 0; index < pageCount; index++)
        {
            var pageObjectId = 3 + (index * 2);
            pageReferences.Append($"{pageObjectId} 0 R ");
        }

        objects.Add(ToPdfBytes("<< /Type /Catalog /Pages 2 0 R >>"));
        objects.Add(ToPdfBytes($"<< /Type /Pages /Kids [{pageReferences.ToString().TrimEnd()}] /Count {pageCount} >>"));

        for (var index = 0; index < pageCount; index++)
        {
            var contentObjectId = 4 + (index * 2);
            var pageObject =
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth.ToString("0", CultureInfo.InvariantCulture)} {PageHeight.ToString("0", CultureInfo.InvariantCulture)}] " +
                $"/Resources << /Font << /F1 {fontObjectId} 0 R >> >> /Contents {contentObjectId} 0 R >>";

            var contentBytes = Encoding.ASCII.GetBytes(pageContents[index]);
            var streamHeader = $"<< /Length {contentBytes.Length} >>\nstream\n";
            var streamFooter = "endstream";

            objects.Add(ToPdfBytes(pageObject));
            objects.Add(
                CombineBytes(
                    Encoding.ASCII.GetBytes(streamHeader),
                    contentBytes,
                    Encoding.ASCII.GetBytes("\n" + streamFooter)));
        }

        objects.Add(ToPdfBytes("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"));

        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);

        writer.Write("%PDF-1.4\n");
        writer.Flush();

        List<long> offsets = [0];

        for (var index = 0; index < objects.Count; index++)
        {
            offsets.Add(stream.Position);
            writer.Write($"{index + 1} 0 obj\n");
            writer.Flush();
            stream.Write(objects[index], 0, objects[index].Length);
            writer.Write("\nendobj\n");
            writer.Flush();
        }

        var xrefPosition = stream.Position;
        writer.Write($"xref\n0 {objects.Count + 1}\n");
        writer.Write("0000000000 65535 f \n");

        for (var index = 1; index < offsets.Count; index++)
        {
            writer.Write($"{offsets[index]:D10} 00000 n \n");
        }

        writer.Write("trailer\n");
        writer.Write($"<< /Size {objects.Count + 1} /Root 1 0 R >>\n");
        writer.Write("startxref\n");
        writer.Write($"{xrefPosition}\n");
        writer.Write("%%EOF");
        writer.Flush();

        return stream.ToArray();
    }

    private static byte[] ToPdfBytes(string value)
    {
        return Encoding.ASCII.GetBytes(value);
    }

    private static byte[] CombineBytes(params byte[][] parts)
    {
        var totalLength = parts.Sum(part => part.Length);
        var result = new byte[totalLength];
        var offset = 0;

        foreach (var part in parts)
        {
            Buffer.BlockCopy(part, 0, result, offset, part.Length);
            offset += part.Length;
        }

        return result;
    }
}

using System.IO.Compression;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using ExcelDataReader;

namespace InsightFlow.Core.Validation;

public sealed class DatasetFileValidator
{
    private const int MaxColumns = 500;
    private const int MaxRows = 1_000_000;

    public async Task ValidateAsync(
        Stream stream,
        string fileName,
        long maxFileSizeBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidDataException("A filename is required.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (extension is not ".csv" and not ".xlsx")
            throw new InvalidDataException(
                "Only CSV and XLSX files are supported.");

        if (maxFileSizeBytes <= 0)
            throw new InvalidOperationException(
                "The configured maximum file size is invalid.");

        if (!stream.CanRead)
            throw new InvalidDataException("The file cannot be read.");

        if (!stream.CanSeek)
            throw new InvalidDataException(
                "The upload stream must support seeking.");

        if (stream.Length == 0)
            throw new InvalidDataException("The uploaded file is empty.");

        if (stream.Length > maxFileSizeBytes)
            throw new InvalidDataException(
                $"The file exceeds the {maxFileSizeBytes}-byte size limit.");

        stream.Position = 0;

        if (extension == ".csv")
            await ValidateCsvAsync(stream, cancellationToken);
        else
            ValidateXlsx(stream);
    }


    private static async Task ValidateCsvAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        // Copy to memory so CSV parsing can handle quoted fields
        // containing commas and line breaks.
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        using var textReader = new StreamReader(
            memory,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: false);

        using var parser = new TextFieldParser(textReader)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = false
        };

        parser.SetDelimiters(",");

        var rowCount = 0;
        var expectedColumns = -1;

        while (!parser.EndOfData)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string[]? fields;

            try
            {
                fields = parser.ReadFields();
            }
            catch (MalformedLineException ex)
            {
                throw new InvalidDataException(
                    "CSV contains invalid or malformed data.", ex);
            }

            if (fields is null ||
                fields.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            rowCount++;

            if (rowCount > MaxRows)
            {
                throw new InvalidDataException(
                    $"CSV exceeds the maximum of {MaxRows} non-empty rows.");
            }

            if (fields.Length > MaxColumns)
            {
                throw new InvalidDataException(
                    $"CSV exceeds the maximum of {MaxColumns} columns.");
            }

            if (expectedColumns == -1)
            {
                expectedColumns = fields.Length;
            }
            else if (fields.Length != expectedColumns)
            {
                throw new InvalidDataException(
                    "CSV rows have inconsistent column counts.");
            }
        }

        if (rowCount < 2)
        {
            throw new InvalidDataException(
                "CSV must contain a header and at least one data row.");
        }

        stream.Position = 0;
    }



    private static void ValidateXlsx(Stream stream)
    {
        stream.Position = 0;

        try
        {
            using var reader = ExcelReaderFactory.CreateReader(stream);

            var rowCount = 0;
            var expectedColumns = -1;

            do
            {
                while (reader.Read())
                {
                    var fields = Enumerable.Range(0, reader.FieldCount)
                        .Select(reader.GetValue)
                        .ToArray();

                    // Ignore completely empty rows.
                    if (fields.All(value =>
                        value is null ||
                        string.IsNullOrWhiteSpace(value.ToString())))
                    {
                        continue;
                    }

                    rowCount++;

                    if (rowCount > MaxRows)
                    {
                        throw new InvalidDataException(
                            $"XLSX exceeds the maximum of {MaxRows} non-empty rows.");
                    }

                    if (fields.Length > MaxColumns)
                    {
                        throw new InvalidDataException(
                            $"XLSX exceeds the maximum of {MaxColumns} columns.");
                    }

                    if (expectedColumns == -1)
                    {
                        expectedColumns = fields.Length;
                    }
                    else if (fields.Length != expectedColumns)
                    {
                        throw new InvalidDataException(
                            "XLSX rows have inconsistent column counts.");
                    }
                }

                // Validate the first worksheet only.
                break;
            }
            while (reader.NextResult());

            if (rowCount < 2)
            {
                throw new InvalidDataException(
                    "XLSX must contain a header and at least one data row.");
            }
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception ex) when (
            ex is IOException or
            ArgumentException or
            System.Xml.XmlException or
            FormatException or
            IndexOutOfRangeException)
        {
            throw new InvalidDataException(
                "The XLSX file is corrupt or invalid.", ex);
        }
        finally
        {
            stream.Position = 0;
        }
    }

}

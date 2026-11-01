
using ExcelDataReader;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Text;

namespace InsightFlow.Core.Services;

public sealed class DatasetReader
{
    private const int MaxColumns = 500;
    private const int MaxRows = 1_000_000;

    public async Task<DatasetReadResult> ReadAsync(
        Stream stream,
        string fileFormat,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
            throw new InvalidDataException("The file cannot be read.");

        if (!stream.CanSeek)
            throw new InvalidDataException("The stream must support seeking.");

        stream.Position = 0;

        var format = fileFormat.Trim().ToUpperInvariant();

        List<string[]> rows = format switch
        {
            "CSV" => await ReadCsvAsync(stream, cancellationToken),
            "XLSX" => ReadXlsx(stream, cancellationToken),
            _ => throw new InvalidDataException(
                "Only CSV and XLSX files are supported.")
        };

        if (rows.Count < 2)
            throw new InvalidDataException(
                "The dataset must contain a header and at least one data row.");

        var headers = rows[0];

        if (headers.Length > MaxColumns)
            throw new InvalidDataException(
                $"The dataset exceeds {MaxColumns} columns.");

        if (headers.Any(string.IsNullOrWhiteSpace))
            throw new InvalidDataException(
                "Column headers cannot be empty.");

        if (headers.Distinct(StringComparer.OrdinalIgnoreCase).Count()
            != headers.Length)
            throw new InvalidDataException(
                "Column headers must be unique.");

        var dataRows = rows.Skip(1).ToList();

        if (dataRows.Count > MaxRows)
            throw new InvalidDataException(
                $"The dataset exceeds {MaxRows} data rows.");

        if (dataRows.Any(row => row.Length != headers.Length))
            throw new InvalidDataException(
                "Rows have inconsistent column counts.");


        var columns = new List<DatasetColumnInfo>();

        var totalRows = (long)dataRows.Count;
        const int maxDistinctValuesToTrack = 10_000;

        for (var index = 0; index < headers.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            long missingCount = 0;
            var distinctValues = new HashSet<string>(
                StringComparer.Ordinal);

            var distinctCountIsExact = true;
            var allInteger = true;
            var allDecimal = true;
            var allDateTime = true;
            var allBoolean = true;
            var nonEmptyCount = 0;

            foreach (var row in dataRows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var value = row[index];

                if (string.IsNullOrWhiteSpace(value))
                {
                    missingCount++;
                    continue;
                }

                nonEmptyCount++;

                if (!long.TryParse(
                        value, NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out _))
                    allInteger = false;

                if (!decimal.TryParse(
                        value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out _))
                    allDecimal = false;

                if (!DateTime.TryParse(
                        value, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out _))
                    allDateTime = false;

                if (!bool.TryParse(value, out _))
                    allBoolean = false;

                if (distinctCountIsExact)
                {
                    distinctValues.Add(value);

                    if (distinctValues.Count > maxDistinctValuesToTrack)
                    {
                        distinctValues.Clear();
                        distinctCountIsExact = false;
                    }
                }
            }


            columns.Add(new DatasetColumnInfo
            {
                ColumnName = headers[index].Trim(),
                DataType = nonEmptyCount == 0
                        ? "String"
                        : allInteger ? "Integer"
                        : allDecimal ? "Decimal"
                        : allDateTime ? "DateTime"
                        : allBoolean ? "Boolean"
                        : "String",
                SemanticRole = DetectSemanticRole(headers[index]),
                OrdinalPosition = index,
                IsNullable = missingCount > 0,
                MissingCount = missingCount,
                MissingPercentage = totalRows == 0
                    ? 0
                    : (double)missingCount / totalRows * 100,
                DistinctCount = distinctCountIsExact
                    ? distinctValues.Count
                    : null
            });
        }


        return new DatasetReadResult
        {
            RowCount = dataRows.Count,
            Columns = columns
        };
    }


    public async Task<DatasetPreviewResult> ReadPreviewAsync(
        Stream stream,
        string fileFormat,
        int maxRows = 10,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
            throw new InvalidDataException("The file cannot be read.");

        if (!stream.CanSeek)
            throw new InvalidDataException("The stream must support seeking.");

        if (maxRows < 1 || maxRows > 100)
            throw new ArgumentOutOfRangeException(
                nameof(maxRows), "Preview rows must be between 1 and 100.");

        stream.Position = 0;

        var format = fileFormat.Trim().ToUpperInvariant();

        List<string[]> rows = format switch
        {
            "CSV" => await ReadCsvAsync(stream, cancellationToken),
            "XLSX" => ReadXlsx(stream, cancellationToken),
            _ => throw new InvalidDataException(
                "Only CSV and XLSX files are supported.")
        };

        if (rows.Count < 2)
            throw new InvalidDataException(
                "The dataset must contain a header and at least one data row.");

        var headers = rows[0];

        if (headers.Length > MaxColumns ||
            headers.Any(string.IsNullOrWhiteSpace) ||
            headers.Distinct(StringComparer.OrdinalIgnoreCase).Count()
                != headers.Length)
        {
            throw new InvalidDataException(
                "The dataset has invalid or unsupported column headers.");
        }

        var dataRows = rows.Skip(1).ToList();

        if (dataRows.Any(row => row.Length != headers.Length))
            throw new InvalidDataException(
                "Rows have inconsistent column counts.");

        return new DatasetPreviewResult
        {
            Columns = headers.Select(header => header.Trim()).ToList(),
            Rows = dataRows
                .Take(maxRows)
                .Select(row => (IReadOnlyList<string>)row)
                .ToList(),
            TotalRows = dataRows.Count,
            ReturnedRows = Math.Min(dataRows.Count, maxRows)
        };
    }


    public async Task<DatasetPreviewResult> ReadAllRowsAsync(
        Stream stream,
        string fileFormat,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
            throw new InvalidDataException("The file cannot be read.");

        if (!stream.CanSeek)
            throw new InvalidDataException("The stream must support seeking.");

        stream.Position = 0;

        var format = fileFormat.Trim().ToUpperInvariant();

        List<string[]> rows = format switch
        {
            "CSV" => await ReadCsvAsync(stream, cancellationToken),
            "XLSX" => ReadXlsx(stream, cancellationToken),
            _ => throw new InvalidDataException(
                "Only CSV and XLSX files are supported.")
        };

        if (rows.Count < 2)
            throw new InvalidDataException(
                "The dataset must contain a header and at least one data row.");

        var headers = rows[0];

        if (headers.Length > MaxColumns ||
            headers.Any(string.IsNullOrWhiteSpace) ||
            headers.Distinct(StringComparer.OrdinalIgnoreCase).Count()
                != headers.Length)
        {
            throw new InvalidDataException(
                "The dataset has invalid or unsupported column headers.");
        }

        var dataRows = rows.Skip(1).ToList();

        if (dataRows.Count > MaxRows)
            throw new InvalidDataException(
                $"The dataset exceeds {MaxRows} data rows.");

        if (dataRows.Any(row => row.Length != headers.Length))
            throw new InvalidDataException(
                "Rows have inconsistent column counts.");

        return new DatasetPreviewResult
        {
            Columns = headers.Select(header => header.Trim()).ToList(),
            Rows = dataRows
                .Select(row => (IReadOnlyList<string>)row)
                .ToList(),
            TotalRows = dataRows.Count,
            ReturnedRows = dataRows.Count
        };
    }


    private static async Task<List<string[]>> ReadCsvAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        using var textReader = new StreamReader(
            memory,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);

        using var parser = new TextFieldParser(textReader)
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = false
        };

        parser.SetDelimiters(",");

        var rows = new List<string[]>();

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
                    "The CSV contains malformed data.", ex);
            }

            if (fields is null ||
                fields.All(string.IsNullOrWhiteSpace))
                continue;

            rows.Add(fields);

            if (rows.Count > MaxRows + 1)
                throw new InvalidDataException(
                    $"The CSV exceeds {MaxRows} data rows.");
        }

        return rows;
    }

    private static List<string[]> ReadXlsx(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var rows = new List<string[]>();

        // Read the first worksheet.
        while (reader.Read())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fields = Enumerable.Range(0, reader.FieldCount)
                .Select(index => Convert.ToString(
                    reader.GetValue(index),
                    CultureInfo.InvariantCulture) ?? string.Empty)
                .ToArray();

            if (fields.All(string.IsNullOrWhiteSpace))
                continue;

            rows.Add(fields);

            if (rows.Count > MaxRows + 1)
                throw new InvalidDataException(
                    $"The XLSX exceeds {MaxRows} data rows.");
        }

        return rows;
    }



    private static string? DetectSemanticRole(string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return null;

        // Normalize headers: Order_Date, order-date and OrderDate
        // all become orderdate.
        var name = new string(
            columnName
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());

        return name switch
        {
            // Date and time
            "date" or
            "orderdate" or
            "transactiondate" or
            "salesdate" or
            "invoicedate" or
            "purchasedate" or
            "createddate" or
            "timestamp" or
            "datetime" or
            "orderdatetime" => "Date",

            // Revenue and sales
            "revenue" or
            "totalrevenue" or
            "sales" or
            "totalsales" or
            "netsales" or
            "grosssales" or
            "salesamount" or
            "salesvalue" or
            "ordervalue" or
            "orderamount" or
            "totalamount" or
            "netamount" => "Revenue",

            // Profit
            "profit" or
            "totalprofit" or
            "netprofit" or
            "grossprofit" or
            "profitamount" or
            "profitmargin" => "Profit",

            // Cost
            "cost" or
            "totalcost" or
            "unitcost" or
            "productcost" or
            "costprice" or
            "purchasecost" => "Cost",

            // Quantity
            "quantity" or
            "qty" or
            "units" or
            "unitssold" or
            "quantitysold" or
            "salesquantity" or
            "itemquantity" => "Quantity",

            // Unit price
            "unitprice" or
            "priceperunit" or
            "sellingprice" or
            "sellingpriceperunit" or
            "productprice" => "UnitPrice",

            // Product
            "product" or
            "productname" or
            "item" or
            "itemname" or
            "productid" or
            "sku" or
            "skucode" => "Product",

            // Category
            "category" or
            "productcategory" or
            "itemcategory" or
            "subcategory" or
            "productsubcategory" => "Category",

            // Geography
            "region" or
            "territory" or
            "country" or
            "state" or
            "city" or
            "location" or
            "salesregion" or
            "geography" => "Region",

            // Customer
            "customer" or
            "customerid" or
            "customername" or
            "client" or
            "clientid" or
            "clientname" or
            "customersegment" or
            "segment" => "Customer",

            // Transaction
            "orderid" or
            "ordernumber" or
            "invoiceid" or
            "invoicenumber" or
            "transactionid" or
            "transactionnumber" or
            "billnumber" => "Transaction",

            // Generic identifiers
            "id" or
            "identifier" or
            "recordid" or
            "referenceid" => "Identifier",

            // Unknown or ambiguous headers
            _ => null
        };
    }

}

public sealed class DatasetReadResult
{
    public int RowCount { get; init; }

    public IReadOnlyList<DatasetColumnInfo> Columns { get; init; }
        = Array.Empty<DatasetColumnInfo>();
}

public sealed class DatasetColumnInfo
{
    public string ColumnName { get; init; } = string.Empty;

    public string DataType { get; init; } = "String";

    // A heuristic suggestion, not a guaranteed business meaning.
    public string? SemanticRole { get; init; }

    public int OrdinalPosition { get; init; }

    public bool IsNullable { get; init; }

    public long MissingCount { get; init; }

    public double MissingPercentage { get; init; }

    public long? DistinctCount { get; init; }
}


public sealed class DatasetPreviewResult
{
    public IReadOnlyList<string> Columns { get; init; }
        = Array.Empty<string>();

    public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; }
        = Array.Empty<IReadOnlyList<string>>();

    public int TotalRows { get; init; }

    public int ReturnedRows { get; init; }
}


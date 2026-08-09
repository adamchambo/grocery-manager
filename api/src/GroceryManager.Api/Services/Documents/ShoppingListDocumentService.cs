using System.Globalization;
using System.Text;
using GroceryManager.Api.Enums.Shopping;
using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services;
using GroceryManager.Api.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Services.Documents;

public sealed class ShoppingListDocumentService(
    GroceryManagerDbContext db,
    ICurrentUserContext currentUser) : IShoppingListDocumentService
{
    public async Task<byte[]> GeneratePdfAsync(Guid listId, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        var list = await db.ShoppingLists.AsNoTracking().SingleOrDefaultAsync(x => x.Id == listId && x.PantryId == pantryId, cancellationToken)
            ?? throw new KeyNotFoundException("Shopping list not found.");
        var items = await db.ShoppingListItems.AsNoTracking().Where(x => x.ShoppingListId == list.Id)
            .OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);

        return BuildPdf(list.Name, list.GeneratedAtUtc, items);
    }

    private static byte[] BuildPdf(string name, DateTimeOffset generatedAtUtc, IReadOnlyList<GroceryManager.Api.Entities.Shopping.ShoppingListItem> items)
    {
        var sections = items.GroupBy(x => string.IsNullOrWhiteSpace(x.CategoryNameSnapshot) ? "Other items" : x.CategoryNameSnapshot)
            .OrderBy(x => CategoryRank(x.Key))
            .ThenBy(x => x.Key)
            .SelectMany(group => new[] { (Text: group.Key, IsHeading: true) }.Concat(group.Select(item =>
            {
                var quantity = item.Outcome is ShoppingListItemOutcome.Purchased or ShoppingListItemOutcome.PartiallyPurchased ? item.ActualPurchaseQuantity : item.SuggestedPurchaseQuantity;
                return (Text: $"{item.ItemNameSnapshot}|{FormatQuantity(quantity ?? 0, item.TrackingUnitSnapshot)}", IsHeading: false);
            }))).ToList();
        var pages = sections.Chunk(20).ToList(); if (pages.Count == 0) pages.Add([]);
        var fontObjectNumber = 3 + (pages.Count * 2);
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [{string.Join(' ', Enumerable.Range(0, pages.Count).Select(index => $"{3 + (index * 2)} 0 R"))}] /Count {pages.Count} >>"
        };
        for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
            var content = new StringBuilder("q 0.12 0.48 0.27 rg 50 790 512 4 re f Q ");
            content.Append("BT /F1 22 Tf 50 754 Td (").Append(Escape(name)).Append(") Tj ET ");
            content.Append("BT /F1 10 Tf 50 735 Td (Generated ").Append(generatedAtUtc.ToString("d MMM yyyy", CultureInfo.InvariantCulture)).Append("  |  Check off items as you shop) Tj ET ");
            var y = 695;
            foreach (var line in pages[pageIndex])
            {
                if (line.IsHeading)
                {
                    content.Append("q 0.96 g 50 ").Append(y - 7).Append(" 512 24 re f Q ");
                    content.Append("BT /F1 11 Tf 62 ").Append(y).Append(" Td (").Append(Escape(line.Text)).Append(") Tj ET "); y -= 35;
                }
                else
                {
                    var parts = line.Text.Split('|', 2);
                    content.Append("0.5 w 0.72 G 62 ").Append(y - 11).Append(" 12 12 re S ");
                    content.Append("BT /F1 12 Tf 86 ").Append(y - 1).Append(" Td (").Append(Escape(parts[0])).Append(") Tj ET ");
                    content.Append("BT /F1 12 Tf ").Append(RightAlignedX(parts[1])).Append(' ').Append(y - 1).Append(" Td (").Append(Escape(parts[1])).Append(") Tj ET "); y -= 30;
                }
            }
            content.Append("BT /F1 9 Tf 50 38 Td (Grocery Manager) Tj ET ");
            var stream = content.ToString();
            var contentObjectNumber = 4 + (pageIndex * 2);
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 842] /Resources << /Font << /F1 {fontObjectNumber} 0 R /F2 {fontObjectNumber + 1} 0 R >> >> /Contents {contentObjectNumber} 0 R >>");
            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream");
        }
        objects.Add($"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>");
        var pdf = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.Append(i + 1).Append(" 0 obj\n").Append(objects[i]).Append("\nendobj\n");
        }
        var xref = Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.Append("xref\n0 ").Append(objects.Count + 1).Append("\n0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1)) pdf.Append(offset.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        pdf.Append("trailer << /Size ").Append(objects.Count + 1).Append(" /Root 1 0 R >>\nstartxref\n").Append(xref).Append("\n%%EOF");
        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    private static int RightAlignedX(string value) => Math.Max(430, 550 - (value.Length * 6));

    private static string DisplayUnit(string unit) => unit.ToLowerInvariant() switch
    {
        "weight" => "kg",
        "volume" => "litres",
        "item" => "items",
        "package" => "packages",
        _ => unit.ToLowerInvariant()
    };

    private static string FormatQuantity(decimal quantity, string? unit)
    {
        var label = DisplayUnit(unit ?? "");
        var number = quantity.ToString("0.###", CultureInfo.InvariantCulture);
        if (quantity == 1) label = label switch { "items" => "item", "packages" => "package", "litres" => "litre", _ => label };
        return string.IsNullOrWhiteSpace(label) ? number : $"{number} {label}";
    }

    private static int CategoryRank(string category) => category.ToLowerInvariant() switch
    {
        "produce" or "fruit & vegetables" or "fruit and vegetables" => 0,
        "bakery" => 1,
        "meat" or "meat & seafood" or "meat and seafood" => 2,
        "dairy" or "dairy & eggs" or "dairy and eggs" => 3,
        "drinks" or "beverages" => 4,
        "pantry" => 5,
        "frozen" => 6,
        "household" => 7,
        _ => 8
    };
}

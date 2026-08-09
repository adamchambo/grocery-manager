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
            .OrderBy(x => x.Key).SelectMany(group => new[] { (Text: group.Key, IsHeading: true) }.Concat(group.Select(item =>
            {
                var quantity = item.Outcome is ShoppingListItemOutcome.Purchased or ShoppingListItemOutcome.PartiallyPurchased ? item.ActualPurchaseQuantity : item.SuggestedPurchaseQuantity;
                var unit = string.IsNullOrWhiteSpace(item.TrackingUnitSnapshot) ? "" : $" {item.TrackingUnitSnapshot.ToLowerInvariant()}";
                return (Text: $"{item.ItemNameSnapshot}|{(quantity ?? 0).ToString("0.###", CultureInfo.InvariantCulture)}{unit}", IsHeading: false);
            }))).ToList();
        var pages = sections.Chunk(28).ToList(); if (pages.Count == 0) pages.Add([]);
        var fontObjectNumber = 3 + (pages.Count * 2);
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [{string.Join(' ', Enumerable.Range(0, pages.Count).Select(index => $"{3 + (index * 2)} 0 R"))}] /Count {pages.Count} >>"
        };
        for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
            var content = new StringBuilder("q 0.12 0.48 0.27 rg 0 800 612 42 re f Q ");
            content.Append("BT /F2 24 Tf 50 754 Td (").Append(Escape(name)).Append(") Tj ET ");
            content.Append("BT /F1 10 Tf 50 735 Td (Generated ").Append(generatedAtUtc.ToString("d MMM yyyy", CultureInfo.InvariantCulture)).Append(") Tj ET ");
            var y = 695;
            foreach (var line in pages[pageIndex])
            {
                if (line.IsHeading)
                {
                    content.Append("q 0.93 g 50 ").Append(y - 6).Append(" 512 22 re f Q ");
                    content.Append("BT /F2 11 Tf 62 ").Append(y).Append(" Td (").Append(Escape(line.Text)).Append(") Tj ET "); y -= 32;
                }
                else
                {
                    var parts = line.Text.Split('|', 2);
                    content.Append("0.35 w 0.55 G 62 ").Append(y - 10).Append(" 12 12 re S ");
                    content.Append("BT /F1 12 Tf 86 ").Append(y - 1).Append(" Td (").Append(Escape(parts[0])).Append(") Tj ET ");
                    content.Append("BT /F2 12 Tf 500 ").Append(y - 1).Append(" Td (").Append(Escape(parts[1])).Append(") Tj ET "); y -= 27;
                }
            }
            content.Append("BT /F1 9 Tf 50 38 Td (Grocery Manager) Tj ET ");
            var stream = content.ToString();
            var contentObjectNumber = 4 + (pageIndex * 2);
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 842] /Resources << /Font << /F1 {fontObjectNumber} 0 R >> >> /Contents {contentObjectNumber} 0 R >>");
            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream");
        }
        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
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
}

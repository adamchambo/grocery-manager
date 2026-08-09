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

        var lines = new List<string> { list.Name, $"Generated {list.GeneratedAtUtc:d MMM yyyy}" };
        lines.AddRange(items.Select(x =>
        {
            var quantity = x.Outcome is ShoppingListItemOutcome.Purchased or ShoppingListItemOutcome.PartiallyPurchased
                ? x.ActualPurchaseQuantity : x.SuggestedPurchaseQuantity;
            return $"{x.ItemNameSnapshot} - {(quantity ?? 0).ToString("0.###", CultureInfo.InvariantCulture)}";
        }));
        return BuildPdf(lines);
    }

    private static byte[] BuildPdf(IEnumerable<string> lines)
    {
        var allLines = lines.ToList();
        var pages = allLines.Chunk(38).ToList();
        if (pages.Count == 0) pages.Add([]);
        var fontObjectNumber = 3 + (pages.Count * 2);
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [{string.Join(' ', Enumerable.Range(0, pages.Count).Select(index => $"{3 + (index * 2)} 0 R"))}] /Count {pages.Count} >>"
        };
        for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
            var pageLines = pages[pageIndex];
            var content = new StringBuilder("BT /F1 18 Tf 50 790 Td ");
            var first = true;
            foreach (var line in pageLines)
            {
                if (!first) content.Append("0 -20 Td ");
                content.Append('(').Append(Escape(line)).Append(") Tj ");
                first = false;
            }
            content.Append("ET");
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

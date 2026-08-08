using GroceryManager.Api.Common.Exceptions;
using GroceryManager.Api.Dtos.ShoppingPresets;
using GroceryManager.Api.Entities.ShoppingPresets;
using GroceryManager.Api.Enums.ShoppingPresets;
using GroceryManager.Api.Persistence;
using GroceryManager.Api.Services;
using GroceryManager.Api.Services.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Services.ShoppingPresets;

public sealed class ShoppingPresetService(
    GroceryManagerDbContext db,
    ICurrentUserContext currentUser) : IShoppingPresetService
{
    public async Task<IReadOnlyList<ShoppingPresetResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        var presets = await db.ShoppingPresets.AsNoTracking().Where(x => x.PantryId == pantryId && !x.IsArchived)
            .OrderByDescending(x => x.IsEverythingPreset).ThenBy(x => x.Name).ToListAsync(cancellationToken);
        return await MapManyAsync(presets, cancellationToken);
    }

    public async Task<ShoppingPresetResponse> GetAsync(Guid presetId, CancellationToken cancellationToken) =>
        await MapAsync(await FindAsync(presetId, cancellationToken), cancellationToken);

    public async Task<ShoppingPresetResponse> CreateAsync(CreateShoppingPresetRequest request, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        await ValidateMembershipAsync(pantryId, request.CategoryIds, request.ItemRules, cancellationToken);
        if (request.CoverageDays < 0) throw new ArgumentOutOfRangeException(nameof(request));
        var now = DateTimeOffset.UtcNow;
        var preset = new ShoppingPreset
        {
            Id = Guid.NewGuid(), PantryId = pantryId, Name = request.Name.Trim(), CoverageDays = request.CoverageDays,
            CreatedAtUtc = now, UpdatedAtUtc = now
        };
        db.ShoppingPresets.Add(preset);
        ReplaceMembership(preset.Id, request.CategoryIds, request.ItemRules);
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(preset, cancellationToken);
    }

    public async Task<ShoppingPresetResponse> UpdateAsync(Guid presetId, UpdateShoppingPresetRequest request, CancellationToken cancellationToken)
    {
        var preset = await FindAsync(presetId, cancellationToken);
        if (preset.IsEverythingPreset) throw new ConflictException("The Everything preset cannot be edited.");
        await ValidateMembershipAsync(preset.PantryId, request.CategoryIds, request.ItemRules, cancellationToken);
        if (request.CoverageDays < 0) throw new ArgumentOutOfRangeException(nameof(request));
        ServiceSupport.ApplyVersion(db, preset, request.Version);
        preset.Name = request.Name.Trim(); preset.CoverageDays = request.CoverageDays; preset.UpdatedAtUtc = DateTimeOffset.UtcNow;
        db.PresetCategories.RemoveRange(db.PresetCategories.Where(x => x.ShoppingPresetId == preset.Id));
        db.PresetItemRules.RemoveRange(db.PresetItemRules.Where(x => x.ShoppingPresetId == preset.Id));
        ReplaceMembership(preset.Id, request.CategoryIds, request.ItemRules);
        await db.SaveChangesAsync(cancellationToken);
        return await MapAsync(preset, cancellationToken);
    }

    public async Task ArchiveAsync(Guid presetId, CancellationToken cancellationToken)
    {
        var preset = await FindAsync(presetId, cancellationToken);
        if (preset.IsEverythingPreset) throw new ConflictException("The Everything preset cannot be archived.");
        preset.IsArchived = true; preset.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ShoppingPresetPreviewResponse> PreviewAsync(Guid presetId, CancellationToken cancellationToken)
    {
        var preset = await FindAsync(presetId, cancellationToken);
        var categories = await db.PresetCategories.AsNoTracking().Where(x => x.ShoppingPresetId == preset.Id)
            .Select(x => x.CategoryId).ToListAsync(cancellationToken);
        var rules = await db.PresetItemRules.AsNoTracking().Where(x => x.ShoppingPresetId == preset.Id)
            .ToDictionaryAsync(x => x.PantryItemId, x => x.RuleType, cancellationToken);
        var items = await db.PantryItems.AsNoTracking().Where(x => x.PantryId == preset.PantryId && !x.IsArchived)
            .OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return new(preset.Id, items.Select(x =>
        {
            var byCategory = preset.IsEverythingPreset || categories.Contains(x.CategoryId);
            var included = rules.TryGetValue(x.Id, out var rule) ? rule == PresetItemRuleType.Include : byCategory;
            var reason = rules.TryGetValue(x.Id, out rule) ? rule.ToString() : preset.IsEverythingPreset ? "Everything preset" : byCategory ? "Included category" : "Not selected";
            return new ShoppingPresetPreviewItemResponse(x.Id, x.Name, included, reason);
        }).ToList());
    }

    private async Task<ShoppingPreset> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var pantryId = await ServiceSupport.RequirePantryIdAsync(db, currentUser, cancellationToken);
        return await db.ShoppingPresets.SingleOrDefaultAsync(x => x.Id == id && x.PantryId == pantryId, cancellationToken)
            ?? throw new KeyNotFoundException("Shopping preset not found.");
    }

    private async Task ValidateMembershipAsync(Guid pantryId, IReadOnlyList<Guid> categoryIds,
        IReadOnlyList<ShoppingPresetItemRuleRequest> rules, CancellationToken cancellationToken)
    {
        if (categoryIds.Distinct().Count() != categoryIds.Count || rules.Select(x => x.PantryItemId).Distinct().Count() != rules.Count)
            throw new ArgumentException("Preset membership contains duplicates.");
        if (categoryIds.Count != await db.Categories.CountAsync(x => categoryIds.Contains(x.Id) && x.PantryId == pantryId && !x.IsArchived, cancellationToken))
            throw new ArgumentException("One or more preset categories are invalid.");
        var itemIds = rules.Select(x => x.PantryItemId).ToArray();
        if (itemIds.Length != await db.PantryItems.CountAsync(x => itemIds.Contains(x.Id) && x.PantryId == pantryId && !x.IsArchived, cancellationToken))
            throw new ArgumentException("One or more preset items are invalid.");
    }

    private void ReplaceMembership(Guid presetId, IEnumerable<Guid> categories, IEnumerable<ShoppingPresetItemRuleRequest> rules)
    {
        db.PresetCategories.AddRange(categories.Select(x => new PresetCategory { ShoppingPresetId = presetId, CategoryId = x }));
        db.PresetItemRules.AddRange(rules.Select(x => new PresetItemRule { ShoppingPresetId = presetId, PantryItemId = x.PantryItemId, RuleType = x.RuleType }));
    }

    private async Task<IReadOnlyList<ShoppingPresetResponse>> MapManyAsync(IReadOnlyList<ShoppingPreset> presets, CancellationToken cancellationToken)
    {
        var ids = presets.Select(x => x.Id).ToArray();
        var categories = await db.PresetCategories.AsNoTracking().Where(x => ids.Contains(x.ShoppingPresetId)).ToListAsync(cancellationToken);
        var rules = await db.PresetItemRules.AsNoTracking().Where(x => ids.Contains(x.ShoppingPresetId)).ToListAsync(cancellationToken);
        return presets.Select(x => new ShoppingPresetResponse(x.Id, x.Name, x.CoverageDays, x.IsEverythingPreset, x.IsArchived,
            categories.Where(y => y.ShoppingPresetId == x.Id).Select(y => y.CategoryId).ToList(),
            rules.Where(y => y.ShoppingPresetId == x.Id).Select(y => new ShoppingPresetItemRuleRequest(y.PantryItemId, y.RuleType)).ToList(),
            ServiceSupport.EncodeVersion(x.Version))).ToList();
    }

    private async Task<ShoppingPresetResponse> MapAsync(ShoppingPreset preset, CancellationToken cancellationToken) =>
        (await MapManyAsync([preset], cancellationToken))[0];
}

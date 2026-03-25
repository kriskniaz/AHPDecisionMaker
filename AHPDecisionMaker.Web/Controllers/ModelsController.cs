using AHPDecisionMaker.Web.Data;
using AHPDecisionMaker.Web.Entities;
using AHPDecisionMaker.Web.Services;
using AHPDecisionMaker.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHPDecisionMaker.Web.Controllers;

public class ModelsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAhpCalculationService _ahp;

    public ModelsController(AppDbContext db, IAhpCalculationService ahp)
    {
        _db = db;
        _ahp = ahp;
    }

    // GET /Models/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var model = await LoadModel(id);
        if (model == null) return NotFound();

        return View(BuildViewModel(model));
    }

    // POST /Models/SaveGoal
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGoal(int modelId, string? goal)
    {
        var model = await _db.DecisionModels.FindAsync(modelId);
        if (model == null) return NotFound();

        model.Goal = goal;
        await UpdateProjectTimestamp(model.ProjectId);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = modelId });
    }

    // POST /Models/AddCriterion
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCriterion(int modelId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Criterion name cannot be empty.";
            return RedirectToAction(nameof(Edit), new { id = modelId });
        }

        var model = await _db.DecisionModels
            .Include(m => m.Criteria)
            .FirstOrDefaultAsync(m => m.ModelId == modelId);
        if (model == null) return NotFound();

        var orderIndex = model.Criteria.Any() ? model.Criteria.Max(c => c.OrderIndex) + 1 : 0;
        var criterion = new Criterion { ModelId = modelId, Name = name.Trim(), OrderIndex = orderIndex };
        _db.Criteria.Add(criterion);

        // Remove stale criteria comparisons (will be rebuilt)
        var oldComps = await _db.CriteriaComparisons.Where(c => c.ModelId == modelId).ToListAsync();
        _db.CriteriaComparisons.RemoveRange(oldComps);

        await UpdateProjectTimestamp(model.ProjectId);
        await _db.SaveChangesAsync();

        // Rebuild criteria comparison placeholders
        await RebuildCriteriaComparisons(modelId);

        return RedirectToAction(nameof(Edit), new { id = modelId });
    }

    // POST /Models/DeleteCriterion
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCriterion(int modelId, int criterionId)
    {
        var criterion = await _db.Criteria.FindAsync(criterionId);
        if (criterion == null || criterion.ModelId != modelId) return NotFound();

        // Remove alternative comparisons for this criterion
        var altComps = await _db.AlternativeComparisons
            .Where(a => a.CriterionId == criterionId).ToListAsync();
        _db.AlternativeComparisons.RemoveRange(altComps);

        _db.Criteria.Remove(criterion);

        // Remove all criteria comparisons and rebuild
        var critComps = await _db.CriteriaComparisons.Where(c => c.ModelId == modelId).ToListAsync();
        _db.CriteriaComparisons.RemoveRange(critComps);

        var model = await _db.DecisionModels.FindAsync(modelId);
        if (model != null) await UpdateProjectTimestamp(model.ProjectId);

        await _db.SaveChangesAsync();
        await RebuildCriteriaComparisons(modelId);

        return RedirectToAction(nameof(Edit), new { id = modelId });
    }

    // POST /Models/AddAlternative
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAlternative(int modelId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Alternative name cannot be empty.";
            return RedirectToAction(nameof(Edit), new { id = modelId });
        }

        var model = await _db.DecisionModels
            .Include(m => m.Alternatives)
            .Include(m => m.Criteria)
            .FirstOrDefaultAsync(m => m.ModelId == modelId);
        if (model == null) return NotFound();

        var orderIndex = model.Alternatives.Any() ? model.Alternatives.Max(a => a.OrderIndex) + 1 : 0;
        var alternative = new Alternative { ModelId = modelId, Name = name.Trim(), OrderIndex = orderIndex };
        _db.Alternatives.Add(alternative);

        // Remove stale alternative comparisons
        var oldComps = await _db.AlternativeComparisons.Where(a => a.ModelId == modelId).ToListAsync();
        _db.AlternativeComparisons.RemoveRange(oldComps);

        await UpdateProjectTimestamp(model.ProjectId);
        await _db.SaveChangesAsync();

        // Rebuild alternative comparison placeholders for all criteria
        await RebuildAlternativeComparisons(modelId);

        return RedirectToAction(nameof(Edit), new { id = modelId });
    }

    // POST /Models/DeleteAlternative
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAlternative(int modelId, int alternativeId)
    {
        var alternative = await _db.Alternatives.FindAsync(alternativeId);
        if (alternative == null || alternative.ModelId != modelId) return NotFound();

        var altComps = await _db.AlternativeComparisons
            .Where(a => a.AlternativeAId == alternativeId || a.AlternativeBId == alternativeId)
            .ToListAsync();
        _db.AlternativeComparisons.RemoveRange(altComps);

        _db.Alternatives.Remove(alternative);

        var model = await _db.DecisionModels.FindAsync(modelId);
        if (model != null) await UpdateProjectTimestamp(model.ProjectId);

        await _db.SaveChangesAsync();
        await RebuildAlternativeComparisons(modelId);

        return RedirectToAction(nameof(Edit), new { id = modelId });
    }

    // POST /Models/SaveComparisons
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveComparisons(int modelId, Dictionary<int, double> criteriaValues, Dictionary<int, double> alternativeValues)
    {
        // Save criteria comparisons
        foreach (var kv in criteriaValues)
        {
            var comp = await _db.CriteriaComparisons.FindAsync(kv.Key);
            if (comp != null && comp.ModelId == modelId)
                comp.Value = Math.Clamp(kv.Value, 1.0 / 9.0, 9.0);
        }

        // Save alternative comparisons
        foreach (var kv in alternativeValues)
        {
            var comp = await _db.AlternativeComparisons.FindAsync(kv.Key);
            if (comp != null && comp.ModelId == modelId)
                comp.Value = Math.Clamp(kv.Value, 1.0 / 9.0, 9.0);
        }

        var model = await _db.DecisionModels.FindAsync(modelId);
        if (model != null) await UpdateProjectTimestamp(model.ProjectId);

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = modelId, tab = "comparisons" });
    }

    // POST /Models/CreateForProject
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateForProject(int projectId)
    {
        var project = await _db.Projects
            .Include(p => p.DecisionModel)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId);

        if (project == null) return NotFound();

        // Only create if no model exists
        if (project.DecisionModel != null)
            return RedirectToAction(nameof(Edit), new { id = project.DecisionModel.ModelId });

        var model = new DecisionModel
        {
            ProjectId = projectId,
            Goal = project.Goal
        };
        _db.DecisionModels.Add(model);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = model.ModelId });
    }

    // ---- helpers ----

    private async Task<DecisionModel?> LoadModel(int id)
    {
        return await _db.DecisionModels
            .Include(m => m.Project)
            .Include(m => m.Criteria)
            .Include(m => m.Alternatives)
            .Include(m => m.CriteriaComparisons)
                .ThenInclude(c => c.CriterionA)
            .Include(m => m.CriteriaComparisons)
                .ThenInclude(c => c.CriterionB)
            .Include(m => m.AlternativeComparisons)
                .ThenInclude(a => a.Criterion)
            .Include(m => m.AlternativeComparisons)
                .ThenInclude(a => a.AlternativeA)
            .Include(m => m.AlternativeComparisons)
                .ThenInclude(a => a.AlternativeB)
            .FirstOrDefaultAsync(m => m.ModelId == id);
    }

    private ModelEditViewModel BuildViewModel(DecisionModel model)
    {
        var criteria = model.Criteria.OrderBy(c => c.OrderIndex).ToList();
        var alternatives = model.Alternatives.OrderBy(a => a.OrderIndex).ToList();

        var vm = new ModelEditViewModel
        {
            ModelId = model.ModelId,
            ProjectId = model.ProjectId,
            ProjectGoal = model.Project.Goal,
            Goal = model.Goal,
            Criteria = criteria.Select(c => new CriterionItemViewModel
            {
                CriterionId = c.CriterionId,
                Name = c.Name,
                OrderIndex = c.OrderIndex
            }).ToList(),
            Alternatives = alternatives.Select(a => new AlternativeItemViewModel
            {
                AlternativeId = a.AlternativeId,
                Name = a.Name,
                OrderIndex = a.OrderIndex
            }).ToList(),
            CriteriaComparisons = model.CriteriaComparisons
                .OrderBy(c => c.CriterionAId).ThenBy(c => c.CriterionBId)
                .Select(c => new ComparisonPairViewModel
                {
                    Id = c.Id,
                    CriterionAId = c.CriterionAId,
                    CriterionAName = c.CriterionA.Name,
                    CriterionBId = c.CriterionBId,
                    CriterionBName = c.CriterionB.Name,
                    Value = c.Value
                }).ToList(),
            AlternativeComparisonGroups = criteria.Select(criterion => new AlternativeComparisonGroupViewModel
            {
                CriterionId = criterion.CriterionId,
                CriterionName = criterion.Name,
                Pairs = model.AlternativeComparisons
                    .Where(a => a.CriterionId == criterion.CriterionId)
                    .OrderBy(a => a.AlternativeAId).ThenBy(a => a.AlternativeBId)
                    .Select(a => new AlternativeComparisonPairViewModel
                    {
                        Id = a.Id,
                        AlternativeAId = a.AlternativeAId,
                        AlternativeAName = a.AlternativeA.Name,
                        AlternativeBId = a.AlternativeBId,
                        AlternativeBName = a.AlternativeB.Name,
                        Value = a.Value
                    }).ToList()
            }).ToList()
        };

        // Run AHP calculation if we have enough data
        if (criteria.Count >= 2 && alternatives.Count >= 2 && model.CriteriaComparisons.Any())
            vm.CalculationResult = _ahp.Calculate(model);

        return vm;
    }

    private async Task RebuildCriteriaComparisons(int modelId)
    {
        var criteria = await _db.Criteria
            .Where(c => c.ModelId == modelId)
            .OrderBy(c => c.OrderIndex)
            .ToListAsync();

        for (int i = 0; i < criteria.Count; i++)
        {
            for (int j = i + 1; j < criteria.Count; j++)
            {
                _db.CriteriaComparisons.Add(new CriteriaPairwiseComparison
                {
                    ModelId = modelId,
                    CriterionAId = criteria[i].CriterionId,
                    CriterionBId = criteria[j].CriterionId,
                    Value = 1.0
                });
            }
        }
        await _db.SaveChangesAsync();
    }

    private async Task RebuildAlternativeComparisons(int modelId)
    {
        var criteria = await _db.Criteria.Where(c => c.ModelId == modelId).ToListAsync();
        var alternatives = await _db.Alternatives
            .Where(a => a.ModelId == modelId)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();

        foreach (var criterion in criteria)
        {
            for (int i = 0; i < alternatives.Count; i++)
            {
                for (int j = i + 1; j < alternatives.Count; j++)
                {
                    _db.AlternativeComparisons.Add(new AlternativePairwiseComparison
                    {
                        ModelId = modelId,
                        CriterionId = criterion.CriterionId,
                        AlternativeAId = alternatives[i].AlternativeId,
                        AlternativeBId = alternatives[j].AlternativeId,
                        Value = 1.0
                    });
                }
            }
        }
        await _db.SaveChangesAsync();
    }

    private async Task UpdateProjectTimestamp(int projectId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project != null)
            project.LastUpdatedDate = DateTime.UtcNow;
    }
}

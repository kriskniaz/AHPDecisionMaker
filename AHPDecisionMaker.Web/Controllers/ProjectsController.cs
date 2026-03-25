using AHPDecisionMaker.Web.Data;
using AHPDecisionMaker.Web.Entities;
using AHPDecisionMaker.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AHPDecisionMaker.Web.Controllers;

public class ProjectsController : Controller
{
    private readonly AppDbContext _db;

    public ProjectsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /Projects
    public async Task<IActionResult> Index(string? sortBy)
    {
        var query = _db.Projects
            .Include(p => p.DecisionModel)
            .AsQueryable();

        query = sortBy switch
        {
            "created_asc" => query.OrderBy(p => p.CreatedDate),
            "created_desc" => query.OrderByDescending(p => p.CreatedDate),
            "updated_asc" => query.OrderBy(p => p.LastUpdatedDate),
            "updated_desc" => query.OrderByDescending(p => p.LastUpdatedDate),
            _ => query.OrderByDescending(p => p.CreatedDate)
        };

        var projects = await query.Select(p => new ProjectListItemViewModel
        {
            ProjectId = p.ProjectId,
            Goal = p.Goal,
            Description = p.Description,
            CreatedDate = p.CreatedDate,
            LastUpdatedDate = p.LastUpdatedDate,
            HasModel = p.DecisionModel != null,
            ModelId = p.DecisionModel != null ? p.DecisionModel.ModelId : null
        }).ToListAsync();

        ViewBag.CurrentSort = sortBy ?? "created_desc";
        return View(projects);
    }

    // GET /Projects/Create
    public IActionResult Create()
    {
        return View(new ProjectCreateEditViewModel());
    }

    // POST /Projects/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var project = new Project
        {
            Goal = vm.Goal,
            Description = vm.Description,
            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = project.ProjectId });
    }

    // GET /Projects/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _db.Projects
            .Include(p => p.DecisionModel)
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (project == null)
            return NotFound();

        var vm = new ProjectCreateEditViewModel
        {
            ProjectId = project.ProjectId,
            Goal = project.Goal,
            Description = project.Description,
            ModelId = project.DecisionModel?.ModelId
        };

        return View(vm);
    }

    // POST /Projects/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProjectCreateEditViewModel vm)
    {
        if (id != vm.ProjectId)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(vm);

        var project = await _db.Projects.FindAsync(id);
        if (project == null)
            return NotFound();

        project.Goal = vm.Goal;
        project.Description = vm.Description;
        project.LastUpdatedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Project saved successfully.";
        return RedirectToAction(nameof(Edit), new { id = project.ProjectId });
    }

    // POST /Projects/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null)
            return NotFound();

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}

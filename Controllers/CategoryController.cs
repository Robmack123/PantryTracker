using Microsoft.AspNetCore.Mvc;
using PantryTracker.Data;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly PantryTrackerDbContext _dbContext;

    public CategoryController(PantryTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult GetCategories()
    {
        try
        {
            var categories = _dbContext.Categories.ToList();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching categories: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred.", Exception = ex.Message });
        }
    }
}

using Data;
using AutoMapper;
using DTOs;
using Services;
using Models;
using Microsoft.EntityFrameworkCore;
using Exceptions;

public class ReqCategoryService : IReqCategoryService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ReqCategoryService> _logger;

    public ReqCategoryService(AppDbContext context, IMapper mapper, ILogger<ReqCategoryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReqCategoryDto> CreateCategoryAync(CreateCategoryDto dto)
    {
        try
        {
            _logger.LogInformation("Creating new category with name: {Name}", dto.Name);

            var category = _mapper.Map<ReqCategory>(dto);
            category.isDeleted = false;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category created successfully with ID: {CategoryId}", category.Id);

            return _mapper.Map<ReqCategoryDto>(category);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error occurred while creating category {Name}", dto.Name);
            throw new InternalServerException("Could not create category due to a database error.");
        }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        _logger.LogInformation("Attempting to delete category with ID: {CategoryId}", id);

        var category = await _context.Categories.FindAsync(id);
        if (category == null || category.isDeleted)
        {
            _logger.LogWarning("Delete failed. Category with ID: {CategoryId} not found or already deleted", id);
            throw new NotFoundException($"Category with ID {id} was not found.");
        }

        try
        {
            category.isDeleted = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category with ID: {CategoryId} marked as deleted", id);
            return true;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while deleting category {CategoryId}", id);
            throw new InternalServerException("Could not delete category due to a database error.");
        }
    }

    public async Task<IEnumerable<ReqCategoryDto>> GetAllCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all categories...");

            var categories = await _context.Categories
                .Where(r => !r.isDeleted)
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} categories", categories.Count);

            return _mapper.Map<List<ReqCategoryDto>>(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching categories");
            throw new InternalServerException("Could not fetch categories.");
        }
    }

    public async Task UpdateCategoryAsync(int id, CreateCategoryDto dto)
    {
        _logger.LogInformation("Updating category with ID: {CategoryId}", id);

        var existingCategory = await _context.Categories.FindAsync(id);
        if (existingCategory == null || existingCategory.isDeleted)
        {
            _logger.LogWarning("Update failed. Category with ID: {CategoryId} not found", id);
            throw new NotFoundException($"Category with ID {id} was not found.");
        }

        bool isModified = existingCategory.Name != dto.Name;

        if (!isModified)
        {
            _logger.LogInformation("No changes detected for category with ID: {CategoryId}", id);
            return;
        }

        try
        {
            _mapper.Map(dto, existingCategory);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Category with ID: {CategoryId} updated successfully", id);


        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while updating category {CategoryId}", id);
            throw new InternalServerException("Could not update category due to a database error.");
        }
    }
}

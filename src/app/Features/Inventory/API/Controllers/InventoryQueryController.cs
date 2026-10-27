using Backend.src.app.Features.Inventory.Application.UseCases;
using Backend.src.app.Features.Inventory.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.src.app.Features.Inventory.API.controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryQueryController : ControllerBase
{
    private readonly GetInventoryCategoriesUseCase _getAllCategories;
    private readonly GetInventoryCategoryByIdUseCase _getCategoryById;
    private readonly GetInventoryMovementsUseCase _getAllMovements;
    private readonly GetAppointmentInventoryItemsUseCase _getAppointmentItems;

    private readonly GetInventoryItemsUseCase _getAllItems;
    private readonly GetInventoryItemByIdUseCase _getItemById;
    private readonly GetInventoryMovementByIdUseCase _getMovementById;

    public InventoryQueryController(
        GetInventoryCategoriesUseCase getAllCategories,
        GetInventoryCategoryByIdUseCase getCategoryById,
        GetInventoryItemsUseCase getAllItems,
        GetInventoryItemByIdUseCase getItemById,
        GetInventoryMovementsUseCase getAllMovements,
        GetInventoryMovementByIdUseCase getMovementById,
        GetAppointmentInventoryItemsUseCase getAppointmentItems)
    {
        _getAllCategories = getAllCategories;
        _getCategoryById = getCategoryById;
        _getAllMovements = getAllMovements;
        _getAppointmentItems = getAppointmentItems;

        _getAllItems = getAllItems;
        _getItemById = getItemById;
        _getMovementById = getMovementById;
    }

    // =========================================
    // CATEGORIES
    // =========================================

    [HttpGet("categories")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> GetAllCategories(
        [FromQuery] bool includeInactive = false)
    {
        if (includeInactive &&
            !User.IsInRole("Administrador"))
        {
            return Forbid();
        }

        var categories = await _getAllCategories.Execute(
            includeInactive);

        return Ok(categories);
    }

    [HttpGet("categories/{id:int}")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var category = await _getCategoryById.Execute(id);

        if (!category.IsActive &&
            !User.IsInRole("Administrador"))
        {
            return NotFound();
        }

        return Ok(category);
    }

    // =========================================
    // INVENTORY ITEMS
    // =========================================

    [HttpGet("items")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> GetAllItems(
        [FromQuery] int? categoryId = null,
        [FromQuery] InventoryItemType? itemType = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] bool onlyLowStock = false,
        [FromQuery] string? search = null)
    {
        if (includeInactive &&
            !User.IsInRole("Administrador"))
        {
            return Forbid();
        }

        var items = await _getAllItems.Execute(
            categoryId,
            itemType,
            includeInactive,
            onlyLowStock,
            search);

        return Ok(items);
    }

    [HttpGet("items/{id:int}")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> GetItemById(int id)
    {
        var item = await _getItemById.Execute(id);

        if (!item.IsActive &&
            !User.IsInRole("Administrador"))
        {
            return NotFound();
        }

        return Ok(item);
    }

    // GET: api/inventory/movements
    [HttpGet("movements")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> GetAllMovements(
        [FromQuery] int? inventoryItemId = null,
        [FromQuery] InventoryMovementType? movementType = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var movements = await _getAllMovements.Execute(
            inventoryItemId,
            movementType,
            from,
            to);

        return Ok(movements);
    }

    // GET: api/inventory/movements/{id}
    [HttpGet("movements/{id:long}")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> GetMovementById(long id)
    {
        var movement = await _getMovementById.Execute(id);

        return Ok(movement);
    }

    // GET: api/inventory/appointments/{appointmentId}/items
    [HttpGet("appointments/{appointmentId:int}/items")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> GetAppointmentItems(int appointmentId)
    {
        var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized("Usuario no válido.");
        }

        var isAdmin = User.IsInRole("Administrador");

        var result = await _getAppointmentItems.Execute(
            appointmentId, userId, isAdmin);

        return Ok(result);
    }
}
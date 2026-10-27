using Backend.src.app.Features.Inventory.Application.DTOs;
using Backend.src.app.Features.Inventory.Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.src.app.Features.Inventory.API.controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryCommandController : ControllerBase
{
    private readonly CreateInventoryCategoryUseCase _createCategory;
    private readonly UpdateInventoryCategoryUseCase _updateCategory;
    private readonly DeactivateInventoryCategoryUseCase _deactivateCategory;
    private readonly ReactivateInventoryCategoryUseCase _reactivateCategory;
    private readonly CreateInventoryEntryUseCase _registerEntry;
    private readonly CreateInventoryExitUseCase _registerExit;
    private readonly RegisterAppointmentInventoryItemUseCase _registerAppointmentItem;
    private readonly UpdateAppointmentInventoryItemUseCase _updateAppointmentItem;
    private readonly RemoveAppointmentInventoryItemUseCase _removeAppointmentItem;

    private readonly CreateInventoryItemUseCase _createItem;
    private readonly UpdateInventoryItemUseCase _updateItem;
    private readonly DeactivateInventoryItemUseCase _deactivateItem;
    private readonly ReactivateInventoryItemUseCase _reactivateItem;
    private readonly CreateInventoryAdjustmentUseCase _registerAdjustment;

    public InventoryCommandController(
        CreateInventoryCategoryUseCase createCategory,
        UpdateInventoryCategoryUseCase updateCategory,
        DeactivateInventoryCategoryUseCase deactivateCategory,
        CreateInventoryItemUseCase createItem,
        UpdateInventoryItemUseCase updateItem,
        DeactivateInventoryItemUseCase deactivateItem,
        ReactivateInventoryCategoryUseCase reactivateCategory,
        ReactivateInventoryItemUseCase reactivateItem,
        CreateInventoryEntryUseCase registerEntry,
        CreateInventoryAdjustmentUseCase registerAdjustment,
        CreateInventoryExitUseCase registerExit,
        RegisterAppointmentInventoryItemUseCase registerAppointmentItem,
UpdateAppointmentInventoryItemUseCase updateAppointmentItem,
RemoveAppointmentInventoryItemUseCase removeAppointmentItem)
    {
        _createCategory = createCategory;
        _updateCategory = updateCategory;
        _deactivateCategory = deactivateCategory;
        _reactivateCategory = reactivateCategory;
        _registerEntry = registerEntry;
        _registerExit = registerExit;
        _registerAppointmentItem = registerAppointmentItem;
        _updateAppointmentItem = updateAppointmentItem;
        _removeAppointmentItem = removeAppointmentItem;

        _createItem = createItem;
        _updateItem = updateItem;
        _deactivateItem = deactivateItem;
        _reactivateItem = reactivateItem;
        _registerAdjustment = registerAdjustment;
    }

    // =========================================
    // CATEGORIES
    // =========================================

    [HttpPost("categories")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CreateCategory(
        [FromBody] InventoryCategoryRequestDto dto)
    {
        var category = await _createCategory.Execute(dto);

        return Created(
            $"/api/inventory/categories/{category.Id}",
            category);
    }

    [HttpPut("categories/{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        [FromBody] InventoryCategoryRequestDto dto)
    {
        var category = await _updateCategory.Execute(id, dto);

        return Ok(category);
    }

    [HttpPatch("categories/{id:int}/deactivate")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeactivateCategory(int id)
    {
        await _deactivateCategory.Execute(id);

        return Ok(new
        {
            message = "Categoría desactivada correctamente."
        });
    }

    // PATCH: api/inventory/categories/{id}/activate
    [HttpPatch("categories/{id:int}/activate")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ReactivateCategory(int id)
    {
        await _reactivateCategory.Execute(id);

        return Ok(new
        {
            message = "Categoría reactivada correctamente."
        });
    }

    // =========================================
    // INVENTORY ITEMS
    // =========================================

    [HttpPost("items")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CreateItem(
        [FromBody] InventoryItemRequestDto dto)
    {
        var item = await _createItem.Execute(dto);

        return Created(
            $"/api/inventory/items/{item.Id}",
            item);
    }

    [HttpPut("items/{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> UpdateItem(
        int id,
        [FromBody] InventoryItemRequestDto dto)
    {
        var item = await _updateItem.Execute(id, dto);

        return Ok(item);
    }

    [HttpPatch("items/{id:int}/deactivate")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeactivateItem(int id)
    {
        await _deactivateItem.Execute(id);

        return Ok(new
        {
            message = "Artículo desactivado correctamente."
        });
    }

    // PATCH: api/inventory/items/{id}/activate
    [HttpPatch("items/{id:int}/activate")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ReactivateItem(int id)
    {
        await _reactivateItem.Execute(id);

        return Ok(new
        {
            message = "Artículo reactivado correctamente."
        });
    }

    // POST: api/inventory/movements/entries
    [HttpPost("movements/entries")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> RegisterEntry(
        [FromBody] InventoryEntryRequestDto dto)
    {
        var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized("Usuario no válido.");
        }

        var movement = await _registerEntry.Execute(userId, dto);

        return Created(
            $"/api/inventory/movements/{movement.Id}",
            movement);
    }

    // POST: api/inventory/movements/adjustments
    [HttpPost("movements/adjustments")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> RegisterAdjustment(
        [FromBody] InventoryAdjustmentRequestDto dto)
    {
        var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized("Usuario no válido.");
        }

        var movement = await _registerAdjustment.Execute(userId, dto);

        return Created(
            $"/api/inventory/movements/{movement.Id}",
            movement);
    }

    // POST: api/inventory/movements/exits
    [HttpPost("movements/exits")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> RegisterExit(
        [FromBody] InventoryExitRequestDto dto)
    {
        var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized("Usuario no válido.");
        }

        var movement = await _registerExit.Execute(userId, dto);

        return Created(
            $"/api/inventory/movements/{movement.Id}",
            movement);
    }

    // POST: api/inventory/appointments/{appointmentId}/items
    [HttpPost("appointments/{appointmentId:int}/items")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> RegisterAppointmentItem(
        int appointmentId,
        [FromBody] AppointmentInventoryItemRequestDto dto)
    {
        var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized("Usuario no válido.");
        }

        var isAdmin = User.IsInRole("Administrador");

        var result = await _registerAppointmentItem.Execute(
            appointmentId, userId, isAdmin, dto);

        return Created(
            $"/api/inventory/appointments/{appointmentId}/items/{result.Id}",
            result);
    }

    // PUT: api/inventory/appointments/{appointmentId}/items/{id}
    [HttpPut("appointments/{appointmentId:int}/items/{id:long}")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> UpdateAppointmentItem(
        int appointmentId,
        long id,
        [FromBody] UpdateAppointmentInventoryItemDto dto)
    {
        var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized("Usuario no válido.");
        }

        var isAdmin = User.IsInRole("Administrador");

        var result = await _updateAppointmentItem.Execute(
            appointmentId, id, userId, isAdmin, dto);

        return Ok(result);
    }

    // DELETE: api/inventory/appointments/{appointmentId}/items/{id}
    [HttpDelete("appointments/{appointmentId:int}/items/{id:long}")]
    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> RemoveAppointmentItem(
        int appointmentId,
        long id)
    {
        var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized("Usuario no válido.");
        }

        var isAdmin = User.IsInRole("Administrador");

        await _removeAppointmentItem.Execute(
            appointmentId, id, userId, isAdmin);

        return Ok(new
        {
            message = "Artículo retirado de la reparación correctamente."
        });
    }
}
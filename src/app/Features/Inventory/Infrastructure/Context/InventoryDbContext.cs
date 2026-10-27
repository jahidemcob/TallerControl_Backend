using Backend.src.app.Features.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.src.app.Features.Inventory.Infrastructure.Context;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(
        DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryCategory> InventoryCategories
        => Set<InventoryCategory>();

    public DbSet<InventoryItem> InventoryItems
        => Set<InventoryItem>();

    public DbSet<InventoryMovement> InventoryMovements
        => Set<InventoryMovement>();

    public DbSet<AppointmentInventoryItem> AppointmentInventoryItems
        => Set<AppointmentInventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================================
        // INVENTORY CATEGORIES
        // =========================================

        modelBuilder.Entity<InventoryCategory>(entity =>
        {
            entity.ToTable("InventoryCategories");

            entity.HasKey(category => category.Id);

            entity.Property(category => category.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(category => category.Description)
                .HasMaxLength(250);

            entity.Property(category => category.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(category => category.CreatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            entity.HasIndex(category => category.Name)
                .IsUnique();

            entity.HasMany(category => category.Items)
                .WithOne(item => item.Category)
                .HasForeignKey(item => item.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =========================================
        // INVENTORY ITEMS
        // =========================================

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.ToTable("InventoryItems", table =>
            {
                table.HasCheckConstraint(
                    "CK_InventoryItems_ItemType",
                    "[ItemType] IN ('Repuesto', 'Consumible')");

                table.HasCheckConstraint(
                    "CK_InventoryItems_AvailableQuantity_NonNegative",
                    "[AvailableQuantity] >= 0");

                table.HasCheckConstraint(
                    "CK_InventoryItems_MinimumStock_NonNegative",
                    "[MinimumStock] >= 0");

                table.HasCheckConstraint(
                    "CK_InventoryItems_PurchaseCost_NonNegative",
                    "[PurchaseCost] >= 0");

                table.HasCheckConstraint(
                    "CK_InventoryItems_SalePrice_NonNegative",
                    "[SalePrice] >= 0");
            });

            entity.HasKey(item => item.Id);

            entity.Property(item => item.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(item => item.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(item => item.ItemType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(item => item.Brand)
                .HasMaxLength(100);

            entity.Property(item => item.Description)
                .HasMaxLength(500);

            entity.Property(item => item.AvailableQuantity)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            entity.Property(item => item.MinimumStock)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            entity.Property(item => item.PurchaseCost)
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            entity.Property(item => item.SalePrice)
                .HasColumnType("decimal(12,2)")
                .HasDefaultValue(0m)
                .IsRequired();

            entity.Property(item => item.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(item => item.CreatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            entity.HasIndex(item => item.Code)
                .IsUnique();

            entity.HasIndex(item => item.CategoryId);

            entity.HasIndex(item => item.Name);
        });

        // =========================================
        // INVENTORY MOVEMENTS
        // =========================================

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.ToTable("InventoryMovements", table =>
            {
                table.HasCheckConstraint(
                    "CK_InventoryMovements_MovementType",
                    "[MovementType] IN ('Entrada', 'Salida', " +
                    "'AjustePositivo', 'AjusteNegativo')");

                table.HasCheckConstraint(
                    "CK_InventoryMovements_Quantity_Positive",
                    "[Quantity] > 0");

                table.HasCheckConstraint(
                    "CK_InventoryMovements_UnitCost_NonNegative",
                    "[UnitCost] IS NULL OR [UnitCost] >= 0");
            });

            entity.HasKey(movement => movement.Id);

            entity.Property(movement => movement.MovementType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(movement => movement.Quantity)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(movement => movement.UnitCost)
                .HasColumnType("decimal(12,2)");

            entity.Property(movement => movement.MovementDate)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            entity.Property(movement => movement.Reason)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(movement => movement.AppointmentId)
                .IsRequired(false);

            entity.Property(movement => movement.UserId)
                .IsRequired();

            entity.Property(movement => movement.AppointmentInventoryItemId)
                .IsRequired(false);

            entity.HasOne(movement => movement.InventoryItem)
                .WithMany(item => item.Movements)
                .HasForeignKey(movement => movement.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(movement => movement.AppointmentInventoryItem)
                .WithMany(line => line.Movements)
                .HasForeignKey(movement => movement.AppointmentInventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(movement => movement.InventoryItemId);

            entity.HasIndex(movement => movement.MovementDate);

            entity.HasIndex(movement => movement.UserId);

            entity.HasIndex(movement => movement.AppointmentId);
        });

        // =========================================
        // APPOINTMENT INVENTORY ITEMS
        // =========================================

        modelBuilder.Entity<AppointmentInventoryItem>(entity =>
        {
            entity.ToTable("AppointmentInventoryItems", table =>
            {
                table.HasCheckConstraint(
                    "CK_AppointmentInventoryItems_Quantity_Positive",
                    "[Quantity] > 0");

                table.HasCheckConstraint(
                    "CK_AppointmentInventoryItems_UnitCost_NonNegative",
                    "[UnitCost] >= 0");

                table.HasCheckConstraint(
                    "CK_AppointmentInventoryItems_UnitPrice_NonNegative",
                    "[UnitPrice] >= 0");
            });

            entity.HasKey(line => line.Id);

            entity.Property(line => line.Quantity)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(line => line.UnitCost)
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(line => line.UnitPrice)
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(line => line.Subtotal)
                .HasColumnType("decimal(14,2)")
                .HasComputedColumnSql(
                    "[Quantity] * [UnitPrice]",
                    stored: true);

            entity.Property(line => line.UserId)
                .IsRequired();

            entity.Property(line => line.RecordedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            entity.Property(line => line.InventoryProcessed)
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(line => line.InventoryProcessedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);

            entity.HasOne(line => line.InventoryItem)
                .WithMany(item => item.AppointmentItems)
                .HasForeignKey(line => line.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(line => line.AppointmentId);

            entity.HasIndex(line => line.InventoryItemId);

            entity.HasIndex(line => line.UserId);

            entity.HasIndex(line =>
                new { line.AppointmentId, line.InventoryProcessed });
        });
    }
}
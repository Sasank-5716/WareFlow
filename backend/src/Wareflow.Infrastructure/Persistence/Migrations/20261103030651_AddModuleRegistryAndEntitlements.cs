using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wareflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleRegistryAndEntitlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant_module_settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_module_settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_module_settings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_subscription_entitlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModuleKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Included = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_subscription_entitlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_subscription_entitlements_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_module_settings_TenantId_ModuleKey",
                table: "tenant_module_settings",
                columns: new[] { "TenantId", "ModuleKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscription_entitlements_TenantId_ModuleKey",
                table: "tenant_subscription_entitlements",
                columns: new[] { "TenantId", "ModuleKey" },
                unique: true);

            migrationBuilder.Sql("""
                ALTER TABLE tenant_module_settings ENABLE ROW LEVEL SECURITY;
                ALTER TABLE tenant_module_settings FORCE ROW LEVEL SECURITY;
                CREATE POLICY tenant_module_settings_tenant_isolation
                    ON tenant_module_settings
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::uuid)
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::uuid);

                ALTER TABLE tenant_subscription_entitlements ENABLE ROW LEVEL SECURITY;
                ALTER TABLE tenant_subscription_entitlements FORCE ROW LEVEL SECURITY;
                CREATE POLICY tenant_subscription_entitlements_tenant_isolation
                    ON tenant_subscription_entitlements
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::uuid)
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::uuid);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP POLICY IF EXISTS tenant_module_settings_tenant_isolation ON tenant_module_settings;
                DROP POLICY IF EXISTS tenant_subscription_entitlements_tenant_isolation ON tenant_subscription_entitlements;
                """);

            migrationBuilder.DropTable(
                name: "tenant_module_settings");

            migrationBuilder.DropTable(
                name: "tenant_subscription_entitlements");
        }
    }
}

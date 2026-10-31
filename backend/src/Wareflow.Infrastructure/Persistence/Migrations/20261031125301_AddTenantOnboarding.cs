using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wareflow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "tenant_memberships",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Member");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "tenant_memberships",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.CreateTable(
                name: "tenant_invitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InvitedByIdentityUserId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_invitations_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitations_TenantId_Email_Status",
                table: "tenant_invitations",
                columns: new[] { "TenantId", "Email", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_invitations_TokenHash",
                table: "tenant_invitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.Sql("""
                DROP POLICY IF EXISTS tenant_memberships_tenant_isolation ON tenant_memberships;
                CREATE POLICY tenant_memberships_tenant_isolation
                    ON tenant_memberships
                    USING (
                        "TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::uuid
                        OR "IdentityUserId" = NULLIF(current_setting('app.identity_user_id', true), '')
                    )
                    WITH CHECK (
                        "TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::uuid
                    );

                ALTER TABLE tenant_invitations ENABLE ROW LEVEL SECURITY;
                ALTER TABLE tenant_invitations FORCE ROW LEVEL SECURITY;
                CREATE POLICY tenant_invitations_tenant_isolation
                    ON tenant_invitations
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::uuid)
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::uuid);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP POLICY IF EXISTS tenant_invitations_tenant_isolation ON tenant_invitations;
                DROP POLICY IF EXISTS tenant_memberships_tenant_isolation ON tenant_memberships;
                CREATE POLICY tenant_memberships_tenant_isolation
                    ON tenant_memberships
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::uuid)
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::uuid);
                """);

            migrationBuilder.DropTable(
                name: "tenant_invitations");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "tenant_memberships");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "tenant_memberships");

        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RealEase.Persistence.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace RealEase.Persistence.Migrations;

[DbContext(typeof(RealEaseDbContext))]
[Migration("20261114190000_AddMarketplaceListingsAndAgentInquiries")]
public partial class AddMarketplaceListingsAndAgentInquiries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "city",
            table: "properties",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "listing_type",
            table: "properties",
            type: "text",
            nullable: false,
            defaultValue: "rent");

        migrationBuilder.AddColumn<decimal>(
            name: "sale_price",
            table: "properties",
            type: "numeric(12,2)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "service_area",
            table: "users",
            type: "text",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "agent_inquiries",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "text", nullable: false),
                email = table.Column<string>(type: "text", nullable: false),
                phone_number = table.Column<string>(type: "text", nullable: false),
                location = table.Column<string>(type: "text", nullable: false),
                intent = table.Column<string>(type: "text", nullable: false),
                message = table.Column<string>(type: "text", nullable: true),
                property_id = table.Column<long>(type: "bigint", nullable: true),
                agent_id = table.Column<Guid>(type: "uuid", nullable: true),
                status = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_agent_inquiries", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_agent_inquiries_agent_id_created_at",
            table: "agent_inquiries",
            columns: new[] { "agent_id", "created_at" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "agent_inquiries");
        migrationBuilder.DropColumn(name: "city", table: "properties");
        migrationBuilder.DropColumn(name: "listing_type", table: "properties");
        migrationBuilder.DropColumn(name: "sale_price", table: "properties");
        migrationBuilder.DropColumn(name: "service_area", table: "users");
    }
}

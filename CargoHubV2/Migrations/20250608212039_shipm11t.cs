using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoHubV2.Migrations
{
    /// <inheritdoc />
    public partial class shipm11t : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Stap 1: Update foute data
            migrationBuilder.Sql(@"
                UPDATE ""Shipments"" SET ""ShipmentDate"" = '2000-01-01T00:00:00Z' 
                WHERE ""ShipmentDate"" IS NULL OR trim(""ShipmentDate"") = '';");

            migrationBuilder.Sql(@"
                UPDATE ""Shipments"" SET ""OrderDate"" = '2000-01-01T00:00:00Z' 
                WHERE ""OrderDate"" IS NULL OR trim(""OrderDate"") = '';");

            migrationBuilder.Sql(@"
                UPDATE ""Shipments"" SET ""RequestDate"" = '2000-01-01T00:00:00Z' 
                WHERE ""RequestDate"" IS NULL OR trim(""RequestDate"") = '';");

            // Stap 2: Drop defaults (essentieel!)
            migrationBuilder.Sql(@"ALTER TABLE ""Shipments"" ALTER COLUMN ""ShipmentDate"" DROP DEFAULT;");
            migrationBuilder.Sql(@"ALTER TABLE ""Shipments"" ALTER COLUMN ""OrderDate"" DROP DEFAULT;");
            migrationBuilder.Sql(@"ALTER TABLE ""Shipments"" ALTER COLUMN ""RequestDate"" DROP DEFAULT;");

            // Stap 3: Convert datatype
            migrationBuilder.Sql(@"
                ALTER TABLE ""Shipments""
                ALTER COLUMN ""ShipmentDate"" TYPE timestamp with time zone 
                USING ""ShipmentDate""::timestamp with time zone;");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Shipments""
                ALTER COLUMN ""OrderDate"" TYPE timestamp with time zone 
                USING ""OrderDate""::timestamp with time zone;");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Shipments""
                ALTER COLUMN ""RequestDate"" TYPE timestamp with time zone 
                USING ""RequestDate""::timestamp with time zone;");

            // Stap 4: (optioneel) Set NOT NULL
            migrationBuilder.Sql(@"ALTER TABLE ""Shipments"" ALTER COLUMN ""ShipmentDate"" SET NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE ""Shipments"" ALTER COLUMN ""OrderDate"" SET NOT NULL;");
            migrationBuilder.Sql(@"ALTER TABLE ""Shipments"" ALTER COLUMN ""RequestDate"" SET NOT NULL;");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Zet terug naar tekstveld bij rollback
            migrationBuilder.Sql(@"
                ALTER TABLE ""Shipments""
                ALTER COLUMN ""ShipmentDate"" TYPE text
                USING ""ShipmentDate""::text;");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Shipments""
                ALTER COLUMN ""OrderDate"" TYPE text
                USING ""OrderDate""::text;");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Shipments""
                ALTER COLUMN ""RequestDate"" TYPE text
                USING ""RequestDate""::text;");
        }
    }
}

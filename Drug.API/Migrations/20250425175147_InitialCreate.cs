using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drug.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DrugCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_CODE = table.Column<int>(type: "int", nullable: false),
                    MFR_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    COMPANY_CODE = table.Column<int>(type: "int", nullable: false),
                    COMPANY_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    COMPANY_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADDRESS_MAILING_FLAG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADDRESS_BILLING_FLAG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADDRESS_NOTIFICATION_FLAG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADDRESS_OTHER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SUITE_NUMBER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STREET_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CITY_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PROVINCE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    COUNTRY = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POSTAL_CODE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    POST_OFFICE_BOX = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_CODE = table.Column<int>(type: "int", nullable: false),
                    PHARM_FORM_CODE = table.Column<int>(type: "int", nullable: false),
                    PHARMACEUTICAL_FORM = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugForms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_CODE = table.Column<int>(type: "int", nullable: false),
                    ACTIVE_INGREDIENT_CODE = table.Column<int>(type: "int", nullable: false),
                    INGREDIENT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    INGREDIENT_SUPPLIED_IND = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STRENGTH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STRENGTH_UNIT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STRENGTH_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DOSAGE_VALUE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BASE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DOSAGE_UNIT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NOTES = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugIngredients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DDInterID_A = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DDInterID_B = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Drug_A = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Drug_B = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Management = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InteractionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugInteractions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugPackagings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_CODE = table.Column<int>(type: "int", nullable: false),
                    UPC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PACKAGE_SIZE_UNIT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PACKAGE_TYPE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PACKAGE_SIZE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PRODUCT_INFORMATION = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugPackagings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugPharmaceuticalStds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_CODE = table.Column<int>(type: "int", nullable: false),
                    PHARMACEUTICAL_STD = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugPharmaceuticalStds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_CODE = table.Column<int>(type: "int", nullable: false),
                    ROUTE_OF_ADMINISTRATION_CODE = table.Column<int>(type: "int", nullable: false),
                    ROUTE_OF_ADMINISTRATION = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugRoutes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drugs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_CODE = table.Column<int>(type: "int", nullable: false),
                    PRODUCT_CATEGORIZATION = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CLASS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_IDENTIFICATION_NUMBER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BRAND_NAME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DESCRIPTOR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PEDIATRIC_FLAG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ACCESSION_NUMBER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NUMBER_OF_AIS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LAST_UPDATE_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AI_GROUP_NO = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drugs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_CODE = table.Column<int>(type: "int", nullable: false),
                    SCHEDULE = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugSchedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_CODE = table.Column<int>(type: "int", nullable: false),
                    CURRENT_STATUS_FLAG = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    STATUS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HISTORY_DATE = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugTherapeuticClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_CODE = table.Column<int>(type: "int", nullable: false),
                    TC_ATC_NUMBER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TC_ATC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TC_AHFS_NUMBER = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TC_AHFS = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugTherapeuticClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugVeterinarySpecies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HASH = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DRUG_CODE = table.Column<int>(type: "int", nullable: false),
                    VET_SPECIES = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VET_SUB_SPECIES = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugVeterinarySpecies", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrugCompanies");

            migrationBuilder.DropTable(
                name: "DrugForms");

            migrationBuilder.DropTable(
                name: "DrugIngredients");

            migrationBuilder.DropTable(
                name: "DrugInteractions");

            migrationBuilder.DropTable(
                name: "DrugPackagings");

            migrationBuilder.DropTable(
                name: "DrugPharmaceuticalStds");

            migrationBuilder.DropTable(
                name: "DrugRoutes");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropTable(
                name: "DrugSchedules");

            migrationBuilder.DropTable(
                name: "DrugStatuses");

            migrationBuilder.DropTable(
                name: "DrugTherapeuticClasses");

            migrationBuilder.DropTable(
                name: "DrugVeterinarySpecies");
        }
    }
}

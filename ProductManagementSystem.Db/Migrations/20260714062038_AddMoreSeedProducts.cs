using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductManagementSystem.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreSeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { 6, 1, "Apple iPad Air 11-inch M2, 128GB Wi-Fi, Space Gray", "/images/ipad.jpg", "iPad Air M2", 23900.00m, 15 },
                    { 7, 1, "Portable waterproof wireless speaker with deep bass", "/images/speaker.jpg", "Bluetooth Speaker", 1890.00m, 30 },
                    { 8, 1, "RGB backlit hot-swappable mechanical gaming keyboard", "/images/keyboard.jpg", "Mechanical Keyboard", 3200.00m, 25 },
                    { 9, 1, "Lightweight ergonomic rechargeable gaming mouse", "/images/mouse.jpg", "Wireless Gaming Mouse", 2490.00m, 40 },
                    { 10, 2, "Digital hot air fryer 4.5L with preset cooking modes", "/images/airfryer.jpg", "Air Fryer", 3590.00m, 18 },
                    { 11, 2, "Double-wall stainless steel rapid boil electric kettle 1.7L", "/images/kettle.jpg", "Electric Kettle", 1290.00m, 50 },
                    { 12, 2, "High-speed mini bullet blender for smoothies and shakes", "/images/blender.jpg", "Personal Blender", 1590.00m, 22 },
                    { 13, 3, "Non-slip eco-friendly exercise mat with carrying strap", "/images/yogamat.jpg", "Yoga Mat", 690.00m, 60 },
                    { 14, 3, "Adjustable chrome dumbbell set with storage case", "/images/dumbbell.jpg", "Dumbbell Set 10kg", 1490.00m, 35 },
                    { 15, 3, "Waterproof double-layer family camping tent for 3-4 persons", "/images/tent.jpg", "Camping Tent", 2990.00m, 10 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);
        }
    }
}

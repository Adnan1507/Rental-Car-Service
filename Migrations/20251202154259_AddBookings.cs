// inside Up() — replace the constraints portion of CreateTable for "Bookings"
constraints: table =>
{
    table.PrimaryKey("PK_Bookings", x => x.Id);
    table.ForeignKey(
        name: "FK_Bookings_AspNetUsers_RenterId",
        column: x => x.RenterId,
        principalTable: "AspNetUsers",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);

    table.ForeignKey(
        name: "FK_Bookings_Cars_CarId",
        column: x => x.CarId,
        principalTable: "Cars",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict);
});
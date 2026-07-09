using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaAPI.Migrations
{
    /// <inheritdoc />
    public partial class ComentarioUsuario2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_AspNetUsers_usuarioId",
                table: "Comentarios");

            migrationBuilder.RenameColumn(
                name: "usuarioId",
                table: "Comentarios",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Comentarios_usuarioId",
                table: "Comentarios",
                newName: "IX_Comentarios_UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_AspNetUsers_UsuarioId",
                table: "Comentarios",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_AspNetUsers_UsuarioId",
                table: "Comentarios");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Comentarios",
                newName: "usuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Comentarios_UsuarioId",
                table: "Comentarios",
                newName: "IX_Comentarios_usuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_AspNetUsers_usuarioId",
                table: "Comentarios",
                column: "usuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}


using Microsoft.Playwright;

namespace WinMoversAutoTest.Pages
{
    public class InventarioPage
    {
        private readonly IPage _page;

        public InventarioPage(IPage page)
        {
            _page = page;
        }

        public async Task IrAlInventarioAsync()
        {
            await _page.GotoAsync("/Inventario");
        }

        public async Task AbrirNuevoMaterialAsync()
        {
            await _page.GetByRole(
                AriaRole.Link,
                new() { Name = "Nuevo Material" }
            ).ClickAsync();

            await _page.WaitForURLAsync("**/Inventario/Create");
        }

        public async Task RegistrarMaterialAsync(
            string nombre,
            string categoria,
            string descripcion,
            string unidad,
            int existencias,
            int stockMinimo)
        {
            await _page.Locator("#NombreMaterial").FillAsync(nombre);

            await _page.Locator("#Categoria")
                .SelectOptionAsync(categoria);

            await _page.Locator("#Descripcion")
                .FillAsync(descripcion);

            await _page.Locator("#Unidad").FillAsync(unidad);

            await _page.Locator("#Existencias")
                .FillAsync(existencias.ToString());

            await _page.Locator("#StockMinimo")
                .FillAsync(stockMinimo.ToString());

            await _page.GetByRole(
                AriaRole.Button,
                new() { Name = "Guardar" }
            ).ClickAsync();
        }

        public async Task<bool> MaterialVisibleAsync(string nombre)
        {
            return await _page.GetByRole(AriaRole.Row)
                .Filter(new() { HasText = nombre })
                .IsVisibleAsync();
        }

        public async Task<string> ObtenerMensajeExitoAsync()
        {
            var mensaje = _page.Locator("#toastMsg");

            await mensaje.WaitForAsync(
                new() { State = WaitForSelectorState.Visible }
            );

            return (await mensaje.InnerTextAsync()).Trim();
        }

        public async Task AbrirEdicionMaterialAsync(string nombreMaterial)
        {
            var fila = _page.GetByRole(AriaRole.Row)
                .Filter(new() { HasText = nombreMaterial });

            await fila.Locator("a[title='Editar']").ClickAsync();

            await _page.WaitForURLAsync("**/Inventario/Edit/**");
        }

        public async Task EditarMaterialAsync(
            string nombre,
            string categoria,
            string descripcion,
            string unidad,
            int existencias,
            int stockMinimo)
        {
            await _page.Locator("#NombreMaterial").FillAsync(nombre);

            await _page.Locator("#Categoria").SelectOptionAsync(categoria);

            await _page.Locator("#Descripcion").FillAsync(descripcion);

            await _page.Locator("#Unidad").FillAsync(unidad);

            await _page.Locator("#Existencias")
                .FillAsync(existencias.ToString());

            await _page.Locator("#StockMinimo")
                .FillAsync(stockMinimo.ToString());

            await _page.GetByRole(
                AriaRole.Button,
                new() { Name = "Guardar Cambios" }
            ).ClickAsync();
        }

        public async Task AbrirEliminacionMaterialAsync(string nombreMaterial)
        {
            var fila = _page.GetByRole(AriaRole.Row)
                .Filter(new() { HasText = nombreMaterial });

            await fila.Locator("a[title='Eliminar']").ClickAsync();

            await _page.WaitForURLAsync("**/Inventario/Delete/**");
        }

        public async Task ConfirmarEliminacionMaterialAsync()
        {
            await _page.GetByRole(
                AriaRole.Button,
                new() { Name = "Eliminar" }
            ).ClickAsync();

            await _page.WaitForURLAsync("**/Inventario");
        }

        public async Task<int> ObtenerExistenciasMaterialAsync(string nombreMaterial)
        {
            var fila = _page.GetByRole(AriaRole.Row)
                .Filter(new() { HasText = nombreMaterial });

            await fila.WaitForAsync(
                new() { State = WaitForSelectorState.Visible }
            );

            string existenciasTexto =
                (await fila.Locator("td").Nth(3).InnerTextAsync()).Trim();

            return int.Parse(existenciasTexto);
        }

        public async Task BuscarMaterialAsync(string nombreMaterial)
        {
            await _page.GotoAsync(
                $"/Inventario?buscar={Uri.EscapeDataString(nombreMaterial)}"
            );
        }


    }
}

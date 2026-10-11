
using Microsoft.Playwright;
using NUnit.Framework;
using WinMoversAutoTest.Pages;
using WinMoversAutoTest.Infrastructure;

namespace WinMoversAutoTest.Tests
{
    [TestFixture]
    public class InventarioTest : TestBase
    {

        [Test]
        public async Task TC_INV_001_AsignacionExitosa_DescuentaInventario()
        {
            // 1. Iniciar sesión
            var loginPage = new LoginPage(Page);

            await loginPage.NavigateAsync();

            await loginPage.LoginAsync(
                "marcoalvmejia@gmail.com",
                "Marco1003"
            );

            await Page.WaitForURLAsync("**http://3.15.121.133/");

            var inventarioPage = new InventarioPage(Page);
            var ordenTrabajoPage = new OrdenesPage(Page);

            // 2. Crear un material exclusivo para esta prueba
            string sufijo = Guid.NewGuid().ToString("N")[..8];
            string nombreMaterial = $"Material Asignacion {sufijo}";

            await inventarioPage.IrAlInventarioAsync();
            await inventarioPage.AbrirNuevoMaterialAsync();

            await inventarioPage.RegistrarMaterialAsync(
                nombre: nombreMaterial,
                categoria: "Embalaje",
                descripcion: "Material creado para probar su asignación a una mudanza.",
                unidad: "Unidad",
                existencias: 20,
                stockMinimo: 2
            );

            // 3. Verificar que el material se registró
            await Page.WaitForURLAsync("**/Inventario");

            Assert.That(
                await inventarioPage.MaterialVisibleAsync(nombreMaterial),
                Is.True,
                "No se pudo crear el material de prueba."
            );

            int stockInicial =
                await inventarioPage.ObtenerExistenciasMaterialAsync(
                    nombreMaterial
                );

            Assert.That(
                stockInicial,
                Is.EqualTo(20),
                "Las existencias iniciales no coinciden con las esperadas."
            );

            // 4. Abrir una orden de trabajo existente
            await ordenTrabajoPage.AbrirPrimeraOrdenDisponibleAsync();

            // 5. Asignar 5 unidades del material a la orden
            const int cantidadAsignada = 5;

            await ordenTrabajoPage.AsignarMaterialAsync(
                nombreMaterial,
                cantidadAsignada
            );

            // 6. Verificar el mensaje de confirmación
            string mensaje =
                await ordenTrabajoPage.ObtenerMensajeExitoAsync();

            Assert.That(
                mensaje,
                Does.Contain("Material asignado correctamente."),
                "El sistema no confirmó la asignación del material."
            );

            // 7. Verificar que la asignación aparece en la orden
            Assert.That(
                await ordenTrabajoPage.AsignacionVisibleAsync(
                    nombreMaterial,
                    cantidadAsignada
                ),
                Is.True,
                "El material asignado no aparece en la lista de la orden."
            );

            // 8. Consultar nuevamente el inventario
            await inventarioPage.BuscarMaterialAsync(nombreMaterial);

            int stockFinal =
                await inventarioPage.ObtenerExistenciasMaterialAsync(
                    nombreMaterial
                );

            // 9. Comprobar el descuento de existencias
            Assert.That(
                stockFinal,
                Is.EqualTo(stockInicial - cantidadAsignada),
                "El inventario no descontó correctamente las unidades asignadas."
            );

            Assert.That(
                stockFinal,
                Is.EqualTo(15),
                "Se esperaban 15 unidades después de asignar 5."
            );
        }


        [Test]
        public async Task TC_INV_002_CrearMaterial_Exitoso()
        {
            // 1. Iniciar sesión
            var loginPage = new LoginPage(Page);

            await loginPage.NavigateAsync();

            await loginPage.LoginAsync(
                "marcoalvmejia@gmail.com",
                "Marco1003"
            );

            await Page.WaitForURLAsync("**http://3.15.121.133/");

            // 2. Ingresar al inventario
            var inventarioPage = new InventarioPage(Page);

            await inventarioPage.IrAlInventarioAsync();

            // 3. Abrir el formulario de nuevo material
            await inventarioPage.AbrirNuevoMaterialAsync();

            // 4. Preparar datos únicos para la prueba
            string nombreMaterial =
                $"Material Prueba {DateTime.Now:yyyyMMddHHmmssfff}";

            // 5. Completar el formulario y guardar
            await inventarioPage.RegistrarMaterialAsync(
                nombre: nombreMaterial,
                categoria: "Embalaje",
                descripcion: "Material creado mediante prueba automatizada.",
                unidad: "Unidad",
                existencias: 20,
                stockMinimo: 5
            );

            // 6. Verificar el resultado
            await Page.WaitForURLAsync("**/Inventario");

            string mensaje =
                await inventarioPage.ObtenerMensajeExitoAsync();

            Assert.That(
                mensaje,
                Does.Contain("Material registrado correctamente."),
                "El sistema no mostró el mensaje de creación exitosa."
            );

            Assert.That(
                await inventarioPage.MaterialVisibleAsync(nombreMaterial),
                Is.True,
                "El material creado no aparece en el inventario."
            );
        }

        [Test]
        public async Task TC_INV_003_EditarMaterial_Exitoso()
        {
            // 1. Iniciar sesión
            var loginPage = new LoginPage(Page);

            await loginPage.NavigateAsync();

            await loginPage.LoginAsync(
                "marcoalvmejia@gmail.com",
                "Marco1003"
            );

            await Page.WaitForURLAsync("**http://3.15.121.133/");

            // 2. Ingresar al inventario
            var inventarioPage = new InventarioPage(Page);

            await inventarioPage.IrAlInventarioAsync();

            // 3. Crear un material exclusivo para esta prueba
            string sufijo = Guid.NewGuid().ToString("N")[..8];

            string nombreOriginal = $"Material Original {sufijo}";
            string nombreEditado = $"Material Editado {sufijo}";

            await inventarioPage.AbrirNuevoMaterialAsync();

            await inventarioPage.RegistrarMaterialAsync(
                nombre: nombreOriginal,
                categoria: "Embalaje",
                descripcion: "Material inicial para probar la edición.",
                unidad: "Unidad",
                existencias: 20,
                stockMinimo: 5
            );

            // 4. Comprobar que el material inicial se registró
            await Page.WaitForURLAsync("**/Inventario");

            Assert.That(
                await inventarioPage.MaterialVisibleAsync(nombreOriginal),
                Is.True,
                "No se encontró el material inicial de la prueba."
            );

            // 5. Abrir el formulario de edición
            await inventarioPage.AbrirEdicionMaterialAsync(nombreOriginal);

            // 6. Modificar los datos y guardar
            await inventarioPage.EditarMaterialAsync(
                nombre: nombreEditado,
                categoria: "Protección",
                descripcion: "Material actualizado mediante Playwright.",
                unidad: "Paquete",
                existencias: 35,
                stockMinimo: 8
            );

            // 7. Verificar que el sistema regresó al inventario
            await Page.WaitForURLAsync("**/Inventario");

            // 8. Verificar el mensaje de confirmación
            string mensaje =
                await inventarioPage.ObtenerMensajeExitoAsync();

            Assert.That(
                mensaje,
                Does.Contain("Material actualizado correctamente."),
                "No se mostró el mensaje esperado al actualizar el material."
            );

            // 9. Verificar que el material actualizado aparece en la tabla
            Assert.That(
                await inventarioPage.MaterialVisibleAsync(nombreEditado),
                Is.True,
                "El material actualizado no aparece en el inventario."
            );

            // 10. Verificar que el nombre anterior ya no aparece
            Assert.That(
                await inventarioPage.MaterialVisibleAsync(nombreOriginal),
                Is.False,
                "El inventario todavía muestra el nombre anterior."
            );
        }

        [Test]
        public async Task TC_INV_004_EliminarMaterial_Exitoso()
        {
            // 1. Iniciar sesión
            var loginPage = new LoginPage(Page);

            await loginPage.NavigateAsync();

            await loginPage.LoginAsync(
                "marcoalvmejia@gmail.com",
                "Marco1003"
            );

            await Page.WaitForURLAsync("**http://3.15.121.133/");

            var inventarioPage = new InventarioPage(Page);

            // 2. Ingresar al inventario
            await inventarioPage.IrAlInventarioAsync();

            // 3. Crear un material exclusivo para esta prueba
            string sufijo = Guid.NewGuid().ToString("N")[..8];
            string nombreMaterial = $"Material Para Eliminar {sufijo}";

            await inventarioPage.AbrirNuevoMaterialAsync();

            await inventarioPage.RegistrarMaterialAsync(
                nombre: nombreMaterial,
                categoria: "Embalaje",
                descripcion: "Material temporal para probar la eliminación.",
                unidad: "Unidad",
                existencias: 10,
                stockMinimo: 2
            );

            await Page.WaitForURLAsync("**/Inventario");

            // 4. Verificar que el material existe
            Assert.That(
                await inventarioPage.MaterialVisibleAsync(nombreMaterial),
                Is.True,
                "El material de prueba no se creó correctamente."
            );

            // 5. Seleccionar el material que se desea eliminar
            await inventarioPage.AbrirEliminacionMaterialAsync(
                nombreMaterial
            );

            // 6. Verificar que se abrió la confirmación correcta
            await Assertions.Expect(
                Page.GetByRole(
                    AriaRole.Heading,
                    new()
                    {
                        Name = "¿Está seguro que desea eliminar este material?"
                    }
                )
            ).ToBeVisibleAsync();

            await Assertions.Expect(
                Page.Locator("main").GetByText(nombreMaterial)
            ).ToBeVisibleAsync();

            // 7. Confirmar la eliminación
            await inventarioPage.ConfirmarEliminacionMaterialAsync();

            // 8. Verificar el mensaje de éxito
            string mensaje =
                await inventarioPage.ObtenerMensajeExitoAsync();

            Assert.That(
                mensaje,
                Does.Contain("Material eliminado correctamente."),
                "No se mostró el mensaje esperado después de eliminar."
            );

            // 9. Verificar que el material ya no existe en el listado
            Assert.That(
                await inventarioPage.MaterialVisibleAsync(nombreMaterial),
                Is.False,
                "El material eliminado todavía aparece en el inventario."
            );
        }

    }
}

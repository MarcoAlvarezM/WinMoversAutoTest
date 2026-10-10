namespace Simulacion3_Calidad_Software.Utils;

public static class TestConfig
{
    // Cuenta de administrador SOLO de las BD locales (bloqueada en el servidor).
    // Para correr contra el servidor se sobrescribe con variables de entorno.
    public static string Email =>
        Environment.GetEnvironmentVariable("WM_EMAIL") ?? "admin@winmovers.com";

    public static string Password =>
        Environment.GetEnvironmentVariable("WM_PASSWORD") ?? "Admin123!";
}
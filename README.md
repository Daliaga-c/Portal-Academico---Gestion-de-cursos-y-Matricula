# Portal Académico — Gestión de Cursos y Matrículas

Este es el proyecto de evaluación del curso que implementa un sistema web para gestionar cursos y matrículas de estudiantes.

## Stack Tecnológico
* **Backend:** ASP.NET Core MVC (.NET 8/10)
* **Base de Datos:** SQLite (Local) / EF Core
* **Autenticación:** ASP.NET Core Identity
* **Frontend:** Razor Views + Bootstrap
* **Caché:** Redis (A implementar en las siguientes ramas)

## Ejecución Local

### Prerrequisitos
- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- Herramienta global `dotnet-ef`:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Pasos para levantar el proyecto

1. Clonar el repositorio y moverse a la rama correspondiente.
2. Restaurar dependencias:
   ```bash
   dotnet restore
   ```
3. Aplicar las migraciones a la base de datos:
   ```bash
   dotnet ef database update
   ```
   *(La base de datos se inicializará con datos de prueba: un usuario `coordinador@universidad.edu` y 3 cursos).*
4. Ejecutar el proyecto:
   ```bash
   dotnet run
   ```
5. Acceder en el navegador web (por defecto `https://localhost:7112` o `http://localhost:5200`).

## Migraciones de Base de Datos
Las migraciones se generan utilizando EF Core CLI.

Para agregar una nueva migración:
```bash
dotnet ef migrations add NombreDeMigracion
```
Para actualizar la base de datos:
```bash
dotnet ef database update
```

## Variables de Entorno (Producción)
Para el despliegue en Render, se deben configurar las siguientes variables de entorno:

* `ASPNETCORE_ENVIRONMENT`: `Production`
* `ConnectionStrings__DefaultConnection`: (Cadena de conexión en producción si se cambia SQLite por otra db)
* `Redis__ConnectionString`: (Se agregará en la pregunta correspondiente)

## URL de Despliegue (Render)
*Aún no desplegado. Se actualizará en la pregunta correspondiente.*
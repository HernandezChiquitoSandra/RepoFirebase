using Microsoft.EntityFrameworkCore;

namespace AppBlazor.Server.Helpers
{
    public static class HttpContextExtensions
    {
        public async static Task InsertarParametrosPaginacionEnRespuesta<T>(
                    this HttpContext context, IQueryable<T> queryable, int cantidadRegistrosAMostrar)
        {
            if (context is null) { throw new ArgumentNullException(nameof(context)); }

            double conteo = await queryable.CountAsync();//contar registros
            double totalPaginas = Math.Ceiling(conteo / cantidadRegistrosAMostrar);//total registros / los que quiero mostrar
            context.Response.Headers.Add("conteo", conteo.ToString());
            context.Response.Headers.Add("totalPaginas", totalPaginas.ToString());
        }
    }
}

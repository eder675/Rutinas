using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Linq;

namespace Rutinas
{
    public class BuscarEquiposArea : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;

            string q    = (context.Request.QueryString["q"]    ?? "").Trim();
            string area = (context.Request.QueryString["area"] ?? "").Trim();

            if (q.Length < 2 && string.IsNullOrEmpty(area))
            {
                context.Response.Write("[]");
                return;
            }

            // TAGs ya declarados como desmontados en REPORTES
            var yaDeclarados = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            string connReportes = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
            using (SqlConnection connR = new SqlConnection(connReportes))
            {
                connR.Open();
                using (SqlCommand cmdR = new SqlCommand(
                    "SELECT RTRIM(TAG) FROM DesmontajeAvance WHERE Desmontado = 1", connR))
                using (var drR = cmdR.ExecuteReader())
                    while (drR.Read())
                        yaDeclarados.Add(drR.GetString(0).Trim());
            }

            var excluidas  = AreaExcluidaHelper.ObtenerExcluidas();
            var keywords   = EquipoExcluidoHelper.ObtenerKeywords();

            // Si se filtró por un área excluida, devolver vacío
            if (!string.IsNullOrEmpty(area) && excluidas.Contains(area))
            {
                context.Response.Write("[]");
                return;
            }

            string connStr = WebConfigurationManager.ConnectionStrings["VinetasConnectionString"].ConnectionString;
            var lista = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                var where = new System.Collections.Generic.List<string>();
                if (q.Length >= 2)
                    where.Add("(UPPER(RTRIM(Descripcion)) LIKE UPPER(@q) OR UPPER(RTRIM(TAG)) LIKE UPPER(@q))");
                if (!string.IsNullOrEmpty(area))
                    where.Add("RTRIM(Area) = @area");

                string sql = $@"
                    SELECT TOP 200
                        RTRIM(TAG)         AS TAG,
                        RTRIM(Descripcion) AS Descripcion,
                        ISNULL(RTRIM(Area), 'Sin area') AS Area
                    FROM equipos
                    WHERE {string.Join(" AND ", where)}
                    ORDER BY Area, TAG";

                SqlCommand cmd = new SqlCommand(sql, conn);
                if (q.Length >= 2)    cmd.Parameters.AddWithValue("@q",    "%" + q + "%");
                if (!string.IsNullOrEmpty(area)) cmd.Parameters.AddWithValue("@area", area);

                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                    {
                        string tag      = dr["TAG"].ToString().Trim();
                        string desc     = dr["Descripcion"].ToString();
                        string areaEq   = dr["Area"].ToString();
                        if (yaDeclarados.Contains(tag)) continue;
                        if (excluidas.Contains(areaEq)) continue;
                        if (EquipoExcluidoHelper.EsExcluido(tag, desc, keywords)) continue;
                        lista.Add(new { tag, descripcion = desc, area = areaEq });
                    }
            }

            context.Response.Write(new JavaScriptSerializer().Serialize(lista));
        }

        public bool IsReusable => false;
    }
}

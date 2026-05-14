using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace Rutinas
{
    public class ObtenerEquiposPorArea : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;

            string area = (context.Request.QueryString["area"] ?? "").Trim();
            if (string.IsNullOrEmpty(area)) { context.Response.Write("{}"); return; }

            // 1. Todos los equipos del área en Vinetas (excluyendo los marcados como excluidos)
            var keywords = EquipoExcluidoHelper.ObtenerKeywords();
            var todos = new List<string[]>(); // [tag, descripcion]
            string connVinetas = WebConfigurationManager.ConnectionStrings["VinetasConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connVinetas))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT RTRIM(TAG) AS TAG, RTRIM(Descripcion) AS Descripcion
                      FROM equipos WHERE RTRIM(Area) = @Area
                      ORDER BY TAG", conn);
                cmd.Parameters.AddWithValue("@Area", area);
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                    {
                        string tag  = dr["TAG"].ToString().Trim();
                        string desc = dr["Descripcion"].ToString().Trim();
                        if (!EquipoExcluidoHelper.EsExcluido(tag, desc, keywords))
                            todos.Add(new[] { tag, desc });
                    }
            }

            // 2. Desmontados del área en REPORTES
            var desmDict = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            string connReportes = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connReportes))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT RTRIM(TAG) AS TAG, FechaDeclaracion, RTRIM(NombreEmpleado) AS Nombre
                      FROM DesmontajeAvance
                      WHERE Desmontado = 1 AND RTRIM(Area) = @Area", conn);
                cmd.Parameters.AddWithValue("@Area", area);
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                    {
                        string tag   = dr["TAG"].ToString().Trim();
                        string fecha = dr["FechaDeclaracion"] != DBNull.Value
                            ? Convert.ToDateTime(dr["FechaDeclaracion"]).ToString("dd/MM/yyyy")
                            : "";
                        string nombre = dr["Nombre"].ToString();
                        desmDict[tag] = new[] { fecha, nombre };
                    }
            }

            // 3. Separar en desmontados y pendientes
            var desmontados = new List<object>();
            var pendientes  = new List<object>();

            foreach (var eq in todos)
            {
                string tag  = eq[0];
                string desc = eq[1];
                if (desmDict.TryGetValue(tag, out string[] info))
                    desmontados.Add(new { tag, descripcion = desc, fecha = info[0], nombre = info[1] });
                else
                    pendientes.Add(new { tag, descripcion = desc });
            }

            context.Response.Write(new JavaScriptSerializer().Serialize(new { desmontados, pendientes }));
        }

        public bool IsReusable => false;
    }
}

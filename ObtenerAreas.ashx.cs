using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace Rutinas
{
    public class ObtenerAreas : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;

            string connStr = WebConfigurationManager.ConnectionStrings["VinetasConnectionString"].ConnectionString;
            var areas = new List<string>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT DISTINCT RTRIM(Area) AS Area FROM equipos WHERE Area IS NOT NULL AND RTRIM(Area) <> '' ORDER BY Area",
                    conn);
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        areas.Add(dr["Area"].ToString());
            }

            context.Response.Write(new JavaScriptSerializer().Serialize(areas));
        }

        public bool IsReusable => false;
    }
}

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace Rutinas
{
    public class BuscarEquiposArea : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;

            string q = (context.Request.QueryString["q"] ?? "").Trim();
            if (q.Length < 2)
            {
                context.Response.Write("[]");
                return;
            }

            string connStr = WebConfigurationManager.ConnectionStrings["VinetasConnectionString"].ConnectionString;
            var lista = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                const string sql = @"
                    SELECT TOP 100
                        RTRIM(e.TAG)                        AS TAG,
                        RTRIM(e.Descripcion)                AS Descripcion,
                        ISNULL(RTRIM(a.Area), 'Sin area')   AS Area
                    FROM equipos e
                    LEFT JOIN Areas a ON RTRIM(e.TAG) = RTRIM(a.Tag)
                    WHERE UPPER(RTRIM(e.Descripcion)) LIKE UPPER(@q)
                       OR UPPER(RTRIM(e.TAG))         LIKE UPPER(@q)
                    ORDER BY a.Area, e.TAG";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@q", "%" + q + "%");

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new
                        {
                            tag         = dr["TAG"].ToString(),
                            descripcion = dr["Descripcion"].ToString(),
                            area        = dr["Area"].ToString()
                        });
                    }
                }
            }

            context.Response.Write(new JavaScriptSerializer().Serialize(lista));
        }

        public bool IsReusable => false;
    }
}

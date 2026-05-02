using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace Rutinas
{
    public class BuscarInstrumento : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;

            string q = (context.Request.QueryString["q"] ?? "").Trim();
            if (q.Length < 2) { context.Response.Write("[]"); return; }

            string connStr = WebConfigurationManager.ConnectionStrings["VinetasConnectionString"].ConnectionString;
            var lista = new List<object>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 15 RTRIM(TAG) AS TAG, RTRIM(Descripcion) AS Descripcion
                    FROM equipos
                    WHERE RTRIM(TAG) LIKE @q OR UPPER(Descripcion) LIKE UPPER(@q)
                    ORDER BY TAG", conn);
                cmd.Parameters.AddWithValue("@q", "%" + q + "%");
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        lista.Add(new { tag = dr["TAG"].ToString(), descripcion = dr["Descripcion"].ToString() });
            }

            context.Response.Write(new JavaScriptSerializer().Serialize(lista));
        }

        public bool IsReusable => false;
    }
}

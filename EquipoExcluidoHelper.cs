using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace Rutinas
{
    public static class EquipoExcluidoHelper
    {
        public static List<string> ObtenerKeywords()
        {
            var lista = new List<string>();
            string connStr = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT RTRIM(Keyword) FROM EquiposExcluidosDesmontaje ORDER BY Keyword", conn);
                using (SqlDataReader dr = cmd.ExecuteReader())
                    while (dr.Read()) lista.Add(dr.GetString(0).Trim());
            }
            return lista;
        }

        public static bool EsExcluido(string tag, string descripcion, List<string> keywords)
        {
            foreach (var kw in keywords)
            {
                if (tag.IndexOf(kw, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    descripcion.IndexOf(kw, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
    }
}

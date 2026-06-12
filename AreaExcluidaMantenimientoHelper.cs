using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace Rutinas
{
    public static class AreaExcluidaMantenimientoHelper
    {
        public static HashSet<string> ObtenerExcluidas()
        {
            var set = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            string connStr = WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT RTRIM(Area) FROM AreasMantenimientoExcluida", conn);
                using (SqlDataReader dr = cmd.ExecuteReader())
                    while (dr.Read()) set.Add(dr.GetString(0).Trim());
            }
            return set;
        }
    }
}

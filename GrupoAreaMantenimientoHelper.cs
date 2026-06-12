using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace Rutinas
{
    public static class GrupoAreaMantenimientoHelper
    {
        private static string ConnStr =>
            WebConfigurationManager.ConnectionStrings["ConexionRutinasMTI"].ConnectionString;

        // Lista de grupos con sus áreas (Id, Nombre, Areas)
        public static List<(int Id, string Nombre, HashSet<string> Areas)> ObtenerGrupos()
        {
            var grupos = new List<(int Id, string Nombre, HashSet<string> Areas)>();
            var areasPorGrupo = new Dictionary<int, HashSet<string>>();

            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT Id, Nombre FROM MantenimientoGrupoArea ORDER BY Nombre", conn))
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                        areasPorGrupo[dr.GetInt32(0)] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                using (var cmd = new SqlCommand("SELECT GrupoId, Area FROM MantenimientoGrupoAreaDetalle", conn))
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                    {
                        int gid = dr.GetInt32(0);
                        if (areasPorGrupo.ContainsKey(gid))
                            areasPorGrupo[gid].Add(dr.GetString(1));
                    }

                using (var cmd = new SqlCommand("SELECT Id, Nombre FROM MantenimientoGrupoArea ORDER BY Nombre", conn))
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                    {
                        int id = dr.GetInt32(0);
                        grupos.Add((id, dr.GetString(1), areasPorGrupo[id]));
                    }
            }

            return grupos;
        }

        // Codigo_empleado -> conjunto de GrupoId asignados
        public static Dictionary<string, HashSet<int>> ObtenerGruposEmpleado()
        {
            var result = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT Codigo_empleado, GrupoId FROM MantenimientoEmpleadoGrupo", conn))
                using (var dr = cmd.ExecuteReader())
                    while (dr.Read())
                    {
                        string cod = dr.GetString(0);
                        int gid = dr.GetInt32(1);
                        if (!result.ContainsKey(cod))
                            result[cod] = new HashSet<int>();
                        result[cod].Add(gid);
                    }
            }
            return result;
        }

        // Agrega a areasEmp las áreas que cada empleado recibe por sus grupos asignados
        public static void AplicarGruposAAreas(Dictionary<string, HashSet<string>> areasEmp)
        {
            var grupos = ObtenerGrupos();
            var gruposEmpleado = ObtenerGruposEmpleado();

            foreach (var kv in gruposEmpleado)
            {
                string cod = kv.Key;
                if (!areasEmp.ContainsKey(cod))
                    areasEmp[cod] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (int gid in kv.Value)
                {
                    var grupo = grupos.Find(g => g.Id == gid);
                    if (grupo.Areas == null) continue;
                    foreach (string area in grupo.Areas)
                        areasEmp[cod].Add(area);
                }
            }
        }
    }
}

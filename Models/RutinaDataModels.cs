using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rutinas.Models
{
    
        // Define el resultado de la rotación de grupos
        public struct GrupoRotacionAsignado
        {
            public int IdGrupo { get; set; }
            public string NombreGrupo { get; set; }
        }

        // Define la estructura de cada fila de la rutina
        public class ItemRutina
        {
            public string NombreArea { get; set; }
            public string TAG { get; set; }
            public string NombreInstrumento { get; set; }
            public string Actividad { get; set; }
        }
    
}
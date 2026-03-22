/* ================================================================
   shared.js — Transiciones de página y animaciones de entrada
   Incluir antes de </body> en todas las páginas (excepto impresión).
   ================================================================ */
(function () {
    'use strict';

    /* 1. TRANSICIÓN DE SALIDA al navegar con links reales (no postbacks) */
    document.addEventListener('click', function (e) {
        var el = e.target;
        while (el && el !== document.body) {
            if (el.tagName === 'A' &&
                el.href &&
                !el.href.toLowerCase().startsWith('javascript') &&
                el.href.indexOf('#') === -1 &&
                el.target !== '_blank') {
                e.preventDefault();
                var dest = el.href;
                document.body.classList.add('page-exit');
                setTimeout(function () { window.location.href = dest; }, 220);
                return;
            }
            el = el.parentElement;
        }
    }, true);

    /* 2. FADE-IN ESCALONADO en filas de tabla al cargar */
    window.addEventListener('load', function () {
        var rows = document.querySelectorAll('table tbody tr');
        var max  = Math.min(rows.length, 60);
        for (var i = 0; i < max; i++) {
            (function (row, idx) {
                row.style.opacity = '0';
                row.style.animation = 'fadeUp 0.32s ease both';
                row.style.animationDelay = (0.04 + idx * 0.025) + 's';
            })(rows[i], i);
        }
    });

})();

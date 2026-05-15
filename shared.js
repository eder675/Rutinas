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

/* 3. TEMPORIZADOR DE INACTIVIDAD — cierre automático de sesión a los 3 min */
(function () {
    'use strict';

    var path = window.location.pathname.toLowerCase();
    if (path.indexOf('login') >= 0) return;

    var TOTAL_MS   = 3 * 60 * 1000;   // 3 minutos total
    var WARNING_MS = 30 * 1000;        // aviso 30 s antes del cierre
    var warningTimer, logoutTimer, countdownInterval;
    var overlay = null;

    function crearOverlay() {
        overlay = document.createElement('div');
        overlay.style.cssText = [
            'position:fixed', 'inset:0', 'background:rgba(0,0,0,0.55)',
            'display:none', 'align-items:center', 'justify-content:center',
            'z-index:99999', 'font-family:Arial,sans-serif'
        ].join(';');
        overlay.innerHTML =
            '<div style="background:#fff;border-radius:14px;padding:32px 36px;' +
            'text-align:center;max-width:340px;box-shadow:0 10px 40px rgba(0,0,0,0.3)">' +
            '<div style="font-size:2.2em;margin-bottom:8px">&#9200;</div>' +
            '<h3 style="margin:0 0 10px;color:#c62828">Sesión inactiva</h3>' +
            '<p style="color:#555;margin:0 0 18px">' +
            'La sesión cerrará en <strong id="stt-n">30</strong> s.<br>' +
            '¿Deseas continuar?</p>' +
            '<button id="stt-btn" style="background:#2E7D32;color:#fff;border:none;' +
            'padding:10px 28px;border-radius:8px;font-size:1em;cursor:pointer">' +
            'Continuar</button></div>';
        document.body.appendChild(overlay);
        document.getElementById('stt-btn').addEventListener('click', resetTimers);
    }

    function mostrarAviso() {
        if (!overlay) crearOverlay();
        overlay.style.display = 'flex';
        var n = Math.round(WARNING_MS / 1000);
        document.getElementById('stt-n').textContent = n;
        clearInterval(countdownInterval);
        countdownInterval = setInterval(function () {
            n--;
            var el = document.getElementById('stt-n');
            if (el) el.textContent = n;
            if (n <= 0) clearInterval(countdownInterval);
        }, 1000);
    }

    function cerrarSesion() {
        window.location.href = 'Login.aspx?timeout=1';
    }

    function resetTimers() {
        if (overlay) overlay.style.display = 'none';
        clearTimeout(warningTimer);
        clearTimeout(logoutTimer);
        clearInterval(countdownInterval);
        warningTimer = setTimeout(mostrarAviso, TOTAL_MS - WARNING_MS);
        logoutTimer  = setTimeout(cerrarSesion,  TOTAL_MS);
    }

    ['click', 'keypress', 'mousemove', 'touchstart', 'scroll'].forEach(function (ev) {
        document.addEventListener(ev, resetTimers, { passive: true });
    });

    resetTimers();
})();

/* ── Traffic Signal Alert — Audio (MAUI native app edition) ──────────────
   Runs inside a WKWebView (BlazorWebView) with MediaTypesRequiringUser-
   ActionForPlayback = None, so AudioContext starts in 'running' state
   immediately — no unlock dance needed unlike the PWA version.
─────────────────────────────────────────────────────────────────────────── */
window.tsaAudio = (function () {
    let _ctx = null;

    function ensureCtx() {
        if (_ctx) return;
        try {
            _ctx = new (window.AudioContext || window.webkitAudioContext)();
            console.log('[TSA] AudioContext created, state=' + _ctx.state);
        } catch (e) {
            console.warn('[TSA] AudioContext creation failed:', e);
        }
    }

    // Re-resume if the app was backgrounded (shouldn't happen with native wake lock,
    // but kept as a safety net).
    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'visible' && _ctx && _ctx.state !== 'running') {
            _ctx.resume().catch(function () {});
        }
    });

    /* ── Tone helpers ─────────────────────────────────────────────────── */

    function _sq(t0, t1) {                  // square wave beep at C6 (1047 Hz)
        const osc = _ctx.createOscillator(), g = _ctx.createGain();
        osc.connect(g); g.connect(_ctx.destination);
        osc.type = 'square';
        osc.frequency.value = 1047;
        g.gain.setValueAtTime(0.9, t0);
        g.gain.exponentialRampToValueAtTime(0.001, t1);
        osc.start(t0); osc.stop(t1);
    }

    function _sn(t0, t1, hz) {             // sine note
        const osc = _ctx.createOscillator(), g = _ctx.createGain();
        osc.connect(g); g.connect(_ctx.destination);
        osc.type = 'sine';
        osc.frequency.value = hz;
        g.gain.setValueAtTime(0.7, t0);
        g.gain.exponentialRampToValueAtTime(0.001, t1);
        osc.start(t0); osc.stop(t1);
    }

    /* ── Public API ───────────────────────────────────────────────────── */
    return {
        play: function (tone) {
            ensureCtx();
            if (!_ctx) { console.warn('[TSA] no AudioContext'); return; }

            // Resume if suspended (safety net)
            if (_ctx.state !== 'running') {
                _ctx.resume().then(function () { window.tsaAudio.play(tone); });
                return;
            }

            tone = tone || 'triple-beep';
            const t = _ctx.currentTime;
            console.log('[TSA] play: ' + tone + ' t=' + t.toFixed(3));

            if (tone === 'single-beep') {
                _sq(t, t + 0.18);
            } else if (tone === 'alert-chime') {
                _sn(t,        t + 0.28, 1319);  // E6
                _sn(t + 0.30, t + 0.58, 1047);  // C6
                _sn(t + 0.61, t + 0.95,  784);  // G5
            } else {                              // triple-beep (default)
                _sq(t,        t + 0.18);
                _sq(t + 0.25, t + 0.43);
                _sq(t + 0.50, t + 0.68);
            }
        }
    };
})();

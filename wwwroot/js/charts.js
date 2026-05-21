// Stocke les instances Chart.js pour pouvoir les détruire avant recréation
const _charts = {};

/**
 * Graphique Doughnut (camembert) — répartition par type d'actif
 * Appelé depuis Blazor via IJSRuntime
 */
window.renderDoughnutChart = function(canvasId, labels, values, colors) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    // Détruire l'ancienne instance si elle existe
    if (_charts[canvasId]) {
        _charts[canvasId].destroy();
    }

    _charts[canvasId] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: colors,
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    callbacks: {
                        label: (ctx) => ` ${ctx.label} : ${ctx.parsed.toFixed(1)}%`
                    }
                }
            }
        }
    });
};

/**
 * Graphique Barres — gain/perte par actif
 */
window.renderBarChart = function(canvasId, labels, values) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    if (_charts[canvasId]) {
        _charts[canvasId].destroy();
    }

    // Couleur verte si gain, rouge si perte
    const bgColors = values.map(v => v >= 0
        ? 'rgba(39,174,96,0.75)'
        : 'rgba(231,76,60,0.75)'
    );

    _charts[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Gain / Perte (€)',
                data: values,
                backgroundColor: bgColors,
                borderRadius: 6
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: (ctx) => ` ${ctx.parsed.y >= 0 ? '+' : ''}${ctx.parsed.y.toFixed(2)} €`
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: (v) => v + ' €'
                    }
                }
            }
        }
    });
};
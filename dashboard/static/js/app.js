let severityChart = null;

function renderSeverityChart(p1, p2, p3) {
    const ctx = document.getElementById("severityChart");

    if (severityChart) severityChart.destroy();

    severityChart = new Chart(ctx, {
        type: "doughnut",
        data: {
            labels: ["P1 – Critical", "P2 – High", "P3 – Medium"],
            datasets: [{
                data: [p1, p2, p3],
                backgroundColor: [
                    "#FCA5A5",  // red pastel
                    "#FCD34D",  // orange pastel
                    "#93C5FD"   // blue pastel
                ],
                borderWidth: 1
            }]
        },
        options: {
            cutout: "65%",
            plugins: {
                legend: {
                    position: "bottom"
                }
            }
        }
    });
}

async function loadIncidents() {
    const res = await fetch("/api/incidents");
    const incidents = await res.json();

    let p1 = 0, p2 = 0, p3 = 0;
    const tbody = document.querySelector("#incident-table tbody");
    tbody.innerHTML = "";

    incidents.forEach(i => {
        if (i.severity === "P1") p1++;
        if (i.severity === "P2") p2++;
        if (i.severity === "P3") p3++;

        const row = `
            <tr>
                <td>${i.incident_id}</td>
                <td>${i.severity}</td>
                <td>${i.services.join(", ")}</td>
                <td>${i.description}</td>
                <td>${i.timestamp}</td>
            </tr>
        `;
        tbody.innerHTML += row;
    });

    document.getElementById("p1").innerText = p1;
    document.getElementById("p2").innerText = p2;
    document.getElementById("p3").innerText = p3;

    renderServiceChart(incidents);
    renderSeverityChart(p1, p2, p3);

}

async function loadLogs() {
    const res = await fetch("/api/logs");
    const logs = await res.json();
    document.getElementById("logs").innerText = logs.join("");
}

async function refresh() {
    await loadIncidents();
    await loadLogs();
}

let chart;

function renderServiceChart(incidents) {
    const counts = {};

    incidents.forEach(i => {
        i.services.forEach(s => {
            counts[s] = (counts[s] || 0) + 1;
        });
    });

    const ctx = document.getElementById("serviceChart");

    if (chart) chart.destroy();

    chart = new Chart(ctx, {
        type: "bar",
        data: {
            labels: Object.keys(counts),
            datasets: [{
                label: "Incidents",
                data: Object.values(counts),
                backgroundColor: "#FCD34D"
            }]
        },
        options: {
            plugins: { legend: { display: false } },
            scales: {
                y: { beginAtZero: true }
            }
        }
    });
}


setInterval(refresh, 3000);
refresh();

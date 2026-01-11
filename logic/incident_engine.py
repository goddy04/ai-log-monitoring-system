import json
import os
from datetime import datetime
from correlator import load_anomalies, correlate_anomalies

INCIDENTS_PATH = "data/processed/incidents.json"


def assign_severity(services):
    count = len(services)

    if count >= 3:
        return "P1"
    elif count == 2:
        return "P2"
    elif count == 1:
        return "P3"
    else:
        return None


def create_incident(services):
    severity = assign_severity(services)

    if not severity:
        return []

    incident = {
        "incident_id": int(datetime.now().timestamp()),
        "services": services,
        "severity": severity,
        "description": f"Cascading failure detected between {' and '.join(services)}",
        "timestamp": datetime.now().isoformat()
    }

    return [incident]


def save_incidents(incidents):
    os.makedirs(os.path.dirname(INCIDENTS_PATH), exist_ok=True)
    with open(INCIDENTS_PATH, "w") as f:
        json.dump(incidents, f, indent=4)


if __name__ == "__main__":
    print("[INCIDENT] Loading anomalies...")
    anomalies = load_anomalies()

    print("[INCIDENT] Correlating anomalies...")
    affected_services = correlate_anomalies(anomalies)

    print("[INCIDENT] Creating incident...")
    incidents = create_incident(affected_services)

    print("[INCIDENT] Saving incidents...")
    save_incidents(incidents)

    print("[INCIDENT] Incident engine complete.")

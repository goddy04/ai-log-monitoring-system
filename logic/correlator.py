import json

ANOMALIES_PATH = "data/processed/anomalies.json"


def load_anomalies():
    with open(ANOMALIES_PATH, "r") as f:
        return json.load(f)


def correlate_anomalies(anomalies):
    affected_services = []

    for item in anomalies:
        if item["is_anomaly"]:
            affected_services.append(item["service"])

    return affected_services


if __name__ == "__main__":
    print("[CORRELATOR] Loading anomalies...")
    anomalies = load_anomalies()

    print("[CORRELATOR] Correlating events...")
    affected_services = correlate_anomalies(anomalies)

    print("[CORRELATOR] Affected services:", affected_services)

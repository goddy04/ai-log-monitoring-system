import json
import os
import numpy as np
from sklearn.ensemble import IsolationForest

FEATURES_PATH = "data/features/features.json"
ANOMALIES_PATH = "data/processed/anomalies.json"


def load_features():
    with open(FEATURES_PATH, "r") as f:
        return json.load(f)


def save_anomalies(anomalies):
    os.makedirs(os.path.dirname(ANOMALIES_PATH), exist_ok=True)
    with open(ANOMALIES_PATH, "w") as f:
        json.dump(anomalies, f, indent=4)


def train_and_detect(features):
    X = []
    services = []

    for item in features:
        X.append([
            item["log_count"],
            item["error_count"],
            item["avg_response_time"]
        ])
        services.append(item["service"])

    X = np.array(X)

    model = IsolationForest(contamination=0.5, random_state=42)
    model.fit(X)

    scores = model.decision_function(X)
    predictions = model.predict(X)

    results = []
    for i in range(len(services)):
        results.append({
            "service": services[i],
            "is_anomaly": True if predictions[i] == -1 else False,
            "anomaly_score": float(scores[i])
        })

    return results


if __name__ == "__main__":
    print("[ML] Loading features...")
    features = load_features()

    print("[ML] Running anomaly detection...")
    anomalies = train_and_detect(features)

    print("[ML] Saving anomalies...")
    save_anomalies(anomalies)

    print("[ML] Anomaly detection complete.")

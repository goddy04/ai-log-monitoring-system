from flask import Flask, render_template, jsonify
from pathlib import Path
import json

app = Flask(__name__)

BASE_DIR = Path(__file__).resolve().parent.parent
LOG_FILE = BASE_DIR / "data" / "raw_logs" / "logs.txt"
INCIDENT_FILE = BASE_DIR / "data" / "processed" / "incidents.json"


@app.route("/")
def index():
    return render_template("index.html")


@app.route("/api/logs")
def get_logs():
    if not LOG_FILE.exists():
        return jsonify([])

    with open(LOG_FILE, "r") as f:
        logs = f.readlines()

    return jsonify(logs[-20:])


@app.route("/api/incidents")
def get_incidents():
    if not INCIDENT_FILE.exists():
        return jsonify([])

    with open(INCIDENT_FILE, "r") as f:
        incidents = json.load(f)

    return jsonify(incidents)


if __name__ == "__main__":
    app.run(debug=True)

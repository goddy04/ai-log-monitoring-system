import streamlit as st
import json
import time
from pathlib import Path
import pandas as pd

# ----------------------------------
# CONFIG
# ----------------------------------
st.set_page_config(
    page_title="AI Log Monitoring Dashboard",
    layout="wide"
)

BASE_DIR = Path(__file__).resolve().parent.parent

LOG_FILE = BASE_DIR / "data" / "raw_logs" / "logs.txt"
INCIDENT_FILE = BASE_DIR / "data" / "processed" / "incidents.json"

REFRESH_INTERVAL = 3  # seconds

# ----------------------------------
# DATA LOADERS
# ----------------------------------
def load_logs():
    if not LOG_FILE.exists():
        return []

    with open(LOG_FILE, "r") as f:
        lines = f.readlines()

    # Show last 20 logs
    return lines[-20:]


def load_incidents():
    if not INCIDENT_FILE.exists():
        return []

    with open(INCIDENT_FILE, "r") as f:
        return json.load(f)

# ----------------------------------
# UI SECTIONS
# ----------------------------------
def show_header():
    st.title("AI-Powered Real-Time Log Monitoring & Incident Response System")
    st.markdown(
        """
        This dashboard provides real-time visibility into system logs, 
        detected anomalies, and correlated incidents with severity-based prioritization.
        """
    )
    st.divider()


def show_severity_cards(incidents):
    p1 = sum(1 for i in incidents if i["severity"] == "P1")
    p2 = sum(1 for i in incidents if i["severity"] == "P2")
    p3 = sum(1 for i in incidents if i["severity"] == "P3")

    col1, col2, col3 = st.columns(3)

    with col1:
        st.metric("🔴 P1 – Critical", p1)

    with col2:
        st.metric("🟠 P2 – High", p2)

    with col3:
        st.metric("🟡 P3 – Medium", p3)


def show_incident_table(incidents):
    st.subheader("🚨 Active Incidents")

    if not incidents:
        st.info("No active incidents detected.")
        return

    df = pd.DataFrame(incidents)
    df["services"] = df["services"].apply(lambda x: ", ".join(x))

    st.dataframe(df, use_container_width=True)


def show_live_logs(logs):
    st.subheader("📜 Live Log Stream")

    if not logs:
        st.info("No logs available.")
        return

    log_text = "".join(logs)
    st.text(log_text)


def show_service_impact_chart(incidents):
    st.subheader("📊 Service Impact Summary")

    if not incidents:
        st.info("No service impact to display.")
        return

    service_counts = {}

    for incident in incidents:
        for service in incident["services"]:
            service_counts[service] = service_counts.get(service, 0) + 1

    df = pd.DataFrame(
        service_counts.items(),
        columns=["Service", "Incident Count"]
    )

    st.bar_chart(df.set_index("Service"))

# ----------------------------------
# MAIN APP LOOP
# ----------------------------------
def main():
    show_header()

    incidents = load_incidents()
    logs = load_logs()

    show_severity_cards(incidents)
    st.divider()

    show_incident_table(incidents)
    st.divider()

    col1, col2 = st.columns(2)

    with col1:
        show_live_logs(logs)

    with col2:
        show_service_impact_chart(incidents)

    # Auto refresh
    time.sleep(REFRESH_INTERVAL)
    st.rerun()


if __name__ == "__main__":
    main()

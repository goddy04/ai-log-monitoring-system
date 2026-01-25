import os
import subprocess
import sys
import time

DOTNET_PROJECT_PATH = os.path.join(
    os.path.dirname(__file__),
    "services",
    "data-producer"
)
def run_dotnet(mode):
    print(f"Starting .NET in {mode.upper()} mode...")
    return subprocess.Popen(
        ["dotnet", "run", mode],
        cwd=DOTNET_PROJECT_PATH,
        shell=True
    )

def train_mode():
    # 1. Run .NET to generate + preprocess + extract
    process = run_dotnet("train")
    process.wait()   # Wait until C# finishes

    print("Dataset generation finished.")

    # 2. Train ML model in Python
    

def realtime_mode():
    # 1. Start Python ML receiver first
    print("Starting ML realtime detector...")

    # 2. Start .NET pipeline
    process = run_dotnet("realtime")

    print("Realtime system is running.")
    print("Press Ctrl+C to stop.")

    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("Stopping system...")
        process.terminate()

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python main.py train | realtime")
        sys.exit(1)

    mode = sys.argv[1].lower()

    if mode == "train":
        train_mode()

    elif mode == "realtime":
        realtime_mode()

    else:
        print("Invalid mode. Use train or realtime.")
